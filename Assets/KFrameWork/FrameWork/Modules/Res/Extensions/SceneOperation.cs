using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
using UnityEngine.SceneManagement;

public class SceneOperation  {

    private AsyncOperation _async;

    public string loadedSceneName;

    public string ScenePath;

    public bool isDone
    {
        get
        {
            if (_async == null)
                return false;

            return _async.isDone;
        }
    }

    public float progress
    {
        get
        {
            if (_async == null)
                return 0f;

            return _async.progress;
        }
    }
    
    public bool HasLoad
    {
        get
        {
            if (_async == null)
                return false;
            return _async.allowSceneActivation;
        }
    }

    public void DisableScene()
    {
        this._async.allowSceneActivation = false;
    }

    public void EnableScene()
    {

        if (FrameWorkConfig.Open_DEBUG)
            LogMgr.LogFormat("启动场景 ：{0}",this.ScenePath);

        this._async.allowSceneActivation = true;
#if TOLUA
        if (MainLoop.getInstance().OpenLua && LuaClient.GetMainState() != null)
        {
            LuaClient.GetMainState().Collect();
        }
#endif

        IBundleRef bundle = ResBundleMgr.mIns.Cache.TryGetValue(ScenePath);
        if (bundle != null)
        {
            bundle.UnLoad(true);
        }

       // TimeCommand.Create(1f, setFalse).Excute();

    }

    public static implicit operator SceneOperation(AsyncOperation op)
    {

        SceneOperation s = new SceneOperation();
        s._async = op;
        op.allowSceneActivation = false;
        return s;
    }

}
