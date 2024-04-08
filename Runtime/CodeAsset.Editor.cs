using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MischievousByte.Scaffolding
{
    public static partial class CodeAsset
    {
#if UNITY_EDITOR

        private class PreBuiltProcessor : UnityEditor.Build.IPreprocessBuildWithReport
        {
            public int callbackOrder => 1;

            public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
                DeleteDumpAssets();

                var dumpFile = ScriptableObject.CreateInstance<CodeAssetDump>();
                dumpFile.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;

                foreach (var pair in cache)
                    dumpFile.containers.Add(new() { key = pair.Key, json = JsonUtility.ToJson(pair.Value)});

                UnityEditor.AssetDatabase.CreateAsset(dumpFile, "Assets/scaffolding_persistent_dump.asset");
                
                UnityEditor.AssetDatabase.Refresh();

                var assets = UnityEditor.PlayerSettings.GetPreloadedAssets().Where(obj => !(obj is CodeAssetDump)).ToList();
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


        private const string relativeStorageLocation = "ProjectSettings/scaffolding/persistent_data/";
        private static string storageLocation;

        private static partial void Initialize()
        {
            storageLocation = Path.GetFullPath(Path.Combine(Application.dataPath, "../", relativeStorageLocation));
            Directory.CreateDirectory(storageLocation);

            UnityEditor.EditorApplication.quitting += PreventLoss;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += PreventLoss;

            PopulateCache();

            DeleteDumpAssets();
        }

        private static void PopulateCache()
        {
            cache.Clear();

            foreach(var attr in GetValidAttributes())
            {
                try
                {
                    string path = Path.Combine(storageLocation, $"{attr.Key}.json");
                    Type wrapperType = typeof(Wrapper<>).MakeGenericType(attr.Type);

                    IWrapper wrapper;

                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);

                        wrapper = (IWrapper) JsonUtility.FromJson(json, wrapperType);
                    }
                    else
                        wrapper = (IWrapper) Activator.CreateInstance(wrapperType, Activator.CreateInstance(attr.Type));

                    cache.Add(attr.Key, wrapper);
                } catch(Exception e)
                {
                    Debug.LogError($"Error while loading data for {attr.Key}");
                    Debug.LogException(e);
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

            
            string json = JsonUtility.ToJson(cache[key], true);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }


        private static void DeleteDumpAssets()
        {
            var dumps = Resources.FindObjectsOfTypeAll<CodeAssetDump>();
            foreach (var dump in dumps)
                UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(dump));

            UnityEditor.AssetDatabase.Refresh();
        }
#endif
    }
}
