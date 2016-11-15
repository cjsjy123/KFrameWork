using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;

public class AssetBundle_Test : MonoBehaviour {
    public Canvas canvas;

    private long last;

    public long delta =200;
	// Use this for initialization
	void Start () {

        ResBundleMgr.YieldInited(() =>
        {
            this.last = GameSyncCtr.mIns.RenderFrameCount;
           // ResBundleMgr.mIns.LoadAsync("ui_test.prefab", canvas);
        });
    }

    void OnGUI()
    {
        if (GUILayout.Button("Preload Prefab"))
        {
            ResBundleMgr.mIns.PreLoad("ui_test.prefab");
        }

        if (GUILayout.Button("Load Prefab"))
        {
            ResBundleMgr.mIns.Load("ui_test.prefab", canvas);
        }

        if (GUILayout.Button("Async Load Prefab"))
        {
            ResBundleMgr.mIns.LoadAsync("ui_test.prefab", canvas);
        }

        if (GUILayout.Button("Destroy "))
        {
            int cnt = canvas.transform.childCount;
            if (cnt > 0)
            {
                GameObject.Destroy(canvas.transform.GetChild(0).gameObject);
            }
        }

        if (GUILayout.Button("UnloadUnused"))
        {
            ResBundleMgr.UnLoadUnused();
        }

        if (GUILayout.Button("Dump"))
        {
            ResBundleMgr.mIns.Cache.LogDebugInfo();
        }
    }

    // Update is called once per frame
    //void Update () {
    //       // ResBundleMgr.mIns.Cache.LogDebugInfo();
    //       if (GameSyncCtr.mIns.RenderFrameCount - last > this.delta)
    //       {
    //           ResBundleMgr.mIns.LoadAsync("ui_test.prefab", canvas);
    //           this.last = GameSyncCtr.mIns.RenderFrameCount;
    //       }
    //}
}
