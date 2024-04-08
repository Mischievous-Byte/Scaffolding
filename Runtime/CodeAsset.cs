using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MischievousByte.Scaffolding
{
    public static partial class CodeAsset
    {
        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
        public class RequireAttribute : Attribute
        {
            public readonly string Key;
            public readonly Type Type;
            public RequireAttribute(string key, Type type)
            {
                Key = key;
                Type = type;
            }
        }

        internal interface IWrapper
        {
            public object Raw { get; set; }
        }

        [Serializable]
        internal class Wrapper<T> : IWrapper
        {

            public T value;
            
            public object Raw
            {
                get => value;
                set => this.value = (T)value;
            }

            public Wrapper()
            {

            }

            public Wrapper(object value)
            {
                this.value = (T)value;
            }
        }


        internal static Dictionary<string, IWrapper> cache = new();

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        private static void OnLoad() { }

        static CodeAsset()
        {
            Initialize();
        }


        private static partial void Initialize();

        public static ref T Get<T>(string key) where T : struct
        {
            if (!IsValidKey(key))
                throw new ArgumentException(nameof(key));

            if (!cache.ContainsKey(key))
                throw new KeyNotFoundException(key);

            if (cache[key] is Wrapper<T> wrapper)
                return ref wrapper.value;

            throw new ArgumentException(typeof(T).Name);
        }


        private static IEnumerable<RequireAttribute> GetValidAttributes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var all = assembly.GetCustomAttributes<RequireAttribute>();

                var groups = all.GroupBy(attr => attr.Key);

                foreach (var group in groups)
                {
                    RequireAttribute r = null;
                    try
                    {
                        if (!IsValidKey(group.Key))
                            throw new ArgumentException($"Invalid key");

                        if (group.Count() > 1)
                            throw new ArgumentException("Not unique");

                        var attr = group.First();

                        if (!attr.Type.IsValueType)
                            throw new ArgumentException($"Invalid type {attr.Type.Name}");

                        r = attr;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error while processing requirement for {group.Key}");
                        Debug.LogException(ex);
                    }

                    if (r != null)
                        yield return r;
                }
            }
        }

        private static bool IsValidKey(string key) => key != string.Empty && Regex.IsMatch(key, "^[a-zA-Z0-9_.-]*$");
    }
}
