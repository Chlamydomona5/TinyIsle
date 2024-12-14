
public class EnableGlobal : FeatureEffect
{
    public enum GlobalType
    {
        Fishing,
        CrystalNavigation,
    }
    
    public GlobalType globalType;
    
    public override void Effect(UnitFeature_SO feature, UnitEntity self)
    {
        switch (globalType)
        {
            case GlobalType.Fishing:
                GridManager.Instance.fishController.EnableFish(true);
                break;
            case GlobalType.CrystalNavigation:
                GridManager.Instance.EnableGlobalCrystalNavigation(true);
                break;
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
