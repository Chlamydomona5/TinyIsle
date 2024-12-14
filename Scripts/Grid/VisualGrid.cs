using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using RectInt = Unit.RectInt;

public class VisualGrid : SerializedMonoBehaviour
{
    public static int WaterExtendRange = 9;
    
    [SerializeField] private MeshRenderer waterRenderer;
    
    public Dictionary<Adjacent, GameObject> CellPrefabs = new();
    public Dictionary<Adjacent, GameObject> LowCellPrefabs = new();
    public Dictionary<Adjacent, GameObject> HighCellPrefabs = new();

    public GameObject specialLowCellCenter;
    public GameObject specialLowCell3SideUp;
    public GameObject specialLowCell3SideDown;
    public GameObject specialLowCellCorner;
    
    public GameObject specialHighCellCenter;
    public GameObject specialHighCell3SideUp;
    public GameObject specialHighCell3SideDown;
    public GameObject specialHighCellCorner;

    public GameObject noBaseHighCellCorner1;
    public GameObject noBaseHighCellCorner2;
    public GameObject noBaseHighCellCorner3;
    
    public CenteredMap<(GameObject cell, GroundType type, Adjacent adjacent)> CellMap;

    // The original cell prefab belong to (false, false, false, false), (false, false, true, false), (false, false, true, true), (true, false, true, false), (true, false, true, true), (true, true, true, true), The four parameters represent up, right, down, left
    [Button]
    public void InitPrefabList()
    {
        CellPrefabs.Clear();
        LowCellPrefabs.Clear();
        HighCellPrefabs.Clear();
        
        CellPrefabs.Add(new Adjacent(false, false, false, false), null);
        CellPrefabs.Add(new Adjacent(false, false, true, false), null);
        CellPrefabs.Add(new Adjacent(false, false, true, true), null);
        CellPrefabs.Add(new Adjacent(true, false, true, false), null);
        CellPrefabs.Add(new Adjacent(true, false, true, true), null);
        CellPrefabs.Add(new Adjacent(true, true, true, true), null);

    }

    private void Awake()
    {
        CellMap = new CenteredMap<(GameObject cell, GroundType type, Adjacent adjacent)>(GridManager.MaxRange);
    }

    /// <summary>
    /// Update the map with the given boolean map, Only return the tween of the last cell(If there are multiple cells to add)
    /// </summary>
    public Tween UpdateMap(CenteredMap<GroundType> map)
    {
        var newList = new List<Vector2Int>();
        var deleteList = new List<Vector2Int>();
        for (int x = -GridManager.MaxRange / 2; x < GridManager.MaxRange / 2; x++)
        {
            for (int y = -GridManager.MaxRange / 2; y < GridManager.MaxRange / 2; y++)
            {
                //If cell is exist on new map
                if (map[x, y] != GroundType.Empty)
                {
                    //If cell not exist on old map or type is different
                    if (!CellMap[x, y].cell || CellMap[x, y].type != map[x, y])
                    {
                        newList.Add(new Vector2Int(x, y));
                    }
                }
                //If cell not exist on new map, but exist on old map
                else if (CellMap[x, y].cell)
                {
                    deleteList.Add(new Vector2Int(x, y));
                }
            }
        }

        //Debug.Log("New List: " + string.Join(", ", newList.Select(x => x.ToString())));
        var tween = AddCells(newList, map);

        foreach (var coord in deleteList)
        {
            Destroy(CellMap[coord.x, coord.y].cell);
            CellMap[coord.x, coord.y] = (null, GroundType.Empty, new Adjacent(false, false, false, false));
        }

        var all = GridManager.Instance.GetAllBlocks();
        int xMax = all.Max(x => x.x);
        int xMin = all.Min(x => x.x);
        int yMax = all.Max(x => x.y);
        int yMin = all.Min(x => x.y);
        waterRenderer.sharedMaterial.DOVector(-new Vector4(xMin, xMax, yMin, yMax) + new Vector4(1,-1,1,-1) *  WaterExtendRange, "_region", 0.5f);

        return tween;
    }

