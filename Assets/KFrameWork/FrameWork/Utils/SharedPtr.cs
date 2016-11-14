using UnityEngine;
using System.Collections;
using System;
using KUtils;
using System.Collections.Generic;

namespace KFrameWork
{

    /// <summary>
    /// 引用计数对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SharedPtr<T> : ISharedPtr, IEquatable<SharedPtr<T>>,IEquatable<T>, IDisposable where T : class, IDisposable,ICloneable, new()
    {

        private T data;

        private int counter = 0;

        private int lockval = 0;

        public bool isAlive
        {
            get
            {
                return data != null;
            }
        }

        public int Count
        {
            get
            {
                return this.counter;
            }
        }

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

        public SharedPtr()
        {
            this.data = default(T);
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
                this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (counter == 0 && data != null && !this.isLock(LockType.DontDestroy))
                {

                    data.Dispose();
                    data = null;
                    lockval = 0;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public bool Equals (SharedPtr<T> other)
        {
            if(other == null)
                return false;
            if(this.data == null && other.data == null)
                return base.Equals(other);

            return this.data == other.data;
        }

        public bool Equals(T other)
        {
            if (other == null)
                return false;
            if (this.data == null && other == null)
                return false;

            return this.data == other;
        }

        public T get()
        {
            if (this.isLock(LockType.OnlyReadNoWrite))
            {
                return this.data.Clone() as T;
                //return Tools.Copy(this.data);
            }
            else
                return this.data;
        }

        public static implicit operator T(SharedPtr<T> ptr)
        {
            if (ptr != null)
            {
                return ptr.get();
            }

            return null;
        }
    }
}

