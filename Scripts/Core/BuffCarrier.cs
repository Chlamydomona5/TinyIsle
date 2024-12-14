using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class BuffCarrier<T>
    {
        [SerializeField, ReadOnly] private List<Buff<T>> buffList;
        public List<Buff<T>> BuffList => buffList;
        
        public BuffCarrier()
        {
            buffList = new();
        }

        public void AddBuff(Buff<T> buff)
        {
            buffList.Add(buff);
            if(buff.timeLimited) GridManager.Instance.StartCoroutine(BuffTimer(buff));
        }

        public void RemoveBuffFrom(UnitEntity from)
        {
            buffList.RemoveAll(buff => buff.Source == from);
        }

        private IEnumerator BuffTimer(Buff<T> buff)
        {
            yield return new WaitForSeconds(buff.duration);
            buffList.Remove(buff);
        }
    }
}