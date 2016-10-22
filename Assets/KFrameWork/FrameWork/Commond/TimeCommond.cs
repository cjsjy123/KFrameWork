using System;
using UnityEngine;
using System.Collections.Generic;
using KUtils;

namespace KFrameWork
{
    public class TimeCommond:BaseCommond<TimeCommond>
    {

        private float m_delay;

        private float m_starttime;

        private System.Action Callback;


        protected TimeCommond()
        {

        }

        public static TimeCommond Create(System.Action cbk, float time)
        {
            TimeCommond commond = null;
            if(KObjectPool.mIns != null)
            {
                commond =KObjectPool.mIns.Pop<TimeCommond>();
            }

            if(commond == null)
                commond = new TimeCommond();

            commond.m_delay = time;
            commond.Callback = cbk;

            return commond;

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

        protected override TimeCommond OperatorAdd (ICommond other)
        {
            if(this != other)
            {
                this._Add(other);
            }
            return this;
        }

        protected override TimeCommond OperatorReduce (ICommond other)
        {
            if(this != other)
            {
                this._Remove(other);
            }
            return this;
        }


    }
}