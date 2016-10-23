//#define KDEBUG
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace KFrameWork
{
    [ScriptInitOrderAtt(-10000)]
    public class MainLoop : MonoBehaviour
    {
        public bool OpenLog =true;

        public bool OpenFps =true;

        public delegate void LoopDelgate(int value);

        private GameFrameWork framework;

        private Dictionary<int,LoopDelgate> eventList = new Dictionary<int, LoopDelgate>();

        private Dictionary<int,Action<int>> attEventList = new Dictionary<int, Action<int>>();

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

                this._tryCall(LoopMonoEvent.Awake);
                eventList.Remove((int)LoopMonoEvent.Awake);
                attEventList.Remove((int)LoopMonoEvent.Awake);
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
            this._tryCall(LoopMonoEvent.Start);

        }

        private void _Init()
        {
            framework = new GameFrameWork();
            framework.Initialite();

            if(this.OpenFps)
            {
                this.gameObject.AddComponent<FPS>();
            }

        }


        private void _tryCall(LoopMonoEvent em,int val =-1)
        {
            try
            {
                int e = (int)em;
                if(eventList.ContainsKey(e))
                {
                    if(eventList[e] != null)
                        eventList[e](val);
                }

                if(attEventList.ContainsKey(e))
                {
                    if(attEventList[e] != null)
                        attEventList[e](val);
                }
            }
            catch(Exception ex)
            {
                LogMgr.LogException(ex);
            }


        }

        public void RegisterLoopEvent(LoopMonoEvent em , LoopDelgate d,bool first =false)
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
                    LoopDelgate old = this.eventList[e];
                    d += old;
                    this.eventList[e] = d;

                }
               
            }
            else
            {
                this.eventList[e]= d;
            }
        }

        public void UnRegisterLoopEvent(LoopMonoEvent em, LoopDelgate d)
        {
            int e = (int)em;
            if(eventList.ContainsKey(e))
            {
                this.eventList[e] -= d;
            }
        }

        public void RegisterLoopEvent(LoopMonoEvent em , MethodInfo d,bool first =false)
        {
            int e = (int)em;
            Action<int> func = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),d);
            if(attEventList.ContainsKey(e))
            {
                if(!first)
                {
                    this.attEventList[e]+= func;
                }
                else
                {
                    Action<int> old = this.attEventList[e];
                    func += old;
                    this.attEventList[e] = func;
                }

            }
            else
            {
                this.attEventList.Add(e,func);
            }
        }


        void Update()
        {
            this._tryCall(LoopMonoEvent.BeforeUpdate);

            this._tryCall(LoopMonoEvent.Update);
        }

        void LateUpdate()
        {
            this._tryCall(LoopMonoEvent.LateUpdate);
        }

        void FixedUpdate()
        {
            this._tryCall(LoopMonoEvent.FixedUpdate);
        }

        void OnApplicationQuit()
        {
            this._tryCall(LoopMonoEvent.OnApplicationQuit);
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

            this._tryCall(LoopMonoEvent.OnApplicationPause,pauseStatus?1:0);
        }

        void OnApplicationFocus(bool focusStatus)
        {
            this._tryCall(LoopMonoEvent.OnApplicationFocus,focusStatus?1:0);
        }

        void OnLevelWasLoaded(int level)
        {
            if(SceneCtr.mIns.CurScene != null)
            {
                this._tryCall(LoopMonoEvent.OnlevelLeaved,(int)SceneCtr.mIns.CurScene);

                this._tryCall(LoopMonoEvent.OnLevelWasLoaded,(int)SceneCtr.mIns.nextScene);
            }
            else
            {
                if(SceneCtr.DefaultScene == null)
                    this._tryCall(LoopMonoEvent.OnLevelWasLoaded,level);
                else
                    this._tryCall(LoopMonoEvent.OnLevelWasLoaded,(int)SceneCtr.DefaultScene);
            }


        }

        void OnEnable()
        {
            this._tryCall(LoopMonoEvent.OnEnable);
        }

        void OnDisable()
        {
            this._tryCall(LoopMonoEvent.OnDisable);
        }

        void OnDestroy()
        {
            this._tryCall(LoopMonoEvent.OnDestroy);
        }
    }

}

