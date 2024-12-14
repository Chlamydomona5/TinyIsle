using System.Collections.Generic;
using Core;
using Reward;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Fish", menuName = "SO/Fish")]
public class Fish_SO : IDBase_SO
{
    public override string prefix => "Fish";

    public Dictionary<BoxReward, float> Rewards;
    public GameObject modelPrefab;
    public List<Vector2Int> coveredCoords;
    
    [Title("Fish Diffculty")]
    public float fishBiteBaseInterval = 2f;
    public float fishBiteNoise = 1f;

    public Vector2Int fishFakeBaseBiteTimes = new(1, 2);
    
    public float GetInterval()
    {
        var result = fishBiteBaseInterval;
        result += Random.Range(-fishBiteNoise, fishBiteNoise);
        result = Mathf.Max(0.1f, result);
        return result;
    }
}