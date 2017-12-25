using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using KFrameWork;
using System.Linq.Expressions;

public static class DynamicConvert<TFrom, Tto>
{
    private static Func<TFrom, Tto> converter = CreateExpression<TFrom, Tto>(body => Expression.Convert(body, typeof(Tto)));

    public static Tto Convert(TFrom valuetoconvert)
    {
        return converter(valuetoconvert);
    }

    public static Func<TFrom, Tto> Converter
    {
        get
        {
            return converter;
        }
    }

    private static Func<TArg1, TResult> CreateExpression<TArg1, TResult>(Func<Expression, UnaryExpression> body)
    {
        ParameterExpression inp = Expression.Parameter(typeof(TArg1), "inp");
        try
        {
            return Expression.Lambda<Func<TArg1, TResult>>(body(inp), inp).Compile();
        }
        catch (Exception ex)
        {
            LogMgr.LogException(ex);
            return null;
        }
    }
}

public static class FrameWorkTools
{
    public const float kb = 1024;
    public const float mb = 1048576;
    public const float gb = 1073741824;

    /// <summary>
    /// Mathf.Epsilon
    /// </summary>
    private const float constvalue = 0.001f;

    public static T Convert<TFrom, T>(TFrom value)
    {
        //return DynamicConvert<TFrom, T>.Convert(value);
        return (T)System.Convert.ChangeType(value, typeof(T));
    }

    public static string Sub2End(this string old, char rpstr)
    {
        int index = old.IndexOf(rpstr);
        if (index != -1)
        {
            if (old.Length == index + 1)
            {
                return old;
            }
            return old.Substring(index + 1);
        }
        return old;
    }

    public static string Sub2Begin(this string old, char rpstr)
    {
        int index = old.IndexOf(rpstr);
        if (index != -1)
        {
            return old.Substring(0, index);
        }
        return old;
    }

    public static HashSet<T> ConvertHashSet<T>(this T[] array)
    {
        HashSet<T> hashset = new HashSet<T>();
        if (array == null)
            return hashset;

        for (int i = 0; i < array.Length; ++i)
        {
            hashset.Add(array[i]);
        }
        return hashset;
    }

    public static bool LogPlaying()
    {
        if (Application.isPlaying)
        {
            return true;
        }
        else
        {
            LogMgr.LogError("当前游戏未运行");
            return false;
        }
    }

    /// <summary>
    /// tick = 100 毫微秒  10^-7
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static TimeSpan MillsecdstoTime(long time)
    {
        return TimeSpan.FromMilliseconds(time);
    }
    /// <summary>
    /// 秒数
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static TimeSpan SecondsToTime(long time)
    {
        return TimeSpan.FromSeconds(time);
    }


    public static double CurrentTime()
    {
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        return (double)(DateTime.Now - startTime).TotalMilliseconds;
    }

    public static void Swap<T>(ref T t, ref T r) where T : struct
    {
        T temp = t;
        t = r;
        r = temp;
    }

    public static float ByteToMB(long bdata)
    {
        return bdata / mb;
    }

    public static float ByteToGB(long bdata)
    {
        return bdata / gb;
    }

    public static float ByteToKB(long bdata)
    {
        return bdata / kb;
    }

    public static List<int> CreateIntList()
    {
        return new List<int>();
    }

    public static List<bool> CreateBoolList()
    {
        return new List<bool>();
    }

    public static List<string> CreateStringList()
    {
        return new List<string>();
    }

