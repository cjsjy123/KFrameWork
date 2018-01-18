using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KFrameWork
{
    public class GameObjectPool<T> where T:IEquatable<T>
    {
        public string name { get; private set; }

        private SimpleDictionary<T, List<GameObject>> pool = new SimpleDictionary<T, List<GameObject>>();

        GameObject objpool;

        Func<T, GameObject> CreateFunc;

        private GameObjectPool()
        {

        }

        public GameObjectPool(string poolname,Func<T,GameObject> func)
        {
            name = poolname;
            CreateFunc = func;
            CreatePool();
        }

        private void CreatePool()
        {
            objpool = new GameObject(name);
            objpool.SetActive(false);
        }

        public void Clear()
        {
            objpool.RemoveChildren();
            pool.Clear();
        }

        public GameObject Spawn(T key)
        {
            if(pool.ContainsKey(key))
            {
                List<GameObject> list = pool[key];
                if(list.Count ==0)
                {
                    return this.CreateFunc(key);
                }
                GameObject obj = list[0];
                list.RemoveAt(0);

                obj.transform.parent = null;
                return obj;
            }

            return this.CreateFunc(key);
        }

        public bool DeSpawn(T key, GameObject obj)
        {
            if(obj != null)
            {
                objpool.AddInstance(obj);

                if (pool.ContainsKey(key))
                {
                    pool[key].Add(obj);
                }
                else
                {
                    List<GameObject> list = new List<GameObject>();
                    list.Add(obj);

                    pool[key] = list;
                }

                return true;
            }

            return false;
        }
    }
}


