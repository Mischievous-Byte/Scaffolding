using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Scaffolding
{
    internal sealed class CodeAssetDump : ScriptableObject
    {
        [System.Serializable]
        internal struct RuntimeContainer
        {
            public string key;
            public string json;
        }

        [SerializeField]
        public List<RuntimeContainer> containers = new();
    }
}
