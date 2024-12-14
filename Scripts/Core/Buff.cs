using System;
using Sirenix.OdinInspector;

namespace Core
{
    [Serializable]
    public class Buff<T>
    {
        private T impact;
        public T Impact => impact;
        public UnitEntity Source;
        
        public bool timeLimited;
        [ShowIf("timeLimited")] public float duration;
        
        public Buff(UnitEntity source, T impact)
        {
            this.Source = source;
            this.impact = impact;
        }
        
        public Buff(UnitEntity source, T impact, float duration)
        {
            this.Source = source;
            this.impact = impact;
            this.timeLimited = true;
            this.duration = duration;
        }
    }
}
