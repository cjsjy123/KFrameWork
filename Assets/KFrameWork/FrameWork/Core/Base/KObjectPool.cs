using System;
using System.Collections;
using System.Collections.Generic;
using KFrameWork;

namespace KUtils
{
    /// <summary>
    /// 对象池接口，尽量不要在外部持有ipool对象的引用，有的话 请手动保证对象在被清楚的时候对象
    /// </summary>
    public interface IPool
    {
        void AwakeFromPool();
        void RemovedFromPool();
    }

    /// <summary>
    /// 引用类型对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 
    [SingleTon]
    public sealed class KObjectPool
    {
        public static KObjectPool mIns;

        public const int EachRemoveCount = 5;

        public const float RmovedDelta = 10f;

        private Dictionary<Type, List<IPool>> queue = new Dictionary<Type, List<IPool>>(16);

        private Dictionary<Type,List<float> >  deltalist = new Dictionary<Type,List<float>>(16);


        public void Push<T>(T data) where T:IPool
        {
            Type tp = typeof(T);
            if(!this.queue.ContainsKey(tp))
            {
                this.queue.Add(tp, new List<IPool>(8));
                this.deltalist.Add(tp,new List<float>(8));
            }

            this.queue[tp].Add(data);
            this.deltalist[tp].Add(UnityEngine.Time.realtimeSinceStartup);
        }

        [ScenLeave]
        public  static void SceneRemoveUnUsed(int level)
        {
            if(KObjectPool.mIns != null)
            {
                KObjectPool.mIns.RemoveSomeOlded();
            }
        }

        private void RemoveSomeOlded()
        {
            float now = UnityEngine.Time.realtimeSinceStartup;
            //in unity5.4  fixed foreach bug
            int removedCount = 0;

            foreach (var kv in this.queue)
            {
                List<float>  list = this.deltalist[kv.Key];
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (now - list[i] > RmovedDelta)
                    {
                        list.RemoveAt(i);
                        kv.Value[i].RemovedFromPool();
                        kv.Value.RemoveAt(i);
                        removedCount++;

                        if (removedCount >= EachRemoveCount)
                            break;
                    }
                }
            }
 
        }

        public T Seek<T,U>(Func<T,U,int> seekFunc,U userdata,int seekTimes =-1) where T:IPool
        {
            Type tp = typeof(T);
            if (!this.queue.ContainsKey(tp))
            {
                this.queue.Add(tp, new List<IPool>(8));
                this.deltalist.Add(tp, new List<float>(8));
            }

            List<IPool> list = this.queue[tp];

            if (list.Count == 0)
            {
                return default(T);
            }
            else
            {
                T nearst = default(T);
                int? worthValue = null;
                for(int i =0; i < list.Count;++i )
                {
                    if(seekTimes == 0)
                    {
                        break;
                    }
                    else
                    {
                        T data =(T)list[i];
                        int currentworth =Math.Abs(seekFunc(data,userdata));
                        if(worthValue.HasValue)
                        {
                            if(currentworth == 0)
                            {
                                nearst = data;
                                break;
                            }
                            else if(worthValue > currentworth)
                            {
                                worthValue  = currentworth;
                                nearst =data;
                            }

                        }else
                        {
                            worthValue = currentworth;
                            nearst = data;
                        }
                        seekTimes--;
                    }
                }

                list.Remove(nearst);
                return nearst;
            }
            
           

        }

        public void Clear()
        {
            this.queue.Clear();
            this.deltalist.Clear();
        }

        /// <summary>
        /// Pop this instance.(不使用New（） 因为这个会调用反射的Activator.CreateInstance 产生GC)
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Pop<T>() where T:IPool
        {
            Type tp = typeof(T);
            if (!this.queue.ContainsKey(tp))
            {
                this.queue.Add(tp, new List<IPool>(8));
                this.deltalist.Add(tp, new List<float>(8));
            }

           List<IPool> list = this.queue[tp];

           if (list.Count == 0)
            {
                return default(T);
            }
            else
            {
                IPool first = list[0];
                list.RemoveAt(0);
                this.deltalist[tp].RemoveAt(0);
                first.AwakeFromPool();
                return (T)first;
            }
        }

    }
}

