using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Priority_Queue;
using KUtils;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
#if Advance
using AdvancedInspector;
#endif
#if UNITY_5_5 || UNITY_5_4
using UnityEngine.SceneManagement;
#endif

namespace KFrameWork
{
#if Advance
    [AdvancedInspector]
#endif
    [TimeSetAttribute(1f/FrameWorkConfig.PHYSTEP_VALUE)]
    [ScriptInitOrder(-9999)]
    public sealed class MainLoop : MonoBehaviour
    {
#if Advance
        [Inspect(2)]
        public bool AwakeLogMode = true;

        [Inspect(1)]
        public bool RunningLogmode
        {
            get
            {
                return LogMgr.OpenLog;
            }
        }
            
#else
        public bool AwakeLogMode = true;
#endif
#if Advance
        [Inspect(3)]
        public bool OpenFps = true;
#else
        public bool OpenFps = true;
#endif

#if Advance
        [Inspect(3)]
        public bool OpenLua = true;
#else
        public bool OpenLua = true;
#endif
        public const string Version = "0.0.200a";
        private FrameworkAttRegister framework;

        private Dictionary<int, Action<int>> eventList = new Dictionary<int, Action<int>>();

        private Dictionary<int, StaticDelegate> methodDic= new Dictionary<int, StaticDelegate>();

        private Dictionary<int, InstanceCacheDelegate> attEvents = new Dictionary<int, InstanceCacheDelegate>();
        /// <summary>
        /// 异步初始化完成事件分发
        /// </summary>
        public Action AsyncDoneEvent;

        public Action FrameWorkEvent;
#if Advance
        [Inspect(6),Descriptor("异步初始化","开启异步初始化")]
#endif
        public bool AsyncInit = false;

        private Thread thread;

        private int flag =0;

        public bool HasInit
        {
            get
            {
                return flag ==2;
            }
        }

        private static MainLoop _mIns;
#if Advance
        [Inspect(100)]
        public void Rename()
        {
            this.gameObject.name = "KFrameWork";
        }

        [Inspect(101)]
        public void OpenLogBtn()
        {
            LogMgr.OpenLog = !LogMgr.OpenLog;
        }
#endif
        #if Advance
        [Inspect(7)]
        #endif
        public bool LoadFromCache =false;

        private RegisterCachedTypes cached;

        private string MainLoopAsset = "MainLoopAsset";

        private Stopwatch initwatch ;

#if Advance
        [Inspect(102)]
        public void SaveTypeCache()
        {
#if UNITY_EDITOR
            LogMgr.Log("开始打包类型缓存");
            framework = new FrameworkAttRegister();
            framework.SaveToCache(MainLoopAsset);
            LogMgr.Log("结束打包类型缓存");
#endif
        }
#else
        public void SaveTypeCache()
        {
#if UNITY_EDITOR
            framework = new FrameworkAttRegister();
            framework.SaveToCache(MainLoopAsset);
#endif
        }
#endif
        public static MainLoop getLoop()
        {
            return _mIns;
        }

        public static void Destroy()
        {
            if (_mIns != null)
            {
                _mIns.framework = null;
                _mIns.eventList.Clear();
                _mIns.attEvents.Clear();
                _mIns.methodDic.Clear();
                _mIns.eventList = null;
                _mIns.attEvents = null;
                _mIns.methodDic = null;
                GameObject.Destroy(_mIns);
                _mIns = null;
            }
        }
            

        void Awake()
        {
            LogMgr.OpenLog = this.AwakeLogMode;
            GameObject fkObject = GameObject.Find("KFrameWork");

            if (fkObject == null)
            {
                this.gameObject.name = "KFrameWork";
            }

            if(Application.isPlaying)
                DontDestroyOnLoad(this.gameObject);

            _mIns = this;
            initwatch = new Stopwatch();
            initwatch.Start();

            FrameWorkConfig.LoadConfig(this);
            if (AsyncInit)
            {
                cached = Resources.Load<RegisterCachedTypes>(MainLoopAsset);
                thread = new Thread(AsyncWork);
                thread.Start();
                //ThreadPool.QueueUserWorkItem(AsyncWork);
            }
            else
            {
                _Init();
                CheckEvents();
            }
        }

        void AsyncWork(System.Object o)
        {
            _Init();
        }

        void Start()
        {
#if UNITY_5_5 || UNITY_5_4
            SceneManager.sceneLoaded += this.OnSceneLoad;
#endif
        }

