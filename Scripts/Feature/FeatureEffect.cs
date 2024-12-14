using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public abstract class FeatureEffect
{
    public enum EffectStatFloat
    {
        CanceledParam0,
        CanceledParam1,
        Interval,
        IntervalAmount,
        IntervalRatio,
        SellPriceAmount,
        SellPriceRatio,
        CritChanceRatio,
        CanceledParam2,
        Possibliity,
        Duration,
        ConvertRatio,
        Delay,
    }
    
    public enum EffectStatInt
    {
        ProduceCount,
        XpAmount,
        MaxEffectAmount,
        ConvertCount,
        BlockCount,
        MaxAscendLevel,
        AscendLevelStep,
        AddAscend,
        AddSize
    }
    
    [Title("参数字典")]
    [Title("小数类型",bold: false)]
    public Dictionary<EffectStatFloat, float> paramsDictFloat = new();
    [Title("升级增量(加算)")]
    public Dictionary<EffectStatFloat, float> upgradeParamsDictFloat = new();
    [Title("整数类型",bold: false)]
    public Dictionary<EffectStatInt, int> paramsDictInt = new();
    [Title("升级增量(加算)")]
    public Dictionary<EffectStatInt, int> upgradeParamsDictInt = new();
    [Title("特殊选项")]
    public bool destroyBeforeActivate;
    
    public float GetFloat(EffectStatFloat name)
    {
        if (paramsDictFloat.TryGetValue(name, out var value))
        {
            return value;
        }
        else
        {
            return default;
        }
    }
    
    public int GetInt(EffectStatInt name)
    {
        if (paramsDictInt.TryGetValue(name, out int value))
        {
            return value;
        }
        else
        {
            Debug.LogWarning($"Param {name} not found in {this}");
            return default;
        }
    }

    public void SelfEffect(UnitFeature_SO feature, UnitEntity self)
    {
        if(!SpecialCondition(feature, self)) return;
        
        var feedback = self.model.OnFeatureEffects.FirstOrDefault(x => x.featureID == feature.ID);

        if (feedback != default)
        {
            if(!feedback.feedback.IsPlaying)
                feedback.feedback.PlayFeedbacks();
        }
        else
        {
            if (destroyBeforeActivate) self.BreakDestroy();
            Effect(feature, self);
        }
    }
    
    public void Upgrade()
    {
        foreach (var param in upgradeParamsDictFloat)
        {
            paramsDictFloat[param.Key] += param.Value;
        }
        
        foreach (var param in upgradeParamsDictInt)
        {
            paramsDictInt[param.Key] += param.Value;
        }
    }

    public virtual bool SpecialCondition(UnitFeature_SO feature, UnitEntity self)
    {
        return true;
    }
    
    public abstract void Effect(UnitFeature_SO feature, UnitEntity self);
    public abstract void BeforeMove(UnitFeature_SO feature, UnitEntity self);
    public abstract void AfterMove(UnitFeature_SO feature, UnitEntity self);
    public abstract void BeforeDestroy(UnitFeature_SO feature, UnitEntity self);
}
