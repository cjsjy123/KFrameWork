using UnityEngine;
using System.Collections;
using KFrameWork;
using System;

public class UITestContent : BaseContent
{
    protected override void Start() {
        base.Start();
        LogMgr.Log("UITestContent start");
        LogMgr.Log("UITestContent end");
    }

    public override void OnDisCover(GameObject discoverObject)
    {
        LogMgr.Log("UITestContent OnDisCover");
    }

    public override void OnEnter()
    {
        LogMgr.Log("UITestContent OnEnter");
    }

    public override void OnExit()
    {
        LogMgr.Log("UITestContent OnExit");
    }
}