        private void _Init()
        {

            Stopwatch watch = Stopwatch.StartNew();  
            watch.Start();

            framework = new FrameworkAttRegister();
            if(LoadFromCache)
            {
                if(AsyncInit)
                    framework.LoadFromCache(cached);
                else
                    framework.LoadFromCache(MainLoopAsset);
            }
            else
                framework.Initialite();

            flag++;

            watch.Stop();

            if(FrameWorkConfig.Open_DEBUG)
                LogMgr.Log(":::Framework init Finished");

            LogMgr.LogFormat("::::FrameWork Cost Time :{0}",watch.Elapsed.TotalSeconds.ToString());
        }


        private void _tryCall(MainLoopEvent em, int val = -1)
        {
            try
            {
                int e = (int)em;

                if (this.methodDic.ContainsKey(e))
                {
                    this.methodDic[e].Invoke(val);
                }

                if (eventList.ContainsKey(e))
                {
                    if (FrameWorkConfig.Open_DEBUG)
                    {
                        var list = eventList[e].GetInvocationList();
                        for (int i = 0; i < list.Length; ++i)
                        {
                            list[i].DynamicInvoke(val);
                        }
                    }
                    else
                    {
                        if (eventList[e] != null)
                            eventList[e](val);
                    }
            
                }

                if (this.attEvents.ContainsKey(e))
                {
                    if (attEvents[e] != null)
                        attEvents[e].Invoke(e);
                }

            }
            catch (FrameWorkException ex)
            {
                LogMgr.LogException(ex);

                ex.RaiseExcption();
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
            }


        }

        public void PreRegisterCachedAction(MainLoopEvent em, Action<System.Object, int> act)
        {
            int e = (int)em;
            if (this.attEvents.ContainsKey(e))
            {
                InstanceCacheDelegate d = this.attEvents[e];
                d.PreAdd(act);
            }
            else
            {
                InstanceCacheDelegate d = new InstanceCacheDelegate();
                d.PreAdd(act);
                this.attEvents.Add(e, d);
            }
        }

        public void RegisterCachedAction(MainLoopEvent em, int id, System.Object ins)
        {
            int e = (int)em;

            if (this.attEvents.ContainsKey(e))
            {
                InstanceCacheDelegate d = this.attEvents[e];
                d.Add(id, ins);
            }
            else
            {
                InstanceCacheDelegate d = new InstanceCacheDelegate();
                d.Add(id, ins);
                this.attEvents.Add(e, d);
            }
        }

        public void UnRegisterCachedAction(MainLoopEvent em, int id, System.Object ins)
        {
            int e = (int)em;
            if (this.attEvents.ContainsKey(e))
            {
                InstanceCacheDelegate d = this.attEvents[e];
                d.Remove(id, ins);
            }
        }

        public void UnRegisterCachedAction(MainLoopEvent em, int id)
        {
            int e = (int)em;
            if (this.attEvents.ContainsKey(e))
            {
                InstanceCacheDelegate d = this.attEvents[e];
                d.Remove(id);
            }
        }

        public void RegisterLoopEvent(MainLoopEvent em, Action<int> d, bool first = false)
        {
            int e = (int)em;
            if (eventList.ContainsKey(e))
            {
                if (!first)
                {
                    this.eventList[e] += d;
                }
                else
                {
                    Action<int> old = this.eventList[e];
                    d += old;
                    this.eventList[e] = d;
                }
            }
            else
            {
                this.eventList[e] = d;
            }
        }

        public void UnRegisterLoopEvent(MainLoopEvent em, MethodInfo method)
        {
            int e = (int)em;
            if (eventList.ContainsKey(e))
            {
                this.eventList[e] -= (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), method);
            }
        }

        public void UnRegisterLoopEvent(MainLoopEvent em, Action<int> d)
        {
            int e = (int)em;
            if (eventList.ContainsKey(e))
            {
                this.eventList[e] -= d;
            }
        }

