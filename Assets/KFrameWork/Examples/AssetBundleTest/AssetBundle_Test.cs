using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;

public class AssetBundle_Test : MonoBehaviour {
    public Canvas canvas;
	// Use this for initialization
	void Start () {

        ResBundleMgr.YieldInited(() =>
        {
            ResBundleMgr.mIns.Load("ui_test.prefab", canvas);
        });
	}

    void OnGUI()
    {
        if (GUILayout.Button("Load Prefab"))
        {
            ResBundleMgr.mIns.Load("ui_test.prefab", canvas);
        }
    }

	// Update is called once per frame
	void Update () {
       // ResBundleMgr.mIns.Cache.LogDebugInfo();

	}
}
