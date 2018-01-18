using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Reflection;
#if UNITY_EDITOR
using NodeEditorFramework;
using NodeEditorFramework.Standard;
using UnityEditor;
using UnityEditorInternal;

namespace KFrameWork
{
    public static class EditorAttRegister  {

        public static void Register(EditorTools tools)
        {

            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostAllAssetNameAttribute),_Register_AllAssets);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostImportAssetNameAttribute),_Register_PostImportAssets);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostDelAssetNameAttribute),_Register_PostDelAssets);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostMoveAssetNameAttribute),_Register_PostMoveAssets);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostMoveFromAssetNameAttribute),_Register_PostMoveFromAssets);

            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostModelAttribute),_Register_PostModelFromAssets);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostABNameChangeAttribute),_Register_PostABNameChangeFromAssets);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostAudioAttribute),_Register_PostAudioAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostSpeedTreeAttribute),_Register_PostSpeedTreeAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostTextureAttribute),_Register_PostTextureAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostGameobjecPropertyAttribute),_Register_PostGameobjecPropertyAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostSpritesAttribute),_Register_PostSpritesAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PostAssignModelsAttribute),_Register_PostAssignModelsAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PrepareAudioAttribute),_Register_PostAudioAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PrepareModelAttribute),_Register_PrepareModelAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PrepareSpeedTreeAttribute),_Register_PrepareSpeedTreeAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PrepareTextureAttribute),_Register_PrepareTextureAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PrepareAnimationAttribute),_Register_PrepareAnimationAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt, typeof(NoteEditorMenuAttribute), Register_NoteEditorMenuAtt);

            tools.RegisterHandler(RegisterType.ClassAttr, typeof(TimeSetAttribute), Register_Fixed);
            tools.RegisterHandler(RegisterType.ClassAttr, typeof(TagSetAttribute), Register_TagAdd);
            tools.RegisterHandler(RegisterType.ClassAttr, typeof(LayerSetAttribute), Register_LayerAdd);

            tools.RegisterHandler(RegisterType.ClassAttr,typeof(ScriptMarcoDefineAttribute), Register_ScriptMarce);
        }

        static void Register_NoteEditorMenuAtt(object att, object target)
        {
            NoteEditorMenuAttribute attribute = att as NoteEditorMenuAttribute;
            MethodInfo method = target as MethodInfo;

            if (attribute != null)
            {
                ActionMenuTools.GetInstance().AddMenuaction(attribute.menuname, attribute, method);
            }
        }

        static void Register_ScriptMarce(object att, object target)
        {
            ScriptMarcoDefineAttribute attribute = att as ScriptMarcoDefineAttribute;

            if (attribute != null)
            {
                string marcos = null;
                if (GameApplication.isAndroid)
                {
                    marcos =PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                }
                else if (GameApplication.isIOS)
                {
                    marcos =PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
                }

                //string[] marcosGroup = marcos.Split(';');
                //just for one  remove this
                //for (int i = 0; i < marcosGroup.Length; ++i)
                //{
                //    string s = marcosGroup[i];
                //    if (attribute.names.Contains(s))
                //    {
                //        attribute.names.Remove(s);
                //    }
                //}
                //and add this
                marcos = "";

                if (attribute.names.Count > 0)
                {
                    for (int i = 0; i < attribute.names.Count; ++i)
                    {
                        marcos += ";" + attribute.names[i];
                    }

                }

                if (GameApplication.isAndroid)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, marcos);
                }
                else if (GameApplication.isIOS)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, marcos);
                }
                else
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,marcos);
                }
            }
        }

        static void Register_LayerAdd(object att, object target)
        {

            LayerSetAttribute attribute = att as LayerSetAttribute;
            if (attribute != null)
            {
                SerializedObject result = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty it = result.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.name.Equals("layers"))
                    {
                        List<string> keys = new List<string>();

                        for (int i =8; i < it.arraySize; i++)
                        {
                            SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                            if (!string.IsNullOrEmpty(dataPoint.stringValue))
                            {
                                keys.TryAdd(dataPoint.stringValue);
                            }
                        }

                        bool ret = false;
                        foreach (var subtag in attribute.layers)
                        {
                            if (keys.TryAdd(subtag))
                            {
                                ret = true;
                            }
                        }

                        if (!ret)
                            return;


                        if (keys.Count + 8 > it.arraySize)
                        {
                            int old = it.arraySize;
                            int newsize = keys.Count;
                            while (newsize > 0)
                            {
                                it.InsertArrayElementAtIndex(old++);
                                newsize--;
                            }
                            //result.ApplyModifiedProperties();
                        }
                        else if (keys.Count + 8 < it.arraySize)
                        {
                            it.arraySize = keys.Count + 8;
                        }

                        for (int i = 0; i < keys.Count; i++)
                        {
                            it.GetArrayElementAtIndex(i+8).stringValue = keys[i];
                        }

                        result.ApplyModifiedProperties();
                        return;
                    }
                }
            }
        }

        static void Register_TagAdd(object att, object target)
        {
            TagSetAttribute attribute = att as TagSetAttribute;
            if (attribute != null)
            {
                SerializedObject result = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty it = result.GetIterator();

                while (it.NextVisible(true))
                {
                    if (it.name == "tags")
                    {

                        List<string> keys = new List<string>();

                        for (int i = 0; i < it.arraySize; i++)
                        {
                            SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                            if (!string.IsNullOrEmpty(dataPoint.stringValue))
                            {
                                keys.Add(dataPoint.stringValue);
                            }
                        }

                        bool ret = false;
                        foreach (var subtag in attribute.tags)
                        {
                            if (keys.TryAdd(subtag))
                            {
                                ret = true;
                            }
                        }

                        if (!ret)
                            return;

                        if (keys.Count > it.arraySize)
                        {
                            int old = it.arraySize;
                            //it.arraySize = attribute.tags.Count ;//预留
                            for (int i = old; i < keys.Count; ++i)
                            {
                                it.InsertArrayElementAtIndex(i);
                            }
                            result.ApplyModifiedProperties();
                        }
                        else if(keys.Count < it.arraySize)
                        {
                            int old = it.arraySize;

                            for (int i = keys.Count; i < old; ++i)
                            {
                                it.DeleteArrayElementAtIndex(i);
                            }
                        }

                        for (int i = 0; i < keys.Count; i++)
                        {
                            it.GetArrayElementAtIndex( i).stringValue = keys[i];
                        }

                        result.ApplyModifiedProperties();
                        return;
                    }
                }
            }

        }

        static void Register_Fixed(object att, object target)
        {
            TimeSetAttribute attribute = att as TimeSetAttribute;
            if (attribute != null)
            {
                SerializedObject result = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset")[0]);
                SerializedProperty it = result.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.name.StartsWith("Fixed Timestep"))
                    {
                        it.floatValue = attribute.fixedstep;
                        result.ApplyModifiedProperties();
                        return;
                    }
                }
            }
        }

        static void _Register_AllAssets(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostAllAssetNameAttribute attribute = att as PostAllAssetNameAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostAllAssets, attribute);
            }
        }

        static void _Register_PostImportAssets(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostImportAssetNameAttribute attribute = att as PostImportAssetNameAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostImportAsset, attribute);
            }
        }

        static void _Register_PostDelAssets(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostDelAssetNameAttribute attribute = att as PostDelAssetNameAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostDelAsset, attribute);
            }
        }

        static void _Register_PostMoveAssets(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostMoveAssetNameAttribute attribute = att as PostMoveAssetNameAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostMoveAsset, attribute);
            }
        }

        static void _Register_PostMoveFromAssets(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostMoveFromAssetNameAttribute attribute = att as PostMoveFromAssetNameAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostMoveFromAsset, attribute);
            }
        }

        static void _Register_PostModelFromAssets(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostModelAttribute attribute = att as PostModelAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostModel, attribute);
            }
        }

        static void _Register_PostABNameChangeFromAssets(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostABNameChangeAttribute attribute = att as PostABNameChangeAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostAbNameChange, attribute);
            }
        }

        static void _Register_PostAudioAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostAudioAttribute attribute = att as PostAudioAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostAudio, attribute);
            }
        }

        static void _Register_PostSpeedTreeAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostSpeedTreeAttribute attribute = att as PostSpeedTreeAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostSpeedTree, attribute);
            }
        }

        static void _Register_PostTextureAttribute(object att,object target)
        {

            MethodInfo method = target as MethodInfo;
            PostTextureAttribute attribute = att as PostTextureAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostTexture, attribute);
            }
        }

        static void _Register_PostGameobjecPropertyAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostGameobjecPropertyAttribute attribute = att as PostGameobjecPropertyAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostGameObjectProperty, attribute);
            }
        }



        static void _Register_PostSpritesAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostSpritesAttribute attribute = att as PostSpritesAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PostSprites, attribute);
            }
        }



        static void _Register_PrepareAudioAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PrepareAudioAttribute attribute = att as PrepareAudioAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PrepareAudio, attribute);
            }
        }

        static void _Register_PrepareModelAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PrepareModelAttribute attribute = att as PrepareModelAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PrepareModel, attribute);
            }
        }

        static void _Register_PrepareSpeedTreeAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PrepareSpeedTreeAttribute attribute = att as PrepareSpeedTreeAttribute;

            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PrepareTree, attribute);
            }
        }

        static void _Register_PrepareAnimationAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PrepareAnimationAttribute attribute = att as PrepareAnimationAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PrepareAnimation, attribute);
            }
        }

        static void _Register_PrepareTextureAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PrepareTextureAttribute attribute = att as PrepareTextureAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.PrepareTexture, attribute);
            }
        }

        static void _Register_PostAssignModelsAttribute(object att,object target)
        {
            MethodInfo method = target as MethodInfo;
            PostAssignModelsAttribute attribute = att as PostAssignModelsAttribute;
            if(method.LogStaticMethod() && attribute.LogInfo(method))
            {
                attribute.SetDelegate(method);
                EditorTools.getInstance().PushImportAtt(AssetImportDefine.AssignModel, attribute);
            }
        }
    }
}

#endif
