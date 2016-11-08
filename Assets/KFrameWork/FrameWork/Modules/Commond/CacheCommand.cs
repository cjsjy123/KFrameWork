using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public abstract class CacheCommand:ICommand
{
    protected static Dictionary<int,Queue<ICommand>> CMDCache ;

    public abstract void Release (bool force);

    public abstract void Excute ();

    protected int? _CMD;

    public int? CMD
    {
        get
        {

            return _CMD;
        }
    }

    private static int Counter =0;

    private int m_UID;

    public int UID
    {
        get{
            return this.m_UID;
        }
    }

    protected AbstractParams _Gparams ;

    public AbstractParams CallParms
    {
        get
        {
            if(_Gparams == null)
                _Gparams = GenericParams.Create();
            return _Gparams;
        }
    }

    public bool HasCallParams
    {
        get
        {
            return _Gparams != null;
        }
    }

    private ICommand _Next;
    public ICommand Next
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

    protected AbstractParams _RParams;

    /// <summary>
    /// 当有返回值得时候用户请自行dispose
    /// </summary>
    /// <value>The return parameters.</value>
    public AbstractParams ReturnParams
    {
        get
        {
            return _RParams;
        }
        set
        {
            _RParams = value;
        }
    }
        

    protected void GenID()
    {
        this.m_UID = Counter++;
    }

    public abstract void Stop();
    public abstract void Pause();
    public abstract void Resume();
}
