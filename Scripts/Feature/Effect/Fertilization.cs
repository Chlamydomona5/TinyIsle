using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Serialization;

public class Fertilization : RangeEffect
{
    public Fertilization()
    {
        paramsDictFloat.Add(EffectStatFloat.IntervalAmount, 0f);
        paramsDictFloat.Add(EffectStatFloat.IntervalRatio, 0f);
        paramsDictFloat.Add(EffectStatFloat.CritChanceRatio, 0f);
        paramsDictFloat.Add(EffectStatFloat.SellPriceAmount, 0f);
        paramsDictFloat.Add(EffectStatFloat.SellPriceRatio, 0f);
        paramsDictFloat.Add(EffectStatFloat.Possibliity, 1f);
        
        paramsDictInt.Add(EffectStatInt.XpAmount, 0);
        paramsDictInt.Add(EffectStatInt.ProduceCount, 0);
    }
    
    public void EffectOthers(ProduceUnitEntity produceUnit)
    {
        produceUnit.buffCarrier.AddBuff(AssembleBuff(produceUnit));
        
        produceUnit.produceUnitSo.sellPriceConstant += GetFloat(EffectStatFloat.SellPriceAmount);
        produceUnit.produceUnitSo.sellPriceConstant *= GetFloat(EffectStatFloat.SellPriceRatio);
        
        produceUnit.AddXP(GetInt(EffectStatInt.XpAmount));
    }
    
    private Buff<ProduceImpact> AssembleBuff(ProduceUnitEntity unitEntity)
    {
        var impact = new ProduceImpact();

        impact.produceCount = GetInt(EffectStatInt.ProduceCount);
        impact.intervalAmount = GetFloat(EffectStatFloat.IntervalAmount);
        impact.intervalMultiplier = GetFloat(EffectStatFloat.IntervalRatio);
        impact.critChanceRatio = GetFloat(EffectStatFloat.CritChanceRatio);
        
        return new Buff<ProduceImpact>(unitEntity, impact);
    }

    public override void Effect(UnitFeature_SO feature, UnitEntity self)
    {
        if(Random.value > GetFloat(EffectStatFloat.Possibliity)) return;
        
        foreach (var unit in GetTargetUnits<ProduceUnitEntity>(self as ProduceUnitEntity))
        {
            EffectOthers(unit);
        }
    }

    public override void BeforeMove(UnitFeature_SO feature, UnitEntity self)
    {
    }

    public override void AfterMove(UnitFeature_SO feature, UnitEntity self)
    {
    }

    public override void BeforeDestroy(UnitFeature_SO feature, UnitEntity self)
    {
    }
}