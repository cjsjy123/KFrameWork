
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    /// <summary>
    /// 主要目的是低gc的遍历功能提供 Simple dictionary.Key可以是struct 如果是class则用clsDictionary ENABLE_O1 提高查询效率,并且查询的时候不会调用equal而是调用gethashcode
    /// </summary>
    [Serializable]
    public class SimpleDictionary<K, V> : ISerializationCallbackReceiver where K:IEquatable<K>
    {
        [SerializeField]
        private List<K> keys;
        [SerializeField]
        private List<V> values;
        [NonSerialized]
        private Dictionary<K, V> dic  ;

        [NonSerialized]
        public readonly bool enable_O1 = false;
        
        public V this[K key]
        {
            get
            {
                try
                {
                    if (enable_O1)
                    {
                        V outvalue =default(V);
                        if (dic.TryGetValue(key,out outvalue))
                        {
                            return outvalue;
                        }

                        return outvalue;
                    }

                    int index = this.keys.IndexOf(key);
                    if (index == -1)
                        return default(V);
                    return this.values[index];
                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                    return default(V);
                }

            }
            set
            {
                try
                {
                    if (enable_O1)
                        dic[key] = value;

                    int index = this.keys.IndexOf(key);
                    if (index == -1)
                    {
                        this.values.Add(value);
                        this.keys.Add(key);
                    }
                    else
                    {
                        this.values[index] = value;
                    }
                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                }

            }
        }

        public int Count
        {
            get
            {
                return this.keys.Count;
            }
        }
        /// <summary>
        /// low gc 获取的是原始对象
        /// </summary>
        public List<K> ReadOnlyKeys
        {
            get
            {
                return keys;
            }
        }
        /// <summary>
        /// low gc 获取的是原始对象
        /// </summary>
        public List<V> ReadOnlyValues
        {
            get
            {
                return values;
            }
        }

        public List<K> Keys
        {
            get
            {
                List<K> list = ListPool.TrySpawn<List<K>>();
                list.AddRange(this.keys);
                return list;
            }
        }

        public List<V> Values
        {
            get
            {
                List<V> list = ListPool.TrySpawn<List<V>>();
                list.AddRange(this.values);
                return list;
            }
        }

        public SimpleDictionary():this(0)
        {

        }

        public SimpleDictionary(bool enable) : this(0)
        {
            this.enable_O1 = enable;
            if (enable_O1)
            {
                dic = new Dictionary<K, V>();
            }
        }

        public SimpleDictionary(int cap)
        {
            this.keys = new List<K>(cap);
            this.values = new List<V>(cap);

        }

        public SimpleDictionary(int cap, bool enable)
        {
            this.keys = new List<K>(cap);
            this.values = new List<V>(cap);
            this.enable_O1 = enable;
            if (enable_O1)
            {
                dic = new Dictionary<K, V>();
            }
        }

        /// <summary>
        /// 获得第几个元素
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public V Get(int cnt)
        {
            if (cnt < this.values.Count)
            {
                return this.values[cnt];
            }
            return default(V);
        }

        public bool ContainsKey(K k)
        {
            for (int i = 0; i < keys.Count; ++i)
            {
                K key = keys[i];
                if (key.Equals(k))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsValue(V v)
        {
            for (int i = 0; i < values.Count; ++i)
            {
                V value = values[i];
                if (value.Equals(v))
                {
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            this.values.Clear();
            this.keys.Clear();
        }

        public List<V> FindALL(Predicate<K> p)
        {
            List<V> values = ListPool.TrySpawn<List<V>>();
            List<K> keys = this.keys.FindAll(p);
            for (int i = 0; i < keys.Count; ++i)
            {
                values.Add(this[keys[i]]);
            }

            return values;
        }

        public bool RemoveALL(Predicate<K> p)
        {
            List<K> keys = this.keys.FindAll(p);
            if (keys.Count == 0)
                return false;

            for (int i = 0; i < keys.Count; ++i)
            {
                K k = keys[i];
                values.Remove(this[k]);
                keys.Remove(k);

                if(enable_O1)
                    this.dic.Remove(k);
            }

            return true;
        }

        public bool TryGetValue(K key, out V v)
        {
            v = this[key];
            return v != null;   
        }

        public void Add(K key, V value)
        {
            this[key] = value;
        }

        public bool RemoveKey(K Key)
        {
            if (enable_O1)
                this.dic.Remove(Key);

            int index = this.keys.IndexOf(Key);
            if (index != -1)
            {
                this.keys.RemoveAt(index);
                this.values.RemoveAt(index);

                return true;
            }
            return false;
        }

        public bool RemoveValue(V value)
        {
            int index = this.values.IndexOf(value);
            if (index != -1)
            {
                if (enable_O1)
                    this.dic.Remove(keys[index]);

                this.keys.RemoveAt(index);
                this.values.RemoveAt(index);
                return true;
            }
            return false;
        }

        #region Implementation of ISerializationCallbackReceiver
        public void OnAfterDeserialize()
        {
            if (enable_O1)
            {
                dic.Clear();
                for (int i = 0; i < keys.Count; i++)
                    if (keys[i] != null && (!(keys[i] is UnityEngine.Object) || ((UnityEngine.Object)(object)keys[i])))
                        dic.Add(keys[i], values[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            if (enable_O1)
            {
                dic.Clear();
                for (int i = 0; i < keys.Count; i++)
                    if (keys[i] != null && (!(keys[i] is UnityEngine.Object) || ((UnityEngine.Object)(object)keys[i])))
                        dic.Add(keys[i], values[i]);
            }
        }
        #endregion
    }

    [Serializable]
    public class SimpleClsDictionary<K, V> where K:class
    {
        [SerializeField]
        private List<K> keys;
        [SerializeField]
        private List<V> values;
        [NonSerialized]
        private Dictionary<K, V> dic;

        [NonSerialized]
        public readonly bool enable_O1 = false;

        public V this[K key]
        {
            get
            {
                try
                {
                    if (enable_O1)
                    {
                        V outvalue = default(V);
                        if (dic.TryGetValue(key, out outvalue))
                        {
                            return outvalue;
                        }
                        return outvalue;
                    }

                    int index = this.keys.IndexOf(key);
                    if (index == -1)
                        return default(V);
                    return this.values[index];
                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                    return default(V);
                }

            }
            set
            {
                try
                {
                    if (enable_O1)
                        dic[key] = value;

                    int index = this.keys.IndexOf(key);
                    if (index == -1)
                    {
                        this.values.Add(value);
                        this.keys.Add(key);
                    }
                    else
                    {
                        this.values[index] = value;
                    }
                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                }

            }
        }

        public int Count
        {
            get
            {
                return this.keys.Count;
            }
        }
        /// <summary>
        /// low gc 获取的是原始对象
        /// </summary>
        public List<K> ReadOnlyKeys
        {
            get
            {
                return keys;
            }
        }
        /// <summary>
        /// low gc 获取的是原始对象
        /// </summary>
        public List<V> ReadOnlyValues
        {
            get
            {
                return values;
            }
        }

        public List<K> Keys
        {
            get
            {
                List<K> list = ListPool.TrySpawn<List<K>>();
                list.AddRange(this.keys);
                return list;
            }
        }

        public List<V> Values
        {
            get
            {
                List<V> list = ListPool.TrySpawn<List<V>>();
                list.AddRange(this.values);
                return list;
            }
        }

        public SimpleClsDictionary():this(0)
        {

        }

        public SimpleClsDictionary(bool enable) : this(0)
        {
            this.enable_O1 = enable;
            if (enable_O1)
            {
                dic = new Dictionary<K, V>();
            }
        }

        public SimpleClsDictionary(int cap)
        {
            this.keys = new List<K>(cap);
            this.values = new List<V>(cap);

        }

        public SimpleClsDictionary(int cap, bool enable)
        {
            this.keys = new List<K>(cap);
            this.values = new List<V>(cap);
            this.enable_O1 = enable;
            if (enable_O1)
            {
                dic = new Dictionary<K, V>();
            }
        }

        /// <summary>
        /// 获得第几个元素
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public V Get(int cnt)
        {
            if (cnt < this.values.Count)
            {
                return this.values[cnt];
            }
            return default(V);
        }

        public bool ContainsKey(K k)
        {
            for (int i = 0; i < keys.Count; ++i)
            {
                K key = keys[i];
                if (key.Equals(k))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsValue(V v)
        {
            for (int i = 0; i < values.Count; ++i)
            {
                V value = values[i];
                if (value.Equals(v))
                {
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            this.values.Clear();
            this.keys.Clear();
        }

        public List<V> FindALL(Predicate<K> p)
        {
            List<V> values = ListPool.TrySpawn<List<V>>();
            List<K> keys = this.keys.FindAll(p);
            for (int i = 0; i < keys.Count; ++i)
            {
                values.Add(this[keys[i]]);
            }

            return values;
        }

        public bool RemoveALL(Predicate<K> p)
        {
            List<K> keys = this.keys.FindAll(p);
            if (keys.Count == 0)
                return false;

            for (int i = 0; i < keys.Count; ++i)
            {
                K k = keys[i];
                values.Remove(this[k]);
                keys.Remove(k);

                if (enable_O1)
                    this.dic.Remove(k);
            }

            return true;
        }

        public bool TryGetValue(K key, out V v)
        {
            v = this[key];
            return v != null;
        }

        public void Add(K key, V value)
        {
            this[key] = value;
        }

        public bool RemoveKey(K Key)
        {
            if (enable_O1)
                this.dic.Remove(Key);

            int index = this.keys.IndexOf(Key);
            if (index != -1)
            {
                this.keys.RemoveAt(index);
                this.values.RemoveAt(index);

                return true;
            }
            return false;
        }

        public bool RemoveValue(V value)
        {
            int index = this.values.IndexOf(value);
            if (index != -1)
            {
                if (enable_O1)
                    this.dic.Remove(keys[index]);

                this.keys.RemoveAt(index);
                this.values.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool RemoveValue(V value,out K Key)
        {
            int index = this.values.IndexOf(value);
            if (index != -1)
            {
                if (enable_O1)
                    this.dic.Remove(keys[index]);

                Key = keys[index];
                this.keys.RemoveAt(index);
                this.values.RemoveAt(index);
                return true;
            }
            Key = null;
            return false;
        }

    }
}

