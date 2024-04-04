using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MischievousByte.Scaffolding
{
    public static partial class PersistentData
    {

        private static Dictionary<string, object> cache = new();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        private static void OnLoad() { }

        static PersistentData() => Initialize();


        private static partial void Initialize();

        public static T Load<T>(string key) where T : class, new()
        {
            if (!Regex.IsMatch(key, "^[a-zA-Z0-9_.-]*$"))
                throw new ArgumentException(nameof(key));

            if (cache.ContainsKey(key) && cache[key] is T)
                return (T)cache[key];

            T data = new T();
            cache.Add(key, data);

            return data;
        }
    }
}
