using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = System.Object;

namespace KFrameWork
{
    public class StaticCacheDelegate   {
        /// <summary>
        /// 减低查询的性能，避免遍历的gc
        /// </summary>
        private List<KeyValuePair<Action<Object,int>,List<Object>>> caches = new List<KeyValuePair<Action<Object,int>,List<Object>>>(8);

        public StaticCacheDelegate()
        {
           
        }

        public bool Contains(Action<Object,int> t)
        {
            for(int i=0;i < this.caches.Count;++i)
            {
                if(this.caches[i].Key == t)
                {
                    return true;
                }
            }
            return false;
        }

        public List<Object> Get(Action<Object,int> t)
        {
            for(int i=0;i < this.caches.Count;++i)
            {
                if(this.caches[i].Key == t)
                {
                    return this.caches[i].Value;
                }
            }
            return null;
        }

        public void Invoke(int arg)
        {
            for(int i=0;i < this.caches.Count;++i)
            {
                KeyValuePair<Action<Object,int>,List<Object>> k = this.caches[i];
                if(k.Key != null && k.Value.Count >0)
                {
                    for(int j=0; j < k.Value.Count;++j)
                    {
                        k.Key(k.Value[j],arg);
                    }
                }
            }
        }

        public void PreAdd(Action<Object,int> t)
        {
            if(!this.Contains(t))
            {
                List<Object> dic = new List<Object>(8);

                this.caches.Add(new KeyValuePair<Action<object, int>, List<object>>(t,dic));
            }
        }

        public void Add(Action<Object,int> t,System.Object ins)
        {
            List<Object> list = this.Get(t);
            if(list  != null)
            {

                if(!list.Contains(ins))//log n
                {
                    list.Add(ins);
                }

            }
            else
            {
                list= new List<Object>(8);
                list.Add(ins);
                this.caches.Add(new KeyValuePair<Action<object, int>, List<object>>(t,list));
            }
        }

        public bool Remove(Action<Object,int> t ,Object ins)
        {
            List<Object> list = this.Get(t);
            if(list != null)
            {

                if(list.Contains(ins))
                {
                    return list.Remove(ins);
                }
            }
            return false;
        }
    }
}


