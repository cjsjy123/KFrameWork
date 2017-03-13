
#define DYNAMIC_REGISTER
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Reflection;

#if TOLUA
using LuaInterface;
#endif

namespace KFrameWork
{

    [SingleTon]
    public sealed class ScriptLogicCtr  {
        private sealed class ScriptPkg:IDisposable,IPool
        {

            private IScriptLoader _script;

            public IScriptLoader Loader
            {
                get
                {
                    return this._script;
                }
            }

            public void RemoveToPool ()
            {
                if(this.Loader != null)
                    this.Loader.Reset();
            }

            public void RemovedFromPool ()
            {
                this._script= null;
            }

            public void Dispose()
            {
                if(KObjectPool.mIns != null)
                    KObjectPool.mIns.Push(this);
            }
                
            public AbstractParams Invoke(AbstractParams ScriptParms)
            {
                try
                {
                    return this.Loader.Invoke(ScriptParms);
                }
                catch(FrameWorkException ex)
                {
                    LogMgr.LogException(ex);

                    ex.RaiseExcption();
                    return null;
                }
                catch(Exception ex)
                {
                    LogMgr.LogException(ex);
                    return null;
                }
            }

            public static ScriptPkg CreateSharp(MethodInfo method, string methodname, object instance = null)
            {
                ScriptPkg pkg = null;
                if(KObjectPool.mIns != null)
                    pkg = KObjectPool.mIns.Pop<ScriptPkg>();

                if (pkg == null)
                    pkg = new ScriptPkg();


                pkg._script = new SharpScriptLoader();
                SimpleParams InitParams = SimpleParams.Create(4);
                if (instance != null)
                {
                    InitParams.WriteObject(instance);
                    var passes = method.GetParameters();
                    if (passes != null && passes.Length ==1 && passes[0].ParameterType.IsSubclassOf(typeof(AbstractParams)))
                    {
                        throw new FrameWorkException("method mismatching ");
                    }

                    InitParams.WriteObject(method);
                }
                else
                {
                    Type RetTp = method.ReturnType;

                    if (RetTp == typeof(void))
                    {
                        InitParams.WriteObject(Delegate.CreateDelegate(typeof(Action<AbstractParams>), method));
                    }
                    else
                    {
                        InitParams.WriteObject(Delegate.CreateDelegate(typeof(Func<AbstractParams, AbstractParams>), method));
                    }
                    InitParams.WriteObject(RetTp);
                    InitParams.WriteString(methodname);
                }

                pkg.Loader.Init(InitParams);

                return pkg;
            }
#if TOLUA
            public static ScriptPkg CreateLua( object attvalue)
            {
                ScriptPkg pkg = null;
                if (KObjectPool.mIns != null)
                    pkg = KObjectPool.mIns.Pop<ScriptPkg>();

                if (pkg == null)
                    pkg = new ScriptPkg();

                pkg._script = new LuaScriptLoader();
                if (LuaClient.Instance != null)
                {
                    SimpleParams InitParams = SimpleParams.Create(1);
                    Script_LuaLogicAttribute luaAttribute = attvalue as Script_LuaLogicAttribute;
                    InitParams.WriteObject(luaAttribute);
                    pkg.Loader.Init(InitParams);
                }
                else
                {
                    LogMgr.Log("场景中未包含LuaClient，但是程序集中包含了带有lua目标的函数的注册");
                }


                return pkg;
            }
#endif
        }


        public static ScriptLogicCtr mIns;

        private Queue<ScriptCommand> CommandQueue = new Queue<ScriptCommand>(64);

        private Dictionary<int ,Dictionary<int,ScriptPkg>> ScriptDic = new Dictionary<int, Dictionary<int, ScriptPkg>>();

