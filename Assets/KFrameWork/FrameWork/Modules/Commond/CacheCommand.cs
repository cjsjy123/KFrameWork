using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public abstract class CacheCommand
{
    protected static Dictionary<int,Queue<CacheCommand>> CMDCache ;

    public abstract void Release (bool force);

    public abstract void Excute ();

    private static int Counter = 0;

    protected CommandState _state;

    public CommandState RunningState
    {
        get
        {
            return this._state;
        }
    }

    protected bool m_paused =false;

    public bool Paused
    {
        get
        {
            return this.m_paused;
        }

        set
        {
            if(value)
            {
                this._state = CommandState.Paused;
                this.Pause();
            }
            else
            {
                this._state = CommandState.Running;
                this.Resume();
            }
            this.m_paused =value;
        }
    }


    private int m_UID;

    public int UID
    {
        get{
            return this.m_UID;
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

    protected bool _isDone;
    /// <summary>
    /// 命令是否完成，请确保当完成的时候，其被移除掉，不占用引用
    /// </summary>
    /// <value><c>true</c> if is done; otherwise, <c>false</c>.</value>
    public bool isDone
    {
        get
        {
            return this._isDone;
        }
    }


        

    protected void GenID()
    {
        this.m_UID = Counter++;
    }

    public virtual void Stop()
    {
        if (this._state == CommandState.Running)
        {
            this._state = CommandState.Stoped;
        }
    }

    public virtual void Pause()
    {
        if (this._state == CommandState.Running)
        {
            this._state = CommandState.Paused;
        }
    }

    public virtual void Resume()
    {
        if (this._state == CommandState.Paused)
        {
            this._state = CommandState.Running;
        }
    }

    protected virtual void Reset()
    {
        this.m_paused = false;
        this._isDone = false;
        this._state = CommandState.PrePared;
    }


    public void ExcuteAndRelease()
    {
        this.Excute();
        this.Release(false);
    }
}
