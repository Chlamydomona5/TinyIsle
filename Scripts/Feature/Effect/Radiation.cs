using System.Collections.Generic;
using Core;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Radiation : RangeEffect
{
    [SerializeField,ReadOnly] List<ProduceUnitEntity> _buffedFacilities = new();

    private UnityAction<UnitEntity, Vector2Int, Vector2Int> _movehandler;
    private UnityAction<UnitEntity, Vector2Int> _createHandler;

    public Radiation()
    {
        paramsDictFloat.Add(EffectStatFloat.IntervalAmount, 0f);
        paramsDictFloat.Add(EffectStatFloat.IntervalRatio, 0f);
        paramsDictFloat.Add(EffectStatFloat.CritChanceRatio, 0f);
        paramsDictFloat.Add(EffectStatFloat.Duration, -1f);
        
        paramsDictInt.Add(EffectStatInt.ProduceCount, 0);
        paramsDictInt.Add(EffectStatInt.AddAscend, 0);
        paramsDictInt.Add(EffectStatInt.AddSize, 0);
    }
    
    private void SetBuff(ProduceUnitEntity self, UnitFeature_SO feature)
    {
        var buff = AssembleBuff(self);
        
        if(_buffedFacilities == null) _buffedFacilities = new List<ProduceUnitEntity>();
        //Remove old
        foreach (var target in _buffedFacilities)
        {
            if(!target) continue;
            target.buffCarrier.RemoveBuffFrom(self);
        }
        _buffedFacilities.Clear();

        //Add new
        foreach (var target in GetTargetUnits<ProduceUnitEntity>(self))
        {
            if (!_buffedFacilities.Contains(target))
            {
                target.buffCarrier.AddBuff(buff);
                _buffedFacilities.Add(target);
            }
        }
    }

    private Buff<ProduceImpact> AssembleBuff(ProduceUnitEntity unitEntity)
    {
        var impact = new ProduceImpact();

        impact.produceCount = GetInt(EffectStatInt.ProduceCount);
        impact.intervalAmount = GetFloat(EffectStatFloat.IntervalAmount);
        impact.intervalMultiplier = GetFloat(EffectStatFloat.IntervalRatio);
        impact.critChanceRatio = GetFloat(EffectStatFloat.CritChanceRatio);
        impact.addAscend = GetInt(EffectStatInt.AddAscend);
        impact.addSize = GetInt(EffectStatInt.AddSize);
        
        float time = GetFloat(EffectStatFloat.Duration);

        Buff<ProduceImpact> buff;
        if (time > 0)
        {
            buff = new Buff<ProduceImpact>(unitEntity, impact, time);
        }
        else
        {
            buff = new Buff<ProduceImpact>(unitEntity, impact);
        }
        
        return buff;
    }
    

    public override void Effect(UnitFeature_SO feature, UnitEntity self)
    {
        _movehandler = (_, _, _) => SetBuff(self as ProduceUnitEntity, feature);
        _createHandler = (_, _) => SetBuff(self as ProduceUnitEntity,feature);
        GridManager.Instance.onUnitMove.AddListener(_movehandler);
        GridManager.Instance.onUnitCreate.AddListener(_createHandler);
        
        SetBuff(self as ProduceUnitEntity, feature);
    }

    public override void BeforeMove(UnitFeature_SO feature, UnitEntity self)
    {
    }

    public override void AfterMove(UnitFeature_SO feature, UnitEntity self)
    {
    }

    public override void BeforeDestroy(UnitFeature_SO feature, UnitEntity self)
    {
        GridManager.Instance.onUnitMove.RemoveListener(_movehandler);
        GridManager.Instance.onUnitCreate.RemoveListener(_createHandler);
        
        if(_buffedFacilities != null)
            foreach (var target in _buffedFacilities)
            {
                if(!target) continue;
                target.buffCarrier.RemoveBuffFrom(self);
            }
    }
}