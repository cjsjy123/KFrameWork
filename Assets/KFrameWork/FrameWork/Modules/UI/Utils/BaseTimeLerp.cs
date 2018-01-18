using UnityEngine;
using UnityEngine.UI;
using System;
using KFrameWork;

#if Advance
using AdvancedInspector;
#endif

public abstract class BaseTimeLerp<T> : BaseLerp<T> where T:struct
{
    #if Advance
    [Inspect,Descriptor("运行时间",""),RangeValue(0.001f,100000f)]
#endif
    public float runningtime;

    protected float currenttime;

    protected override void StartCallBack()
    {
        base.StartCallBack();
        currenttime = 0f;
    }

    protected override void EndCallBack()
    {
        base.EndCallBack();
        currenttime = 0f;
    }

    public override void ResetValue()
    {
        currenttime = 0f;
    }

    protected override void Update()
    {
        if (isRunning)
        {
            currenttime += GameSyncCtr.mIns.RenderDeltaTime;
            setCurrentLerpTarget(CheckParams(getCurrentLerpTarget()));

            if (LoopCnt > 0 && currentLoopCnt > LoopCnt)
            {
                this.EndLerp();
            }
        }
    }
}
