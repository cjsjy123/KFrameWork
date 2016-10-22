#define TOLUA
#define TOLUA_EDIT
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
    public class ScriptLogicCtr  {
        private class ScriptPkg:IDisposable,IPool
        {

            private IScriptLoader _script;

            public IScriptLoader Loader
            {
                get
                {
                    return this._script;
                }
            }
                


            public void AwakeFromPool ()
            {
                
            }

            public void ReleaseToPool ()
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
                catch(Exception ex)
                {
                    LogMgr.LogException(ex);
                    return null;
                }

            }


            public static ScriptPkg Create(MethodInfo method,string MethodName,ScriptTarget t)
            {
                ScriptPkg pkg = null;
                if(KObjectPool.mIns != null)
                {
                    pkg = KObjectPool.mIns.Pop<ScriptPkg>();
                }

                if(pkg == null)
                    pkg = new ScriptPkg();

                Type RetTp=  method.ReturnType;

                if(t == ScriptTarget.Sharp)
                {
                    pkg._script = new SharpScriptLoader();
                    SimpleParams InitParams = SimpleParams.Create(2);
                    if(RetTp == typeof(void))
                    {
                        InitParams.WriteObject(Delegate.CreateDelegate(typeof(Action<AbstractParams>),method));
                    }
                    else
                    {
                        InitParams.WriteObject(Delegate.CreateDelegate(typeof(Func<AbstractParams,AbstractParams>),method));
                    }

                    InitParams.WriteObject(RetTp);

                    pkg.Loader.Init(InitParams);
                }
                else if(t == ScriptTarget.Lua)
                {
                    #if TOLUA
                    pkg._script = new LuaScriptLoader();
                    if(LuaClient.Instance != null)
                    {
                        SimpleParams InitParams = SimpleParams.Create(1);
                        LuaFunction Func =LuaClient.GetMainState().GetFunction(MethodName);
                        InitParams.WriteObject(Func);
                        pkg.Loader.Init(InitParams);
       
                        if(Func == null)
                        {
                            LogMgr.LogErrorFormat("未发现匹配的lua函数 :{0}",MethodName);
                        }
                    }
                    else
                    {
                        LogMgr.Log("场景中未包含LuaClient，但是程序集中包含了带有lua目标的函数的注册");
                    }
                    #endif
                }

                return pkg;
            }

            public static ScriptPkg Create<T>(Action<AbstractParams> method,T value) where T:new()
            {
                ScriptPkg pkg = null;
                if(KObjectPool.mIns != null)
                {
                    pkg = KObjectPool.mIns.Pop<ScriptPkg>();
                }

                if(pkg == null)
                    pkg = new ScriptPkg();
 
                pkg._script = new SharpScriptLoader();

                SimpleParams initParams = SimpleParams.Create(2);
                initParams.WriteObject(method);
                initParams.WriteObject(typeof(void));
                pkg.Loader.Init(initParams);
                pkg.Loader.PushAttachObject(value);

                return pkg;
            }

            public static ScriptPkg Create<T>(Func<AbstractParams,AbstractParams> method,T value) where T:new()
            {
                ScriptPkg pkg = null;
                if(KObjectPool.mIns != null)
                {
                    pkg = KObjectPool.mIns.Pop<ScriptPkg>();
                }

                if(pkg == null)
                    pkg = new ScriptPkg();

                SimpleParams initParams = SimpleParams.Create(2);
                initParams.WriteObject(method);
                initParams.WriteObject(typeof(AbstractParams));
                pkg.Loader.Init(initParams);

                pkg.Loader.Init(initParams);
                pkg.Loader.PushAttachObject(value);

                return pkg;
            }
        }


        public static ScriptLogicCtr mIns;

        private Queue<ICommond> CommondQueue = new Queue<ICommond>(64);

        private Dictionary<int ,Dictionary<int,ScriptPkg>> ScriptDic = new Dictionary<int, Dictionary<int, ScriptPkg>>();


        public ScriptLogicCtr()
        {
            //MainLoop.getLoop().RegisterLoopEvent(LoopMonoEvent.LateUpdate,DispathCommond);
        }

        public void RegisterLogicFunc<T>(int CMD, Action<AbstractParams> callback,T value) where T:new()
        {
            if(FrameWorkDebug.Open_DEBUG)
            {
                System.Delegate d = callback;
                if(!d.Method.IsStatic)
                {
                    throw new ArgumentException("必须为静态函数");
                }

            }
                
            if(ScriptDic.ContainsKey(CMD))
            {
                Dictionary<int,ScriptPkg> dic = this.ScriptDic[CMD];
                if(dic.ContainsKey((int)ScriptTarget.Sharp))
                {
                    dic[(int)ScriptTarget.Sharp].Loader.PushAttachObject(value);
                }
                else
                {
                    ScriptPkg pkg =  ScriptPkg.Create(callback,value);
                    dic.Add((int)ScriptTarget.Sharp,pkg);
                }
            }
            else
            {
                ScriptPkg pkg =  ScriptPkg.Create(callback,value);
                Dictionary<int,ScriptPkg> dic = new Dictionary<int, ScriptPkg>();
                dic.Add((int)ScriptTarget.Sharp,pkg);

                this.ScriptDic.Add(CMD,dic);
            }
        }

        public void RegisterLogicFunc(MethodInfo method,int CMD,string MethodName,ScriptTarget target = ScriptTarget.Sharp)
        {

            if(ScriptDic.ContainsKey(CMD))
            {
                Dictionary<int,ScriptPkg> dic = this.ScriptDic[CMD];
                if(dic.ContainsKey((int)target))
                {
                    LogMgr.LogErrorFormat("重复注册逻辑函数 {0}",CMD);
                }
                else
                {
                    ScriptPkg pkg =  ScriptPkg.Create(method,MethodName,target);
                    dic.Add((int)target,pkg);
                }
            }
            else
            {
                ScriptPkg pkg =  ScriptPkg.Create(method,MethodName,target);
                Dictionary<int,ScriptPkg> dic = new Dictionary<int, ScriptPkg>();
                dic.Add((int)target,pkg);

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

        public void UnRegisterLogicFunc<T>(int CMD,T value) where T:new()
        {
            if(this.ScriptDic.ContainsKey(CMD))
            {
                Dictionary<int,ScriptPkg> dic = this.ScriptDic[CMD];
                if(dic.ContainsKey((int)ScriptTarget.Sharp))
                {
                    ScriptPkg pkg = dic[(int)ScriptTarget.Sharp];
                    if(pkg.Loader.CanDispose)
                    {
                        pkg.Dispose();
                        dic.Remove((int)ScriptTarget.Sharp);
                    }
                    else
                    {
                        pkg.Loader.RemovettachObject(value);
                    }

                }
            }
        }

        public void PushCommond(ICommond command)
        {

            if(command.CMD.HasValue)
            {
                CommondQueue.Enqueue(command);

                DispathCommond();
            }
            else
            {
                LogMgr.Log("命令消息未空");
            }
        }

        private void DispathCommond()
        {
            try
            {
                while(CommondQueue != null && CommondQueue.Count >0)
                {
                    ICommond cmd = CommondQueue.Dequeue();

                    if(this.ScriptDic.ContainsKey(cmd.CMD.Value))
                    {
                        Dictionary<int,ScriptPkg> dic = this.ScriptDic[cmd.CMD.Value];
                        if(dic.ContainsKey((int)ScriptTarget.Sharp))
                        {
                            if(cmd.HasCallParams)
                            {
                                cmd.ReturnParams = dic[(int)ScriptTarget.Sharp].Invoke(cmd.CallParms);
                            }
                            else
                            {
                                cmd.ReturnParams = dic[(int)ScriptTarget.Sharp].Invoke(null);
                            }


                            if(cmd.ReturnParams == null)
                            {
                                cmd.Release(true);
                            }
                            else
                            {
                                cmd.Release(false);
                            }
                        }
                        else if(dic.ContainsKey((int)ScriptTarget.Lua))
                        {
                            if(cmd.HasCallParams)
                            {
                                cmd.ReturnParams = dic[(int)ScriptTarget.Lua].Invoke(cmd.CallParms);
                            }
                            else
                            {
                                cmd.ReturnParams = dic[(int)ScriptTarget.Lua].Invoke(null);
                            }

                            if(cmd.ReturnParams == null)
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
            catch(Exception ex)
            {
                LogMgr.LogException(ex);
            }

        }

    }
}