    public static List<short> CreateShortList()
    {
        return new List<short>();
    }
    /// <summary>
    /// 浅拷贝,反射版本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="origion"></param>
    /// <returns></returns>
    public static T Copy<T>(T origion) where T : class
    {
        Type tp = origion.GetType();

        MethodInfo method = tp.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null)
        {
            return (T)method.Invoke(origion, null);
        }
        return null;
    }
    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="origion"></param>
    /// <returns></returns>
    public static T DeepCopy<T>(T origion) where T : class
    {
        object retval = null;
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(ms, origion);
            ms.Seek(0, SeekOrigin.Begin);

            retval = bf.Deserialize(ms);
            ms.Close();
        }
        return (T)retval;
    }

    /// <summary>
    /// 反射
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="name"></param>
    /// <param name="objs"></param>
    /// <returns></returns>
    public static long CalculateCostTime<T>(T t, string name, params object[] objs)
    {
        Type tp = typeof(T);
        var methodinfo = tp.GetMethod(name);
        var watch = Stopwatch.StartNew();
        methodinfo.Invoke(t, objs);
        watch.Stop();
        return watch.ElapsedMilliseconds;
    }

    public static int GetHashCode(object o)
    {
        return RuntimeHelpers.GetHashCode(o);
    }

    public static void DestorySelf(this GameObject tr)
    {
        if (tr != null)
            GameObject.Destroy(tr);
    }

    public static void DestorySelf(this Component tr)
    {
        if (tr != null)
            GameObject.Destroy(tr.gameObject);
    }

    public static float PlayForward(this Animation an, string name, float speed = 1f)
    {
        AnimationState state = an[name];

        //state.speed = speed <0f? -speed: speed;
        an.CrossFade(name);
        return state.length;
    }

    public static float PlayBackward(this Animation an, string name, float speed = -1f)
    {
        AnimationState state = an[name];
        state.speed = speed > 0 ? -speed : speed;
        an.Play(name);
        return state.length;
    }

    public static float GetCurAnimationTime(this Animator an)
    {
        var state = an.GetCurrentAnimatorStateInfo(0);
        return state.length;
    }

    public static float PlayForward(this Animator an, string name, float speed = 1f)
    {
        an.speed = speed < 0 ? -speed : speed;
        an.Play(name);
        var state = an.GetCurrentAnimatorStateInfo(0);
        return state.length;
    }

    public static float PlayBackward(this Animator an, string name, float speed = -1f)
    {
        an.speed = speed > 0 ? -speed : speed;
        an.Play(name);
        var state = an.GetCurrentAnimatorStateInfo(0);
        return state.length;
    }

    public static void RemoveChild(this Transform tr, int idx)
    {
        GameObject.Destroy(tr.transform.GetChild(idx).gameObject);
    }

    public static void RemoveChild(this Component tr, int idx)
    {
        GameObject.Destroy(tr.transform.GetChild(idx).gameObject);
    }

    public static void RemoveChild(this GameObject tr, int idx)
    {
        GameObject.Destroy(tr.transform.GetChild(idx).gameObject);
    }

    public static void RemoveChildren(this Transform tr)
    {
        int cnt = tr.childCount;
        for (int i = cnt - 1; i >= 0; i--)
        {
            GameObject.Destroy(tr.GetChild(i).gameObject);
        }
    }

    public static void RemoveChildren(this Component tr)
    {
        int cnt = tr.transform.childCount;
        for (int i = cnt - 1; i >= 0; i--)
        {
            GameObject.Destroy(tr.transform.GetChild(i).gameObject);
        }
    }

    public static void RemoveChildren(this GameObject tr)
    {
        int cnt = tr.transform.childCount;
        for (int i = cnt - 1; i >= 0; i--)
        {
            GameObject.Destroy(tr.transform.GetChild(i).gameObject);
        }
    }

    public static void RemoveChildrenImmediate(this Transform tr)
    {
        int cnt = tr.childCount;
        for (int i = cnt - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(tr.GetChild(i).gameObject);
        }
    }

    public static void RemoveChildrenImmediate(this Component tr)
    {
        int cnt = tr.transform.childCount;
        for (int i = cnt - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(tr.transform.GetChild(i).gameObject);
        }
    }

    public static void RemoveChildrenImmediate(this GameObject tr)
    {
        int cnt = tr.transform.childCount;
        for (int i = cnt - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(tr.transform.GetChild(i).gameObject);
        }
    }
    #region 拓展方法

    public static void SetChildrenLayer(this GameObject g, LayerMask mask)
    {
        Renderer[] rs = g.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rs.Length; ++i)
        {
            rs[i].gameObject.layer = mask;
        }
    }

    public static void SetChildrenLayer(this GameObject g, int mask)
    {
        Renderer[] rs = g.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rs.Length; ++i)
        {
            rs[i].gameObject.layer = mask;
        }
    }

    public static void SetChildrenLayer(this Transform g, LayerMask mask)
    {
        Renderer[] rs = g.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rs.Length; ++i)
        {
            rs[i].gameObject.layer = mask;
        }
    }

    public static void SetChildrenLayer(this Transform g, int mask)
    {
        Renderer[] rs = g.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rs.Length; ++i)
        {
            rs[i].gameObject.layer = mask;
        }
    }

    public static void SetTheParent(this Transform tr, Transform p, bool worldPositionStays = false)
    {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
        tr.SetParent(p, worldPositionStays);
#else
            tr.parent = p;
#endif
    }

    public static void SetTheParent(this Transform tr, GameObject p, bool worldPositionStays = true)
    {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
        tr.SetParent(p.transform, worldPositionStays);
#else
            tr.parent = p.transform;
#endif
    }

    public static void SetTheParent(this GameObject tr, Transform p, bool worldPositionStays = false)
    {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
        tr.transform.SetParent(p, worldPositionStays);
#else
            tr.transform.parent = p;
#endif
    }

    public static void SetTheParent(this GameObject tr, GameObject p, bool worldPositionStays = false)
    {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
        tr.transform.SetParent(p.transform, worldPositionStays);
#else
            tr.transform.parent = p.transform;
#endif
    }
    /// <summary>
    /// if hasnt existed,will create a new one,
    /// </summary>
    /// <param name="g"></param>
    /// <param name="findname"></param>
    /// <returns></returns>
    public static GameObject TryGetGameObject(this GameObject g, string findname)
    {
        Transform trans = g.transform.Find(findname);
        if (trans == null)
        {
            GameObject gameobject = new GameObject(findname);
            g.AddInstance(gameobject);

            trans = gameobject.transform;
        }
        return trans.gameObject;
    }

    public static GameObject TryGetGameObject(this Transform g, string findname)
    {
        Transform trans = g.Find(findname);
        if (trans == null)
        {
            GameObject gameobject = new GameObject(findname);
            g.AddInstance(gameobject);

            trans = gameobject.transform;
        }
        return trans.gameObject;
    }

    public static Component FullGetComponent(this Component trans, string name)
    {
        Component c = trans.GetComponent(name);
        if (c == null)
        {
            LogMgr.LogErrorFormat("Missing Component :{0}", name);

        }
        return c;
    }

    public static Component FullGetComponent(this GameObject trans, string name)
    {
        Component c = trans.GetComponent(name);
        if (c == null)
        {
            LogMgr.LogErrorFormat("Missing Component :{0}", name);

        }
        return c;
    }

    public static Component FullGetComponent(this Transform trans, string name)
    {
        Component c = trans.GetComponent(name);
        if (c == null)
        {
            LogMgr.LogErrorFormat("Missing Component :{0}", name);

        }
        return c;
    }

    public static Component FullGetComponentInParent(this Component trans, string name)
    {
        Component c = trans.GetComponentInParent(Type.GetType(name));
        if (c == null)
        {
            LogMgr.LogErrorFormat("Missing Component :{0}", name);

        }
        return c;
    }

    public static Component FullGetComponentInParent(this GameObject trans, string name)
    {
        Component c = trans.GetComponentInParent(Type.GetType(name));
        if (c == null)
        {
            LogMgr.LogErrorFormat("Missing Component :{0}", name);

        }
        return c;
    }

    public static Component FullGetComponentInParent(this Transform trans, string name)
    {
        Component c = trans.GetComponentInParent(Type.GetType(name));
        if (c == null)
        {
            LogMgr.LogErrorFormat("Missing Component :{0}", name);

        }
        return c;
    }

    public static void PauseGame(this Transform self)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#endif
    }

    public static void ResumeGame(this Transform self)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = false;
