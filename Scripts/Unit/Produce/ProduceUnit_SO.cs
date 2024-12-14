using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ProduceUnit", menuName = "SO/ProduceUnit")]
public class ProduceUnit_SO : Unit_SO
{
    public override string prefix => "UnitProduce";

    [Title("Import Parameters")]
    public CrystalType crystalType;
    public ProduceType produceType;

    public bool produce;
    public int produceCount = 1;
    public int crystalSize = 1;
    public int ascendLevel = 1;
    public int distance = 0;
    public float produceInterval;
    public float produceIntervalGrowthConstant;
    public float critPossibility;
    public float critMultiplier;

    public bool willTired;
    public float tiredProduceTimes;
    public float tiredDuration;
    
    [Title("Default Parameters")]
    public float toCorePossiblity;

    public override List<(string iconId, string text, int lineNumber)> GetIconValuePair()
    {
        var list = base.GetIconValuePair();
        if(produce)
            list.Add(("produce_" + crystalType.ToString(), produceInterval.ToString("F1") + "s", 1));
        list.Add(("crystalSize", crystalSize.ToString(), 1));
        if(distance != 0)
            list.Add(("distance", distance.ToString(), 1));
        if(produceCount != 1)
            list.Add(("produceCount", produceCount.ToString(), 1));
        return list;
    }

    public override List<(string iconId, string text)> GetIconDescPair()
    {
        var list = new List<(string iconId, string text)>(base.GetIconDescPair());
        if(produce)
            list.Add(("produce_" + crystalType.ToString(), "每" + produceInterval.ToString("F1") + "秒产出一次"));

        list.Add(("crystalSize", "水晶等级：" + crystalSize));
        if(distance != 0)
            list.Add(("distance", "弹射距离：" + distance));
        if(produceCount != 1)
            list.Add(("produceCount", "每次产出：" + produceCount + "个"));
        return list;
    }
}