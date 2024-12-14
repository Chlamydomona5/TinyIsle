using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unit.Consume;
using UnityEngine;

[CreateAssetMenu(fileName = "New UnitFeature", menuName = "SO/UnitFeature", order = 1)]
public class UnitFeature_SO : IDBase_SO
{
    public override string prefix => "Feature";
    [Title("解锁进化等级")] 
    public int unlockEvolveLevel = 1;
    [Title("触发条件")]
    public EffectTrigger effectTrigger;
    [Title("具体效果")]
    public FeatureEffect effect;
    
    public void OnActivate(UnitEntity unitEntity)
    {
        //Register to feedback if it got one
        if(unitEntity.model.OnFeatureEffects.Exists(x => x.featureID == ID)) unitEntity.model.OnFeatureEffects.
            First(x => x.featureID == ID).feedback.GetFeedbackOfType<MMF_Events>().PlayEvents.AddListener(() =>
        {
            if (effect.destroyBeforeActivate)
            {
                unitEntity.BreakDestroy();
            }
            effect.Effect(this, unitEntity);
        });
        
        if (effectTrigger.HasFlag(EffectTrigger.Activate))
            effect?.SelfEffect(this, unitEntity);
    }
    
    public void OnEvolve(UnitEntity unitEntity, int level)
    {
        effect.Upgrade();
        if (effectTrigger.HasFlag(EffectTrigger.Upgrade))
            effect?.SelfEffect(this, unitEntity);
    }
    
    public void OnFixedInterval(UnitEntity unitEntity)
    {
        //Debug.Log("OnFixedInterval " + unitEntity.name);
        if (effectTrigger.HasFlag(EffectTrigger.Interval))
            effect?.SelfEffect(this, unitEntity);
    }

    public void OnTap(UnitEntity unitEntity)
    {
        //Debug.Log("OnTap " + unitEntity.name);
        if (effectTrigger.HasFlag(EffectTrigger.Tap))
            effect?.SelfEffect(this, unitEntity);
    }

    public void OnBreakDestroy(UnitEntity unitEntity)
    {
        if (effectTrigger.HasFlag(EffectTrigger.Destroy))
            effect?.SelfEffect(this, unitEntity);
    }

    public void OnUnitSoldDestroy(UnitEntity unitEntity)
    {
        if (effectTrigger.HasFlag(EffectTrigger.Sold))
            effect?.SelfEffect(this, unitEntity);
    }

    public void OnCrystalIn(UnitEntity unitEntity)
    {
        if (effectTrigger.HasFlag(EffectTrigger.CrytalIn))
            effect?.SelfEffect(this, unitEntity);
    }
    
    public void OnProduce(ProduceUnitEntity unitEntity, float gold)
    {
        if (effectTrigger.HasFlag(EffectTrigger.Produce))
            effect?.SelfEffect(this, unitEntity);
    }
    
    public void OnMove(UnitEntity unitEntity)
    {
        if (effectTrigger.HasFlag(EffectTrigger.Move))
            effect?.SelfEffect(this, unitEntity);
    }

    public void OnConsume(ConsumeUnitEntity unitEntity, float totalAmount, CrystalType crystalType)
    {
        if (effectTrigger.HasFlag(EffectTrigger.Consume))
        {
            if (effect is ConsumeEffect consumeEffect)
            {
                consumeEffect.SelfEffect(this, unitEntity, totalAmount, crystalType);
            }
            else
            {
                effect?.SelfEffect(this, unitEntity);
            }
        }
    }

    public void OnHarvest(ProduceUnitEntity produceUnitEntity)
    {
        if (effectTrigger.HasFlag(EffectTrigger.Harvest))
            effect?.SelfEffect(this, produceUnitEntity);
    }
}

[Flags]
public enum EffectTrigger
{
    Produce = 1,
    Interval = 2,
    Upgrade = 4,
    Activate = 8,
    Tap = 16,
    Destroy = 32,
    Sold = 64,
    CrytalIn = 128,
    Consume = 256,
    Move = 512,
    Harvest = 1024
}