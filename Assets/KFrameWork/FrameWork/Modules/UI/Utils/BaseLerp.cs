using UnityEngine;
using UnityEngine.UI;
#if Advance
using AdvancedInspector;
#endif
public enum LerpType
{
    Fixed,
    Lerp,
}

public enum ChangeType
{
    Rise,
    PingPong,
    InversePingPong,
    Reduce,
    Loop,
}

#if Advance
[AdvancedInspector]
#endif

public abstract class BaseLerp<T> : BaseMeshEffect where T:struct
{
#if Advance
    [Inspect(0), Tooltip("数值改变类型")]
#endif
    public ChangeType changeType;
#if Advance
    [Inspect(1), Tooltip("数值插值类型")]
#endif
    public LerpType Lerptype;
#if Advance
    [Inspect(2), Tooltip("数值最大值")]
#endif
    public T MaxValue = default(T);
#if Advance
    [Inspect(3), Tooltip("数值最小值")]
#endif
    public T MinValue = default(T);
#if Advance
    [Inspect(4), Tooltip("数值变化的固定值")]
#endif
    public T FixedValue = default(T);
#if Advance
    [Inspect(5), Tooltip("插值速率")]
#endif
    public T LerpSpeed = default(T);
#if Advance
    [Inspect(6), ReadOnly, Descriptor("运行状态", "")]
#endif
    public bool isRunning { get; protected set; }
#if Advance
    [Inspect(7), Descriptor("循环次数","")]
#endif
    public int LoopCnt = -1;
#if Advance
    [Inspect(8), ReadOnly,Descriptor("当前循环次数","")]
#endif
    protected int currentLoopCnt = 0;

    protected T lerpTempTarget;

#if Advance
    [Inspect(-1), Descriptor("开始插值", "")]

    void EditorStart()
    {
        StartLerp();
    }

#endif
#if Advance
    [Inspect(-2), Descriptor("结束插值", "")]

    void EditorStop()
    {
        EndLerp();
    }
#endif

    protected abstract T CheckParams(T value);

    protected abstract void Update();

    public abstract void ResetValue();

    public abstract T getCurrentLerpTarget();

    public abstract void setCurrentLerpTarget(T value);


    public virtual void StartLerp()
    {
        if (!Application.isPlaying)
            return;

        if (!isRunning)
        {
            isRunning = true;
            currentLoopCnt = 0;
            StartCallBack();
        }
    }

    
    public virtual void EndLerp()
    {
        if (!Application.isPlaying)
            return;

        if (isRunning)
        {
            isRunning = false;
            currentLoopCnt = 0;
            ResetValue();
            EndCallBack();
        }
    }
#if Advance
    [Inspect(-3), Descriptor("重置", "")]
#endif
    void ResetParams()
    {
        ResetValue();
        StartCallBack();

        currentLoopCnt = 0;
        graphic.SetVerticesDirty();
    }

    protected virtual void EndCallBack()
    {

    }

    protected virtual void StartCallBack()
    {
        if (Lerptype == LerpType.Lerp)
        {
            if (this.changeType == ChangeType.PingPong)
            {
                lerpTempTarget = this.MaxValue;
            }
            else if (this.changeType == ChangeType.InversePingPong)
            {
                lerpTempTarget = this.MinValue;
            }
            else if (this.changeType == ChangeType.Rise)
            {
                lerpTempTarget = this.MaxValue;
            }
            else if (this.changeType == ChangeType.Reduce)
            {
                lerpTempTarget = this.MinValue;
            }
        }
    }
}
