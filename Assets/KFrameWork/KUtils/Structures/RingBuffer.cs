using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public class RingBuffer<T> :IDisposable where T:ILife,new() {

    private T[] _buff;

    private int pos;

    private int len;

    /// <summary>
    /// 已经填充的元素个数
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
        get
        {
            return  this.len;
        }
    }

    public T this[int index]
    {
        get
        {
            if(this._buff.Length <= index)
            {
                LogMgr.LogErrorFormat("ringbuffer 读取{0}元素 越界",index);
            }

            return this._buff[index];
        }
    }

    public int GetPreviousIndex()
    {
        if(len == 0)
            return 0;

        int previous = this.pos -1;
        if(previous <0)
        {
            return len + (previous % len ); 
        }
        else
        {
            return previous;
        }
    }

    public int GetNextIndex()
    {
        if(len == 0)
            return 0;

        int next = this.pos +1;

        return next % len;
    }


    public RingBuffer():this(8)
    {
        
    }

    public RingBuffer(int size)
    {
        _buff = new T[size];

    }


    public void Push(T item)
    {
        this.len = Math.Min(this.len+1,this._buff.Length);

        int next= this.GetNextIndex();

        T old = this._buff[pos];
        if(old != null)
        {
            old.Destoryed();
        }

        this._buff[next] = item;
        this.pos++;

    }
    /// <summary>
    /// 弹出最后一个元素
    /// </summary>
    /// <returns>The back.</returns>
    public T Pop_back()
    {
        int previous = this.GetPreviousIndex();

        T data = this._buff[previous];
        if(data != null)
        {
            data.Created();
        }

        for(int i = previous; i < len;++i)//
        {
            if(i + 1 < len)
            {
                this._buff[i] = this._buff[i+1];
            }
            else
            {
                this._buff[i] = default(T);
            }
        }

        return data;
    }

    public T Pop_Front()
    {
//        int previous = this.GetPreviousIndex();
//
        T data = this._buff[pos];
        if(data != null)
        {
            data.Created();
        }

        for(int i = pos; i < len;++i)//
        {
            if(i + 1 < len)
            {
                this._buff[i] = this._buff[i+1];
            }
            else
            {
                this._buff[i] = default(T);
            }
        }

        return data;
    }

    public void Clear()
    {
        for(int i =0;i < this._buff.Length;++i)
        {
            T data = this._buff[i];
            if(data != null)
                data.Destoryed();

            this._buff[i]= default(T);
        }

        this.pos =0;
        this.len =0;
    }

    public void Dispose ()
    {
        this._buff = null;
        this.pos =0;
        this.len =0;
    }
}
