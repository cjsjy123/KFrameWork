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

        private Dictionary<int, List<KeyValuePair<Type, GameAttriHandler>>> _attributeHandlers = new Dictionary<int, List<KeyValuePair<Type, GameAttriHandler>>>();

        public void RegisterHandler(RegisterType tp, Type target, GameAttriHandler handler)
        {
            if (!this._attributeHandlers.ContainsKey((int)tp))
            {
                this._attributeHandlers.Add((int)tp, new List<KeyValuePair<Type, GameAttriHandler>>());
            }

            var queue = this._attributeHandlers[(int)tp];

            queue.Add(new KeyValuePair<Type, GameAttriHandler>(target, handler));
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

        private void InitAttributes(Assembly asm)
        {
            Type[] types = asm.GetTypes();

            SortTypes(types);

            int i = (int)RegisterType.Register;
            int end = (int)RegisterType.END;
            for(;i < end;++i)
            {
                RegisterType registerTp = (RegisterType)i;
                if(this._attributeHandlers.ContainsKey(i))
                {
                    var list = this._attributeHandlers[i];
                    for(int j = 0; j < list.Count;++j)
                    {
                        for (int m = 0; m < types.Length; ++m)
                        {
                            ReadAttribute(types[m], list[j], registerTp);
                        }
                    }
                }
            }

        }

        private void ReadAttribute(Type targetType, KeyValuePair<Type, GameAttriHandler> attributedata, RegisterType registerTp)
        {
            if (registerTp == RegisterType.ClassAttr)
            {
                object[] att = targetType.GetCustomAttributes(attributedata.Key, false);

                if (att != null && att.Length == 1)
                {
                    attributedata.Value(att[0], targetType);
                }
            }
            else if (registerTp == RegisterType.MethodAtt)
            {
                MethodInfo[] methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

                for (int j = 0; j < methods.Length; ++j)
                {
                    object[] att = methods[j].GetCustomAttributes(attributedata.Key, false);
                    if (att != null && att.Length > 0 && methods[j].IsStatic)
                    {
                        attributedata.Value(att[0], methods[j]);
                    }
                    else if (att != null && att.Length > 0 && !methods[j].IsStatic)
                    {
                        LogMgr.LogError("虽然注册了，但是不是静态函数 =>" + methods[j].Name);
                    }
                }
            }
            else if (registerTp == RegisterType.InstacenMethodAttr)
            {
                MethodInfo[] methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                for (int j = 0; j < methods.Length; ++j)
                {
                    object[] att = methods[j].GetCustomAttributes(attributedata.Key, false);
                    if (att != null && att.Length > 0)
                    {
                        attributedata.Value(att[0], methods[j]);
                    }
                }
            }
            else if (registerTp == RegisterType.Register)
            {
                MethodInfo[] methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

                for (int j = 0; j < methods.Length; ++j)
                {
                    object[] att = methods[j].GetCustomAttributes(attributedata.Key, false);
                    if (att != null && att.Length > 0)
                    {
                        attributedata.Value(att[0], new KeyValuePair<BaseAttributeRegister, MethodInfo>(this, methods[j]));
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

            RegisterCachedTypes cachedTypes = ScriptableObject.CreateInstance<RegisterCachedTypes>(); 

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
                    var list = this._attributeHandlers[i];
                    for (int j = 0; j < list.Count; ++j)
                    {
                        for (int m = 0; m < types.Length; ++m)
                        {
                            ReadType(cachedTypes,types[m], list[j], registerTp);
                        }
                    }
                }
            }

            AssetDatabase.CreateAsset(cachedTypes, "Assets/Resources/" + path);
            AssetDatabase.Refresh();
            return cachedTypes;
        }

        private void ReadType(RegisterCachedTypes cachedTypes,Type tp, KeyValuePair<Type, GameAttriHandler> tupledata, RegisterType registerTp)
        {
            if (registerTp == RegisterType.ClassAttr)
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
                            methods[j].Invoke(null, new object[] { this });
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
                        var list = this._attributeHandlers[i];
                        for (int j = 0; j < list.Count; ++j)
                        {
                            KeyValuePair<Type,GameAttriHandler> typedata = list[j];
                            for (int m = 0; m < tuples.TypeList.Count; m++)
                            {
                                if (tuples.TypeList[m].attributename.Equals(typedata.Key.FullName))
                                {
                                    cachedTypes.Analyze(this,i, tuples.TypeList[m], typedata.Key, typedata.Value);
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
            this.InitAttributes(passtp.Assembly);
        }

        protected virtual void Start(Assembly asm)
        {
            this.InitAttributes(asm);
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