    /// <summary>
    /// Only return the tween of the last cell(If there are multiple cells to add)
    /// </summary>
    private Tween AddCells(List<Vector2Int> coords, CenteredMap<GroundType> map)
    {
        Tween tween = null;
        foreach (var coord in coords)
        {
            //Update cells around the new cell that exist on the map previously
            var adjacentCoords = new List<Vector2Int>()
            {
                new(coord.x, coord.y + 1),
                new(coord.x + 1, coord.y),
                new(coord.x, coord.y - 1),
                new(coord.x - 1, coord.y)
            };
            foreach (var adjacentCoord in adjacentCoords.Where(x =>
                         (!coords.Contains(x)) && map[x.x, x.y] != GroundType.Empty))
            {
                UpdateCell(adjacentCoord, map);
            }
            
            var newTween = AddCellWithAnim(coord, GetPartialAdjacentSameType(coord, coords, map), GetAdjacentSameType(coord, map), GetAdjacentNotEmpty(coord, map), GetCornerAdjacentSameType(coord, map), GetAdjacentLow(coord, map), map[coord.x, coord.y]);
            if(newTween != null) tween = newTween;
        }

        return tween;
    }

    private Tween AddCellWithAnim(Vector2Int coord, Adjacent appealAdjacent, Adjacent adjacentSameType, Adjacent adjacentNotEmpty, Adjacent cornerAdjacentSameType, Adjacent adjacentLow, GroundType type)
    {
        //Dont Instantiate if the cell is in the center altar
        if (coord.x >= -1 && coord.x <= 2 && coord.y >= -1 && coord.y <= 2) return null;
        
        if (CellMap[coord.x, coord.y].cell)
        {
            Destroy(CellMap[coord.x, coord.y].cell);
        }
        
        var animCell = GenerateCellInstance(type, appealAdjacent, adjacentNotEmpty, adjacentLow, cornerAdjacentSameType);
        //CellMap[coord.x, coord.y] = (animCell, type, adjacent);
        animCell.transform.position = new Vector3(coord.x, 0 - 2f, coord.y);
        var tween = animCell.transform.DOMoveY(0, 0.5f).SetEase(Ease.OutSine);
        tween.onComplete += () =>
        {
            Destroy(animCell);
            AddCell(coord, adjacentSameType, adjacentNotEmpty, cornerAdjacentSameType, adjacentLow, type);
        };
        return tween;
    }

