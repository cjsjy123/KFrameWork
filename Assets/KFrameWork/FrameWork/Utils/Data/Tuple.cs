using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = System.Object;

namespace KUtils
{

    public struct KVTuple<T, U> : IEquatable<KVTuple<T, U>>
    {

        public T Key;

        public U Value;

        public KVTuple(T t, U u)
        {
            this.Key = t;
            this.Value = u;
        }


        /// <summary>
        /// only check Key
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(KVTuple<T, U> other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            if (!other.Key.Equals(this.Key)) return false;
            if (!other.Value.Equals(this.Value)) return false;

            return true;
        }
    }
    [Serializable]
    public class ClsTuple<T, U> : IEquatable<ClsTuple<T, U>>
    {

        public T Key;

        public U Value;

        public ClsTuple(T t, U k)
        {
            Key = t;
            Value = k;
        }

        /// <summary>
        /// only check Key
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ClsTuple<T, U> other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            if (!other.Key.Equals(this.Key)) return false;
            if (!other.Value.Equals(this.Value)) return false;

            return true;
        }
    }


    public struct Tuple<T, Y, K> : IEquatable<Tuple<T, Y, K>>
    {
        public T k1;
        public Y k2;
        public K k3;

        public Tuple(T t, Y y, K k)
        {
            this.k1 = t;
            this.k2 = y;
            this.k3 = k;
        }

        public bool Equals(Tuple<T, Y, K> other)
        {
            if (!k1.Equals(other.k1)) return false;
            if (!k2.Equals(other.k2)) return false;
            if (!k3.Equals(other.k3)) return false;
            return true;

        }
    }

    public struct Tuple<T, Y, K, U> : IEquatable<Tuple<T, Y, K, U>>
    {
        public T k1;
        public Y k2;
        public K k3;
        public U k4;

        public Tuple(T t, Y y, K k, U u)
        {
            this.k1 = t;
            this.k2 = y;
            this.k3 = k;
            this.k4 = u;
        }

        public bool Equals(Tuple<T, Y, K, U> other)
        {
            if (!k1.Equals(other.k1)) return false;
            if (!k2.Equals(other.k2)) return false;
            if (!k3.Equals(other.k3)) return false;
            if (!k4.Equals(other.k4)) return false;
            return true;

        }
    }

    [Serializable]
    public class ObjTuple : IEquatable<ObjTuple>
    {
        [SerializeField]
        private List<Object> args = new List<Object>();

        public int Count
        {
            get
            {
                return args.Count;
            }
        }

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

        public void Add(object o)
        {
            if (args.Contains(o) == false)
                args.Add(o);
        }


        public void Remove(object o)
        {
            args.Remove(o);
        }

        public void Clear()
        {
            args.Clear();
        }

        public bool Equals(ObjTuple other)
        {
            return this.args.Equals(other.args);
        }
    }
}

