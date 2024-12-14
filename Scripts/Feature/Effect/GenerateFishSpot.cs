using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class GenerateFishSpot : FeatureEffect
{
    [OdinSerialize] private bool _useSelfSpots;
    [OdinSerialize,ShowIf("_useSelfSpots")] private FishSpotSet _spotSet;
    
    public override void Effect(UnitFeature_SO feature, UnitEntity self)
    {
        GridManager.Instance.RandomEmptyCoordInRange(2);
        if (_useSelfSpots)
            GridManager.Instance.fishController.GenerateSpot(_spotSet.GetSpot());
        else GridManager.Instance.fishController.GenerateSpotRandom();
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