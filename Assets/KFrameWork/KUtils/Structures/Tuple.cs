using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = System.Object;

namespace KUtils
{

    public struct KVTuple<T, U> : IEquatable<KVTuple<T, U>>, IPool
    {

        public T Key;

        public U Value;

//        public KVTuple()
//        {
//            Key =default(T);
//            Value = default(U);
//        }
//
//        public KVTuple(T t,U u)
//        {
//            this.Key=t;
//            this.Value =u;
//        }


        public void AwakeFromPool() { 

        }

        public void ReleaseToPool()
        {
            if (KObjectPool.mIns != null)
            {
                KObjectPool.mIns.Push(this);
            }
            else
            {
                LogMgr.Log("对象池未初始化");
            }
            
        }

        public void RemovedFromPool()
        {
            this.Key=default(T);
            this.Value =default(U);
 
        }

        /// <summary>
        /// only check Key
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(KVTuple<T, U> other)
        {
            if(object.ReferenceEquals(other,null)) return false;
            if(!other.Key.Equals(this.Key)) return false;
            if(!other.Value.Equals(this.Value)) return false;

            return true;
        }
    }

    public class Tuple <T,Y>: IEquatable<Tuple<T,Y>>
    {
        public T k1;
        public Y k2;
        public bool Equals(Tuple<T, Y> other)
        {
            if (!k1.Equals(other.k1)) return false;
            if (!k2.Equals(other.k2)) return false;
            return true;
        }
    }

    public class Tuple<T, Y,K> : IEquatable<Tuple<T, Y,K>>
    {
        public T k1;
        public Y k2;
        public K k3;
        public bool Equals(Tuple<T, Y, K> other)
        {
            if (!k1.Equals(other.k1)) return false;
            if (!k2.Equals(other.k2)) return false;
            if (!k3.Equals(other.k3)) return false;
            return true;

        }
    }

    public class ObjTuple : IEquatable<ObjTuple>
    {
        private List<Object> args = new List<Object>();

        public object this[int index]
        {
            get
            {
                if (index < 0 || index > args.Count - 1)
                    throw new IndexOutOfRangeException();

                return this.args[index];
 
            }
            set
            {
                if (index < 0)
                    throw new IndexOutOfRangeException();

                if (index > args.Count - 1)
                {
                    args.Add(value);
                }
                else
                {
                    args[index] = value;
                }
 
            }
        }

        public bool Equals(ObjTuple other)
        {
            return this.args.Equals(other.args);
        }
    }
}

