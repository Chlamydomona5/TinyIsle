using System.Collections.Generic;

namespace Core
{
    public class RarityProbability
    {
        private Dictionary<Rarity, float> _originProbability;
        public BuffCarrier<LuckImpact> BuffCarrier;

        public Dictionary<Rarity, float> WeightDict
        {
            get
            {
                var result = new Dictionary<Rarity, float>(_originProbability);

                foreach (var buff in BuffCarrier.BuffList)
                {
                    result = buff.Impact.Effect(result);
                }

                // Normalize
                var sum = 0f;
                foreach (var weight in result.Values)
                {
                    sum += weight;
                }

                result[Rarity.Common] /= sum;
                result[Rarity.Rare] /= sum;
                result[Rarity.Epic] /= sum;
                result[Rarity.Legend] /= sum;


                return result;
            }
        }

        public RarityProbability()
        {
            _originProbability = Methods.Rarity2Weight;
            BuffCarrier = new();
        }

        public class LuckImpact
        {
            public Dictionary<Rarity, float> Rarity2Weight;
            public Methods.OperatorType OperatorType;

            public LuckImpact(Dictionary<Rarity, float> weightMask, Methods.OperatorType operatorType)
            {
                Rarity2Weight = weightMask;
                OperatorType = operatorType;
            }

            // Dict<Rarity, float> + LuckImpact
            public Dictionary<Rarity, float> Effect(Dictionary<Rarity, float> dict)
            {
                var result = new Dictionary<Rarity, float>(dict);
                foreach (var (rarity, weight) in Rarity2Weight)
                {
                    switch (OperatorType)
                    {
                        case Methods.OperatorType.Add:
                            result[rarity] += weight;
                            break;
                        case Methods.OperatorType.Multiply:
                            result[rarity] *= weight;
                            break;
                    }
                }
                return result;
            }
        }
    }
}
