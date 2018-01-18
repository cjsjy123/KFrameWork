using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using Object = System.Object;
using KFrameWork;
using KUtils;
using Priority_Queue;
using System.Reflection;

namespace KFrameWork
{
    /// <summary>
    /// 实例对象的代理缓存
    /// </summary>
    public sealed class InstanceCacheDelegate   {
        /// <summary>
        /// 减低查询的性能，避免遍历的gc
        /// </summary>
        private List<int> caches ;

        private Dictionary<int,List<Object>> callbackListMap;

        private Dictionary<int ,Action<Object,int>> callbackMap;

        private Queue<KeyValuePair<int, Object>> loadQueue ;

        public InstanceCacheDelegate()
        {
            this.caches = new List<int>(8);
            this.callbackMap = new Dictionary<int, Action<object, int>>(8);
            this.callbackListMap = new Dictionary<int, List<object>>(8);
            this.loadQueue = new Queue<KeyValuePair<int, Object>>();
        }

        public void Dump(MainLoopEvent mainloopevent)
        {
#if UNITY_EDITOR
            var en = this.callbackMap.GetEnumerator();
            while (en.MoveNext())
            {
                Action<object, int> act = en.Current.Value ;
                if (act != null && callbackListMap[en.Current.Key].Count >0)
                {
                    System.Delegate d = act as System.Delegate;
                    Delegate[] delegates = d.GetInvocationList();
                    for (int i = 0; i < delegates.Length; ++i)
                    {
                        LogMgr.LogWarningFormat(" InstanceCacheDelegate {0} in {1} not clear at {2}", delegates[i].Method.Name, delegates[i].Method.DeclaringType, mainloopevent);
                    }
                }
            }

            var listen = this.callbackListMap.GetEnumerator();
            while (listen.MoveNext())
            {
                List<object> list = listen.Current.Value ;
                if (list != null && list.Count >0)
                {
                    for (int i = 0; i < list.Count; ++i)
                    {
                        object o = list[i];
                        LogMgr.LogWarningFormat("InstanceCacheDelegate {0} not clear at {1}", o, mainloopevent);
                    }
                }
            }
#endif
        }

        public bool Contains(int hashcode)
        {
            if(hashcode ==0)
            {
                LogMgr.Log("这是一个空对象");
                return false;
            }
            return this.caches.Contains(hashcode);
        }

        public bool Contains(Action<Object,int> t)
        {
            int hashcode = FrameWorkTools.GetHashCode(t);

            return this.Contains(hashcode);
        }

        public List<Object> Get(int hashcode)
        {
            if(callbackListMap.ContainsKey(hashcode))//logn
            {
                return callbackListMap[hashcode];
            }
            return null;
        }

        public List<Object> Get(Action<Object,int> t)
        {
            int hashcode = FrameWorkTools.GetHashCode(t);
            return this.Get(hashcode);
        }


        public void Invoke(int arg)
        {
            for(int i=0;i < this.caches.Count;++i)
            {
                int idx = this.caches[i];
                if(idx !=0 && this.callbackListMap.ContainsKey(idx)  )
                {
                    if(!this.callbackMap.ContainsKey(idx))
                    {
                        LogMgr.Log("缓存中不存在此回调");
                    }
                    else
                    {
                        List<Object> elements =ListPool.TrySpawn<List<Object>>();
                        elements.AddRange(callbackListMap[idx]);
     
                        for(int j= elements.Count -1 ; j>=0;--j)
                        {
                            callbackMap[idx](elements[j], arg);
                        }

                        ListPool.TryDespawn(elements);

                        while (loadQueue.Count > 0)
                        {
                            var cell = loadQueue.Dequeue();
                            callbackListMap[cell.Key].Insert(0,cell.Value);
                        }
                    }
                }
            }
        }

        private void TryPushtoDic(int id,Action<Object,int> t)
        {
            if(!this.callbackMap.ContainsKey(id))
            {
                this.callbackMap.Add(id,t);
            }
        }

