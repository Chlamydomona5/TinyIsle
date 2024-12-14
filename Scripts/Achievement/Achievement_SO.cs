using Core;
using UnityEngine;

[CreateAssetMenu(fileName = "Achievement_SO", menuName = "SO/Achievement_SO")]
public class Achievement_SO : IDBase_SO
{
    public override string prefix => "Achi";

    public AchievementType type;
    public int amount;
    
    public int worthPoint;
}

public enum AchievementType
{
    /// 水晶最高升华等级
    AscendLevel,
    // 水晶升华次数
    AscendCount,
    // 总水晶数
    GoldCrystalCount,
    // 平均生产量
    AvrProduce,
    // 总种植数
    PlantCount,
    // 总进化数
    EvolveCount,

    // 总魔方数
    ExpandCrystalCount,
    // 总钥匙数
    UnlockCrystalCount,
    // 解锁项数
    Unlock,
    // 精灵数量
    SpiritCount,
    // 最高岛屿面积
    IslandArea,
    // 最高高地面积
    HighLandArea,
    // 最高水池面积
    PoolArea,
    // 总建造家具数量
    FurnitureCount,
    
    // 总鱼数
    FishCount,
    // 鱼种类数
    FishTypeCount,
    // 放生鱼数
    FishLetGoCount,
}