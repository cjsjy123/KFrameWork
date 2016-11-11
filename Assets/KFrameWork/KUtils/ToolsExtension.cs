using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace KUtils
{
    public static class ToolsExtension
    {
        public static void BindParent(this Transform tr, Transform p)
        {
#if UNITY_5_3 || UNITY_5_4
            tr.SetParent(p);
#else
            tr.parent = p;
#endif
        }

        public static void BindParent(this Transform tr, GameObject p)
        {
#if UNITY_5_3 || UNITY_5_4
            tr.SetParent(p.transform);
#else
            tr.parent = p.transform;
#endif
        }

        public static void BindParent(this Transform tr, Component p)
        {
#if UNITY_5_3 || UNITY_5_4
            tr.SetParent(p.transform);
#else
            tr.parent = p.transform;
#endif
        }

        public static GameObject InstancePrefab(this GameObject prefab, GameObject parent)
        {
            GameObject ins = GameObject.Instantiate(prefab);

            Transform transform = ins.transform;
            transform.BindParent(parent);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.gameObject.layer = parent.layer;
            transform.tag = parent.tag;

            return ins;
        }

        public static GameObject InstancePrefab(this GameObject prefab, Transform parent)
        {
            GameObject ins = GameObject.Instantiate(prefab);

            Transform transform = ins.transform;
            transform.BindParent(parent);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.gameObject.layer = parent.gameObject.layer;
            transform.tag = parent.tag;

            return ins;
        }

        public static GameObject AddInstance(this Component parent, GameObject ins)
        {

            Transform transform = ins.transform;
            transform.BindParent(parent);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.gameObject.layer = parent.gameObject.layer;
            transform.tag = parent.tag;

            return ins;
        }

        public static GameObject AddInstance(this Component parent, Transform ins)
        {

            Transform transform = ins.transform;
            transform.BindParent(parent);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.gameObject.layer = parent.gameObject.layer;
            transform.tag = parent.tag;
            return ins.gameObject;
        }

        public static GameObject AddPrefab(this Component parent, GameObject prefab)
        {
            GameObject ins = GameObject.Instantiate(prefab);

            Transform transform = ins.transform;
            transform.BindParent(parent);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.gameObject.layer = parent.gameObject.layer;
            transform.tag = parent.tag;

            return ins;
        }

        public static GameObject AddPrefab(this Component parent, Transform prefab)
        {
            GameObject ins = GameObject.Instantiate(prefab.gameObject);

            Transform transform = ins.transform;
            transform.BindParent(parent);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.gameObject.layer = parent.gameObject.layer;
            transform.tag = parent.tag;

            return ins;
        }

        public static bool LogStaticMethod(this MethodInfo m)
        {
            if(!m.IsStatic)
            {
                LogMgr.LogError("非静态函数");
                return false;
            }
            return true;
        }

        public static bool TryAdd<T>(this List<T> list,T data)
        {
            if (list.Contains(data) )
            {
                return false;
            }
            list.Add(data);
            return true;
        }


        public static bool FloatEqual(this float tf, float other)
        {
            if (Math.Abs(tf - other) < 0.0001f)
            {
                return true;
            }
            return false;
        }

        public static bool FloatLessEqual(this float tf, float other)
        {
            return tf.FloatEqual(other) || tf < other;
        }

        public static bool FloatLargetEqual(this float tf, float other)
        {
            return tf.FloatEqual(other) || tf > other;
        }

        public static bool FloatEqual(this float? tf, float other)
        {
            if(!tf.HasValue)
                return false;

            if (Math.Abs(tf.Value - other) < 0.0001f)
            {
                return true;
            }
            return false;
        }

        public static bool FloatEqual(this float? tf, float? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;
            
            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;

            if (Math.Abs(tf.Value - other.Value) < 0.0001f)
            {
                return true;
            }
            return false;
        }

        public static bool FloatLessEqual(this float? tf, float other)
        {
            if(!tf.HasValue)
                return false;
            return tf.FloatEqual(other) || tf.Value < other;
        }

        public static bool FloatLargetEqual(this float? tf, float other)
        {
            if(!tf.HasValue)
                return false;
            return tf.FloatEqual(other) || tf.Value > other;
        }

        public static bool FloatLessEqual(this float? tf, float? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;
            return tf.FloatEqual(other) || tf.Value < other;
        }

        public static bool FloatLargetEqual(this float? tf, float? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;
            return tf.FloatEqual(other) || tf.Value > other;
        }


        public static bool DoubleLessEqual(this double tf, double other)
        {
            return tf.DoubleEqual(other) || tf < other;
        }

        public static bool DoubleLargetEqual(this double tf, double other)
        {
            return tf.DoubleEqual(other) || tf > other;
        }

        public static bool DoubleEqual(this double tf, double other)
        {
            if (Math.Abs(tf - other) < 0.0001d)
            {
                return true;
            }
            return false;
        }

        public static bool DoubleLessEqual(this double? tf, double other)
        {
            if(!tf.HasValue )
                return false;

            return tf.DoubleEqual(other) || tf < other;
        }

        public static bool DoubleLargetEqual(this double? tf, double other)
        {
            if(!tf.HasValue )
                return false;

            return tf.DoubleEqual(other) || tf > other;
        }

        public static bool DoubleEqual(this double? tf, double other)
        {
            if(!tf.HasValue )
                return false;

            if (Math.Abs(tf.Value - other) < 0.0001d)
            {
                return true;
            }
            return false;
        }

        public static bool DoubleLessEqual(this double? tf, double? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;

            return tf.DoubleEqual(other) || tf < other;
        }

        public static bool DoubleLargetEqual(this double? tf, double? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;

            return tf.DoubleEqual(other) || tf > other;
        }

        public static bool DoubleEqual(this double? tf, double? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;

            if (Math.Abs(tf.Value - other.Value) < 0.0001d)
            {
                return true;
            }
            return false;
        }

    }
}

