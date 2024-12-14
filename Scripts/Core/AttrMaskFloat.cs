using System;
using UnityEngine;

//Type can only be int, float
namespace Core
{
    [Serializable]
    public class AttrMaskFloat
    {
        [SerializeField] private float value;
        private BuffCarrier<float> _buffCarrier = new();
    
        public float Value
        {
            get
            {
                if(_buffCarrier == null) _buffCarrier = new();
                var result = value;
                foreach (var buff in _buffCarrier.BuffList)
                {
                    result += buff.Impact;
                }

                return result;
            }
        }
    
        public AttrMaskFloat(float value)
        {
            this.value = value;
        }
    
        public void AddBuff(Buff<float> buff)
        {
            _buffCarrier.AddBuff(buff);
        }
    
        public void RemoveBuffFrom(ProduceUnitEntity from)
        {
            _buffCarrier.RemoveBuffFrom(from);
        }
    }
}
