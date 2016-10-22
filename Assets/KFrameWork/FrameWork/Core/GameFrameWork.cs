using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using KUtils;

namespace KFrameWork
{
    public class GameFrameWork 
    {
        /// <summary>
        /// o 为att对象，target 返回类或者方法对象
        /// </summary>
        public delegate void GameAttriHandler(object o, object target);

        public readonly float FrameWorkStartTime ;

        public const string Version ="0.0.01";

        private bool m_binit;
        public bool Inited
        {
            get
            {
                return this.m_binit;
            }
        }

        #region 容器

        private Dictionary<int, List<KeyValuePair<Type, GameAttriHandler>>> _attributeHandlers = new Dictionary<int, List<KeyValuePair<Type, GameAttriHandler>>>();

        #endregion

        public GameFrameWork()
        {
            FrameWorkStartTime = Time.realtimeSinceStartup;
        }

        public void RegisterHandler(RegisterType tp, Type target, GameAttriHandler handler)
        {
            if (!this._attributeHandlers.ContainsKey((int)tp))
            {
                this._attributeHandlers.Add((int)tp, new List<KeyValuePair<Type, GameAttriHandler>>(128));
            }
                

            this._attributeHandlers[(int)tp].TryAdd(new KeyValuePair<Type, GameAttriHandler>(target, handler));
        }

        protected void InitAttributes()
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
//
//            int count = 0;
//            while (types != null && count < types.Length)
//            {
//
//                Type tp = types[count];
//                var iterator = _attributeHandlers.GetEnumerator();
//
//                while (iterator.MoveNext())
//                {
//                    List<KVTuple<Type, GameAttriHandler>> list = iterator.Current.Value;
//                    if (iterator.Current.Key == RegisterType.ClassAttr)
//                    {
//                        for (int i = 0; i < list.Count; ++i)
//                        {
//                            KVTuple<Type, GameAttriHandler> tupledata = list[i];
//                            object[] att = tp.GetCustomAttributes(tupledata.Key, false);
//                            if (att != null && att.Length == 1)
//                            {
//                                tupledata.Value(att[0], tp);
//                            }
//                        }
//                    }
//                    else if (iterator.Current.Key == RegisterType.MethodAtt)
//                    {
//                        for (int i = 0; i < list.Count; ++i)
//                        {
//                            KVTuple<Type, GameAttriHandler> tupledata = list[i];
//
//                            MethodInfo[] methods = tp.GetMethods(BindingFlags.Instance| BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
//
//                            for(int j =0; j < methods.Length;++j)
//                            {
//                                object[] att = methods[j].GetCustomAttributes(tupledata.Key, false);
//                                if (att != null && att.Length >0 && methods[j].IsStatic)
//                                {
//                                    tupledata.Value(att[0], methods[j]);
//                                }
//                                else if(att != null && att.Length >0 && !methods[j].IsStatic)
//                                {
//                                    LogMgr.LogError("虽然注册了，但是不是静态函数 =>"+ methods[j].Name);
//                                }
//
//                            }
//                        }
//                    }
//
//                }
//
//                count++;
//            }
//
//

        }

        public void Initialite()
        {
            try
            {
                AttributeRegister.Register(this);


                this.InitAttributes();
                //remove defualt attributes
                this._attributeHandlers.Clear();
                this.m_binit = true;
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
                this.m_binit =false;
            }

        }


    }
}