        private void TryPushtoListDic(int id,Object t)
        {
            if(!this.callbackListMap.ContainsKey(id))
            {
                List<Object> list = new List<object>(8);
                this.callbackListMap.Add(id,list);
                loadQueue.Enqueue(new KeyValuePair<int, object>(id, t));
            }
            else
            {
                List<Object> list =this.callbackListMap[id];
                if(!list.Contains(t))
                {
                    loadQueue.Enqueue(new KeyValuePair<int, object>(id, t));
                }

            }
        }

        public void PreAdd(Action<Object,int> t)
        {
            int hashcode = FrameWorkTools.GetHashCode(t);
            if(!this.Contains(hashcode))
            {
                this.caches.Add(hashcode);

                List<Object> list = new List<object>(8);
                this.callbackListMap.Add(hashcode,list);

                this.callbackMap[hashcode] =t;
                //this.TryPushtoDic(hashcode,t);
            }
        }

        public void Add(int uid,System.Object ins)
        {
            this.TryPushtoListDic(uid,ins);
        }

        public bool Remove(Action<Object,int> t ,Object ins)
        {
            int hashcode = FrameWorkTools.GetHashCode(t);
            return this.Remove(hashcode,ins);
        }

        public bool Remove(int hashcode)
        {
            List<Object> list = this.Get(hashcode);
            if(list != null)
                list.Clear();
            return false;
        }

        public bool Remove(int hashcode ,Object ins)
        {
            List<Object> list = this.Get(hashcode);
            if(list != null)
            {
                return list.Remove(ins);
            }
            return false;
        }
    }

    #region StaticDelegate

    public sealed class StaticDelegate
    {
        /// <summary>
        /// key = methodid value is delegate
        /// </summary>
        private List<KeyValuePair< int, Action<int>>> cachesMap = new List<KeyValuePair< int, Action<int>>>();

        public int Count
        {
            get
            {
               return this.cachesMap.Count;
            }
        }

        public void Dump(MainLoopEvent mainloopevent)
        {
#if UNITY_EDITOR
            var en = this.cachesMap.GetEnumerator();
            while (en.MoveNext())
            {
                Action<int> act = en.Current.Value;
                if (act != null)
                {
                    System.Delegate d = act as System.Delegate;
                    Delegate[] delegates = d.GetInvocationList();
                    for (int i = 0; i < delegates.Length; ++i)
                    {
                        LogMgr.LogWarningFormat(" StaticDelegate {0} in {1} not clear at:{2}", delegates[i].Method.Name, delegates[i].Method.DeclaringType, mainloopevent);
                    }
                }
            }
#endif
        }

        public bool Contains(Action<int> Action)
        {
            for(int i =0; i < this.cachesMap.Count;++i)
            {
                KeyValuePair<int,Action<int>> kv = this.cachesMap[i];
                if(kv.Value == Action)
                    return true;
            }
            return false;
        }

        public void Add(Action<int> act,int priority =0)
        {
            if (!Contains(act))
            {
                bool insert =false;
                for(int i =0; i < this.cachesMap.Count;++i)
                {
                    KeyValuePair<int,Action<int>> kv = this.cachesMap[i];
     
                    if(kv.Key > priority)
                    {
                        this.cachesMap.Insert(i,new KeyValuePair<int, Action<int>>(priority,act));
                        insert =true;
                        break;
                    }
                }

                if(!insert)
                    this.cachesMap.Add(new KeyValuePair<int, Action<int>>(priority,act));
            }
        }

        public void Invoke(int value)
        {
            if (this.cachesMap.Count > 0)
            {
                for (int i = 0; i < this.cachesMap.Count; ++i)
                {
                    cachesMap[i].Value(value);
                }
            }
        }


        public void Rmove(Action<int> act)
        {
            for(int i =0; i < this.cachesMap.Count;++i)
            {
                KeyValuePair<int,Action<int>> kv = this.cachesMap[i];
                if(kv.Value == act)
                {
                    this.cachesMap.RemoveAt(i);
                    return;
                }
            }

        }
    }

    #endregion
}


