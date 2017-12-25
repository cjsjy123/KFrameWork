using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using UnityEditor;
using System.IO;

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

               // var r = EditorUtility.CollectDependencies(new UnityEngine.Object[] { Selection.activeObject });
                string[] cols = AssetDatabase.GetDependencies(new string[] { path });

                foreach (var sub in cols)
                {
                    if(sub != path && !sub.EndsWith(".cs") && !sub.EndsWith(".js"))
                        LogMgr.Log("Need: "+sub);
                }
            }
        }

        [MenuItem("Assets/Tools/Bundle/SpriteChecker")]
        static void CheckSpritesTagsAndBundles()
        {
            // Get all the GUIDs (identifiers in project) of the Sprites in the Project
            string[] guids = AssetDatabase.FindAssets("t:sprite");
            HashSet<string> uids = new HashSet<string>();
            foreach (var sub in guids)
            {
                uids.Add(sub);
            }

            // Dictionary to store the tags and bundle names
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string guid in uids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;

                // If the tag is not in the dictionary, add it
                if (!dict.ContainsKey(ti.spritePackingTag))
                    dict.Add(ti.spritePackingTag, ti.assetBundleName);
                else
                {
                    // If the tag is associated with another bundle name, show an error
                    if (dict[ti.spritePackingTag] != ti.assetBundleName)
                        LogMgr.LogWarning("Sprite : " + ti.assetPath + " should be packed in " + dict[ti.spritePackingTag]);
                }

            }
        }

        [MenuItem("Assets/Tools/Bundle/BuildLua")]
        public static void ApendLuaAssets()
        {
            if (Directory.Exists(Application.dataPath + "/Lua") == false)
                Directory.CreateDirectory(Application.dataPath + "/Lua");

            if (Directory.Exists(Application.dataPath + "/ToLua") == false)
                Directory.CreateDirectory(Application.dataPath + "/ToLua");

            FileUtil.DeleteFileOrDirectory(Application.dataPath + "/Lua");
#if TOLUA
            EditorTools.DirectoryCopy(LuaConst.luaDir, Application.dataPath + "/Lua", ".bytes",".meta", true);

            EditorTools.DirectoryCopy(LuaConst.toluaDir, Application.dataPath + "/ToLua", ".bytes", ".meta", true);
            //确保absystem包含了此路径
#endif
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Tools/Bundle/BuildDepall")]
        public static void PackPkgAssets()
        {
            try
            {
                string deppath = Application.streamingAssetsPath + "/" + BundleConfig.ABSavePath + "/" + BundleConfig.ABVersionPath;

                if (File.Exists(deppath))
                {
                    byte[] bys = File.ReadAllBytes(deppath);
                    BundlePkgAsset pkgasset = ScriptableObject.CreateInstance<BundlePkgAsset>();
                    pkgasset.bytes = bys;

                    //暂时不先删除原始资源dep.all
                    string resname = "Assets/dep.asset";
                    AssetDatabase.CreateAsset(pkgasset, resname);

                    AssetBundleBuild build = new AssetBundleBuild();
                    build.assetBundleName = BundleConfig.depAssetBundlePath;
                    build.assetNames = new string[] { resname };

                    BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath + "/" + BundleConfig.ABSavePath, new AssetBundleBuild[] { build }, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
                    AssetDatabase.DeleteAsset(resname);
                }
            }
            catch (System.Exception ex)
            {
                LogMgr.LogError(ex);
                EditorUtility.ClearProgressBar();
            }

        }

    }
}


