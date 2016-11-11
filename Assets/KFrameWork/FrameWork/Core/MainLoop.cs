//#define KDEBUG
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_5_5 || UNITY_5_4
using UnityEngine.SceneManagement;
#endif

namespace KFrameWork
{
    [ScriptInitOrder(-10000)]
    public sealed class MainLoop : MonoBehaviour
    {
        public bool OpenLog =true;

        public bool OpenFps =true;

        private GameFrameWork framework;

        private Dictionary<int,Action<int>> eventList = new Dictionary<int, Action<int>>();

        private Dictionary<int,StaticCacheDelegate> attEvents = new Dictionary<int, StaticCacheDelegate>();

        private static MainLoop _mIns;

        public static MainLoop getLoop()
        {
            return _mIns ;
        }

        void Awake()
        {

            LogMgr.OpenLog =this.OpenLog;
            #if KDEBUG
            GameObject fkObject = GameObject.Find("KFrameWork");

            if(fkObject == null)
            {
            #endif
                DontDestroyOnLoad(this.gameObject);
                this.gameObject.name ="KFrameWork";

                _mIns = this;
                _Init();

                this._tryCall(MainLoopEvent.Awake);
                eventList.Remove((int)MainLoopEvent.Awake);
                #if KDEBUG
            }
            else
            {
                LogMgr.Log("Dupliate Framework");

            }
                #endif
        }

        void Start()
        {
            this._tryCall(MainLoopEvent.Start);

        }

        private void Preload()
        {
            for(int i = (int)MainLoopEvent.Awake;i <(int) MainLoopEvent.END;++i)
            {
                this.attEvents.Add(i,new StaticCacheDelegate());
            }
        }

        private void _Init()
        {

            this.Preload();

            framework = new GameFrameWork();
            framework.Initialite();

            if(this.OpenFps)
            {
                this.gameObject.AddComponent<FPS>();
            }


        }


        private void _tryCall(MainLoopEvent em,int val =-1)
        {
            try
            {
                int e = (int)em;
                if(eventList.ContainsKey(e))
                {
                    if(eventList[e] != null)
                        eventList[e](val);
                }

                if(this.attEvents.ContainsKey(e))
                {
                    if(attEvents[e] != null)
                        attEvents[e].Invoke(e);
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

        public void PreRegisterCachedAction(MainLoopEvent em,Action<System.Object,int> act )
        {
            int e = (int)em;
            if(this.attEvents.ContainsKey(e))
            {
                StaticCacheDelegate d = this.attEvents[e];
                d.PreAdd(act);

            }
            else
            {
                StaticCacheDelegate d = new StaticCacheDelegate();
                d.PreAdd(act);
                this.attEvents.Add(e,d);

            }
        }

        public void RegisterCachedAction(MainLoopEvent em,int id,System.Object ins )
        {
            int e = (int)em;

            if(this.attEvents.ContainsKey(e))
            {
                StaticCacheDelegate d = this.attEvents[e];
                d.Add(id,ins);
            }
            else
            {
                StaticCacheDelegate d = new StaticCacheDelegate();
                d.Add(id, ins);
                this.attEvents.Add(e, d);

            }

        }

        public void UnRegisterCachedAction(MainLoopEvent em,int id,System.Object ins )
        {
            int e = (int)em;
            if(this.attEvents.ContainsKey(e))
            {
                StaticCacheDelegate d = this.attEvents[e];
                d.Remove(id,ins);
            }
        }

        public void RegisterLoopEvent(MainLoopEvent em , Action<int> d,bool first =false)
        {
            int e = (int)em;
            if(eventList.ContainsKey(e))
            {
                if(!first)
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
                this.eventList[e]= d;
            }
        }

        public void UnRegisterLoopEvent(MainLoopEvent em, Action<int> d)
        {
            int e = (int)em;
            if(eventList.ContainsKey(e))
            {
                this.eventList[e] -= d;
            }
        }

        public void RegisterLoopEvent(MainLoopEvent em , MethodInfo d,bool first =false)
        {
            int e = (int)em;
            Action<int> func = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),d);
            if(this.eventList.ContainsKey(e))
            {
                if(!first)
                {
                    this.eventList[e]+= func;
                }
                else
                {
                    Action<int> old = this.eventList[e];
                    func += old;
                    this.eventList[e] = func;
                }

            }
            else
            {
                this.eventList.Add(e,func);
            }
        }


        void Update()
        {
            this._tryCall(MainLoopEvent.BeforeUpdate);

            this._tryCall(MainLoopEvent.Update);
        }

        void LateUpdate()
        {
            this._tryCall(MainLoopEvent.LateUpdate);
        }

        void FixedUpdate()
        {
            this._tryCall(MainLoopEvent.FixedUpdate);
        }

        void OnApplicationQuit()
        {
            this._tryCall(MainLoopEvent.OnApplicationQuit);
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if(FrameWorkDebug.Open_DEBUG)
            {
                if(pauseStatus)
                {
                    LogMgr.Log("游戏暂停");
                }
                else
                {
                    LogMgr.Log("游戏唤醒");
                }
            }

            this._tryCall(MainLoopEvent.OnApplicationPause,pauseStatus?1:0);
        }

        void OnApplicationFocus(bool focusStatus)
        {
            this._tryCall(MainLoopEvent.OnApplicationFocus,focusStatus?1:0);
        }

#if UNITY_5_5 || UNITY_5_4
        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (SceneCtr.mIns.CurScene != null)
            {
                this._tryCall(MainLoopEvent.OnlevelLeaved, (int)SceneCtr.mIns.CurScene);

                this._tryCall(MainLoopEvent.OnLevelWasLoaded, (int)SceneCtr.mIns.nextScene);
            }
            else
            {
                if (SceneCtr.DefaultScene == null)
                    this._tryCall(MainLoopEvent.OnLevelWasLoaded, scene.buildIndex);
                else
                    this._tryCall(MainLoopEvent.OnLevelWasLoaded, (int)SceneCtr.DefaultScene);
            }
        }
#else
        void OnLevelWasLoaded(int level)
        {
            if(SceneCtr.mIns.CurScene != null)
            {
                this._tryCall(MainLoopEvent.OnlevelLeaved,(int)SceneCtr.mIns.CurScene);

                this._tryCall(MainLoopEvent.OnLevelWasLoaded,(int)SceneCtr.mIns.nextScene);
            }
            else
            {
                if(SceneCtr.DefaultScene == null)
                    this._tryCall(MainLoopEvent.OnLevelWasLoaded,level);
                else
                    this._tryCall(MainLoopEvent.OnLevelWasLoaded,(int)SceneCtr.DefaultScene);
            }


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
            this._tryCall(MainLoopEvent.OnDestroy);
        }
    }

}

