using System;
using System.Collections.Generic;
using System.Linq;
using CarterGames.Assets.AudioManager;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
using RectInt = Unit.RectInt;

public class CrystalController : SerializedMonoBehaviour, ISaveable
{
    private CenteredMap<CrystalStack> _crystalMap;
    private GridManager Grid => GridManager.Instance;

    [SerializeField] private Crystal crystalPrefab;
    [SerializeField] private Chest chestPrefab;

    [SerializeField, ReadOnly] private List<Crystal> crystalPool = new();
    private List<CrystalStack> _stackPool = new();


    private void Start()
    {
        _crystalMap = new CenteredMap<CrystalStack>(GridManager.MaxRange);
    }

    public bool IsCrystalable(Vector2Int coord, CrystalType type)
    {
        //Special: Expand Crystal can be placed in sea
        if (type == CrystalType.Expand && !Grid.IsBlockExist(coord)) return true;
        
        //General
        if (!Grid.IsBlockExist(coord)) return false;
        
        //Special: Unlock Crystal can not be placed in low
        if (type == CrystalType.Unlock && Grid.GetGroundType(coord) == GroundType.Low) return false;
        
        var stack = _crystalMap[coord.x, coord.y];
        //If the coord is occupied by crystals and the stack is full
        if (stack != null && stack.Count > 0 && stack.Count >= Crystal.StackLimits[stack.CurrentType]) return false;
        //If the coord is occupied by crystals and the type is not the same
        if (stack != null && stack.Count > 0 && stack.CurrentType != type) return false;

        //Specific Type
        var hasEntityOrLocked = Grid.HasEntityOrLocked(coord);
        var unit = Grid.FindUnitAt(coord);
        //If the unit can coexist with crystals
        if (unit && unit.unitSo.crystalCoexistable) return true;
        //If the unit can consume the crystal
        if (unit && unit is ConsumeUnitEntity consumer && consumer.consumeUnitSo.typeMask.HasFlag(type)) return true;
        
        if (hasEntityOrLocked) return false;
        
        return true;
    }

    public bool HasCrystal(Vector2Int coord)
    {
        if (!Grid.IsBlockExist(coord)) return false;
        if (_crystalMap[coord.x, coord.y] != null && _crystalMap[coord.x, coord.y].Count > 0)
        {
            return true;
        }

        return false;
    }

    public bool HasCrystal(Vector2Int coord, CrystalType mask, out int amount)
    {
        amount = 0;
        if (!Grid.IsBlockExist(coord)) return false;
        if (_crystalMap[coord.x, coord.y] != null && _crystalMap[coord.x, coord.y].Count > 0 &&
            mask.HasFlag(_crystalMap[coord.x, coord.y].CurrentType))
        {
            amount = _crystalMap[coord.x, coord.y].Count;
            return true;
        }

        return false;
    }

     public bool HasCrystal(Vector2Int coord, CrystalType mask)
    {
        return HasCrystal(coord, mask, out _);
    }

    public CrystalStack PopCrystalStack(Vector2Int coord)
    {
        if (_crystalMap[coord.x, coord.y] == null) return null;

        var stack = new CrystalStack(_crystalMap[coord.x, coord.y]);
        _crystalMap[coord.x, coord.y].ClearAll();
        return stack;
    }

    public List<(Vector2Int coord, int count)> FindAllCrystal(CrystalType mask)
    {
        var result = new List<(Vector2Int, int)>();

        foreach (var block in Grid.GetAllBlocks())
        {
            if (_crystalMap[block.x, block.y] == null) continue;
            if (_crystalMap[block.x, block.y].Count == 0) continue;
            
            if (mask.HasFlag(_crystalMap[block.x, block.y].CurrentType))
            {
                result.Add((block, _crystalMap[block.x, block.y].Count));
            }
        }

        return result;
    }

