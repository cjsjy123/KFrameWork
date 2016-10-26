using System;
using UnityEngine;
using System.Collections.Generic;
using KUtils;

namespace KFrameWork
{
    public class TimeCommand:BaseCommand<TimeCommand>
    {

        private float m_delay;

        private float m_starttime;

        private System.Action Callback;


        protected TimeCommand()
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
                    MainLoop.getLoop().RegisterLoopEvent(LoopMonoEvent.LateUpdate,this._ConfirmFrameDone);
                }
            }
            catch(Exception ex)
            {
                LogMgr.LogException(ex);
            }

        }

        private void _ConfirmFrameDone(int value)
        {
            if(GameSyncCtr.mIns.FrameWorkTime  - this.m_starttime >= this.m_delay)
            {
                MainLoop.getLoop().UnRegisterLoopEvent(LoopMonoEvent.LateUpdate,this._ConfirmFrameDone);

                if(this.Callback != null)
                {
                    this.Callback ();
                }

                this._isDone = true;

                if(this.Next != null && !this.m_isBatching)
                {
                    this.m_isBatching =true;
                    this.Next.Excute();
                    //  MainLoop.getLoop().RegisterLoopEvent(LoopMonoEvent.LateUpdate,this._SequenceCall);
                }
            }
        }
            

        public override void ReleaseToPool ()
        {
            base.ReleaseToPool();
            this.m_delay = 0;
            this.Callback = null;

        }

        public override void RemovedFromPool ()
        {
            base.RemovedFromPool();
            this._CMD = null;
            this._Gparams = null;
            this._RParams = null;
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

        protected override TimeCommand OperatorAdd (ICommand other)
        {
            if(this != other)
            {
                this._Add(other);
            }
            return this;
        }

        protected override TimeCommand OperatorReduce (ICommand other)
        {
            if(this != other)
            {
                this._Remove(other);
            }
            return this;
        }


    }
}