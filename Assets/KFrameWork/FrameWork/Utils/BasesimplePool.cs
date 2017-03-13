using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using UnityEngine;

namespace KFrameWork
{
    public class BasesimplePool
    {
        protected static List<System.Object> caches = new List<System.Object>();

        public static void TryDespawn(object o)
        {
            if (!caches.Contains(o))
            {
                caches.Add(o);
            }
            else if (FrameWorkConfig.Open_DEBUG)
            {
                LogMgr.LogErrorFormat("重复入池:{0}",o);
            }
        }

        protected static T Spawn<T>() 
        {
            for (int i = 0; i < caches.Count; ++i)
            {
                object o = caches[i];
                if (o is T)
                {
                    T t = (T)o;
                    caches.RemoveAt(i);
                    return t;
                }
            }
            return default(T);
        }

        public static void ClearPool()
        {
            for (int i = 0; i < caches.Count; ++i)
            {
                object o = caches[i];
                if (o is IPool)
                {
                    IPool t = (IPool)o;
                    t.RemovedFromPool();
                }
            }
            caches.Clear();
            caches = null;
        }

    }

    public class ListPool 
    {

        public static T TrySpawn<T>() where T : IList, ICollection, IEnumerable, new()
        {
            T t = KObjectPool.mIns.Pop<T>();
            if (t == null)
                t = new T();
            t.Clear();

            return t;
        }

        public static void TryDespawn(object o)
        {
            KObjectPool.mIns.Push(o);
        }
    }

    public class ArrayPool:BasesimplePool
    {

        public static T[] TrySpawn<T>(int size) 
        {
            T[] t = Spawn<T>(size);
            if(t == null)
                t = new T[size];

            return t;
        }

        public static T[,] TrySpawnSecondArray<T>(int wid,int height) 
        {
            T[,] t = Spawn<T>(wid,height);
            if(t == null)
                t = new T[wid,height];

            return t;
        }

        protected static T[,] Spawn<T>(int wid,int height) 
        {
            T[,] outvalue = null;
            for (int i = 0; i < caches.Count; ++i)
            {
                object o = caches[i];
                if (o is T[,] )
                {
                    T[,] t = (T[,])o;
                    int w = t.GetLength(0);
                    int h = t.GetLength(1);
                    if(w == wid && h == height)
                    {
                        caches.RemoveAt(i);
                        return t;
                    }
                    else if(w >= wid && h >= height)
                    {
                        outvalue =t;
                    }
                }
            }
            if(outvalue != null)
                caches.Remove(outvalue);

            return outvalue;
        }

        protected static T[] Spawn<T>(int size) 
        {
            T[] outvalue = null;
            for (int i = 0; i < caches.Count; ++i)
            {
                object o = caches[i];
                if (o is T[] )
                {
                    T[] t = (T[])o;
                    if(t.Length == size)
                    {
                        caches.RemoveAt(i);
                        return t;
                    }
                    else if(t.Length > size)
                    {
                        outvalue =t;
                    }
                }
            }
            if (outvalue != null)
                caches.Remove(outvalue);

            return outvalue;
        }
    }

    public class DictionaryPool 
    {

        public static T TrySpawn<T>() where T : IDictionary, ICollection, IEnumerable, new()
        {
            T t = KObjectPool.mIns.Pop<T>();
            if (t == null)
                t = new T();
            t.Clear();
            return t;
        }

        public static void TryDespawn(object o)
        {
            KObjectPool.mIns.Push(o);
        }
    }

    public class ShaderPool
    {
        private static Dictionary<string, Shader> caches = new Dictionary<string, Shader>();

        public static Shader TrySpawn(string Key)
        {
            if (caches.ContainsKey(Key))
            {
                return caches[Key];
            }

            Shader shader = Shader.Find(Key);
            if (shader != null && shader.isSupported)
            {
                caches.Add(Key, shader);
                return shader;
            }

            return null;
        }
    }

    public class MaterialPool 
    {
        public static Material TrySpawn(Shader shader)  
        {
            Material t = KObjectPool.mIns.Pop<Material>();
            if (t == null)
                t = new Material(shader);
            else
                t.shader = shader;
            return t;
        }

        public static Material TrySpawn(Material old)
        {
            Material t = KObjectPool.mIns.Pop<Material>();
            if (t == null)
                t = new Material(old);
            else
                t.CopyPropertiesFromMaterial(old);
            return t;
        }

        public static void TryDespawn(object o)
        {
            KObjectPool.mIns.Push(o);
        }
    }

}

