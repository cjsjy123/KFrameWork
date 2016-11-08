using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using Object = System.Object;

namespace KFrameWork
{
    public sealed class StaticCacheDelegate   {
        /// <summary>
        /// 减低查询的性能，避免遍历的gc
        /// </summary>
        private List<int> caches ;

        private Dictionary<int,List<Object>> listDic;

        private Dictionary<int ,Action<Object,int>> dic;

        public StaticCacheDelegate()
        {
            this.caches = new List<int>(8);
            this.dic = new Dictionary<int, Action<object, int>>(8);
            this.listDic = new Dictionary<int, List<object>>(8);
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
            int hashcode =  RuntimeHelpers.GetHashCode(t);

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
            int hashcode = RuntimeHelpers.GetHashCode(t);
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
                        List<Object> l =listDic[id];
                        for(int j=0; j <l.Count;++j)
                        {
                            dic[id](l[j],arg);
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
                list.Add(t);
                this.listDic.Add(id,list);
            }
            else
            {
                List<Object> list =this.listDic[id];
                if(!list.Contains(t))
                {
                    this.listDic[id].Add(t);
                }

            }
        }

        public void PreAdd(Action<Object,int> t)
        {
            int hashcode = RuntimeHelpers.GetHashCode(t);
            if(!this.Contains(hashcode))
            {

                this.caches.Add(hashcode);

                List<Object> list = new List<object>(8);
                this.listDic.Add(hashcode,list);

                this.TryPushtoDic(hashcode,t);
            }
        }

        public void Add(int uid,System.Object ins)
        {
            this.TryPushtoListDic(uid,ins);
        }

        public bool Remove(Action<Object,int> t ,Object ins)
        {
            int hashcode = RuntimeHelpers.GetHashCode(t);
            return this.Remove(hashcode,ins);
        }

        public bool Remove(int hashcode ,Object ins)
        {
            List<Object> list = this.Get(hashcode);
            if(list != null)
            {

                if(list.Contains(ins))
                {
                    return list.Remove(ins) ;
                }
            }
            return false;
        }
    }
}


