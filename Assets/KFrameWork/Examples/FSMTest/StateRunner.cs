using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;
#if EXAMPLE
public class FrameTestRunner:FSMRunner
{
    public override FSMRunningType runningType {
        get {
            return FSMRunningType.Frame;
        }
    }

    public override void FrameUpdateForLogic ()
    {
        LogMgr.LogFormat("<color=#FFB500FF>FrameUpdateForLogic {0} /{1}</color>",GameSyncCtr.mIns.RenderFrameCount.ToString(),this.name);
    }

    public override void DelayTimeUpdateForLogic ()
    {
        LogMgr.LogFormat("<color=#00B500FF>DelayTimeUpdateForLogic {0} /{1} </color>",Time.realtimeSinceStartup.ToString(),this.name);
    }

    public override void InvokeOnceWhenInit ()
    {
        LogMgr.LogFormat("<color=#00B5FFFF>InvokeOnceWhenInit {0}</color>",this.name);
    }

    public override void InvokeOnceWhenEnable ()
    {
        LogMgr.LogFormat("<color=#00B50000>InvokeOnceWhenEnable {0}</color>",this.name);
    }

    public override void DelayFrameUpdateForLogic ()
    {
        LogMgr.LogFormat("<color=#00AA00FF>DelayFrameUpdateForLogic {0}/ {1}</color>",GameSyncCtr.mIns.RenderFrameCount.ToString(),this.name);
    }
}

public class DelayTimeRunner:FrameTestRunner
{
    public override FSMRunningType runningType {
        get {
            return FSMRunningType.DelayTime;
        }
    }


    public DelayTimeRunner()
    {
        this.delaytime = 5f;
    }

}
#endif