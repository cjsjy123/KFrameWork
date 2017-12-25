using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Render2Logic  {

    /// <summary>
    /// logic frame cnt
    /// </summary>
    private long logicValue;

    private float delta;

    private float left;

    private bool ignoreScale = false;

    private Action eachUpdate;

    public long DeltaFrame { get; private set; }

    public Render2Logic(int firstcnt,float constdelta,Action callback)
    {
        this.Reset(firstcnt,constdelta);
        this.SetUpdateCallBack(callback);
    }

    public void SetUpdateCallBack(Action cbk)
    {
        eachUpdate = cbk;
    }

    public int GetValue()
    {
        return (int)logicValue;
    }

    public long GetLongValue()
    {
        return logicValue;
    }

    public void SetIgnore(bool b)
    {
        this.ignoreScale = b;
    }

    public void Reset(int firstcnt, float constdelta)
    {
        this.logicValue = firstcnt;
        this.delta = constdelta;
        this.left = 0;
    }

    float getTimeDelta()
    {
        if (ignoreScale)
        {
            return Time.unscaledDeltaTime;
        }
        else
        {
            return Time.deltaTime;
        }

    }

    /// <summary>
    /// when render update //call
    /// </summary>
    public void RenderUpdate()
    {
        this.left += getTimeDelta();
        int cnt = 0;
        while (this.left > this.delta)
        {
            logicValue++;
            this.left -= this.delta;
            cnt++;

            if (eachUpdate != null)
            {
                eachUpdate();
            }
        }

        DeltaFrame = cnt;

        //if (cnt >2)//FrameWorkConfig.Open_DEBUG && 
        //{
        //    LogMgr.LogFormat("at this time ,Cost :{0} Frame",cnt);
        //}
    }
}