        public void RegisterLogicFunc(MethodInfo method,int CMD,string MethodName,object attvalue,ScriptTarget target )
        {
            if (ScriptDic.ContainsKey(CMD))
            {
                Dictionary<int,ScriptPkg> dic = this.ScriptDic[CMD];
                if(dic.ContainsKey((int)target))
                {
                    LogMgr.LogErrorFormat("重复注册逻辑函数 {0}",CMD);
                }
                else
                {
                    if (target == ScriptTarget.Sharp)
                    {
                        dic[(int)target] = ScriptPkg.CreateSharp(method, MethodName);
                    }
                    else if(target == ScriptTarget.Lua)
                    {
#if TOLUA
                        dic[(int)target] = ScriptPkg.CreateLua(attvalue);
#else
                        throw new FrameWorkException("Missing Lua");
#endif
                    }
                }
            }
            else
            {
                Dictionary<int,ScriptPkg> dic = new Dictionary<int, ScriptPkg>();
                if (target == ScriptTarget.Sharp)
                {
                    dic[(int)target] = ScriptPkg.CreateSharp(method, MethodName);
                }
                else if (target == ScriptTarget.Lua)
                {
#if TOLUA
                    dic[(int)target] = ScriptPkg.CreateLua(attvalue);
#else
                    throw new FrameWorkException("Missing Lua");
#endif
                }

                this.ScriptDic.Add(CMD,dic);
            }

        }
        /// <summary>
        /// 强制移除
        /// </summary>
        /// <param name="CMD">CM.</param>
        /// <param name="target">Target.</param>
        public void UnRegisterLogicFunc(int CMD,ScriptTarget target = ScriptTarget.Sharp)
        {
            if(this.ScriptDic.ContainsKey(CMD))
            {
                Dictionary<int,ScriptPkg> dic = this.ScriptDic[CMD];
                if(dic.ContainsKey((int)target))
                {
                    dic[(int)target].Dispose();
                    dic.Remove((int)target);
                }
            }
        }

        public void PushCommand(ScriptCommand command)
        {

            if( !command.isDone)
            {
                CommandQueue.Enqueue(command);

                DispathCommand();
            }
            else 
            {
                LogMgr.LogError("消息已经完成");
            }

        }

        private void RegisterLua(ScriptCommand cmd, string path, string methodname)
        {
#if TOLUA
            Script_LuaLogicAttribute att = new Script_LuaLogicAttribute(cmd.CMD, path, methodname);
            ScriptPkg pkg = ScriptPkg.CreateLua(att);
            //add
            var dic = new Dictionary<int, ScriptPkg>();
            dic.Add((int)cmd.target, pkg);
            ScriptDic[cmd.CMD] = dic;
#endif
        }

