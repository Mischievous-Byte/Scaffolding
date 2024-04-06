using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MischievousByte.Scaffolding
{
    public static partial class PersistentData
    {
#if UNITY_EDITOR

        private class PreBuiltProcessor : UnityEditor.Build.IPreprocessBuildWithReport
        {
            public int callbackOrder => 1;

            public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
                DeleteDumpAssets();

                

                var dumpFile = ScriptableObject.CreateInstance<PersistentDataDump>();
                dumpFile.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;

                foreach (var pair in cache)
                    dumpFile.containers.Add(new() { key = pair.Key, data = pair.Value.Raw });

                UnityEditor.AssetDatabase.CreateAsset(dumpFile, "Assets/scaffolding_persistent_dump.asset");
                
                UnityEditor.AssetDatabase.Refresh();

                var assets = UnityEditor.PlayerSettings.GetPreloadedAssets().Where(obj => !(obj is PersistentDataDump)).ToList();
                assets.Add(dumpFile);
                UnityEditor.PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }

        private class PostBuiltProcessor : UnityEditor.Build.IPostprocessBuildWithReport
        {
            public int callbackOrder => 0;

            public void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
                DeleteDumpAssets();
            }
        }

        [Serializable]
        private struct SerializableContainer
        {
            public string type;

            [SerializeReference]
            public object data;
        }

        private const string relativeStorageLocation = "ProjectSettings/scaffolding/persistent_data/";
        private static string storageLocation;

        private static partial void Initialize()
        {
            storageLocation = Path.GetFullPath(Path.Combine(Application.dataPath, "../", relativeStorageLocation));
            Directory.CreateDirectory(storageLocation);

            UnityEditor.EditorApplication.quitting += PreventLoss;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += PreventLoss;

            Import();
            Ensure();

            DeleteDumpAssets();

        }

        private static void Import()
        {
            foreach (string path in Directory.GetFiles(storageLocation).Where(s => s.EndsWith(".json")))
            {
                string key = Path.GetFileNameWithoutExtension(path);
                byte[] bytes = File.ReadAllBytes(path);
                string json = Encoding.UTF8.GetString(bytes);

                
                try
                {
                    SerializableContainer container = JsonUtility.FromJson<SerializableContainer>(json);

                    Type wt = typeof(Wrapper<>).MakeGenericType(container.data.GetType());
                    IWrapper wrapper = (IWrapper) Activator.CreateInstance(wt, container.data);

                    cache.Add(key, wrapper);
                } catch(Exception e)
                {
                    Debug.LogError(e);
                }
               
            }
        }

        private static void Ensure()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var attr in assembly.GetCustomAttributes<EnsureAttribute>())
                {
                    try
                    {
                        if (!IsValidKey(attr.Key))
                            throw new ArgumentException();

                        if (attr.Type == null)
                            throw new ArgumentNullException(nameof(attr.Type));

                        if (!attr.Type.IsValueType)
                            throw new ArgumentException();


                        if (cache.ContainsKey(attr.Key))
                        {
                            if (cache[attr.Key].Raw.GetType().Equals(attr.Type))
                                continue; //Everything is fine

                            Debug.LogWarning($"Type mismatch! {attr.Key}: {cache[attr.Key].Raw.GetType().Name} -> {attr.Type.Name}");
                            cache.Remove(attr.Key);
                        }

                        Type wt = typeof(Wrapper<>).MakeGenericType(attr.Type);
                        IWrapper wrapper = (IWrapper)Activator.CreateInstance(wt, Activator.CreateInstance(attr.Type));

                        cache.Add(attr.Key, wrapper);
                    } catch(Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
                
                    
        }

        private static void PreventLoss()
        {
            UnityEditor.EditorApplication.quitting -= PreventLoss;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= PreventLoss;

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

            SerializableContainer container = new()
            {
                type = cache[key].Raw.GetType().AssemblyQualifiedName,
                data = cache[key].Raw
            };

            string json = JsonUtility.ToJson(container, true);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }


        private static void DeleteDumpAssets()
        {
            var dumps = Resources.FindObjectsOfTypeAll<PersistentDataDump>();
            foreach (var dump in dumps)
                UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(dump));

            UnityEditor.AssetDatabase.Refresh();
        }
#endif
    }
}
