using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
using System.Reflection;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public class EditorMenus  {

    public static void CreateUGUIAtlas(string prefabpath,string atlaspath)
    {

        if (prefabpath == null && EditorUtility.DisplayDialog("选择预设", "选择ugui 图集预设", "OK"))
        {
            prefabpath = EditorUtility.OpenFilePanel("", "Assets/", "");
            prefabpath = EditorTools.GetUnityAssetPath(prefabpath);
        }

        if (atlaspath ==null && EditorUtility.DisplayDialog("选择图集目录", "选择ugui 图集目录", "OK"))
        {
            atlaspath = EditorUtility.OpenFolderPanel("", "Assets/", "");
            atlaspath = EditorTools.GetUnityAssetPath(atlaspath);
        }

        if (!string.IsNullOrEmpty(prefabpath) && !string.IsNullOrEmpty(atlaspath))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabpath);
            SpriteAtlasMgr spritelist = prefab.TryAddComponent<SpriteAtlasMgr>();
            string[] guids = AssetDatabase.FindAssets("t:Sprite",new string[] { atlaspath });
            HashSet<string> hashset = guids.ConvertHashSet();

            spritelist.Clear();
            foreach (string guid in hashset)
            {
                string assetpath = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object[] allsprs = AssetDatabase.LoadAllAssetsAtPath(assetpath);
                SpriteAtlas atlas = new SpriteAtlas();
                atlas.LoadSprites(allsprs);

                spritelist.InitSprites(atlas);
            }

            EditorUtility.SetDirty(prefab);
            
        }
    }

    public static void ChangeSprite(string prefabpath,params  List<Sprite>[] sprList)
    {

        if (prefabpath == null && EditorUtility.DisplayDialog("选择预设", "选择预设", "OK"))
        {
            prefabpath = EditorUtility.OpenFilePanel("", "Assets/", "");
            prefabpath = EditorTools.GetUnityAssetPath(prefabpath);
        }

        if (!string.IsNullOrEmpty(prefabpath) && sprList != null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabpath);
            Component[] comps = prefab.GetComponentsInChildren<Component>(true);

            foreach (Component c in comps)
            {
                SerializedObject Serialize = new SerializedObject(c);
                var it = Serialize.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null && it.objectReferenceValue is Sprite)
                    {
                        Sprite old = it.objectReferenceValue as Sprite;
                        int count = 0;
                        for (int j = 0; j < sprList.Length; ++j)
                        {
                            Sprite newSpr = sprList[j].Find(p => p.name == old.name);
                            if (newSpr != null)
                            {
                                it.objectReferenceValue = newSpr;
                            }
                            else
                            {
                                count++;
                            }
                        }

                        if (count == sprList.Length)
                        {
                            LogMgr.LogWarningFormat("{0} 没有找到 From :{1} in:{2}", old.name, c, prefabpath);
                        }

                    }
                }

                Serialize.ApplyModifiedProperties();
            }


            EditorUtility.SetDirty(prefab);

        }
    }
}
