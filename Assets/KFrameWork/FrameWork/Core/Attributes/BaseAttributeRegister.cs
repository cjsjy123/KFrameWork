using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using KUtils;
using Priority_Queue;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KFrameWork
{
    public abstract class BaseAttributeRegister  {

        /// <summary>
        /// o 为att对象，target 返回类或者方法对象
        /// </summary>
        public delegate void GameAttriHandler(object o, object target);

        #region 容器

        private Dictionary<int, Queue<KeyValuePair<Type, GameAttriHandler>>> _attributeHandlers = new Dictionary<int, Queue<KeyValuePair<Type, GameAttriHandler>>>();

        #endregion

        public void RegisterHandler(RegisterType tp, Type target, GameAttriHandler handler)
        {
            if (!this._attributeHandlers.ContainsKey((int)tp))
            {
                this._attributeHandlers.Add((int)tp, new Queue<KeyValuePair<Type, GameAttriHandler>>());
            }

            var queue = this._attributeHandlers[(int)tp];

            queue.Enqueue(new KeyValuePair<Type, GameAttriHandler>(target, handler));
        }

        private void SortTypes(Type[] types)
        {
            Array.Sort<Type>(types, CompareType);
        }

        private int CompareType(Type left, Type right)
        {
            object[] leftatts = left.GetCustomAttributes(typeof(TypeInitAttribute),true);
            object[] rightatts = right.GetCustomAttributes(typeof(TypeInitAttribute), true);

            int leftOrder = 0;
            int rightOrder = 0;
            if (leftatts.Length > 0)
                leftOrder = (leftatts[0] as TypeInitAttribute).typeSorder;

            if (rightatts.Length > 0)
                rightOrder = (rightatts[0] as TypeInitAttribute).typeSorder;

            return leftOrder - rightOrder;
        }

        private void InitAttributes(Type passtp)
        {
            Assembly asm = passtp.Assembly;
            Type[] types = asm.GetTypes();

            SortTypes(types);

            int i = (int)RegisterType.Register;
            int end = (int)RegisterType.END;
            for(;i < end;++i)
            {
                RegisterType registerTp = (RegisterType)i;
                if(this._attributeHandlers.ContainsKey(i))
                {
                    var queue = this._attributeHandlers[i];
                    while (queue.Count > 0)
                    {
                        KeyValuePair<Type, GameAttriHandler> tupledata = queue.Dequeue();
                        for (int m = 0; m < types.Length; ++m)
                        {
                            Type tp = types[m];
                            if (registerTp == RegisterType.ClassAttr)
                            {
                                object[] att = tp.GetCustomAttributes(tupledata.Key, false);

                                if (att != null && att.Length == 1)
                                {
                                    tupledata.Value(att[0], tp);
                                }
                            }
                            else if (registerTp == RegisterType.MethodAtt)
                            {
                                MethodInfo[] methods = tp.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

                                for (int j = 0; j < methods.Length; ++j)
                                {
                                    object[] att = methods[j].GetCustomAttributes(tupledata.Key, false);
                                    if (att != null && att.Length > 0 && methods[j].IsStatic)
                                    {
                                        tupledata.Value(att[0], methods[j]);
                                    }
                                    else if (att != null && att.Length > 0 && !methods[j].IsStatic)
                                    {
                                        LogMgr.LogError("虽然注册了，但是不是静态函数 =>" + methods[j].Name);
                                    }
                                }
                            }
                            else if (registerTp == RegisterType.InstacenMethodAttr)
                            {
                                MethodInfo[] methods = tp.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                                for (int j = 0; j < methods.Length; ++j)
                                {
                                    object[] att = methods[j].GetCustomAttributes(tupledata.Key, false);
                                    if (att != null && att.Length > 0)
                                    {
                                        tupledata.Value(att[0], methods[j]);
                                    }
                                }
                            }
                            else if (registerTp == RegisterType.Register)
                            {
                                MethodInfo[] methods = tp.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

                                for (int j = 0; j < methods.Length; ++j)
                                {
                                    object[] att = methods[j].GetCustomAttributes(tupledata.Key, false);
                                    if (att != null && att.Length > 0)
                                    {
                                        tupledata.Value(att[0], new KeyValuePair<BaseAttributeRegister,MethodInfo>(this, methods[j]));
                                    }
                                }
                            }
                        }
                    }

                }
            }

        }
#if UNITY_EDITOR
        private RegisterCachedTypes InitAttributesToCache(Type passtp,string path)
        {
            string[] sparray = path.Split('.');
            if (sparray.Length == 1)
                path += ".asset";

            RegisterCachedTypes cachedTypes = Resources.Load<RegisterCachedTypes>(sparray[0]);
            bool first = false;
            if (cachedTypes == null)
            {
                first = true;
                cachedTypes = ScriptableObject.CreateInstance<RegisterCachedTypes>(); ;
            }
            else
            {
                cachedTypes.Clear();
            }

            cachedTypes.CachedTime = DateTime.Now.ToString();

            Assembly asm = passtp.Assembly;
            Type[] types = asm.GetTypes();

            SortTypes(types);

            int i = (int)RegisterType.None;
            int end = (int)RegisterType.END;
            for (; i < end; ++i)
            {
                RegisterType registerTp = (RegisterType)i;
                if (this._attributeHandlers.ContainsKey(i))
                {
                    var queue = this._attributeHandlers[i];
                    while (queue.Count > 0)
                    {
                        KeyValuePair<Type, GameAttriHandler> tupledata = queue.Dequeue();
                        for (int m = 0; m < types.Length; ++m)
                        {
                            Type tp = types[m];

                            if (registerTp == RegisterType.ClassAttr )
                            {
                                object[] att = tp.GetCustomAttributes(tupledata.Key, false);

                                if (att != null && att.Length == 1)
                                {
                                    cachedTypes.LoadTypes(registerTp, tp, tupledata.Key);
                                }
                            }
                            else if (registerTp == RegisterType.MethodAtt || registerTp == RegisterType.Register)
                            {
                                MethodInfo[] methods = tp.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                                List<MethodInfo> methodList = new List<MethodInfo>();
                                for (int j = 0; j < methods.Length; ++j)
                                {
                                    object[] att = methods[j].GetCustomAttributes(tupledata.Key, false);
                                    if (att != null && att.Length > 0 && methods[j].IsStatic)
                                    {
                                        methodList.Add(methods[j]);
                                        if (registerTp == RegisterType.Register)
                                        {
                                            methods[j].Invoke(null,new object[] { this});
                                        }
                                    }
                                    else if (att != null && att.Length > 0 && !methods[j].IsStatic)
                                    {
                                        LogMgr.LogError("虽然注册了，但是不是静态函数 =>" + methods[j].Name);
                                    }
                                }

                                if (methodList.Count > 0)
                                {
                                    cachedTypes.LoadTypes(registerTp, tp, tupledata.Key, methodList);

                                }
                            }
                        }
                    }
                }
            }


            //
            if (first)
            {
                AssetDatabase.CreateAsset(cachedTypes, "Assets/Resources/" + path);
            }
            else
            {
                EditorUtility.SetDirty(cachedTypes);
            }

            AssetDatabase.Refresh();


            return cachedTypes;
        }
#endif

        private void InitFromCache(string path)
        {
            string[] sparray = path.Split('.');
            if (sparray.Length == 1)
                path += ".asset";

            RegisterCachedTypes cachedTypes = Resources.Load<RegisterCachedTypes>(sparray[0]);
            if (cachedTypes == null)
                throw new Exception("Missing Cache : " + path);

            InitFromCache(cachedTypes);
        }

        private void InitFromCache(RegisterCachedTypes cachedTypes )
        {
            int i = (int)RegisterType.None;
            int end = (int)RegisterType.END;
            for (; i < end; ++i)
            {
                CachedListStringWrapper tuples = cachedTypes.getTypes(i);
                if (tuples != null)
                {
                    if (this._attributeHandlers.ContainsKey(i))
                    {
                        var queue = this._attributeHandlers[i];
                        while (queue.Count > 0)
                        {
                            var queueTuple = queue.Dequeue();
                            for (int j = 0; j < tuples.TypeList.Count; j++)
                            {
                                if (tuples.TypeList[j].attributename.Equals(queueTuple.Key.FullName))
                                {
                                    cachedTypes.Analyze(this,i, tuples.TypeList[j], queueTuple.Key, queueTuple.Value);
                                }
                            }
                        }
                    }
                    else
                    {
                        LogMgr.LogError("缓存数据不匹配可能导致异常");
                    }
                }
            }
        }

        protected virtual void Start(Type passtp)
        {
            this.InitAttributes(passtp);
        }
#if UNITY_EDITOR
        protected virtual RegisterCachedTypes StartToCache(Type passtype,string path)
        {
            return InitAttributesToCache(passtype,path);
        }
#endif

        protected virtual void StartFromCache(string path)
        {
            InitFromCache(path);
        }

        protected virtual void StartFromCache(RegisterCachedTypes types)
        {
            InitFromCache(types);
        }

        protected void End()
        {
            this._attributeHandlers.Clear();
        }
    }
}