    public Vector2Int FindMostCrystalable(Vector2Int coordinate, CrystalType type)
    {
        var list = FindAllCrystalable(type);
        if (list.Count == 0) return new Vector2Int(-1000, -1000);
        //return the one with least queue count
        var places = list.FindAll(x => _crystalMap[x.x, x.y] == null);
        //If has no null, find the one with least queue count
        if (places.Count == 0)
        {
            var min = list.Min(x => _crystalMap[x.x, x.y].Count);
            places = list.FindAll(x => _crystalMap[x.x, x.y].Count == min);
        }

        if (places.Count == 0) return new Vector2Int(-1000, -1000);

        //Find the closest one
        var ordered = places.OrderBy(x => Vector2Int.Distance(x, coordinate)).First();
        return ordered;
    }

    public List<Vector2Int> FindCrystalableAround(UnitEntity entity, CrystalType type)
    {
        var coveredCoords = entity.RealCoveredCoords;

        var allAround = Grid.GetAround8Dir(coveredCoords);
        allAround.RemoveAll(x => !IsCrystalable(x, type));

        var groundType = Grid.GetGroundType(entity.coordinate);
        allAround.RemoveAll(x => !groundType.CanFall(Grid.GetGroundType(x)));

        return allAround;
    }
    
    public Vector2Int FindBestCrystalableAround(ProduceUnitEntity entity, Vector2Int coord)
    {
        var coveredCoords = entity.produceUnitSo.coveredCoords.ConvertAll(x => x + coord);

        var allAround = Grid.GetAround8Dir(coveredCoords);
        allAround.RemoveAll(x => !IsCrystalable(x, entity.produceUnitSo.crystalType));

        var groundType = Grid.GetGroundType(entity.coordinate);
        allAround.RemoveAll(x => !groundType.CanFall(Grid.GetGroundType(x)));
        
        //Find the closest one
        var ordered = allAround.OrderBy(x => DistanceToCore(x,coord)).ToList();
        if(ordered.Count == 0) return new Vector2Int(-1000, -1000);
        return ordered.First();
    }

    public Vector2Int FindBestCrystalableByDistance(Vector2Int source, int distance)
    {
        var allCrystalable = FindAllCrystalable(CrystalType.Gold);
        var inRange = allCrystalable.FindAll(x => Mathf.Abs(x.x - source.x) <= distance && Mathf.Abs(x.y - source.y) <= distance);
        if (inRange.Count == 0) return new Vector2Int(-1000, -1000);
        
        //Find the closest one to zero
        var ordered = inRange.OrderBy(x => DistanceToCore(x,source)).ToList();
        return ordered.First();
    }

    /// <summary>
    /// Distance to the (-1,-1)-(2,2) matrix
    /// </summary>
    public float DistanceToCore(Vector2Int coord, Vector2Int origin)
    {
        int edgeX;
        int edgeY;
        if (origin.x <= 2 && origin.x >= -1) edgeX = origin.x;
        else if (origin.x > 2) edgeX = 2;
        else edgeX = -1;
        
        if (origin.y <= 2 && origin.y >= -1) edgeY = origin.y;
        else if (origin.y > 2) edgeY = 2;
        else edgeY = -1;
        
        return Vector2Int.Distance(coord, new Vector2Int(edgeX, edgeY));
    }

    public List<Crystal> FindCrystalAt(Vector2Int coord, CrystalType mask)
    {
        return _crystalMap[coord.x, coord.y]?.Crystals.ToList().FindAll(x => mask.HasFlag(x.type));
    }

    public List<CrystalStack> FindCrystalAt(List<Vector2Int> coords, CrystalType mask)
    {
        var result = new List<CrystalStack>();
        foreach (var coord in coords)
        {
            if (_crystalMap[coord.x, coord.y] == null || _crystalMap[coord.x, coord.y].Count == 0) continue;
            if (mask.HasFlag(_crystalMap[coord.x, coord.y].CurrentType))
            {
                result.Add(_crystalMap[coord.x, coord.y]);
            }
        }

        return result;
    }

    public List<CrystalStack> FindCrystalAt(Vector2Int center, RectInt range, CrystalType mask)
    {
        var result = new List<CrystalStack>();
        foreach (var coord in GridManager.Instance.GetRectCoords(center, range))
        {
            if (_crystalMap[coord.x, coord.y] == null || _crystalMap[coord.x, coord.y].Count == 0) continue;
            if (mask.HasFlag(_crystalMap[coord.x, coord.y].CurrentType))
            {
                result.Add(_crystalMap[coord.x, coord.y]);
            }
        }

        return result;
    }

