using System;
using UnityEngine;
using System.Collections.Generic;
using KUtils;

namespace KFrameWork
{
    public class FrameCommond:BaseCommond<FrameCommond>
    {

        protected int _frame;

        public int FrameCount
        {
            get
            {
                return _frame;
            }
        }

        private long m_startFrame;

        private System.Action Callback;


        protected FrameCommond()
        {
            
        }

        [FrameWokAwakeAttribute]
        private static void PreLoad(int v)
        {
            for(int i=0;i < FrameWorkDebug.Preload_ParamsCount;++i)
            {
                KObjectPool.mIns.Push(new FrameCommond());
            }
        }

        public static FrameCommond Create(System.Action cbk, int delayFrame=1)
        {
            FrameCommond commond = null;
            if(KObjectPool.mIns != null)
            {
                commond =KObjectPool.mIns.Pop<FrameCommond>();
            }

            if(commond == null)
                commond = new FrameCommond();

            commond._frame = delayFrame;
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
                    this.m_startFrame = GameSyncCtr.mIns.RenderFrameCount;
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
            if(GameSyncCtr.mIns.RenderFrameCount - this.m_startFrame >= FrameCount)
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
            this._frame = 0;
            this.Callback = null;
        }

        public override void RemovedFromPool ()
        {
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

        protected override FrameCommond OperatorAdd (ICommond other)
        {
            if(this != other)
            {
                this._Add(other);
            }
            return this;
        }

        protected override FrameCommond OperatorReduce (ICommond other)
        {
            if(this != other)
            {
                this._Remove(other);
            }
            return this;
        }


    }
}

