using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;

namespace KFrameWork
{

    [SingleTon]
    public class GameSyncCtr
    {
        public static GameSyncCtr mIns;

        public int FrameRate;

        public long GameServerTime;

        public float FrameWorkTime
        {
            get
            {
                return Time.realtimeSinceStartup - this.accnumPausedTime;
            }
        }

        private long _RenderFrameCount;

        public long RenderFrameCount
        {
            get
            {
                return this._RenderFrameCount;
            }
        }

        private long _LogicFrameCount;

        public long LogicFrameCount
        {
            get
            {
                return this._LogicFrameCount;
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


        public GameSyncCtr()
        {
            this._LogicFrameCount = 0;
            this._RenderFrameCount = 0;
            this.FrameRate =30;

            MainLoop.getLoop().RegisterLoopEvent(LoopMonoEvent.Update,_RenderFrameUpdate);
            MainLoop.getLoop().RegisterLoopEvent(LoopMonoEvent.BeforeUpdate,_LogicFrameUpdate);
            MainLoop.getLoop().RegisterLoopEvent(LoopMonoEvent.OnApplicationPause,_GamePaused);
        }

        public void StartSync()
        {
            
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
                this._RenderFrameCount ++;
            }
            else
            {
                if(!Time.timeScale.FloatEqual(0f))
                {
                    float now = Time.realtimeSinceStartup - this.accnumPausedTime;
                    float delta = now - this.recordLasttime;
                    this.recordLasttime = now;

                    float delval = delta;

                    delval /= Time.timeScale;
                    this.accumtime += delval ;
                    this.FpsUpdateTimeleft -= delval;
                    this.FpsRenderCount++;

                    if(FpsUpdateTimeleft.FloatLessEqual(0f))
                    {
                        this.m_fps = this.FpsRenderCount /this.accumtime;
                        this.accumtime =0f;
                        this.FpsRenderCount =0;
                        this.FpsUpdateTimeleft  = this.FpsUpdateInterval;
                    }
                    this._RenderFrameCount ++;
                }
                else
                {
                    this.m_fps =0;
                }
            }

        }

        private void _LogicFrameUpdate(int value)
        {
            this._LogicFrameCount++;   
        }

        public void EndSync()
        {
            
        }
            


    }

}


