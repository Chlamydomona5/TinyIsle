using System.Collections.Generic;
using Reward;
using UnityEngine;

namespace Unit.Consume
{
    public class ConsumeTwister : ConsumeEffect
    {
        public GameObject chestPrefab;
        public Dictionary<BoxReward, float> RewardPool;

        public ConsumeTwister()
        {
            RewardPool = new Dictionary<BoxReward, float>();
        }

        public override void EffectConsume(UnitFeature_SO feature, ConsumeUnitEntity self, float totalAmount, CrystalType crystalType)
        {
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