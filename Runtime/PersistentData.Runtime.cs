using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace MischievousByte.Scaffolding
{


    public static partial class PersistentData
    {
#if !UNITY_EDITOR
        private static partial void Initialize()
        {
            var dump = Resources.FindObjectsOfTypeAll<PersistentDataDump>().First();


            var uniqueContainers = dump.containers.GroupBy(c => c.key).Select(g => g.First());
            
            foreach(var container in uniqueContainers)
            {
                try
                {
                    Type wt = typeof(Wrapper<>).MakeGenericType(container.data.GetType());
                    IWrapper wrapper = (IWrapper)Activator.CreateInstance(wt, container.data);

                    cache.Add(container.key, wrapper);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                
            }
                

            UnityEngine.Object.Destroy(dump);
        }
#endif

    }
}
