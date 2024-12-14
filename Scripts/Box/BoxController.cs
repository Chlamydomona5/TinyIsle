using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Reward;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class BoxController : SerializedMonoBehaviour, ISaveable
{
    [OdinSerialize] private Dictionary<string, Box> _boxPrefabs = new();
    [OdinSerialize] private FishEntity fishPrefab;
    
    [SerializeField, ReadOnly] private List<Box> allBoxes;
    [SerializeField, ReadOnly] private List<FishEntity> allFish;

    public Box GenerateBox(string boxName, Vector2Int coord, Dictionary<BoxReward, float> rewards = default)
    {
        Debug.Log("Generate box: " + boxName);
        var box = Instantiate(_boxPrefabs[boxName], transform);
        var coords = new List<Vector2Int>() { Vector2Int.zero };
        if (rewards == default)
        {
            box.Init(coord, coords);
        }
        else
        {
            box.Init(coord, coords, null, rewards);
        }
        allBoxes.Add(box);
        Debug.Log("All boxes: " + allBoxes.Count);
        GridManager.Instance.crystalController.SqueezeCrystalAt(coords);
        
        return box;
    }

    public FishEntity GenerateFish(Fish_SO fishSO, Vector3 pos, Vector2Int des, bool countAsAchievement = true)
    {
        var fish = Instantiate(fishPrefab, transform);
        fish.Init(des, fishSO);
        allBoxes.Add(fish);
        allFish.Add(fish);

        fish.transform.position = pos;
        fish.transform.FlipToCoordAnim(pos, des, 0.8f, 0, false).onComplete += () =>
        {
            fish.transform.position =
                GridManager.Instance.Coord2Pos(des) + (GridManager.Instance.GetGroundType(des) == GroundType.Low
                    ? FishEntity.YOffsetInWater * Vector3.up : Vector3.zero);
        };
        fish.InWater = GridManager.Instance.GetGroundType(des) == GroundType.Low;
        
        GridManager.Instance.crystalController.SqueezeCrystalAt(fish.RealCoveredCoords);
        
        if (countAsAchievement)
            AchievementManager.Instance.AccumulateProgress(AchievementType.FishCount);
        
        return fish;
    }
    
    public bool HasBox(Vector2Int coord)
    {
        return allBoxes.Exists(box => box.RealCoveredCoords.Contains(coord));
    }
    
    public void RemoveBox(Box box)
    {
        allBoxes.Remove(box);
    }

    public void TapBoxAt(Vector2Int coord)
    {
        if (!HasBox(coord)) return;
        var box = allBoxes.Find(b => b.RealCoveredCoords.Contains(coord));
        box.OnClick();
    }

    public void SqueezeBoxAt(List<Vector2Int> realCoveredCoords)
    {
        foreach (var box in allBoxes)
        {
            if (box.RealCoveredCoords.Intersect(realCoveredCoords).Any())
            {
                box.JumpIntoWater();
            }
        }
    }

    public void Save()
    {
        ES3.Save("FishData", allFish.Select(fish => (fish.coordinate, fish.data)).ToList());
    }

    public void Load()
    {
        var fishData = ES3.Load<List<(Vector2Int, Fish_SO)>>("FishData");
        foreach (var (coord, data) in fishData)
        {
            GenerateFish(data, GridManager.Instance.Coord2Pos(coord), coord, false);
        }
    }
}