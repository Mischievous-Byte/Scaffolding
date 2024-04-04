using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Scaffolding
{
    public static partial class PersistentData
    {
        [Serializable]
        private struct Container
        {
            public string type;

            [SerializeReference]
            public object data;
        }

        private const string validNameRegex = "^[a-zA-Z0-9_.-]*$";


#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif

        private static void OnLoad() { }

        static PersistentData() => Initialize();


        private static partial void Initialize();
        public static partial T Load<T>(string key) where T : class, new();
    }
}
