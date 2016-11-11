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

         BundleMgr.mIns.Load("UI_TEST.prefab", canvas);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
