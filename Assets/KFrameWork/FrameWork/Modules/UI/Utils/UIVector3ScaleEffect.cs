using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
using UnityEngine.UI;

public class UIVector3ScaleEffect : BaseTimeLerp<Vector3>
{
    private Vector3 oldscale;
    protected override void Awake()
    {
        base.Awake();
        oldscale = this.transform.localScale;
    }

    public override void ResetValue()
    {
        base.ResetValue();
        this.transform.localScale = oldscale;
    }

    public override Vector3 getCurrentLerpTarget()
    {
        return this.transform.localScale;
    }

    public override void ModifyMesh(VertexHelper vh)
    {
       
    }

    public override void setCurrentLerpTarget(Vector3 value)
    {
        this.transform.localScale = value;
    }

    protected override void StartCallBack()
    {
        base.StartCallBack();
        if (this.changeType == ChangeType.PingPong)
        {
            this.transform.localScale = this.MinValue;
        }
        else if (this.changeType == ChangeType.InversePingPong)
        {
            this.transform.localScale = this.MaxValue;
        }
    }

    protected override Vector3 CheckParams(Vector3 value)
    {
        if (runningtime.FloatEqual(0f))
        {
            LogMgr.LogError("时间不能为0");
            this.EndLerp();
            return value;
        }

        if (changeType == ChangeType.Loop)
        {
            float percent = this.currenttime / this.runningtime;
            Vector3 target = Vector3.Lerp(this.MinValue, this.MaxValue, percent);

            if (percent >1f )
            {
                target = this.MinValue;
                currentLoopCnt++;
            }

            return target;
        }
        else if (changeType == ChangeType.Rise)
        {
            return Vector3.Lerp(this.MinValue, this.MaxValue, this.currenttime / this.runningtime);
        }
        else if (changeType == ChangeType.Reduce)
        {
            return Vector3.Lerp(this.MaxValue, this.MinValue, this.currenttime / this.runningtime);
        }
        else if (changeType == ChangeType.PingPong)
        {
            float percent = this.currenttime / this.runningtime;

            if (currenttime > runningtime)
            {
                int m =(int)(currenttime /runningtime);
                if (m % 2 == 1)
                {
                    percent = 1f - percent % 1f;
                }
                else
                {
                    percent = percent % 1f;
                }
                currentLoopCnt = m / 2 +1;
            }
            Vector3 target = Vector3.Lerp(this.MinValue, this.MaxValue, percent);
            return target;
        }
        else if (changeType == ChangeType.InversePingPong)
        {
            float percent = this.currenttime / this.runningtime;

            if (currenttime > runningtime)
            {
                int m = (int)(currenttime / runningtime);
                if (m % 2 == 1)
                {
                    percent = 1f - percent % 1f;
                }
                else
                {
                    percent = percent % 1f;
                }
                currentLoopCnt = m / 2 + 1;
            }

            LogMgr.LogError(percent);
            Vector3 target = Vector3.Lerp(this.MaxValue, this.MinValue, percent);
            return target;
        }

        return value;
    }

}
