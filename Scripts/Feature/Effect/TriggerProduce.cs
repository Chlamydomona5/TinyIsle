using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class TriggerProduce : RangeEffect
{
    [Title("是否只触发自己种类")]
    public bool OnlyTriggerSelfType;
    
    public TriggerProduce()
    {
        paramsDictFloat.Add(EffectStatFloat.Possibliity, 1f);
    }
    
    public void Produce(ProduceUnitEntity produceUnit)
    {
        if (produceUnit.model.produceFeedback)
        {
            if (!produceUnit.model.produceFeedback.IsPlaying)
                produceUnit.model.produceFeedback.PlayFeedbacks();
        }
        else produceUnit.TryProduce();
    }

    public override void Effect(UnitFeature_SO feature, UnitEntity self)
    {
        if (Random.value < GetFloat(EffectStatFloat.Possibliity))
        {
            var targetUnits = GetTargetUnits<ProduceUnitEntity>(self);
            if(OnlyTriggerSelfType) targetUnits = targetUnits.FindAll(unit => unit.unitSo.ID == self.unitSo.ID);
            targetUnits = targetUnits.Where(x => x.produceUnitSo.produceType == ProduceType.Interval).ToList();
            targetUnits.ForEach(Produce);
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