    public List<CrystalStack> FindCrystalsAround(UnitEntity unitEntity, CrystalType mask)
    {
        var allAround = GridManager.Instance.GetAround8Dir(unitEntity.RealCoveredCoords);
        return FindCrystalAt(allAround, mask);
    }

    public Crystal GenerateExpandCrystal(Vector3 from, Vector2Int des, int size, bool countAchievement = true, float time = 0.3f)
    {
        var crystal = GetAvailableCrystal();
        crystal.InitAsExpand(des, size);
        var count = _crystalMap[des.x, des.y] == null ? 0 : _crystalMap[des.x, des.y].Count;
        crystal.FlipToAnimation(from, des, time, count).onComplete += () => { PushCrystalTo(des, crystal); };
        
        AudioManager.instance.Play("NewExpandCrystal", 0.5f);
        if(countAchievement)
            AchievementManager.Instance.AccumulateProgress(AchievementType.ExpandCrystalCount);
        
        return crystal;
    }
    
    public Crystal GenerateUnlockCrystal(Vector3 from, Vector2Int des, float time = 0.3f, bool countAchievement = true)
    {
        var crystal = GetAvailableCrystal();
        crystal.InitAsUnlock(des);
        var count = _crystalMap[des.x, des.y] == null ? 0 : _crystalMap[des.x, des.y].Count;
        crystal.FlipToAnimation(from, des, time, count).onComplete += () => { PushCrystalTo(des, crystal); };
        
        UnlockWebManager.Instance.DetectUnlockInform();
        
        if(countAchievement)
            AchievementManager.Instance.AccumulateProgress(AchievementType.UnlockCrystalCount);
        
        return crystal;
    }

    public Crystal GenerateGoldCrystalStandard(Vector3 from, Vector2Int des, int size, int ascendLevel, bool countAchievement = true, float time = 0.3f)
    {
        var crystal = GetAvailableCrystal();
        crystal.InitAsGoldStandard(des, size, ascendLevel);
        
        var count = _crystalMap[des.x, des.y] == null ? 0 : _crystalMap[des.x, des.y].Count;
        crystal.FlipToAnimation(from, des, time, count).onComplete += () => { PushCrystalTo(des, crystal); };

        AudioManager.instance.Play("NewCrystal", 0.1f);
        if (countAchievement)
        {
            AchievementManager.Instance.AccumulateProgress(AchievementType.GoldCrystalCount);
            AchievementManager.Instance.AssignProgress(AchievementType.AscendLevel, ascendLevel);
        }

        return crystal;
    }
    
    public Crystal GenerateGoldCrystalSpecial(Vector3 from, Vector2Int des, int specialValue = 0, float time = 0.3f)
    {
        var crystal = GetAvailableCrystal();
        crystal.InitAsGoldSpecial(des, specialValue);
        
        var count = _crystalMap[des.x, des.y] == null ? 0 : _crystalMap[des.x, des.y].Count;
        crystal.FlipToAnimation(from, des, time, count).onComplete += () => { PushCrystalTo(des, crystal); };

        AudioManager.instance.Play("NewCrystal", 0.1f);
        AchievementManager.Instance.AccumulateProgress(AchievementType.GoldCrystalCount);

        return crystal;
    }

    private Crystal GetAvailableCrystal()
    {
        //Find the first inactive crystal
        foreach (var crystal in crystalPool)
        {
            if (crystal && !crystal.gameObject.activeSelf)
            {
                crystal.gameObject.SetActive(true);
                return crystal;
            }
        }

        //If there's no inactive coin, create a new one
        var newCrystal = Instantiate(crystalPrefab, transform);
        crystalPool.Add(newCrystal);
        return newCrystal;
    }

