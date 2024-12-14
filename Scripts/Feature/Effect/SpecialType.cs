
using UnityEngine;

public class SpecialType : FeatureEffect
{
    [SerializeField] private string _typeName;
    
    public override void Effect(UnitFeature_SO feature, UnitEntity self)
    {
        self.gameObject.AddComponent(System.Type.GetType(_typeName));
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
