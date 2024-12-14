using System;
using System.Linq;
using UnityEngine;

namespace Unit.Consume
{
    [Serializable]
    public abstract class ConsumeEffect : FeatureEffect
    {
        public abstract void EffectConsume(UnitFeature_SO feature ,ConsumeUnitEntity self, float totalAmount, CrystalType crystalType);
        
        public void SelfEffect(UnitFeature_SO feature, ConsumeUnitEntity self, float totalAmount, CrystalType crystalType)
        {
            var feedback = self.model.OnFeatureEffects.FirstOrDefault(x => x.featureID == feature.ID);

            if (feedback != default)
            {
                if (!feedback.feedback.IsPlaying)
                    feedback.feedback.PlayFeedbacks();
            }
            else
            {
                if (destroyBeforeActivate) self.BreakDestroy();
                EffectConsume(feature, self, totalAmount, crystalType);
            }
        }
    }
}