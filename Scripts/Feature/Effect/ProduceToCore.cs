
using UnityEngine;

public class ProduceToCore : FeatureEffect
{
    public ProduceToCore()
    {
        paramsDictFloat.Add(EffectStatFloat.Possibliity, 0f);
    }
    
    public override void Effect(UnitFeature_SO feature, UnitEntity self)
    {
        (self as ProduceUnitEntity).produceUnitSo.toCorePossiblity = GetFloat(EffectStatFloat.Possibliity);
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
