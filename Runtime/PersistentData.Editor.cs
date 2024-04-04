using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MischievousByte.Scaffolding
{
    public static partial class PersistentData
    {
#if UNITY_EDITOR

        private const string relativeStorageLocation = "ProjectSettings/scaffolding/persistent_data/";
        private static string storageLocation;

        private static Dictionary<string, object> cache = new();
        private static partial void Initialize()
        {
            storageLocation = Path.GetFullPath(Path.Combine(Application.dataPath, "../", relativeStorageLocation));

            Directory.CreateDirectory(storageLocation);

            EditorApplication.quitting += OnQuit;
            AssemblyReloadEvents.beforeAssemblyReload += OnReload;
            EditorApplication.playModeStateChanged += OnPlayModeChange;

            foreach(string path in Directory.GetFiles(storageLocation).Where(s => s.EndsWith(".json")))
            {
                string key = Path.GetFileNameWithoutExtension(path);
                byte[] bytes = File.ReadAllBytes(path);
                string json = Encoding.UTF8.GetString(bytes);
                
                Container container = JsonUtility.FromJson<Container>(json);

                cache.Add(key, container.data);
            }
        }

        public static partial T Load<T>(string key) where T : class, new()
        {
            if (!Regex.IsMatch(key, validNameRegex))
                throw new ArgumentException(nameof(key));

            if (cache.ContainsKey(key) && cache[key] is T)
                return (T)cache[key];

            T data = new T();
            cache.Add(key, data);

            return data;
        }

        private static void OnReload()
        {
            EditorApplication.quitting -= OnQuit;
            AssemblyReloadEvents.beforeAssemblyReload -= OnReload;
            EditorApplication.playModeStateChanged -= OnPlayModeChange;

            Dump();
        }

        private static void OnQuit()
        {
            EditorApplication.quitting -= OnQuit;
            AssemblyReloadEvents.beforeAssemblyReload -= OnReload;
            EditorApplication.playModeStateChanged -= OnPlayModeChange;

            Dump();
        }

        private static void OnPlayModeChange(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode || change == PlayModeStateChange.ExitingPlayMode)
                Dump();
        }

        private static void Dump()
        {
            foreach (var pair in cache)
                Dump(pair.Key);
        }

        private static void Dump(string key)
        {
            string path = Path.Combine(storageLocation, $"{key}.json");
            var stream = File.Create(path);

            Container container = new()
            {
                type = cache[key].GetType().AssemblyQualifiedName,
                data = cache[key]
            };

            string json = JsonUtility.ToJson(container, true);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }
#endif
    }
}
