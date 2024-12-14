using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core
{
    public class IDBase_SO : SerializedScriptableObject
    {
        [SerializeField] protected string id;
        public virtual string Description => Methods.GetLocalText(prefix + "_" + ID + "_Desc");

        public virtual string prefix => "";

        public virtual Sprite Icon => Resources.Load<Sprite>("NormalIcon/" + ID);

        public virtual string ID
        {
            get => id;
            set => id = value;
        }

        public string LocalizeName => Methods.GetLocalText(prefix + "_" + ID);

        private void OnValidate()
        {
            ID = name;
        }
    }
}