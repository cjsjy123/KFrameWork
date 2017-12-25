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

        private Dictionary<int,List<Object>> listDic;

        private Dictionary<int ,Action<Object,int>> dic;

        private Queue<KeyValuePair<int, Object>> loadQueue ;

        public InstanceCacheDelegate()
        {
            this.caches = new List<int>(8);
            this.dic = new Dictionary<int, Action<object, int>>(8);
            this.listDic = new Dictionary<int, List<object>>(8);
            this.loadQueue = new Queue<KeyValuePair<int, Object>>();
        }

        public void Dump(MainLoopEvent mainloopevent)
        {
#if UNITY_EDITOR
            var en = this.dic.GetEnumerator();
            while (en.MoveNext())
            {
                Action<object, int> act = en.Current.Value ;
                if (act != null && listDic[en.Current.Key].Count >0)
                {
                    System.Delegate d = act as System.Delegate;
                    Delegate[] delegates = d.GetInvocationList();
                    for (int i = 0; i < delegates.Length; ++i)
                    {
                        LogMgr.LogWarningFormat(" InstanceCacheDelegate {0} in {1} not clear at {2}", delegates[i].Method.Name, delegates[i].Method.DeclaringType, mainloopevent);
                    }
                }
            }

            var listen = this.listDic.GetEnumerator();
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
            if(listDic.ContainsKey(hashcode))//logn
            {
                return listDic[hashcode];
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
                int id = this.caches[i];
                if(id !=0 && this.listDic.ContainsKey(id)  )
                {
                    if(!this.dic.ContainsKey(id))
                    {
                        LogMgr.Log("缓存中不存在此回调");
                    }
                    else
                    {
                        List<Object> list =listDic[id];
                        for(int j= list.Count -1 ; j>=0;--j)
                        {
                            dic[id](list[j], arg);
                        }

                        while (loadQueue.Count > 0)
                        {
                            var cell = loadQueue.Dequeue();
                            listDic[cell.Key].Insert(0,cell.Value);
                        }
                    }
                }
            }
        }

        private void TryPushtoDic(int id,Action<Object,int> t)
        {
            if(!this.dic.ContainsKey(id))
            {
                this.dic.Add(id,t);
            }
        }

        private void TryPushtoListDic(int id,Object t)
        {
            if(!this.listDic.ContainsKey(id))
            {
                List<Object> list = new List<object>(8);
                this.listDic.Add(id,list);
                loadQueue.Enqueue(new KeyValuePair<int, object>(id, t));
            }
            else
            {
                List<Object> list =this.listDic[id];
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
                this.listDic.Add(hashcode,list);

                this.dic[hashcode] =t;
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
        private List<KeyValuePair< int, Action<int>>> cachesDic = new List<KeyValuePair< int, Action<int>>>();

        public int Count
        {
            get
            {
               return this.cachesDic.Count;
            }
        }

        public void Dump(MainLoopEvent mainloopevent)
        {
#if UNITY_EDITOR
            var en = this.cachesDic.GetEnumerator();
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
            for(int i =0; i < this.cachesDic.Count;++i)
            {
                KeyValuePair<int,Action<int>> kv = this.cachesDic[i];
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
                for(int i =0; i < this.cachesDic.Count;++i)
                {
                    KeyValuePair<int,Action<int>> kv = this.cachesDic[i];
     
                    if(kv.Key > priority)
                    {
                        this.cachesDic.Insert(i,new KeyValuePair<int, Action<int>>(priority,act));
                        insert =true;
                        break;
                    }
                }

                if(!insert)
                    this.cachesDic.Add(new KeyValuePair<int, Action<int>>(priority,act));
            }
        }

        public void Invoke(int value)
        {
            if (this.cachesDic.Count > 0)
            {
                for (int i = 0; i < this.cachesDic.Count; ++i)
                {
                    KeyValuePair<int,Action<int>> kv = this.cachesDic[i];
  
                    kv.Value(value);
                }
            }
        }


        public void Rmove(Action<int> act)
        {
            for(int i =0; i < this.cachesDic.Count;++i)
            {
                KeyValuePair<int,Action<int>> kv = this.cachesDic[i];
                if(kv.Value == act)
                {
                    this.cachesDic.RemoveAt(i);
                    return;
                }
            }

        }
    }

    #endregion
}


