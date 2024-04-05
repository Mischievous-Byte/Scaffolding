using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MischievousByte.Scaffolding
{
    public static partial class PersistentData
    {
        internal interface IWrapper
        {
            public object Raw { get; }
        }
        internal class Wrapper<T> : IWrapper
        {

            public T data;
            public object Raw => data;

            public Wrapper()
            {

            }

            public Wrapper(object data)
            {
                this.data = (T) data;
            }
        }

        internal static Dictionary<string, IWrapper> cache = new();

#if UNITY_EDITOR

        internal static event Action onEditorCacheChange;

        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        private static void OnLoad() { }

        static PersistentData()
        {
            Initialize();
#if UNITY_EDITOR
            onEditorCacheChange?.Invoke();
#endif
        }


        private static partial void Initialize();

        public static ref T Load<T>(string key) where T : struct
        {
            if (!Regex.IsMatch(key, "^[a-zA-Z0-9_.-]*$"))
                throw new ArgumentException(nameof(key));
            
            Wrapper<T> wrapper;
            if (!cache.ContainsKey(key))
            {
                wrapper = new() { data = default(T) };
                cache[key] = wrapper;
#if UNITY_EDITOR
                onEditorCacheChange?.Invoke();
#endif
            }
            else if (cache[key] is Wrapper<T>)
                wrapper = cache[key] as Wrapper<T>;
            else
                throw new ArgumentException(typeof(T).Name);

            return ref wrapper.data;
        }
    }
}
