using System.Collections.Generic;
using Core;
using Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockItem", menuName = "SO/UnlockItem")]
public class UnlockItem : IDBase_SO
{
    public override string prefix => "UnlockItem";
    public override Sprite Icon => Effect.Icon;
    
    public Vector2Int coord;
    
    public UnlockType unlockType;
    
    [ShowIf("unlockType", UnlockType.CostExpandCrystal), SerializeField]
    private int costExpandCrystal = 1;
    [ShowIf("unlockType", UnlockType.MiniumAchievementPoint), SerializeField]
    public int minPoint = 1;
    [ShowIf("unlockType", UnlockType.NeedCertainAchievement), SerializeField]
    private Achievement_SO needAchievement;
    
    public int CostExpandCrystal => unlockType == UnlockType.CostExpandCrystal ? costExpandCrystal : 0;
    
    [Title("必须前置项")]
    public List<UnlockItem> preItems;

    public UnlockEffect Effect;

    public bool CanUnlock(int currentExpandCrystalCount)
    {
        return unlockType switch
        {
            UnlockType.CostExpandCrystal => currentExpandCrystalCount >= costExpandCrystal,
            UnlockType.MiniumAchievementPoint => AchievementManager.Instance.TotalPoint >= minPoint,
            UnlockType.NeedCertainAchievement => AchievementManager.Instance.IsAchievementFinished(needAchievement),
            _ => false
        };
    }
    
    public void UnlockEffect()
    {
        Effect?.Effect();
    }
    
    public void UpgradeEffect()
    {
        Effect?.Upgrade();
    }
}

public enum UnlockType
{
    CostExpandCrystal,
    MiniumAchievementPoint,
    NeedCertainAchievement,
}