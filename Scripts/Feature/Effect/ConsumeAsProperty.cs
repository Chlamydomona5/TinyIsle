using System;
using UnityEngine;

namespace Unit.Consume
{
    [Serializable]
    public class ConsumeAsProperty : ConsumeEffect
    {
        public override void EffectConsume(UnitFeature_SO feature ,ConsumeUnitEntity self, float totalAmount, CrystalType crystalType)
        {
            //Debug.Log("ConsumeAsProperty " + totalAmount + " " + crystalType);
            if(crystalType == CrystalType.Gold)
                PropertyManager.Instance.AddProperty(totalAmount, "consume", true, PropertyManager.PropertyType.Gold);
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