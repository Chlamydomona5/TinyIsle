using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CarterGames.Assets.AudioManager;
using DG.Tweening;
using Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using RectInt = Unit.RectInt;

public class GridManager : Singleton<GridManager> , ISaveable
{
    public static int MaxRange = 100;
    
    private CenteredMap<GroundType> _groundMap;
    
    //Events
    [HideInInspector] public UnityEvent<UnitEntity, Vector2Int> onUnitCreate = new();
    [HideInInspector] public UnityEvent<UnitEntity, Vector2Int, Vector2Int> onUnitMove = new();
    [HideInInspector] public UnityEvent<UnitEntity> onUnitBreakDestroy = new();
    [HideInInspector] public UnityEvent<UnitEntity> onUnitSoldDestroy = new();
    [HideInInspector] public UnityEvent onTap;
    
    [Title("Prefabs")] 
    [SerializeField] private UnitEntity defaultEntityPrefab;
    [SerializeField] private ProduceUnitEntity produceEntityPrefab;
    [SerializeField] private ConsumeUnitEntity consumeEntityPrefab;
    [SerializeField] private PortageUnitEntity portageEntityPrefab;
    [SerializeField] private FurnitureUnitEntity furnitureEntityPrefab;

    [SerializeField] private ConsumeUnit_SO coreUnitSo;

    [Title("References")] 
    [SerializeField] public FishController fishController;
    [SerializeField] public RockController rockController;
    [SerializeField] public CrystalController crystalController;
    [SerializeField] public BoxController boxController;
    [SerializeField] private VisualGrid visualGrid;

    [Title("Oberservers")] 
    [SerializeField, ReadOnly] public CoreUnit core;
    [SerializeField, ReadOnly] private List<UnitEntity> allUnits = new();

    private List<ProduceUnitEntity> ProducePool =>
        allUnits.FindAll(x => x is ProduceUnitEntity).Cast<ProduceUnitEntity>().ToList();

    [SerializeField, ReadOnly] private (bool locked, Vector2Int coord) _lockedCoord;
    
    private Tween _consumeTween;
    private Coroutine _heatCoroutine;
    
    private bool _globalCrystalNavigation = false; 
    
    public void Start()
    {
        _groundMap = new CenteredMap<GroundType>(MaxRange);

        //A cross of two 8X4 squares
        for (int i = -3; i < 5; i++)
        {
            for (int j = -3; j < 5; j++)
            {
                if (i > 2 && j < -1) continue;
                if (i < -1 && j > 2) continue;
                if (i < -1 && j < -1) continue;
                _groundMap[i, j] = GroundType.Normal;
            }
        }

        visualGrid.UpdateMap(_groundMap);
        
        core = BuildUnit(coreUnitSo, new Vector2Int(0, 0)).GetComponent<CoreUnit>();
    }
    
    public List<Vector2Int> ExpandToward(Vector2Int coord, int amount, bool effect = true)
    {
        var newMap = new CenteredMap<GroundType>(MaxRange);
        for (int i = -MaxRange / 2; i < MaxRange / 2; i++)
        {
            for (int j = -MaxRange / 2; j < MaxRange / 2; j++)
            {
                newMap[i, j] = _groundMap[i, j];
            }
        }
        
        // Expand by the direction of the coord, and expand blocks by amount
        Vector2 direction = new Vector2(coord.x, coord.y).normalized;
        // Find the edge block on the direction of the coord
        bool exist = true;
        int times = 1;
        Vector2Int edgeCoord = Vector2Int.zero;
        while (exist)
        {
            edgeCoord = new Vector2Int((int)(direction.x * times), (int)(direction.y * times));
            //If the block is exist or occupied by rock
            exist = IsBlockExist(edgeCoord) || rockController.HasRock(edgeCoord);
            times++;
        }

        var expandList = new List<Vector2Int>();
        
        //Expand the blocks
        int expandBlocks = 0;
        //Start from the edge block
        var originList = new List<Vector2Int>() { edgeCoord };
        while (expandBlocks < amount)
        {
            var newList = new List<Vector2Int>();
            foreach (var origin in originList)
            {
                //Expand in 8 directions
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        var newCoord = new Vector2Int(origin.x + i, origin.y + j);
                        //If the block is not exist
                        if (!IsBlockExist(newCoord, newMap))
                        {
                            //If the block is not occupied by rock
                            if (!rockController.HasRock(newCoord))
                            {
                                if (expandBlocks >= amount) break;
                                newMap[newCoord.x, newCoord.y] = GroundType.Normal;
                                expandBlocks++;
                                
                                expandList.Add(newCoord);
                            }
                        }

                        if (!newList.Contains(newCoord))
                            newList.Add(newCoord);
                    }
                }
            }

