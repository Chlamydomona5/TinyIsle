

    using UnityEngine;

    public class ChangeGround : FeatureEffect
    {
        public GroundType GroundType;
        
        public override void Effect(UnitFeature_SO feature, UnitEntity self)
        {
            GridManager.Instance.ChangeGroundType(self.coordinate, GroundType);
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