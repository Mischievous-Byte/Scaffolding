using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MischievousByte.Scaffolding
{


    public static partial class PersistentData
    {
#if !UNITY_EDITOR
        private static partial void Initialize()
        {
            var dump = Resources.FindObjectsOfTypeAll<PersistentDataDump>().First();

            foreach(var container in dump.containers)
                cache.Add(container.key, container.data);

            Object.Destroy(dump);
        }
#endif

    }
}