#endif
    }

    public static void PauseGame(this Component self)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#endif
    }

    public static void ResumeGame(this Component self)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = false;
#endif
    }

    public static void PauseGame(this GameObject self)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#endif
    }

    public static void ResumeGame(this GameObject self)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = false;
#endif
    }


    public static GameObject AddInstance(this GameObject parent, GameObject ins, bool worldPositionStays = false)
    {

        Transform transform = ins.transform;
        transform.SetTheParent(parent, worldPositionStays);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.gameObject.layer = parent.gameObject.layer;
        transform.tag = parent.tag;

        return ins;
    }

    public static GameObject AddInstance(this Component parent, GameObject ins, bool worldPositionStays = false)
    {

        Transform transform = ins.transform;
        transform.SetTheParent(parent.transform, worldPositionStays);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        transform.gameObject.layer = parent.gameObject.layer;
        transform.tag = parent.tag;

        return ins;
    }

    public static GameObject AddInstance(this Component parent, Transform ins, bool worldPositionStays = false)
    {

        Transform transform = ins.transform;
        transform.SetTheParent(parent.transform, worldPositionStays);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        transform.gameObject.layer = parent.gameObject.layer;
        transform.tag = parent.tag;
        return ins.gameObject;
    }

    public static GameObject AddPrefab(this Component parent, GameObject prefab, bool worldPositionStays = false)
    {
        GameObject ins = GameObject.Instantiate(prefab);

        Transform transform = ins.transform;
        transform.SetTheParent(parent.transform, worldPositionStays);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        transform.gameObject.layer = parent.gameObject.layer;
        transform.tag = parent.tag;

        return ins;
    }

    public static GameObject AddPrefab(this GameObject parent, Transform prefab, bool worldPositionStays = false)
    {
        GameObject ins = GameObject.Instantiate(prefab.gameObject);

        Transform transform = ins.transform;
        transform.SetTheParent(parent.transform, worldPositionStays);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        transform.gameObject.layer = parent.gameObject.layer;
        transform.tag = parent.tag;

        return ins;
    }

    public static GameObject AddPrefab(this GameObject parent, GameObject prefab, bool worldPositionStays = false)
    {
        GameObject ins = GameObject.Instantiate(prefab);

        Transform transform = ins.transform;
        transform.SetTheParent(parent.transform, worldPositionStays);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.gameObject.layer = parent.gameObject.layer;
        transform.tag = parent.tag;

        return ins;
    }

    public static GameObject AddPrefab(this Component parent, Transform prefab, bool worldPositionStays = false)
    {
        GameObject ins = GameObject.Instantiate(prefab.gameObject);

        Transform transform = ins.transform;
        transform.SetTheParent(parent.transform, worldPositionStays);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.gameObject.layer = parent.gameObject.layer;
        transform.tag = parent.tag;

        return ins;
    }

    public static Color ToColor(this Vector4 value)
    {
        return new Color(value.x, value.y, value.z, value.w);
    }

    public static Color ToColor(this Vector3 value)
    {
        return new Color(value.x, value.y, value.z, 1);
    }

    public static Vector2 ToVector2(this float value)
    {
        return new Vector2(value, value);
    }

    public static Vector3 ToVector3(this float value)
    {
        return new Vector3(value, value, value);
    }

    public static Vector2 ToVector2(this int value)
    {
        return new Vector2(value, value);
    }

    public static Vector3 ToVector3(this int value)
    {
        return new Vector3(value, value, value);
    }

    public static Vector3 Local2Wolrd(this Transform tr, Vector3 local)
    {
        return tr.localToWorldMatrix.MultiplyPoint(local);
    }

    public static Vector3 World2Local(this Transform tr, Vector3 world)
    {
        return tr.worldToLocalMatrix.MultiplyPoint(world);
    }

    public static Vector4 GetWorldScale(this Transform tr)
    {
        Matrix4x4 matrix = tr.localToWorldMatrix;
        return new Vector4(matrix.m00, matrix.m11, matrix.m22, matrix.m33);
    }

    public static void SetWorldScale(this Transform tr, Vector3 value)
    {
        tr.transform.localScale = tr.World2Local(value);
    }

    public static bool LogStaticMethod(this MethodInfo m)
    {
        if (!m.IsStatic)
        {
            LogMgr.LogError("非静态函数");
            return false;
        }

        return true;
    }

    public static bool isLoopFunction(this MethodInfo m)
    {
        ParameterInfo[] ps = m.GetParameters();
        if (ps.Length == 1 && ps[0].ParameterType == typeof(int))
        {
            return m.IsStatic;
        }
        LogMgr.LogErrorFormat("{0} Not Loop Function ", m);
        return false;
    }

    public static bool TryEnque<T>(this List<T> list, T data)
    {
        if (list.Contains(data))
        {
            return false;
        }
        list.Insert(0, data);
        return true;
    }

    public static bool TryAdd<T>(this List<T> list, T data)
    {
        if (list.Contains(data))
        {
            return false;
        }
        list.Add(data);
        return true;
    }

    public static T TryGet<T>(this List<T> list, int index)
    {
        if (list != null && list.Count > index)
        {
            return list[index];
        }
        return default(T);
    }


    public static bool FloatEqual(this float tf, float other)
    {
        if (Math.Abs(tf - other) < constvalue)
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
        if (!tf.HasValue)
            return false;

        if (Math.Abs(tf.Value - other) < constvalue)
        {
            return true;
        }
        return false;
    }

    public static bool FloatEqual(this float? tf, float? other)
    {
        if (!tf.HasValue && other.HasValue)
            return false;

        if (tf.HasValue && !other.HasValue)
            return false;

        if (!tf.HasValue && !other.HasValue)
            return true;

        if (Math.Abs(tf.Value - other.Value) < constvalue)
        {
            return true;
        }
        return false;
    }

    public static bool FloatLessEqual(this float? tf, float other)
    {
        if (!tf.HasValue)
            return false;
        return tf.FloatEqual(other) || tf.Value < other;
    }

    public static bool FloatLargetEqual(this float? tf, float other)
    {
        if (!tf.HasValue)
            return false;
        return tf.FloatEqual(other) || tf.Value > other;
    }

    public static bool FloatLessEqual(this float? tf, float? other)
    {
        if (!tf.HasValue && other.HasValue)
            return false;

        if (tf.HasValue && !other.HasValue)
            return false;

        if (!tf.HasValue && !other.HasValue)
            return true;
        return tf.FloatEqual(other) || tf.Value < other;
    }

    public static bool FloatLargetEqual(this float? tf, float? other)
    {
        if (!tf.HasValue && other.HasValue)
            return false;

        if (tf.HasValue && !other.HasValue)
            return false;

        if (!tf.HasValue && !other.HasValue)
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
        if (Math.Abs(tf - other) < constvalue)
        {
            return true;
        }
        return false;
    }

    public static bool DoubleLessEqual(this double? tf, double other)
    {
        if (!tf.HasValue)
            return false;

        return tf.DoubleEqual(other) || tf < other;
    }

    public static bool DoubleLargetEqual(this double? tf, double other)
    {
        if (!tf.HasValue)
            return false;

        return tf.DoubleEqual(other) || tf > other;
    }

    public static bool DoubleEqual(this double? tf, double other)
    {
        if (!tf.HasValue)
            return false;

        if (Math.Abs(tf.Value - other) < constvalue)
        {
            return true;
        }
        return false;
    }

    public static bool DoubleLessEqual(this double? tf, double? other)
    {
        if (!tf.HasValue && other.HasValue)
            return false;

        if (tf.HasValue && !other.HasValue)
            return false;

        if (!tf.HasValue && !other.HasValue)
            return true;

        return tf.DoubleEqual(other) || tf < other;
    }

    public static bool DoubleLargetEqual(this double? tf, double? other)
    {
        if (!tf.HasValue && other.HasValue)
            return false;

        if (tf.HasValue && !other.HasValue)
            return false;

        if (!tf.HasValue && !other.HasValue)
            return true;

        return tf.DoubleEqual(other) || tf > other;
    }

    public static bool DoubleEqual(this double? tf, double? other)
    {
        if (!tf.HasValue && other.HasValue)
            return false;

        if (tf.HasValue && !other.HasValue)
            return false;

        if (!tf.HasValue && !other.HasValue)
            return true;

        if (Math.Abs(tf.Value - other.Value) < constvalue)
        {
            return true;
        }
        return false;
    }

    public static bool IgnoreUpOrlower(this string str, string other)
    {
        return string.Equals(str, other, StringComparison.OrdinalIgnoreCase);
    }

    public static T TryAddComponent<T>(this GameObject t) where T : Component
    {
        T c = t.GetComponent<T>();
        if (c == null)
            c = t.AddComponent<T>();
        return c;
    }

    public static T TryAddComponent<T>(this Transform t) where T : Component
    {
        T c = t.gameObject.GetComponent<T>();
        if (c == null)
            c = t.gameObject.AddComponent<T>();
        return c;
    }

    public static T TryAddComponent<T>(this Component t) where T : Component
    {
        T c = t.GetComponent<T>();
        if (c == null)
            c = t.gameObject.AddComponent<T>();
        return c;
    }

    public static Vector3 RemoveX(this Vector3 v)
    {
        return new Vector3(0, v.y, v.z);
    }

    public static Vector3 RemoveXZ(this Vector3 v)
    {
        return new Vector3(0, v.y, 0);
    }

    public static Vector3 RemoveY(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    public static Vector3 RemoveZ(this Vector3 v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public static Vector2 UpdateX(this Vector2 v, float h)
    {
        return new Vector2(v.x + h, v.y);
    }

    public static Vector2 UpdateXY(this Vector2 v, float x, float y)
    {
        return new Vector3(v.x + x, v.y + y);
    }

    public static Vector2 UpdateY(this Vector2 v, float h)
    {
        return new Vector2(v.x, v.y + h);
    }

    public static Vector3 UpdateX(this Vector3 v, float h)
    {
        return new Vector3(v.x + h, v.y, v.z);
    }

    public static Vector3 UpdateXY(this Vector3 v, float x, float y)
    {
        return new Vector3(v.x + x, v.y + y, v.z);
    }

    public static Vector3 UpdateXYZ(this Vector3 v, float x, float y, float z)
    {
        return new Vector3(v.x + x, v.y + y, v.z + v.z);
    }

    public static Vector3 UpdateXZ(this Vector3 v, float x, float z)
    {
        return new Vector3(v.x + x, v.y, v.z + z);
    }

    public static Vector3 UpdateY(this Vector3 v, float h)
    {
        return new Vector3(v.x, v.y + h, v.z);
    }

    public static Vector3 UpdateYZ(this Vector3 v, float h, float z)
    {
        return new Vector3(v.x, v.y + h, v.z + z);
    }

    public static Vector3 UpdateZ(this Vector3 v, float h)
    {
        return new Vector3(v.x, v.y, v.z + h);
    }

    public static float XZsrqMag(this Vector3 v)
    {
        return v.x * v.x + v.z * v.z;
    }

    public static Vector3 MultiLong(this Vector3 v, long value, long factor)
    {
        return new Vector3(v.x * value / factor, v.y * value / factor, v.z * value / factor);
    }

    #endregion

}

