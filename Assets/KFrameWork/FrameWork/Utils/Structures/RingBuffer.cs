using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public class RingBuffer<T> :IDisposable  {

    private T[] _buff;

    private int pos =-1;

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

    public RingBuffer():this(8)
    {
        
    }

    public RingBuffer(int size)
    {
        _buff = new T[size];
        //this.len = size;
    }

    public int GetPreviousIndex(int position)
    {
        if (len == 0)
            return -1;

        int previous = (position % len) - 1;
        if (previous < 0)
        {
            return len - (Mathf.Abs(previous) % (len + 1));
        }
        else
        {
            return previous;
        }
    }

    public int GetNextIndex(int position)
    {
        if (len == 0)
            return -1;

        int next = position + 1;

        return next % len;
    }

    public T SeekFromByNewOrder(Func<T, bool> func)
    {
        int cnt = this.len;
        int position = this.pos;
        while (cnt > 0)
        {
            int Next = this.GetPreviousIndex(position);
            //if (Next == -1)
            //    break;

            T data = this[Next];
            if (func(data))
            {
                return data;//oldest
            }
            position--;
            cnt--;
        }
        return default(T);
    }

    public T SeekFromByOldOrder(Func<T,bool> func)
    {
        int cnt = this.len;
        while (cnt > 0)
        {
            int Next = (this.pos + 1) % len;
            T data = this[Next];
            if (func(data))
            {
                return data;//older
            }

            cnt--;
        }
        return default(T);
    }

    public T getNext()
    {
        int next = this.GetNextIndex(this.pos);
        if (next != -1)
        {
            return this[next];
        }
        return default(T);
    }

    public void Push(T item)
    {
        this.len = Math.Min(this.len+1,this._buff.Length);

        int next= this.GetNextIndex(this.pos);
        if (next != -1)
        {
            //T old = this._buff[pos];
            //if(old != null)
            //{
            //    old.RemovedFromPool();
            //}
            this._buff[next] = item;
            this.pos++;
        }
    }


    public void Clear()
    {
        for(int i =0;i < this._buff.Length;++i)
        {
            //T data = this._buff[i];
            //if(data != null)
            //    data.RemovedFromPool();

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
