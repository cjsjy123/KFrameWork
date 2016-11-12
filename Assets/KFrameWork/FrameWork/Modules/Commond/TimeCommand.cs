using System;
using UnityEngine;
using System.Collections.Generic;
using KUtils;

namespace KFrameWork
{
    public sealed class TimeCommand:BaseCommand<TimeCommand>
    {
        private static readonly int methodID =0;

        private float m_delay;

        private float m_starttime;

        private float m_pausedstarttime;

        private float m_pausedtime;

        public float delayTime
        {
            get
            {
                return this.m_delay + this.m_pausedtime;
            }
        }


        private System.Action Callback;


        private TimeCommand()
        {

        }

        public static TimeCommand Create(System.Action cbk, float time)
        {
            TimeCommand Command = null;
            if(KObjectPool.mIns != null)
            {
                Command =KObjectPool.mIns.Pop<TimeCommand>();
            }

            if(Command == null)
                Command = new TimeCommand();

            Command.m_delay = time;
            Command.Callback = cbk;

            return Command;

        }

        public override void Excute ()
        {
            try
            {
                if(!this.m_bExcuted)
                {
                    base.Excute();
                    this.m_starttime = GameSyncCtr.mIns.FrameWorkTime;
                    ///因为update中还有处理处理逻辑，当帧事件穿插在逻辑之间的时候，可能导致某些依赖此对象的帧逻辑判断错误，目前先放在late中
                    MainLoop.getLoop().RegisterCachedAction(MainLoopEvent.LateUpdate,methodID,this);
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

        [DelegateMethodAttribute(MainLoopEvent.LateUpdate,"methodID",typeof(TimeCommand))]
        private static void _ConfirmFrameDone(System.Object o, int value)
        {
            if(o is TimeCommand)
            {
                TimeCommand cmd = o as TimeCommand;
                if(GameSyncCtr.mIns.FrameWorkTime  - cmd.m_starttime >= cmd.m_delay && !cmd.m_paused)
                {
                    cmd.End();
                }
            }
            else
            {
                LogMgr.LogError(o);
            }
        }

        private void End()
        {
            this.Stop();

            this.TryBatch();

        }

        public override void Stop ()
        {
            MainLoop.getLoop().UnRegisterCachedAction(MainLoopEvent.LateUpdate,methodID,this);

            if(this.Callback != null)
            {
                this.Callback ();
            }

            this._isDone = true;

        }

        public override void Pause ()
        {
            float delta =GameSyncCtr.mIns.FrameWorkTime  - this.m_starttime;
            if(delta < this.m_delay)
            {
                this.m_pausedstarttime = GameSyncCtr.mIns.FrameWorkTime ;

                this.m_paused =true;
            }
        }

        public override void Resume ()
        {
            if(this.m_paused)
            {
                this.m_pausedtime +=GameSyncCtr.mIns.FrameWorkTime  - this.m_pausedstarttime;
                this.m_paused =false;
                if(this.isDone)
                    this.TryBatch();
            }

        }
            

        public override void AwakeFromPool ()
        {
            base.AwakeFromPool ();
            this.m_pausedtime =0f;
            this.m_pausedstarttime =0f;
            this.m_delay =0f;
            this.m_starttime =0f;
            this.Callback = null;
        }


        public override void RemovedFromPool ()
        {
            base.RemovedFromPool();
            this.Callback = null;

        }

        public override void Release (bool force)
        {
            if( (this.isDone || force) && !this.m_bReleased)
            {
                this.m_bReleased =true;

                if(!this.isDone)
                    LogMgr.LogWarning("帧命令未执行完毕，就被销毁了");

                if(KObjectPool.mIns  != null)
                {
                    KObjectPool.mIns.Push(this);
                }
            }
        }

        protected override TimeCommand OperatorAdd (CacheCommand other)
        {
            if(this != other)
            {
                this._Add(other);
            }
            return this;
        }

        protected override TimeCommand OperatorReduce (CacheCommand other)
        {
            if(this != other)
            {
                this._Remove(other);
            }
            return this;
        }


    }
}