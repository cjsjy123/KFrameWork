using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using UnityEditor;

namespace KFrameWork
{
    public class BundleTools 
    {
        [MenuItem("Assets/Tools/Bundle/DumpDepends",false,-100)]
        public static void DumpDepends()
        {
            string[] assetids =  Selection.assetGUIDs;
            if (assetids != null && assetids.Length > 0)
            {
                string uid = assetids[0];
                string path = AssetDatabase.GUIDToAssetPath(uid);

                LogMgr.Log("Will Check: "+ path);

                string[] cols = AssetDatabase.GetDependencies(new string[] { path });

                foreach (var sub in cols)
                {
                    if(sub != path)
                        LogMgr.Log("Need: "+sub);
                }
            }
        }

    }
}