    public void SqueezeCrystalAt(List<Vector2Int> coordinates)
    {
        foreach (var coordinate in coordinates)
        {
            var stack = _crystalMap[coordinate.x, coordinate.y];
            if (stack == null) continue;
            if (stack.Count == 0) continue;
            //Move all coins to the coinable place
            var count = stack.Count;
            for (int i = 0; i < count; i++)
            {
                PassCrystalToRandom(stack.Pop());
            }
        }
    }

    /// <summary>
    /// Deque a crystal from a place to another
    /// </summary>
    public void FlipTopCrystalAt(Vector2Int from, Vector2Int to, int times = 1)
    {
        for (int i = 0; i < times; i++)
        {
            //If the from place has no crystal, return
            if (!HasCrystal(from)) return;
            var crystal = _crystalMap[from.x, from.y].Pop();

            //If the to place has reached the limit, Destroy the crystal
            if (_crystalMap[to.x, to.y] != null && _crystalMap[to.x, to.y].Count > 0 && _crystalMap[to.x, to.y].Count >=
                Crystal.StackLimits[_crystalMap[to.x, to.y].CurrentType])
            {
                crystal.gameObject.SetActive(false);
            }

            var count = _crystalMap[to.x, to.y] == null ? 0 : _crystalMap[to.x, to.y].Count;
            crystal.FlipToAnimation(GridManager.Instance.Coord2Pos(from), to, 0.3f, count).onComplete += () =>
            {
                PushCrystalTo(to, crystal);
            };
        }
    }

    public Tween FlipOnePopedCrystal(Crystal crystal, Vector2Int to)
    {
        var tween = crystal.FlipToAnimation(crystal.transform.position, to);
        tween.onComplete += () => { PushCrystalTo(to, crystal); };
        return tween;
    }

    public void TapCrystalAt(Vector2Int coord)
    {
        //Only works on gold crystal
        if (HasCrystal(coord, CrystalType.Gold))
        {
            if (PropertyManager.Instance.GoldIsFull())
            {
                UIManager.Instance.TipViaID("GoldIsFull");
                return;
            }
            TutorialManager.Instance?.endCurrentEvent.Invoke("CollectCrystal");
            FlipTopCrystalAt(coord, new Vector2Int(1, 1));
        }
    }

    public Crystal PickCrystalAt(Vector2Int coord)
    {
        if (!HasCrystal(coord)) return null;

        var crystal = _crystalMap[coord.x, coord.y].Pop();
        return crystal;
    }

    public void PushCrystalTo(Vector2Int coord, Crystal crystal)
    {
        crystal.transform.DOComplete();
        crystal.coordinate = coord;
        
        //Special: Expand Crystal can be placed in sea
        if (crystal.type == CrystalType.Expand && !Grid.IsBlockExist(coord))
        {
            
            Sequence seq = DOTween.Sequence();
            seq.Append(crystal.transform.DOMoveY(-1f, 0.25f).SetEase(Ease.InQuad));
            seq.AppendCallback(() =>
            {
                VFXManager.Instance.PlayVFX("CrystalDrop", GridManager.Instance.Coord2Pos(coord));
                AudioManager.instance.Play("CrystalDrop");
                crystal.gameObject.SetActive(false);
            });
            seq.AppendInterval(0.35f);
            seq.AppendCallback((() =>
            {
                Grid.ExpandToward(coord, crystal.Value);
                TutorialManager.Instance?.endCurrentEvent.Invoke("Expand");
            }));
            return;
        }

        //Handle unit
        var unit = Grid.FindUnitAt(coord);
        if (unit && !unit.unitSo.crystalCoexistable)
        {
            if (unit is ConsumeUnitEntity consumer && consumer.consumeUnitSo.typeMask.HasFlag(crystal.type))
            {
                AudioManager.instance.Play("CrystalDropIn", 0.1f);
                consumer.ConsumeCrystal(crystal);
            } 
            else
            {
                PassCrystalToRandom(crystal);
            }
        }
        //If the place is not occupied, add the crystal to the queue
        else
        {
            // If Low, The crystal drops into water
            if (!GridManager.Instance.FindUnitAt(coord) && Grid.GetGroundType(coord) == GroundType.Low)
            {
                crystal.transform.FlipToCoordAnim(crystal.transform.position, coord, 0.5f, 0).onComplete += () =>
                {
                    crystal.gameObject.SetActive(false);
                    VFXManager.Instance.PlayVFX("CrystalDrop", GridManager.Instance.Coord2Pos(coord));
                    AudioManager.instance.Play("CrystalDisappear",0.2f);
                };
                return;
            }

            if (_crystalMap[coord.x, coord.y] != null && _crystalMap[coord.x, coord.y].Count > 0 &&
                _crystalMap[coord.x, coord.y].CurrentType != crystal.type)
            {
                Debug.Log("The place is occupied by a different type of crystal");
                if (PassCrystalToRandom(crystal)) return;
            }

            if (_crystalMap[coord.x, coord.y] != null &&
                _crystalMap[coord.x, coord.y].Count >= Crystal.StackLimits[crystal.type])
            {
                Debug.Log("The place is full of crystals");
                if (PassCrystalToRandom(crystal)) return;
            }

            if (_crystalMap[coord.x, coord.y] == null)
            {
                _crystalMap[coord.x, coord.y] = EstablishNewCrystalStack(coord);
            }

            
            _crystalMap[coord.x, coord.y].Push(crystal);

            unit?.OnCrystalIn();
        }
    }

