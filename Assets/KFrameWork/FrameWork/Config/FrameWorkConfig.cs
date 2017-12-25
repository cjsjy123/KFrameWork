
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

[TagSet("CameraUITag", "Camera3DTag")]
//[ScriptMarcoDefine("TOLUA", "Advance", "USE_TANGAB"]//,"UNITY_NAVMESH"
public static class FrameWorkConfig  {

    public static int FPS_Value = 30;

    public const int PHYSTEP_VALUE = 1;

    public const float DisplayUIWidth = 1920f;

    public const float DisplayUIHiehgt = 1080f;

    public static bool Open_DEBUG =false;

    public static int Preload_ParamsCount =10;

    public static bool ShowUnityInfoReport =true;

    [FrameWorkDestroy]
    private static void DestroyFrameWorkEvent(int lv)
    {

        if (Schedule.mIns != null)
        {
            Schedule.mIns.Destroy();
        }

        if (GameSceneCtr.mIns != null)
        {
            GameSceneCtr.mIns.Destroy();
        }

        if(GameUIControl.mIns != null)
        {
            GameUIControl.mIns.Destroy();
        }


        //if (KObjectPool.mIns != null)
        //    KObjectPool.mIns.Destroy();

        if(GameSyncCtr.mIns != null)
            GameSyncCtr.mIns.EndSync();

        FrameCommand.Destroy();
        WaitTaskCommand.Destroy();
        TimeCommand.Destroy();

        FrameworkAttRegister.DestroyAttEvent(MainLoopEvent.Start, typeof(WaitTaskCommand), "PreLoad");
        FrameworkAttRegister.DestroyAttEvent(MainLoopEvent.Start, typeof(TimeCommand), "PreLoad");
        FrameworkAttRegister.DestroyAttEvent(MainLoopEvent.Start, typeof(FrameCommand), "PreLoad");
        FrameworkAttRegister.DestroyAttEvent(MainLoopEvent.Start, typeof(BundlePathConvert), "PreLoad");
        FrameworkAttRegister.DestroyAttEvent(MainLoopEvent.Start, typeof(GenericParams), "Preload");
        FrameworkAttRegister.DestroyAttEvent(MainLoopEvent.Start, typeof(SimpleParams), "Preload");
        FrameworkAttRegister.DestroyAttEvent(MainLoopEvent.OnDestroy, typeof(FrameWorkConfig), "DestroyFrameWorkEvent");
    }

    public static void LoadConfig(MainLoop loop)
    {
        try
        {
            GameApplication.Start();

            if (MainLoop.OpenLua)
            {
#if TOLUA
                new LuaResLoader();
                loop.gameObject.AddComponent(typeof(GameLuaClient));
#endif
            }
#if UNITY_EDITOR
            //maxNumberOfSamplesPerFrame
            PlayerSettings.aotOptions = "nrgctx-trampolines=8096,nimt-trampolines=8096,ntrampolines=4048";
#endif
            Application.targetFrameRate = FPS_Value;

            if (Open_DEBUG)
            {
                loop.TryAddComponent<GameDebugger>();
            }
        }
        catch(Exception ex)
        {
            LogMgr.LogException(ex);
        }

    }
}
