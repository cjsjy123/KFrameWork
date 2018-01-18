using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;
using System.Text;

public enum CommandState
{
    PrePared,
    Running,
    Paused,
    Canceled,
    Finished,
}


public abstract class CacheCommand
{
    
    protected static List<CacheCommand> RunningList = new List<CacheCommand>();

    public const bool AutoPoolManger = true;

    public abstract void Release (bool force);

    private static int Counter = 0;

    private int repeattimes = 1;

    protected CommandState _state;

    public CommandState RunningState
    {
        get
        {
            return this._state;
        }
    }

    private int m_UID;

    public int UID
    {
        get{
            return this.m_UID;
        }
    }

    public bool isRunning
    {
        get
        {
            return this.RunningState == CommandState.Running;
        }
    }

    private CacheCommand _Next;
    public CacheCommand Next
    {
        get
        {
            return _Next;
        }

        set
        {
            this._Next = value;
        }
    }


    /// <summary>
    /// 命令是否完成，请确保当完成的时候，其被移除掉，不占用引用
    /// </summary>
    /// <value><c>true</c> if is done; otherwise, <c>false</c>.</value>
    public bool isDone
    {
        get
        {
            return this.RunningState == CommandState.Finished;
        }
    }

    protected void GenID()
    {
        this.m_UID = Counter++;
    }

    public static void LogInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Running Cmd is : {0}\n", RunningList.Count);
        for (int i = 0; i < RunningList.Count; ++i)
        {
            CacheCommand cmd = RunningList[i];
            sb.AppendFormat("Each is :{0} State :{1} ID:{2}\n",cmd,cmd.RunningState,cmd.UID);
        }

        sb.Append("=-----Over----=");
        LogMgr.Log(sb.ToString());
    }

    public static void CanCelAll()
    {
        for (int i = RunningList.Count - 1; i >= 0; --i)
        {
            RunningList[i].Cancel();
        }

        RunningList.Clear();
    }

    public static void CanCelAllBy(object o)
    {
        for (int i = RunningList.Count-1; i >=0;--i)
        {
            CacheCommand cmd = RunningList[i];
            if (cmd.CancelBy(o))
            {
                cmd.Cancel();
            }
        }
    }

    protected virtual bool CancelBy(object o)
    {
        return false;
    }

    public virtual void Cancel()
    {
        if (this._state == CommandState.Running)
        {
            this._state = CommandState.Canceled;
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("********* Cmd Cancel :{0}", this);

            RunningList.Remove(this);

            if (AutoPoolManger)
                this.Release(true);
        }
    }

    public void RepeatExcute(int times)
    {
        repeattimes = times;
        Excute();
    }

    public virtual void Excute()
    {
        if (this._state != CommandState.Running && this._state != CommandState.Paused)
        {
            this._state = CommandState.Running;

            RunningList.TryAdd(this);
        }
    }

    protected virtual void SetFinished()
    {
        if (this._state == CommandState.Running )
        {
            if (this.repeattimes == 1)
            {
                this._state = CommandState.Finished;

                RunningList.Remove(this);
                if (AutoPoolManger)
                    this.Release(true);
            }
            else
            {
                this.repeattimes--;
                this._state = CommandState.PrePared;
                this.Excute();
            }

        }
    }

    public virtual void Pause()
    {
        if (this._state == CommandState.Running)
        {
            this._state = CommandState.Paused;
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("********* Cmd Pause :{0}", this);
        }
    }

    public virtual void Resume()
    {
        if (this._state == CommandState.Paused)
        {
            this._state = CommandState.Running;
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("********* Cmd Resume :{0}", this);
        }
    }

}