    public CrystalStack EstablishNewCrystalStack(Vector2Int coord)
    {
        var sTransform = new GameObject($"CrystalStack + {coord}");
        sTransform.transform.SetParent(transform);
        var stack = new CrystalStack(coord, sTransform.transform);
        
        _stackPool.Add(stack);
        
        return stack;
    }

    private void FixedUpdate()
    {
        foreach (var stack in _stackPool)
        {
            stack.ReAdjustPos();
        }
    }

    private bool PassCrystalToRandom(Crystal crystal)
    {
        var list = FindAllCrystalable(crystal.type);
        if (list.Count == 0)
        {
            crystal.gameObject.SetActive(false);
            return false;
        }

        var to = list[Random.Range(0, list.Count)];
        var count = _crystalMap[to.x, to.y] == null ? 0 : _crystalMap[to.x, to.y].Count;
        crystal.FlipToAnimation(crystal.transform.position, to, 0.3f, count).onComplete += () => { PushCrystalTo(to, crystal); };
        return true;
    }

    public List<Vector2Int> FindAllCrystalable(CrystalType type, bool includeLow = false)
    {
        var result = new List<Vector2Int>();

        List<Vector2Int> blocks;

        if (includeLow)
        {
            blocks = Grid.GetAllBlocks();
        }
        else
        {
            blocks = Grid.GetAllBlocks(GroundType.High);
            blocks.AddRange(Grid.GetAllBlocks(GroundType.Normal));
        }

        foreach (var cell in blocks)
        {
            if (IsCrystalable(cell, type))
            {
                result.Add(cell);
            }
        }

        return result;
    }

    public void Save()
    {
        var dataToSave = new List<(string type, Vector2Int coord, int size)>();
        var allStack = _crystalMap.AllValuesNotDefault();
        foreach (var stack in allStack)
        {
            if(!stack.IsEmpty && stack.CurrentType == CrystalType.Gold) continue;
            foreach (var crystal in stack.Crystals)
            {
                dataToSave.Add((crystal.type.ToString(), stack.Coordinate, crystal.size));
            }
        }
        
        ES3.Save("CrystalData", dataToSave);
    }

    public void Load()
    {
        if (!ES3.KeyExists("CrystalData")) return;
        var data = ES3.Load<List<(string type, Vector2Int coord, int size)>>("CrystalData");
        foreach (var (type, coord, size) in data)
        {
            CrystalType crystalType = (CrystalType) Enum.Parse(typeof(CrystalType), type);
            switch (crystalType)
            {
                case CrystalType.Expand:
                    GenerateExpandCrystal(Vector3.zero, coord, size, false);
                    break;
                case CrystalType.Unlock:
                    GenerateUnlockCrystal(Vector3.zero, coord, 0, false);
                    break;
            }
        }
    }
}