    private GameObject GenerateCellInstance(GroundType type, Adjacent adjacentSameType, Adjacent adjacentNotEmpty, Adjacent adjacentLow, Adjacent cornerAdjacentSameType)
    {
        var dict = type switch
        {
            GroundType.Low => LowCellPrefabs,
            GroundType.Normal => CellPrefabs,
            GroundType.High => HighCellPrefabs,
            _ => throw new ArgumentOutOfRangeException()
        };

        // The original cell prefab belong to (false, false, false, false), (false, false, true, false), (false, false, true, true), (true, false, true, false), (true, false, true, true), (true, true, true, true), The four parameters represent up, right, down, left
        // The other adjacent cell prefab can be generated by rotating the original prefab
        // Get Original Adjacent and rotation
        GameObject cell = null;
        Adjacent originalAdjacent = new Adjacent(false, false, false, false);
        int rotation = 0;

        var adjacent = type == GroundType.Normal ? adjacentNotEmpty : adjacentSameType;
        
        for (int i = 0; i < 4; i++)
        {
            if (dict.TryGetValue(adjacent.Rotate(i), out cell))
            {
                originalAdjacent = adjacent.Rotate(i);
                rotation = i;
                break;
            }
        }

        //Special case for corner adjacent, the four parameters represent down-left, down-right, up-left, up-right
        if (type != GroundType.Normal)
        {
            var threeSideUp = type == GroundType.Low ? specialLowCell3SideUp : specialHighCell3SideUp;
            var threeSideDown = type == GroundType.Low ? specialLowCell3SideDown : specialHighCell3SideDown;
            var corner = type == GroundType.Low ? specialLowCellCorner : specialHighCellCorner;
            var center = type == GroundType.Low ? specialLowCellCenter : specialHighCellCenter;
            
            if (originalAdjacent == new Adjacent(true, true, true, true))
            {
                for (int i = 0; i < 4; i++)
                {
                    if (!cornerAdjacentSameType.directions[i])
                    {
                        cell = center;
                        rotation = (4 - i) % 4;
                        break;
                    }
                }
            }
            else if (originalAdjacent == new Adjacent(true, false, true, true))
            {
                var index = (6 - rotation) % 4;
                if (!cornerAdjacentSameType.directions[index])
                {
                    cell = threeSideUp;
                }
                /*else if(!cornerAdjacentSameType.directions[(index + 1) % 4])
                {
                    cell = threeSideDown;
                }*/
            }
            else if (originalAdjacent == new Adjacent(false, false, true, true))
            {
                if (adjacentNotEmpty.directions.Count(x => !x) <= 1)
                {
                    cell = corner;
                }

                if (type == GroundType.High)
                {
                    //Get index of false
                    var indexes = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        if (adjacentLow.directions[i])
                        {
                            indexes.Add(i);
                        }
                    }

                    if (indexes.Count == 1)
                    {
                        var realIndex = (indexes[0] + rotation) % 4;
                        if (realIndex == 0)
                        {
                            cell = noBaseHighCellCorner1;
                        }
                        else if (realIndex == 1)
                        {
                            cell = noBaseHighCellCorner2;
                        }
                    }
                    else if (indexes.Count == 2)
                    {
                        cell = noBaseHighCellCorner3;
                    }
                }
            }
        }

