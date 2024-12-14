using System;
using System.Collections.Generic;
using Reward;
using Sirenix.OdinInspector;
using UnityEngine;

public class Rock : EntityBase
{
    [SerializeField]
    public float mineTime = 1f;
    
    [SerializeField]
    public Dictionary<BoxReward, float> Rewards = new();
    
    public List<Vector2Int> GetCoveredCoords()
    {
        var covered = new List<Vector2Int>();
        foreach (var coveredCoord in coveredCoordsRaw)
        {
            covered.Add(coordinate + coveredCoord);
        }
        return covered;
    }
}