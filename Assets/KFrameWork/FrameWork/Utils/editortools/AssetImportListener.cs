using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
#if UNITY_EDITOR
using UnityEditor;

namespace KFrameWork
{
    public enum AssetImportDefine
    {
        PostAllAssets,
        PostImportAsset,
        PostDelAsset,
        PostMoveAsset,
        PostMoveFromAsset,

        PostModel,
        PostAbNameChange,
        PostAudio,
        PostSpeedTree,
        PostTexture,
        PostGameObjectProperty,
        PostSprites,

        PrepareAudio,
        PrepareModel,
        PrepareTree,
        PrepareAnimation,
        PrepareTexture,

        AssignModel,
    }

    public class AssetImportListener : AssetPostprocessor {

        static AssetImportListener()
        {
            new EditorTools();
        }

        #region postprocess
        static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostImportAsset,importedAssets as object);
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostDelAsset,deletedAssets as object) ;
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostMoveAsset,movedAssets as object);
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostMoveFromAsset,movedFromAssetPaths as object);

            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostAllAssets,importedAssets,deletedAssets,movedAssets,movedFromAssetPaths);
        }

        void OnPostprocessModel(GameObject gameobject)
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostModel,gameobject);
        }

        void OnPostprocessAssetbundleNameChanged( string assetPath, string previousAssetBundleName, string newAssetBundleName)
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostAbNameChange,assetPath,previousAssetBundleName,newAssetBundleName);
        }

        void OnPostprocessAudio(AudioClip clip)
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostAudio,clip);
        }

        void OnPostprocessSpeedTree(GameObject gameobject)
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostSpeedTree,gameobject);
        }

        void OnPostprocessTexture(Texture2D texture) 
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostTexture,texture);
        }

        void OnPostprocessGameObjectWithUserProperties ( GameObject go,string[] propNames, System.Object[] values)
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostGameObjectProperty,go,propNames,values);
        }


        void OnPostprocessSprites(Texture2D tex, Sprite[] sprs)
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PostSprites,tex,sprs);
        }
        #endregion

        #region prepare
        void OnPreprocessAudio ()
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PrepareAudio,this.assetImporter,this.assetPath);
        }

        void OnPreprocessModel()
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PrepareModel,this.assetImporter,this.assetPath);
        }

        void OnPreprocessSpeedTree()
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PrepareTree,this.assetImporter,this.assetPath);
        }

        void OnPreprocessTexture () {

            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PrepareTexture,this.assetImporter,this.assetPath);
        }

        void OnPreprocessAnimation()
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.PrepareAnimation,this.assetImporter,this.assetPath);
        }


        #endregion

        #region others
        void OnAssignMaterialModel(Material material,Renderer render)
        {
            EditorTools.getInstance().DynamicInvokeAtt(AssetImportDefine.AssignModel,material,render);
        }
        #endregion

    }
}
#endif