        public void RegisterStaticEvent(MainLoopEvent em, MethodInfo method, int Priority =0)
        {
            int e = (int)em;
            Action<int> func = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), method);
            if (this.methodDic.ContainsKey(e))
            {
                this.methodDic[e].Add(func, Priority);
            }
            else
            {
                StaticDelegate d = new StaticDelegate();
                d.Add(func, Priority);
                this.methodDic.Add(e,d);
            }
        }

        public void UnRegisterStaticEvent(MainLoopEvent em, MethodInfo method)
        {
            int e = (int)em;
            if (methodDic.ContainsKey(e))
            {
                Action<int> func = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), method);
                methodDic[e].Rmove(func);
            }
        }

        void Syncwithevents()
        {
            this.framework.InitFrameWorkModuldes(this);
#if UNITY_EDITOR
            Scene s = EditorSceneManager.GetActiveScene();
            if (s.IsValid())
                this._tryCall(MainLoopEvent.OnLevelWasLoaded, s.buildIndex);
            else
                this._tryCall(MainLoopEvent.OnLevelWasLoaded, 0);
#else
            this._tryCall(MainLoopEvent.OnLevelWasLoaded, 0);
#endif

            if (FrameWorkEvent != null)
                FrameWorkEvent();

            if(AsyncDoneEvent != null)
                AsyncDoneEvent();

            AsyncDoneEvent = null;
            FrameWorkEvent =null;

            if(AsyncInit)
                this._tryCall(MainLoopEvent.Start);
        }

        void CheckEvents()
        {
            if (flag == 1)
            {
                Syncwithevents();
                flag ++;

                this._tryCall(MainLoopEvent.Start);

                initwatch.Stop();

                LogMgr.LogFormat("::::MainLoop All Inited Cost Time :{0}", initwatch.Elapsed.TotalSeconds.ToString());
            }
        }

        void Update()
        {
            CheckEvents();

            if (this.HasInit && GameSyncCtr.mIns.DetermineEnableFrame())
            {
                this._tryCall(MainLoopEvent.BeforeUpdate);

                this._tryCall(MainLoopEvent.Update);

                this._tryCall(MainLoopEvent.AfterUpdate);
            }
        }

        void LateUpdate()
        {
            if (this.HasInit && GameSyncCtr.mIns.DetermineEnableFrame())
            {
                this._tryCall(MainLoopEvent.LateUpdate);
            }
                
        }

        void FixedUpdate()
        {
            this._tryCall(MainLoopEvent.FixedUpdate);
        }

        void OnApplicationQuit()
        {
            this._tryCall(MainLoopEvent.OnApplicationQuit);
            if(AsyncInit && this.thread != null && this.thread.ThreadState == System.Threading.ThreadState.Running)
            {
                if(FrameWorkConfig.Open_DEBUG)
                    LogMgr.Log("发现还在运行的线程，强制停止！");
                this.thread.Abort();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (FrameWorkConfig.Open_DEBUG)
            {
                if (pauseStatus)
                {
                    LogMgr.Log("游戏暂停");
                }
                else
                {
                    LogMgr.Log("游戏唤醒");
                }
            }

            this._tryCall(MainLoopEvent.OnApplicationPause, pauseStatus ? 1 : 0);
        }

        void OnApplicationFocus(bool focusStatus)
        {
            this._tryCall(MainLoopEvent.OnApplicationFocus, focusStatus ? 1 : 0);
        }

#if UNITY_5_5 || UNITY_5_4
        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            this._tryCall(MainLoopEvent.OnLevelLeaved, GameSceneCtr.mIns.CurScene.buildIndex);

            this._tryCall(MainLoopEvent.OnLevelWasLoaded, GameSceneCtr.mIns.nextScene.buildIndex);
        }
#else
        void OnLevelWasLoaded(int level)
        {
            this._tryCall(MainLoopEvent.OnLevelLeaved, GameSceneCtr.mIns.CurScene.buildIndex);

            this._tryCall(MainLoopEvent.OnLevelWasLoaded, GameSceneCtr.mIns.nextScene.buildIndex);
        }
#endif

        void OnEnable()
        {
            this._tryCall(MainLoopEvent.OnEnable);
        }

        void OnDisable()
        {
            this._tryCall(MainLoopEvent.OnDisable);
        }

        void OnDestroy()
        {
            this._tryCall(MainLoopEvent.OnLevelLeaved, GameSceneCtr.mIns.CurScene.buildIndex);

            this._tryCall(MainLoopEvent.OnDestroy);
#if UNITY_5_5 || UNITY_5_4
            SceneManager.sceneLoaded -= this.OnSceneLoad;
#endif
            if (FrameWorkConfig.Open_DEBUG)
            {
                if (this.methodDic != null)
                {
                    var en = methodDic.GetEnumerator();
                    while (en.MoveNext())
                    {
                        en.Current.Value.Dump((MainLoopEvent)en.Current.Key);
                    }
                }

                if (this.attEvents != null)
                {
                    var en = attEvents.GetEnumerator();
                    while (en.MoveNext())
                    {
                        en.Current.Value.Dump((MainLoopEvent)en.Current.Key);
                    }
                }

                if (this.eventList != null)
                {
                    var en = eventList.GetEnumerator();
                    while (en.MoveNext())
                    {
                        Action<int> act = en.Current.Value;
                        if (act != null)
                        {
                            Delegate d = act as Delegate;
                            Delegate[] delegates = d.GetInvocationList();
                            for (int i = 0; i < delegates.Length; ++i)
                            {
                                LogMgr.LogWarningFormat(" eventList {0} in {1} not clear at:{2}", delegates[i].Method.Name, delegates[i].Method.DeclaringType,(MainLoopEvent)en.Current.Key);
                            }
                        }
                    }
                }
            }

        }
    }

}

