using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using KUtils;

namespace KFrameWork
{
    public abstract class BaseAttributeRegister  {


        /// <summary>
        /// o 为att对象，target 返回类或者方法对象
        /// </summary>
        public delegate void GameAttriHandler(object o, object target);

        #region 容器

        private Dictionary<int, List<KeyValuePair<Type, GameAttriHandler>>> _attributeHandlers = new Dictionary<int, List<KeyValuePair<Type, GameAttriHandler>>>();

        #endregion

        public void RegisterHandler(RegisterType tp, Type target, GameAttriHandler handler)
        {
            if (!this._attributeHandlers.ContainsKey((int)tp))
            {
                this._attributeHandlers.Add((int)tp, new List<KeyValuePair<Type, GameAttriHandler>>(128));
            }


            this._attributeHandlers[(int)tp].TryAdd(new KeyValuePair<Type, GameAttriHandler>(target, handler));
        }

        private void InitAttributes()
        {
            Assembly asm = this.GetType().Assembly;
            Type[] types = asm.GetTypes();
            int i = (int)RegisterType.None;
            int end = (int)RegisterType.END;
            for(;i < end;++i)
            {
                RegisterType registerTp = (RegisterType)i;
                if(this._attributeHandlers.ContainsKey(i))
                {
                    List<KeyValuePair<Type, GameAttriHandler>> list = this._attributeHandlers[i];
                    for(int k =0; k < list.Count;++k)
                    {
                        KeyValuePair<Type, GameAttriHandler> tupledata = list[k];
                        for(int m=0 ;m< types.Length;++m)
                        {
                            Type tp = types[m];
                            if(registerTp == RegisterType.ClassAttr)
                            {
                                object[] att = tp.GetCustomAttributes(tupledata.Key, false);
                                if (att != null && att.Length == 1)
                                {
                                    tupledata.Value(att[0], tp);
                                }
                            }
                            else if(registerTp == RegisterType.MethodAtt)
                            {
                                MethodInfo[] methods = tp.GetMethods(BindingFlags.Instance| BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );

                                for(int j=0; j < methods.Length;++j)
                                {
                                    object[] att = methods[j].GetCustomAttributes(tupledata.Key, false);
                                    if (att != null && att.Length >0 && methods[j].IsStatic)
                                    {
                                        tupledata.Value(att[0], methods[j]);
                                    }
                                    else if(att != null && att.Length >0 && !methods[j].IsStatic)
                                    {
                                        LogMgr.LogError("虽然注册了，但是不是静态函数 =>"+ methods[j].Name);
                                    }

                                }
                            }
                        }
                    }

                }
            }

        }

        protected void Start()
        {
            this.InitAttributes();
        }

        protected void End()
        {
            this._attributeHandlers.Clear();
        }
    }
}