            originList = newList;
        }

        if (effect)
        {
            _groundMap = newMap;
            visualGrid.UpdateMap(_groundMap);
            UpdateRockToMine();
            
            AchievementManager.Instance.AssignProgress(AchievementType.IslandArea, GetAllBlocks().Count);
        }

        return expandList;
    }

    public Tween ExpandAt(List<Vector2Int> coords)
    {
        foreach (var coord in coords)
        {
            _groundMap[coord.x, coord.y] = GroundType.Normal;
        }
        
        var tween = visualGrid.UpdateMap(_groundMap);
        UpdateRockToMine();
        return tween;
    }

    private void UpdateRockToMine()
    {
        rockController.UpdateConnectedRocks();
    }


    public bool IsInMaxRange(Vector2Int coord)
    {
        return coord.x >= -MaxRange / 2 && coord.x < MaxRange / 2 && coord.y >= -MaxRange / 2 && coord.y < MaxRange / 2;
    }

    public bool IsBlockExist(Vector2Int coord)
    {
        if (IsInMaxRange(coord))
            return _groundMap[coord.x, coord.y] != GroundType.Empty;
        return false;
    }
    
    public bool IsBlockExist(Vector2Int coord, CenteredMap<GroundType> map)
    {
        if (IsInMaxRange(coord))
            return map[coord.x, coord.y] != GroundType.Empty;
        return false;
    }

    public bool HasUnit(Unit_SO unitSo)
    {
        return allUnits.Exists(x => x.unitSo.ID == unitSo.ID);
    }
    
    public GroundType GetGroundType(Vector2Int coord)
    {
        return _groundMap[coord.x, coord.y];
    }
    
    public bool CanBuildEntity(List<Vector2Int> covered, Vector2Int coord)
    {
        var coveredCoords = GetCoveredCoords(coord, covered);
        var type = _groundMap[coord.x, coord.y];
        
        foreach (var coveredCoord in coveredCoords)
        {
            if (!IsBlockExist(coveredCoord)) return false;
            if (type != _groundMap[coveredCoord.x, coveredCoord.y]) return false;
            if (HasEntityOrLocked(coveredCoord)) return false;
        }

        return true;
    }
    
    public static List<Vector2Int> GetCoveredCoords(Vector2Int pivot, List<Vector2Int> coveredCoords)
    {
        var coords = new List<Vector2Int>();
        for (int i = 0; i < coveredCoords.Count; i++)
        {
            var coord = coveredCoords[i];
            coords.Add(new Vector2Int(pivot.x + coord.x, pivot.y + coord.y));
        }

        return coords;
    }
    
    public bool CanMoveTo(UnitEntity unitEntity, Vector2Int coord)
    {
        if (!IsBlockExist(coord)) return false;

        var type = _groundMap[coord.x, coord.y];
        var coveredCoords = GetCoveredCoords(coord, unitEntity.coveredCoordsRaw);

        if (CanPlaceUnitAtType(unitEntity.unitSo.groundType, type) == false) return false;
        
        foreach (var coveredCoord in coveredCoords)
        {
            if (!IsBlockExist(coveredCoord)) return false;
            if (type != _groundMap[coveredCoord.x, coveredCoord.y]) return false;
            //If the coord is occupied by other units(Not contains boxes)
            if (HasUnitEntityOrLocked(coveredCoord) && !unitEntity.RealCoveredCoords.Contains(coveredCoord)) return false;
        }
        return true;
    }

    public bool ChangeGroundType(Vector2Int coord, GroundType type, bool updateVisual = true)
    {
        if (!IsInMaxRange(coord)) return false;
        
        var unit = FindUnitAt(coord);
        unit?.BreakDestroy();
        crystalController.SqueezeCrystalAt(new List<Vector2Int>() { coord });
        
        _groundMap[coord.x, coord.y] = type;
        //Debug.Log("ChangeGroundType " + coord + " to " + type);
        
        if(updateVisual)
            visualGrid.UpdateMap(_groundMap);
        return true;
    }
    
    public bool ChangeGroundType2X2(Vector2Int coord, GroundType type)
    {
        if (!IsInMaxRange(coord)) return false;
        if(!CanBeSpecialSquare(coord, 2)) return false;
                
        var coordList = new List<Vector2Int>()
        {
            coord,
            new(coord.x + 1, coord.y),
            new(coord.x, coord.y + 1),
            new(coord.x + 1, coord.y + 1)
        };
        
        foreach (var ground in coordList)
        {
            _groundMap[ground.x, ground.y] = type;
        }
        
        visualGrid.UpdateMap(_groundMap);
        AchievementManager.Instance.AssignProgress(AchievementType.HighLandArea, GetAllBlocks(GroundType.High).Count);
        AchievementManager.Instance.AssignProgress(AchievementType.PoolArea, GetAllBlocks(GroundType.Low).Count);
        
        return true;
    }

    public bool HasEntityOrLocked(Vector2Int coord)
    {
        foreach (var placement in allUnits)
        {
            if (placement.RealCoveredCoords.Contains(coord)) return true;
            if(_lockedCoord.locked && _lockedCoord.coord == coord) return true;
        }

        if (boxController.HasBox(coord)) return true;
        return false;
    }
    
    public bool HasUnitEntityOrLocked(Vector2Int coord)
    {
        foreach (var placement in allUnits)
        {
            if (placement.RealCoveredCoords.Contains(coord)) return true;
            if(_lockedCoord.locked && _lockedCoord.coord == coord) return true;
        }

        return false;
    }

    public List<Vector2Int> GetAllBlocks()
    {
        var result = new List<Vector2Int>();
        for (int i = -MaxRange / 2; i < MaxRange / 2; i++)
        {
            for (int j = -MaxRange / 2; j < MaxRange / 2; j++)
            {
                if (_groundMap[i, j] != GroundType.Empty)
                {
                    result.Add(new Vector2Int(i, j));
                }
            }
        }
        return result;
    }

    public List<Vector2Int> GetAllBlocks(GroundType type)
    {
        var result = new List<Vector2Int>();
        for (int i = -MaxRange / 2; i < MaxRange / 2; i++)
        {
            for (int j = -MaxRange / 2; j < MaxRange / 2; j++)
            {
                if (_groundMap[i, j] == type)
                {
                    result.Add(new Vector2Int(i, j));
                }
            }
        }
        return result;
    }

    public bool CanBeSpecialSquare(Vector2Int coord, int size)
    {
        var type = _groundMap[coord.x, coord.y];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                //If is too close to altar
                //if(coord.x + i <= 3 && coord.x + i >= -2 && coord.y + j <= 3 && coord.y + j >= -2) return false;
                //If the block not exist
                if (!IsBlockExist(new Vector2Int(coord.x + i, coord.y + j))) return false;
                //If has unit
                if (HasEntityOrLocked(new Vector2Int(coord.x + i, coord.y + j))) return false;
                //If the block type is not the same
                if (type != _groundMap[coord.x + i, coord.y + j]) return false;
            }
        }
        return true;
    }
    
    public float HeightOf(Vector2Int coord)
    {
        if(IsInMaxRange(coord))
            return _groundMap[coord.x, coord.y].Height();
        return 0;
    }

    public List<(Vector2Int coord, Adjacent edge)> GetEdgeBlocks()
    {
        var result = new List<(Vector2Int coord, Adjacent edge)>();
        for (int i = -MaxRange / 2; i < MaxRange / 2; i++)
        {
            for (int j = -MaxRange / 2; j < MaxRange / 2; j++)
            {
                if (_groundMap[i, j] != 0)
                {
                    var edge = new Adjacent();
                    edge.Up = !IsBlockExist(new Vector2Int(i, j + 1));
                    edge.Right = !IsBlockExist(new Vector2Int(i + 1, j));
                    edge.Down = !IsBlockExist(new Vector2Int(i, j - 1));
                    edge.Left = !IsBlockExist(new Vector2Int(i - 1, j));
                    if (edge.directions.Contains(true))
                        result.Add((new Vector2Int(i, j), edge));
                }
            }
        }

        return result;
    }

    public void LockCoord(Vector2Int coord)
    {
        _lockedCoord = (true, coord);
    }
    
    public void UnlockCoord()
    {
        _lockedCoord = (false, Vector2Int.zero);
    }

    public static Vector2Int Pos2Coord(Vector3 pos)
    {
        //Test if it's in range
        var vector2 = new Vector2(pos.x, pos.z);
        var coordx = (int)((Mathf.Abs(vector2.x) + .5f) * Mathf.Sign(vector2.x));
        var coordy = (int)((Mathf.Abs(vector2.y) + .5f) * Mathf.Sign(vector2.y));
        var coord = new Vector2Int(coordx, coordy);
        return coord;
    }

    public Vector3 Coord2Pos(Vector2Int coord)
    {
        return new Vector3(coord.x, HeightOf(coord), coord.y);
    }

    public void EnableGlobalCrystalNavigation(bool enable)
    {
        _globalCrystalNavigation = enable;
    }
    
    #region Unit

    public bool TrySpawnUnit(Unit_SO unitSo)
    {
        return TrySpawnUnit(unitSo, out _);
    }
    
    public bool TrySpawnUnit(Unit_SO unitSo, out UnitEntity entity)
    {
        entity = null;
        //Try to find place
        var des = FindPlaceForUnit(unitSo);
        //Debug.Log("TrySpawnUnit " + unitSo.ID + " at " + des);
        if (des == new Vector2Int(-1000, -1000)) return false;

        //Build unit on place
        entity = BuildUnit(unitSo, des);
        return true;
    }

    public UnitEntity BuildUnit(Unit_SO info, Vector2Int coord, bool countAsAchievement = true)
    {
        //Debug.Log("BuildUnit " + info.ID + " at " + coord);
        
        if (!CanBuildEntity(info.coveredCoords, coord)) return null;
        if (CanPlaceUnitAtType(info.groundType, _groundMap[coord]) == false) return null;

        if (!info.crystalCoexistable)
            crystalController.SqueezeCrystalAt(GetCoveredCoords(coord, info.coveredCoords));

        UnitEntity entity;
        if (info is ProduceUnit_SO)
        {
            entity = Instantiate(produceEntityPrefab, Coord2Pos(coord), Quaternion.identity, transform);
        }
        else if (info is ConsumeUnit_SO)
        {
            entity = Instantiate(consumeEntityPrefab, Coord2Pos(coord), Quaternion.identity, transform);
        }
        else if (info is PortageUnit_SO)
        {
            entity = Instantiate(portageEntityPrefab, Coord2Pos(coord), Quaternion.identity, transform);
        }
        else if (info is FurnitureUnit_SO)
        {
            entity = Instantiate(furnitureEntityPrefab, Coord2Pos(coord), Quaternion.identity, transform);
        }
        else
        {
            entity = Instantiate(defaultEntityPrefab, Coord2Pos(coord), Quaternion.identity, transform);
        }
        
        entity.Init(info, coord);
        allUnits.Add(entity);
        entity.Evolve();
        //Global
        onUnitCreate.Invoke(entity, coord);
        //Effect
        VFXManager.Instance.ProduceBuildEffect(entity);
        AudioManager.instance.Play("NewUnit");
        if(countAsAchievement)
            AchievementManager.Instance.AccumulateProgress(AchievementType.PlantCount);

        return entity;
    }
    
    public void UpdateVisual()
    {
        visualGrid.UpdateMap(_groundMap);
    }

    public bool MoveUnit(UnitEntity unitEntity, Vector2Int coord)
    {
        if (!CanMoveTo(unitEntity, coord)) return false;
        var beforeCoord = unitEntity.coordinate;
        //Move
        unitEntity.MoveTo(coord);
        //Global
        onUnitMove.Invoke(unitEntity, beforeCoord, coord);
        return true;
    }

    public void RemoveUnit(UnitEntity unitEntity)
    {
        if(allUnits.Contains(unitEntity))
            allUnits.Remove(unitEntity);
    }

    public List<T> FindUnitsInRect<T>(Vector2Int center, RectInt range) where T : UnitEntity
    {
        //Find all the unit in a radius of range
        var coveredList = new List<Vector2Int>();
        for (int i = -range.left; i <= range.right; i++)
        {
            for (int j = -range.down; j <= range.up; j++)
            {
                coveredList.Add(new Vector2Int(center.x + i, center.y + j));
            }
        }

        //If the placement's coords has one coord in the range at least, add it to the result
        var result = new List<T>();
        foreach (var unit in allUnits)
        {
            foreach (var coveredCoord in unit.RealCoveredCoords)
            {
                if (coveredList.Contains(coveredCoord) && unit is T)
                {
                    result.Add(unit as T);
                    break;
                }
            }
        }

        return result;
    }
    
    public List<Vector2Int> GetRectCoords(Vector2Int center, RectInt range)
    {
        var coveredList = new List<Vector2Int>();
        for (int i = -range.left; i <= range.right; i++)
        {
            for (int j = -range.down; j <= range.up; j++)
            {
                if(IsInMaxRange(new Vector2Int(center.x + i, center.y + j)))
                    coveredList.Add(new Vector2Int(center.x + i, center.y + j));
            }
        }

        return coveredList;
    }
    
    public List<Vector2Int> GetAround8Dir(List<Vector2Int> coords)
    {
        var allAround = new List<Vector2Int>();
        foreach (var coord in coords)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    var newCoord = new Vector2Int(coord.x + i, coord.y + j);

                    if (!IsBlockExist(newCoord)) continue;

                    if (!allAround.Contains(newCoord) && !coords.Contains(newCoord))
                    {
                        allAround.Add(newCoord);
                    }
                }
            }
        }

        return allAround;
    }

    public List<Vector2Int> GetAround4Dir(List<Vector2Int> coords)
    {
        var allAround = new List<Vector2Int>();
        foreach (var coord in coords)
        {
            var aroundCoord = new Vector2Int(coord.x + 1, coord.y);
            if (IsBlockExist(aroundCoord) && !coords.Contains(aroundCoord))
            {
                allAround.Add(aroundCoord);
            }
            aroundCoord = new Vector2Int(coord.x - 1, coord.y);
            if (IsBlockExist(aroundCoord) && !coords.Contains(aroundCoord))
            {
                allAround.Add(aroundCoord);
            }
            aroundCoord = new Vector2Int(coord.x, coord.y + 1);
            if (IsBlockExist(aroundCoord) && !coords.Contains(aroundCoord))
            {
                allAround.Add(aroundCoord);
            }
            aroundCoord = new Vector2Int(coord.x, coord.y - 1);
            if (IsBlockExist(aroundCoord) && !coords.Contains(aroundCoord))
            {
                allAround.Add(aroundCoord);
            }
        }

        return allAround;
    }
    
    public List<T> FindUnitsInEffect<T>(UnitEntity self, RectInt range, List<string> tags, bool includeSelf)
        where T : UnitEntity
    {
        var inrange = FindUnitsInRect<T>(self.coordinate, range);
        var result = new List<T>();
        foreach (var unit in inrange)
        {
            //Check if the unit is itself
            if (!includeSelf && unit == self) continue;
            //Check if the unit's tags has one tag in the list at least
            //If the list is empty, add all
            if (tags == null || tags.Count == 0)
            {
                result.Add(unit);
            }
            else
            {
                foreach (var fTag in tags)
                {
                    if (unit.unitSo.tags.Contains(fTag))
                    {
                        result.Add(unit);
                        break;
                    }
                }
            }
        }

        return result;
    }
    
    public List<T> FindUnitsAround<T>(UnitEntity unitEntity, List<string> tagsFilter, bool affectSelf) where T : UnitEntity
    {
        var around = GetAround8Dir(unitEntity.RealCoveredCoords);
        var result = new List<T>();
        foreach (var coord in around)
        {
            var unit = FindUnitAt(coord);
            if (unit && unit is T)
            {
                if (unit == unitEntity && !affectSelf) continue;
                
                //If the list is empty, add all the unit
                if (tagsFilter.Count == 0)
                {
                    result.Add(unit as T);
                }
                else
                {
                    foreach (var fTag in tagsFilter)
                    {
                        if (unit.unitSo.tags.Contains(fTag))
                        {
                            result.Add(unit as T);
                            break;
                        }
                    }
                }
            }
        }

        return result;
    }

    public UnitEntity FindUnitAt(Vector2Int coord)
    {
        foreach (var unit in allUnits)
        {
            //if the coord is in the covered coords of the unit
            if (unit.RealCoveredCoords.Contains(coord))
            {
                return unit;
            }
        }

        return null;
    }

    public void ShowAllLevel(bool show)
    {
        allUnits.ForEach(x => x.ShowLevel(show));
    }

    public static bool CanPlaceUnitAtType(GroundType unitType, GroundType groundType)
    {
        if (unitType == GroundType.Normal)
        {
            return groundType == GroundType.Normal || groundType == GroundType.High;
        }
        else if (unitType == GroundType.Low)
        {
            return groundType == GroundType.Low;
        }
        else if (unitType == GroundType.High)
        {
            return groundType == GroundType.High;
        }
        else
        {
            return false;
        }
    }

    public void SetTutorialCrystalMovable(bool movable)
    {
        
    }

    #endregion

    #region Coord

    public Vector2Int FindPlaceForUnit(Unit_SO unit)
    {
        var emptyList = FindBlockWithoutEntity();
        var cameraCoord = Pos2Coord(CameraManager.Instance.transform.position);

        foreach (var empty in emptyList.OrderBy(x => Vector2Int.Distance(x, cameraCoord)))
        {
            if (CanBuildEntity(unit.coveredCoords, empty) &&
                CanPlaceUnitAtType(unit.groundType, _groundMap[empty.x, empty.y]))
            {
                return empty;
            }
        }

        return new Vector2Int(-1000, -1000);
    }

    public Vector2Int FindPlaceForEntity(List<Vector2Int> covereds)
    {
        var emptyList = FindBlockWithoutEntity();
        var cameraCoord = Pos2Coord(CameraManager.Instance.transform.position);
        
        foreach (var empty in emptyList.OrderBy(x => Vector2Int.Distance(x, cameraCoord)))
        {
            if (CanBuildEntity(covereds, empty))
            {
                return empty;
            }
        }
        return new Vector2Int(-1000, -1000);
    }
    
    public Vector2Int FindPlaceForEntity(List<Vector2Int> covereds, GroundType type)
    {
        var emptyList = FindBlockWithoutEntity();
        var cameraCoord = Pos2Coord(CameraManager.Instance.transform.position);
        
        foreach (var empty in emptyList.OrderBy(x => Vector2Int.Distance(x, cameraCoord)))
        {
            if (CanBuildEntity(covereds, empty) && _groundMap[empty.x, empty.y] == type)
            {
                return empty;
            }
        }
        return new Vector2Int(-1000, -1000);
    }

    public List<Vector2Int> FindBlockWithoutEntity()
    {
        var result = new List<Vector2Int>();

        foreach (var cell in GetAllBlocks())
        {
            if (!HasEntityOrLocked(cell))
            {
                result.Add(cell);
            }
        }

        return result;
    }

    #endregion
    
    #region Heat

    public void OpenHeat(int time, bool all = false)
    {
        CloseHeat();
        _heatCoroutine = StartCoroutine(HeatUpdate(time, all));
    }

    private IEnumerator HeatUpdate(int duration, bool all)
    {
        while (true)
        {
            long max = 0;
            foreach (var unitEntity in ProducePool)
            {
                var heat = all ? unitEntity.produceSum : unitEntity.HeatSum(duration);
                if (heat > max) max = heat;
            }

            foreach (var unit in ProducePool)
            {
                unit.SetHeat(duration, max, all);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void CloseHeat()
    {
        if (_heatCoroutine != null) StopCoroutine(_heatCoroutine);
        foreach (var unit in ProducePool)
        {
            unit.CloseHeat();
        }
    }
    
    /// <returns> xMin, xMax, yMin, yMax </returns>
    public RectInt GetCurrentRange()
    {
        //region = -new Vector4(xMin, xMax, yMin, yMax) + new Vector4(1,-1,1,-1) * WaterExtendRange
        var origin = visualGrid.GetRegion();
        // Reform the region
        var res = - origin;
        var pivot1 = Pos2Coord(new Vector3(res.x, 0, res.z));
        var pivot2 = Pos2Coord(new Vector3(res.y, 0, res.w));
        
        //Log all
        //Debug.Log("Region: " + origin + " -> " + res + " -> " + pivot1 + " -> " + pivot2);
        
        return new RectInt(pivot1.x, pivot2.x, pivot1.y, pivot2.y);
    }

    /// <summary>
    /// Find the empty position in the range without nearby blocks
    /// </summary>
    public Vector2Int RandomEmptyCoordInRange(int distance)
    {
        var range = GetCurrentRange();
        var emptyList = new List<Vector2Int>();
        for (int i = range.up; i <= range.right; i++)
        {
            for (int j = range.down; j <= range.left; j++)
            {
                bool can = false;
                if (i == range.up + distance && j >= range.down + distance && j <= range.left - distance) can = true;
                if (i == range.right - distance && j >= range.down + distance && j <= range.left - distance) can = true;
                if (j == range.down + distance && i >= range.up + distance && i <= range.right - distance) can = true;
                if (j == range.left - distance && i >= range.up + distance && i <= range.right - distance) can = true;

                if (can)
                {
                    if(_groundMap[i, j] == GroundType.Empty) emptyList.Add(new Vector2Int(i, j));
                }
            }
        }
        
        return emptyList[Random.Range(0, emptyList.Count)];
    }

    #endregion

    public void Save()
    {
        //Ground
        ES3.Save("GroundMap", _groundMap.Map);
        //Units
        var coords = allUnits.Select(x => x.coordinate).ToList();
        coords.Remove(Vector2Int.zero);
        
        ES3.Save("AllUnits", coords);
        allUnits.ForEach(x => x.Save());
        
        crystalController.Save();
        boxController.Save();
    }

    public void Load()
    {
        //Ground
        _groundMap.Assign(ES3.Load<GroundType[,]>("GroundMap"));
        visualGrid.UpdateMap(_groundMap);
        //Units
        var coords = ES3.Load<List<Vector2Int>>("AllUnits");
        foreach (var coord in coords)
        {
            BuildUnit(ES3.Load<Unit_SO>($"Unit_{coord.x}_{coord.y}.unitSoRef"), coord, false).Load();
        }
        
        crystalController.Load();
        boxController.Load();
    }
}