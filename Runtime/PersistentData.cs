using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MischievousByte.Scaffolding
{
    public static class PersistentData
    {
        [Serializable]
        private struct Container
        {
            public string type;

            [SerializeReference]
            public object data;
        }


        private const string relativeStorageLocation = "ProjectSettings/scaffolding/persistent_data/";
        private const string validNameRegex = "^[a-zA-Z0-9_.-]*$";

        private static readonly string storageLocation;

        private static readonly Dictionary<string, object> cache = new();

        [RuntimeInitializeOnLoadMethod()]
        private static void OnLoad() { }

        static PersistentData()
        {
            storageLocation = Path.GetFullPath(Path.Combine(Application.dataPath, "../", relativeStorageLocation));

            Directory.CreateDirectory(storageLocation);
        }


        public static T Load<T>(string key) where T : class, new()
        {
            if (cache.ContainsKey(key) && cache[key] is T)
                return (T) cache[key];

            T data;
#if UNITY_EDITOR
            data = Load_Editor<T>(key);
#endif

            return data;
        }


        private static T Load_Editor<T>(string key) where T : class, new()
        {

            if (Regex.IsMatch(key, validNameRegex))
                throw new ArgumentException();

            string path = Path.Combine(storageLocation, $"{key}.json");

            if (!File.Exists(path))
            {
                var stream = File.Create(path);

                T data = new T();

                Container container = new()
                {
                    type = typeof(T).AssemblyQualifiedName,
                    data = data
                };

                string json = JsonUtility.ToJson(container, true);
                byte[] bytes = Encoding.Unicode.GetBytes(json);

                stream.Write(bytes, 0, bytes.Length);
                stream.Close();

                return data;
            }
            else
            {
                byte[] bytes = File.ReadAllBytes(path);
                string json = Encoding.Unicode.GetString(bytes);
                var container = JsonUtility.FromJson<Container>(json);
                Type type = Type.GetType(container.type);

                if (!typeof(T).IsAssignableFrom(type))
                    throw new TypeLoadException();

                return container.data as T;
            }
           
        }

        private static void Write<T>(string key, T data) where T: class, new()
        {
            string path = Path.Combine(storageLocation, $"{key}.json");

            Container container = new Container()
            {
                type = typeof(T).AssemblyQualifiedName,
                data = data
            };

            string json = JsonUtility.ToJson(container, true);
            byte[] bytes = Encoding.Unicode.GetBytes(json);
            File.WriteAllBytes(path, bytes);
        }

    }
}
