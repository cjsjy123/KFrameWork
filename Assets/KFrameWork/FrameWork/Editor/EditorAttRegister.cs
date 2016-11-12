using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Reflection;

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
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PrepareTextureAttribute),_Register_PrepareSpeedTreeAttribute);
            tools.RegisterHandler(RegisterType.MethodAtt,typeof(PrepareAnimationAttribute),_Register_PrepareAnimationAttribute);
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