        private void Registernew(ScriptCommand cmd)
        {
            if ( cmd.InitParams.NextValue() == ParamType.STRING)
            {
                string classname = cmd.InitParams.ReadString();
                if (cmd.InitParams.NextValue() == ParamType.STRING)
                {
                    string methodname = cmd.InitParams.ReadString();
                    //csharp
                    if (cmd.target == ScriptTarget.Sharp)
                    {
                        Type type = Type.GetType(classname);
                        if (type != null)
                        {
                            MethodInfo method = type.GetMethod(methodname, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                            if (method != null)
                            {
                                ScriptPkg pkg = ScriptPkg.CreateSharp(method, methodname);
                                //add

                                if (!ScriptDic.ContainsKey(cmd.CMD))
                                {
                                    var dic = new Dictionary<int, ScriptPkg>();
                                    dic.Add((int)cmd.target, pkg);
                                    ScriptDic[cmd.CMD] = dic;
                                }
                                else
                                {
                                    ScriptDic[cmd.CMD][(int)cmd.target] = pkg;
                                }
                            }
                            else
                            {
                                LogMgr.LogWarningFormat("Missing method :{0} in {1}", methodname, classname);
                            }
                        }
                        else
                        {
                            LogMgr.LogWarningFormat("Missing Type :{0}", classname);
                        }
                    }
                    else if (cmd.target == ScriptTarget.Lua)
                    {
                        RegisterLua(cmd, classname, methodname);
                    }

                }
                else
                {
                    LogMgr.LogWarningFormat("cant register automatically :{0} for {1}", cmd.CMD,classname);
                }
            }
            else
            {
                object o = null;
                if (cmd.InitParams.NextValue() == ParamType.UNITYOBJECT)
                {
                    o = cmd.InitParams.ReadUnityObject();
                }
                else if (cmd.InitParams.NextValue() == ParamType.OBJECT)
                {
                    o = cmd.InitParams.ReadObject();
                }

                if (o != null)
                {
                    if (cmd.InitParams.NextValue() == ParamType.STRING)
                    {
                        string classname = cmd.InitParams.ReadString();
                        if (cmd.InitParams.NextValue() == ParamType.STRING)
                        {
                            string methodname = cmd.InitParams.ReadString();
                            if (cmd.target == ScriptTarget.Sharp)
                            {
                                Type type = Type.GetType(classname);
                                if (type != null)
                                {
                                    MethodInfo method = type.GetMethod( methodname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                                    if (method != null)
                                    {
                                        ScriptPkg pkg = ScriptPkg.CreateSharp(method, methodname,o);
                                        //add

                                        if (!ScriptDic.ContainsKey(cmd.CMD))
                                        {
                                            var dic = new Dictionary<int, ScriptPkg>();
                                            dic.Add((int)cmd.target, pkg);
                                            ScriptDic[cmd.CMD] = dic;
                                        }
                                        else
                                        {
                                            ScriptDic[cmd.CMD][(int)cmd.target] = pkg;
                                        }
                                    }
                                    else
                                    {
                                        LogMgr.LogWarningFormat("Missing method :{0} in {1}", methodname, classname);
                                    }
                                }
                                else
                                {
                                    LogMgr.LogWarningFormat("Missing Type :{0}", classname);
                                }
                            }
                            else if (cmd.target == ScriptTarget.Lua)
                            {
                                RegisterLua( cmd, classname, methodname);
                            }
                        }
                        else
                        {
                            LogMgr.LogWarningFormat("Missing Type :{0}", classname);
                        }
                    }
                    else
                    {
                        LogMgr.LogWarningFormat("cant register automatically :{0} ", cmd.CMD);
                    }
                }
            }
        }

        private void DispathCommand()
        {
            try
            {
                while(CommandQueue != null && CommandQueue.Count >0)
                {
                    ScriptCommand cmd = CommandQueue.Dequeue();

                    if(cmd.isDone)
                        continue;

#if DYNAMIC_REGISTER
                    if (cmd.HasInitParams)
                    {
                        if (FrameWorkConfig.Open_DEBUG)
                            LogMgr.Log("will make a loader for script");

                        Registernew(cmd);
                    }
#endif

                    if (this.ScriptDic.ContainsKey(cmd.CMD))
                    {
                        Dictionary<int,ScriptPkg> dic = this.ScriptDic[cmd.CMD];
                        int target= (int)cmd.target;

                        //double check
                        if (dic.ContainsKey(target))
                        {
                            if (cmd.HasCallParams)
                            {
                                cmd.ReturnParams = dic[target].Invoke(cmd.CallParams);
                            }
                            else
                            {
                                cmd.ReturnParams = dic[target].Invoke(null);
                            }

                            if (cmd.ReturnParams == null)
                            {
                                cmd.Release(true);
                            }
                            else
                            {
                                cmd.Release(false);
                            }
                        }

                    }
                    else
                    {
                        LogMgr.LogFormat("命令消息错误:{0}，未发现匹配的函数",cmd.CMD);
                    }

                }
            }
            catch(FrameWorkException ex)
            {
                LogMgr.LogException(ex);

                ex.RaiseExcption();
            }
            catch(Exception ex)
            {
                LogMgr.LogException(ex);
            }

        }

    }
}


