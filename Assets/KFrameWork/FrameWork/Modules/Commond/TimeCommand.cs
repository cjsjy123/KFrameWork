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

        private AbstractParams cacheParams;

        private System.Action Callback;

        private System.Action<AbstractParams> CallBackWithParams;

        [FrameWorkStartAttribute]
        private static void PreLoad(int v)
        {
            for (int i = 0; i < FrameWorkConfig.Preload_ParamsCount; ++i)
            {
                KObjectPool.mIns.Push(new TimeCommand());
            }
        }

        private TimeCommand()
        {

        }

        public static TimeCommand Create(float time, System.Action<AbstractParams> cbk ,AbstractParams p)
        {
            TimeCommand Command = null;
            if (KObjectPool.mIns != null)
            {
                Command = KObjectPool.mIns.Pop<TimeCommand>();
            }

            if (Command == null)
                Command = new TimeCommand();

            Command.m_delay = time;
            Command.CallBackWithParams = cbk;
            Command.cacheParams = p;

            return Command;
        }

        public static TimeCommand Create(float time,System.Action cbk =null)
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

        public static void Destroy()
        {
            MainLoop.getInstance().UnRegisterCachedAction(MainLoopEvent.BeforeUpdate, methodID);
        }

        public override void Excute ()
        {
            try
            {
                if(!this.isRunning)
                {
                    base.Excute();
                    this.m_starttime = GameSyncCtr.mIns.FrameWorkTime;
                    ///因为update中还有处理处理逻辑，当帧事件穿插在逻辑之间的时候，可能导致某些依赖此对象的帧逻辑判断错误，目前先放在late中
                    MainLoop.getInstance().RegisterCachedAction(MainLoopEvent.BeforeUpdate,methodID,this);
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

        [DelegateMethodAttribute(MainLoopEvent.BeforeUpdate,"methodID",typeof(TimeCommand))]
        private static void _ConfirmFrameDone(System.Object o, int value)
        {
            try
            {
                if(o is TimeCommand)
                {
                    TimeCommand cmd = o as TimeCommand;
                    if(GameSyncCtr.mIns.FrameWorkTime  - cmd.m_starttime >= cmd.m_delay && cmd.RunningState != CommandState.Paused)
                    {
                        cmd.End();
                    }
                }
                else
                {
                    LogMgr.LogError(o);
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

        private void End()
        {
            //if (FrameWorkConfig.Open_DEBUG)
            //    LogMgr.LogFormat("********* Cmd Finished  :{0}", this);

            this.TryBatch();
            this.SetFinished();
        }

        protected override bool CancelBy(object o)
        {
            if (this.Callback != null)
            {
                Delegate d = this.Callback as Delegate;
                if (d.Target == o)
                {
                    return true;
                }
            }

            if (this.CallBackWithParams != null)
            {
                Delegate d = this.CallBackWithParams as Delegate;
                if (d.Target == o)
                {
                    return true;
                }
            }
            return false;
        }

        public override void Cancel()
        {
            MainLoop.getInstance().UnRegisterCachedAction(MainLoopEvent.BeforeUpdate, methodID, this);
            this.Callback = null;
            this.CallBackWithParams = null;
            base.Cancel();

        }

        protected override void SetFinished()
        {
            MainLoop.getInstance().UnRegisterCachedAction(MainLoopEvent.BeforeUpdate,methodID,this);

            if (this.Callback != null)
            {
                this.Callback ();
            }

            if (this.CallBackWithParams != null)
            {
                this.CallBackWithParams(this.cacheParams);
            }

            base.SetFinished();
        }

        public override void Pause ()
        {
            float delta =GameSyncCtr.mIns.FrameWorkTime  - this.m_starttime;
            if(delta < this.m_delay && this.RunningState == CommandState.Running)
            {
                this.m_pausedstarttime = GameSyncCtr.mIns.FrameWorkTime ;
                base.Pause();
            }
        }

        public override void Resume ()
        {
            if(this.RunningState == CommandState.Paused)
            {
                base.Resume();
                this.m_pausedtime +=GameSyncCtr.mIns.FrameWorkTime  - this.m_pausedstarttime;

                if(this.isDone)
                    this.TryBatch();
            }
        }
            
        public override void RemoveToPool ()
        {
            base.RemoveToPool ();
            this.m_pausedtime =0f;
            this.m_pausedstarttime =0f;
            this.m_delay =0f;
            this.m_starttime =0f;
            this.Callback = null;
            this.CallBackWithParams = null;
            this.cacheParams = null;
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