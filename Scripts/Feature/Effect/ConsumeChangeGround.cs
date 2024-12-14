using System;
using UnityEngine;

namespace Unit.Consume
{
    [Serializable]
    public class ConsumeChangeGround : ConsumeEffect
    {
        [SerializeField] private GroundType groundType;
        public override void EffectConsume(UnitFeature_SO feature ,ConsumeUnitEntity self, float totalAmount, CrystalType crystalType)
        {
            //Debug.Log("ConsumeChangeGround " + totalAmount + " " + crystalType);
            GridManager.Instance.ChangeGroundType2X2(self.coordinate, groundType);
        }

        public override void Effect(UnitFeature_SO feature, UnitEntity self)
        {
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
}