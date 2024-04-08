using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace MischievousByte.Scaffolding
{


    public static partial class CodeAsset
    {
#if !UNITY_EDITOR
        private static partial void Initialize()
        {
            cache.Clear();

            var dump = Resources.FindObjectsOfTypeAll<CodeAssetDump>().First();

            foreach(var attr in GetValidAttributes())
            {
                try
                {
                    var candidates = dump.containers.Where(c => c.key == attr.Key);

                    if (candidates.Count() == 0)
                        throw new NullReferenceException("Dump doesn't contain key");
                    if (candidates.Count() > 1)
                        throw new ArgumentException("Duplicate key");

                    var container = candidates.First();

                    Type wt = typeof(Wrapper<>).MakeGenericType(attr.Type);
                    IWrapper wrapper = (IWrapper) JsonUtility.FromJson(container.json, wt);

                    cache.Add(container.key, wrapper);

                } catch(Exception ex)
                {
                    Debug.LogError($"Error while loading data for {attr.Key}");
                    Debug.LogException(ex);
                }
            }

            UnityEngine.Object.Destroy(dump);
        }
#endif

    }
}
