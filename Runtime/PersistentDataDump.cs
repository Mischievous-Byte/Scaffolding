using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Scaffolding
{
    internal sealed class PersistentDataDump : ScriptableObject
    {
        [System.Serializable]
        internal struct Container
        {
            public string key;

            [SerializeReference]
            public object data;
        }

        [SerializeField]
        public List<Container> containers = new();
    }
}
