using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;

namespace KFrameWork
{

    [SingleTon]
    public sealed class GameSyncCtr
    {
        public static GameSyncCtr mIns;

        private bool _RenderReCal =false;
        private bool _LogicRecal =false;

        public bool NeedReCalculateFrameCnt 
        {
            get
            {
                return this._RenderReCal;
            }
            set
            {
                this._RenderReCal =value;
                if(value)
                {
                    this._ReCalFrame = this.RenderFrameCount+1;
                }
            }
        }

        public bool NeedReCalculateLogicFrameCnt 
        {
            get
            {
                return this._LogicRecal;
            }
            set
            {
                this._LogicRecal =value;
                if(value)
                {
                    this._ReCalLogicFrame = this.LogicFrameCount+1;
                }
            }
        }

        private long _ReCalFrame;
        private long _ReCalLogicFrame;


        private int _framerate;
        public int FrameRate
        {
            get
            {
                if (this._framerate <= 0 && Application.targetFrameRate > 0)
                {
                    this._framerate = Application.targetFrameRate;
                }

                return _framerate;
            }
            set
            {
                _framerate = value;
            }
        }


        public float FrameWorkTime
        {
            get
            {
                if(FrameRate == 0)
                {
                    return 0f;
                }

                return RenderFrameCount / (float)FrameRate;
            }
        }

        private long _RenderFrameCount;
        /// <summary>
        /// Gets the render frame count. if you want to use this,shoud be regster to frame update event
        /// </summary>
        /// <value>The render frame count.</value>
        public long RenderFrameCount
        {
            get
            {
                return this._RenderFrameCount;
            }

            private set
            {
                this._RenderFrameCount =value;
                if(this._RenderFrameCount == long.MaxValue -1)
                {
                    this.NeedReCalculateFrameCnt =true;
                }
            }
        }

        private long _LogicFrameCount;
        /// <summary>
        /// Gets the logic frame count.if you want to use this,shoud be regster to frame update event
        /// </summary>
        /// <value>The logic frame count.</value>
        public long LogicFrameCount
        {
            get
            {
                return this._LogicFrameCount;
            }

            private set
            {
                this._LogicFrameCount =value;
                if(this._LogicFrameCount == long.MaxValue -1)
                {
                    this.NeedReCalculateLogicFrameCnt =true;
                }

            }
        }


        private long NeedWaitMax = 0;
        private long NeedWaitFrameCnt = 0;

        private float _scale =1f;
        public float Scale
        {
            get
            {
                if (!Time.timeScale.FloatEqual(_scale))
                {
                    _scale = Time.timeScale;
                }
                return _scale;
            }
            set
            {
                _scale = value;
                Time.timeScale = value;

                if (value.FloatEqual(0f))
                    NeedWaitMax = 0;
                else
                    NeedWaitMax =(int)(1f / Time.timeScale);

                NeedWaitFrameCnt = NeedWaitMax;
            }
        }

        public float RenderDeltaTime
        {
            get
            {
                return Time.deltaTime;
            }
        }

        public float PhyConstantDletaTime
        {
            get
            {
                return Time.fixedDeltaTime;
            }
        }
        #region FPS
        public float FpsUpdateInterval = 1f; //updateInterval

        private float FpsUpdateTimeleft =0f;

        private int FpsRenderCount =0;

        private float recordPasuedtime =0f;

        private float recordLasttime =0f;

        private float accnumPausedTime =1f;

        private float accumtime =0f;

        private float m_fps;

        public float Fps
        {
            get
            {
                return m_fps;
            }
        }
        #endregion

        public void StartSync()
        {
            this.FrameRate = Application.targetFrameRate;
            this.Scale = Time.timeScale;

            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.FixedUpdate,_LogicFrameUpdate);
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.LateUpdate, _FrameUpdateEnd);
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.OnApplicationPause,_GamePaused);
        }

        public bool DetermineEnableFrame()
        {
            if (this.NeedWaitMax == 0)
                return false;

            this.NeedWaitFrameCnt--;
            if (this.NeedWaitFrameCnt > 0  )
            {
                return false;
            }
            else
            {
                this.NeedWaitFrameCnt = this.NeedWaitMax;
                return true;
            }
        }

        private void _GamePaused(int value)
        {
            if(value == 1)
            {
                this.recordPasuedtime = Time.realtimeSinceStartup;
            }
            else
            {
                this.accnumPausedTime += Time.realtimeSinceStartup - this.recordPasuedtime;
            }
        }  

        private void _RenderFrameUpdate(int value)
        {
            
            if(this.RenderFrameCount == 0)
            {
                this.recordLasttime = Time.realtimeSinceStartup - this.accnumPausedTime;
                this.RenderFrameCount ++;
            }
            else
            {
                if(!Time.timeScale.FloatEqual(0f))
                {
                    float now = Time.realtimeSinceStartup - this.accnumPausedTime;
                    float passedtime = (now - this.recordLasttime) / Time.timeScale;
                    this.recordLasttime = now;

                    this.accumtime += passedtime;

                    this.FpsUpdateTimeleft -= passedtime;
                    this.FpsRenderCount++;

                    if(FpsUpdateTimeleft.FloatLessEqual(0f))
                    {
                        this.m_fps = this.FpsRenderCount /this.accumtime;
                        this.accumtime =0f;
                        this.FpsRenderCount =0;
                        this.FpsUpdateTimeleft  = this.FpsUpdateInterval;
                    }
                    this.RenderFrameCount ++;
                }
                else
                {
                    this.m_fps =0;
                }
            }
        }

        private void _LogicFrameUpdate(int value)
        {
            this.LogicFrameCount++;   
        }

        private void _FrameUpdateEnd(int value)
        {
            _RenderFrameUpdate(value);

            if (this.NeedReCalculateFrameCnt && (this._ReCalFrame == this.RenderFrameCount || this.RenderFrameCount == long.MaxValue))
            {
                if(FrameWorkConfig.Open_DEBUG)
                {
                    LogMgr.LogFormat("Re Calculate  Framecnt:{0}",this.RenderFrameCount);
                }

                this._ReCalFrame =0;
                this.RenderFrameCount =0;
                this.NeedReCalculateFrameCnt =false;
            }

            if(this.NeedReCalculateLogicFrameCnt &&(this._ReCalLogicFrame == this.LogicFrameCount || this.LogicFrameCount == long.MaxValue))
            {
                if(FrameWorkConfig.Open_DEBUG)
                {
                    LogMgr.LogFormat("Re Calculate  LogicFramecnt:{0}",this.LogicFrameCount);
                }

                this._ReCalLogicFrame =0;
                this.LogicFrameCount =0;
                this.NeedReCalculateLogicFrameCnt =false;
            }

        }

        public void EndSync()
        {
            MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.Update,_RenderFrameUpdate);
            MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.FixedUpdate,_LogicFrameUpdate);
            MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.LateUpdate, _FrameUpdateEnd);
            MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.OnApplicationPause,_GamePaused);
        }

    }

}


