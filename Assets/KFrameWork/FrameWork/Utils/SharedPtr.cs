using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace KFrameWork
{

    /// <summary>
    /// 引用计数对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SharedPtr<T> :ISharedPtr,IDisposable where T:class,IDisposable,new()
    {

        private T data;

        private int counter = 0;

        private int lockval = 0;

        /// <summary>
        /// 是否已经被锁
        /// </summary>
        public bool Locked
        {
            get
            {
                return lockval != 0;
            }
        }

        public SharedPtr(T target)
        {
            data = target;
        }

        public void Lock(LockType locktp)
        {
            int startval = (int)LockType.OnlyReadNoWrite;
            int endval =(int)LockType.END;
            for (int i = startval; i < endval; ++i)
            {
                if (this.isLock(i))
                {
                    this.lockval = this.lockval | i;
                }
            }
        }

        public void UnLock(LockType locktp)
        {
            int startval = (int)LockType.OnlyReadNoWrite;
            int endval = (int)LockType.END;
            for (int i = startval; i < endval; ++i)
            {
                if (this.isLock(i))
                {
                    this.lockval -= i;
                }
            }
        }

        public bool isLock(LockType locktp)
        {
            int tpval = (int)locktp;
            return (lockval & tpval) == tpval;
        }

        public bool isLock(int locktp)
        {
            return (lockval & locktp) == locktp;
        }

        public static SharedPtr<T> Create()
        {
            return new SharedPtr<T>(new T());
        }

        public void AddRef()
        {
            counter++;
        }

        public void RemoveRef()
        {
            counter--;

            if (counter == 0)
                this.Dispose();
        }

        public void Dispose()
        {
            if (counter == 0 && data != null && !this.isLock(LockType.DontDestroy))
            {

                data.Dispose();
                data = null;
                lockval = 0;
            }
        }

        //public static explicit operator T(SharedPtr<T> ptr)
        //{
        //    if (ptr != null)
        //    {
        //        if (ptr.isLock(LockType.OnlyReadNoWrite))
        //        {
        //            return KUtils.Tools.Copy(ptr.data);
        //        }
        //        else
        //            return ptr.data;
        //    }

        //    return null;
        //}

        public static implicit operator T(SharedPtr<T> ptr)
        {
            if (ptr != null)
            {
                if (ptr.isLock(LockType.OnlyReadNoWrite))
                {

                    return KUtils.Tools.Copy(ptr.data);
                }
                else
                    return ptr.data;
            }

            return null;
        }
    }
}