        if(cell)
        {
            var instance = Instantiate(cell, Vector3.zero, Quaternion.Euler(0, -rotation * 90, 0), transform);
            var col = instance.AddComponent<BoxCollider>();
            col.center = Methods.YtoZero(col.center) - new Vector3(0, 0.05f, 0) + Vector3.up * type.Height();
            col.size = Methods.YtoZero(col.size) + new Vector3(0, 0.1f, 0);
            instance.layer = LayerMask.NameToLayer("Ground");
            return instance;
        }
        return new GameObject();
    }

    private void AddCell(Vector2Int coord, Adjacent adjacentSameType, Adjacent adjacentNotEmpty, Adjacent cornerAdjacentSameType, Adjacent adjacentLow, GroundType type)
    {
        Destroy(CellMap[coord.x, coord.y].cell, .5f);
        
        //Dont Instantiate if the cell is in the center altar
        if (coord.x >= -1 && coord.x <= 2 && coord.y >= -1 && coord.y <= 2) return;

        var cell = GenerateCellInstance(type, adjacentSameType, adjacentNotEmpty,adjacentLow, cornerAdjacentSameType);
        cell.transform.position = new Vector3(coord.x, 0, coord.y);
        CellMap[coord.x, coord.y] = (cell, type, adjacentSameType);
    }

    private void UpdateCell(Vector2Int coord, CenteredMap<GroundType> map)
    {
        //Dont Instantiate if the cell is in the center altar
        if(coord.x >= -1 && coord.x <= 2 && coord.y >= -1 && coord.y <= 2) return;

        Destroy(CellMap[coord.x, coord.y].cell, .5f);
        AddCell(coord, GetAdjacentSameType(coord, map), GetAdjacentNotEmpty(coord, map), GetCornerAdjacentSameType(coord, map), GetAdjacentLow(coord, map), map[coord.x, coord.y]);
    }

    private Adjacent GetAdjacentSameType(Vector2Int coord, CenteredMap<GroundType> map)
    {
        var adjacent = new Adjacent(false, false, false, false);
        var type = map[coord.x, coord.y];
        if (coord.y + 1 < GridManager.MaxRange / 2 && map[coord.x, coord.y + 1] == type)
            adjacent.Up = true;
        if (coord.x + 1 < GridManager.MaxRange / 2 && map[coord.x + 1, coord.y] == type)
            adjacent.Right = true;
        if (coord.y - 1 > -GridManager.MaxRange / 2 && map[coord.x, coord.y - 1] == type)
            adjacent.Down = true;
        if (coord.x - 1 > -GridManager.MaxRange / 2 && map[coord.x - 1, coord.y] == type)
            adjacent.Left = true;

        return adjacent;
    }
    
    private Adjacent GetAdjacentNotEmpty(Vector2Int coord, CenteredMap<GroundType> map)
    {
        var adjacent = new Adjacent(false, false, false, false);
        if (coord.y + 1 < GridManager.MaxRange / 2 && map[coord.x, coord.y + 1] != GroundType.Empty)
            adjacent.Up = true;
        if (coord.x + 1 < GridManager.MaxRange / 2 && map[coord.x + 1, coord.y] != GroundType.Empty)
            adjacent.Right = true;
        if (coord.y - 1 > -GridManager.MaxRange / 2 && map[coord.x, coord.y - 1] != GroundType.Empty)
            adjacent.Down = true;
        if (coord.x - 1 > -GridManager.MaxRange / 2 && map[coord.x - 1, coord.y] != GroundType.Empty)
            adjacent.Left = true;

        return adjacent;
    }
    
    private Adjacent GetCornerAdjacentSameType(Vector2Int coord, CenteredMap<GroundType> map)
    {
        var adjacent = new Adjacent(false, false, false, false);
        var type = map[coord.x, coord.y];
        if (map[coord.x + 1, coord.y + 1] == type)
            adjacent.Up = true;
        if (map[coord.x + 1, coord.y - 1] == type)
            adjacent.Right = true;
        if (map[coord.x - 1, coord.y - 1] == type)
            adjacent.Down = true;
        if (map[coord.x - 1, coord.y + 1] == type)
            adjacent.Left = true;
        return adjacent;
    }
    
    private Adjacent GetAdjacentLow(Vector2Int coord, CenteredMap<GroundType> map)
    {
        var adjacent = new Adjacent(false, false, false, false);
        if (coord.y + 1 < GridManager.MaxRange / 2 && map[coord.x, coord.y + 1] == GroundType.Low)
            adjacent.Up = true;
        if (coord.x + 1 < GridManager.MaxRange / 2 && map[coord.x + 1, coord.y] == GroundType.Low)
            adjacent.Right = true;
        if (coord.y - 1 > -GridManager.MaxRange / 2 && map[coord.x, coord.y - 1] == GroundType.Low)
            adjacent.Down = true;
        if (coord.x - 1 > -GridManager.MaxRange / 2 && map[coord.x - 1, coord.y] == GroundType.Low)
            adjacent.Left = true;
        return adjacent;
    }

    private Adjacent GetPartialAdjacentSameType(Vector2Int coord, List<Vector2Int> partial, CenteredMap<GroundType> map)
    {
        var adjacent = new Adjacent(false, false, false, false);
        var type = map[coord.x, coord.y];
        if (partial.Contains(new Vector2Int(coord.x, coord.y + 1)) && map[coord.x, coord.y + 1] == type)
            adjacent.Up = true;
        if (partial.Contains(new Vector2Int(coord.x + 1, coord.y)) && map[coord.x + 1, coord.y] == type)
            adjacent.Right = true;
        if (partial.Contains(new Vector2Int(coord.x, coord.y - 1)) && map[coord.x, coord.y - 1] == type)
            adjacent.Down = true;
        if (partial.Contains(new Vector2Int(coord.x - 1, coord.y)) && map[coord.x - 1, coord.y] == type)
            adjacent.Left = true;
        return adjacent;
    }
    
    public Vector4 GetRegion()
    {
        return waterRenderer.sharedMaterial.GetVector("_region");
    }
}