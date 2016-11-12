
#define HIDE_INFO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;
using UnityEditor;


#if !HIDE_INFO
public class Import_Test  {

    [PostAllAssetName]
    private static void RevceiveAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach(var imasset in importedAssets)
        {
            LogMgr.Log("RevceiveAllAssets Import asset "+ imasset);
        }

        foreach(var dasset in deletedAssets)
        {
            LogMgr.Log("RevceiveAllAssets Import asset "+ dasset);
        }

        foreach(var masset in movedAssets)
        {
            LogMgr.Log("RevceiveAllAssets Import asset "+ masset);
        }

        foreach(var mvasset in movedFromAssetPaths)
        {
            LogMgr.Log("RevceiveAllAssets Import asset "+ mvasset);
        }
    }

    [PostDelAssetName]
    private static void ReceiveDelAsset(string[] deletedAssets)
    {

        foreach(var dasset in deletedAssets)
        {
            LogMgr.Log("ReceiveDelAsset del asset "+ dasset);
        }
            
    }

    [PostImportAssetName]
    private static void ReceiveImportAsset(string[] importedAssets)
    {

        foreach(var asset in importedAssets)
        {
            LogMgr.Log("ReceiveImportAsset Import asset "+ asset);
        }

    }

    [PostMoveAssetName]
    private static void ReceiveMoveAsset(string[] importedAssets)
    {

        foreach(var asset in importedAssets)
        {
            LogMgr.Log("ReceiveMoveAsset Import asset "+ asset);
        }

    }

    [PostMoveFromAssetName]
    private static void ReceiveMoveFromAsset(string[] importedAssets)
    {

        foreach(var asset in importedAssets)
        {
            LogMgr.Log("ReceiveMoveFromAsset Import asset "+ asset);
        }

    }

}
#endif
