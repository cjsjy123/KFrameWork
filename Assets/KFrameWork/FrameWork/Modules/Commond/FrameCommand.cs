using System;
using UnityEngine;
using System.Collections.Generic;
using KUtils;

namespace KFrameWork
{
    public sealed class FrameCommand:BaseCommand<FrameCommand>
    {
        private static readonly int methodID =0;

        private long _frame;

        public long FrameCount
        {
            get
            {
                return _frame +m_pausedFrameCnt;
            }
        }


        private long m_pausedFrameCnt;

        private long m_pausedStartFrame;

        private long m_startFrame;

        private System.Action Callback;


        private FrameCommand()
        {
            
        }

        [FrameWokAwakeAttribute]
        private static void PreLoad(int v)
        {
            for(int i=0;i < FrameWorkDebug.Preload_ParamsCount;++i)
            {
                KObjectPool.mIns.Push(new FrameCommand());
            }
        }

        public static FrameCommand Create(System.Action cbk, int delayFrame=1)
        {
            FrameCommand Command = null;
            if(KObjectPool.mIns != null)
            {
                Command =KObjectPool.mIns.Pop<FrameCommand>();
            }

            if(Command == null)
                Command = new FrameCommand();

            Command._frame = delayFrame;
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

                    this.m_startFrame = GameSyncCtr.mIns.RenderFrameCount;

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

        [DelegateMethodAttribute(MainLoopEvent.LateUpdate,"methodID",typeof(FrameCommand))]
        private static void _ConfirmFrameDone(System.Object ins, int value)
        {
            if(ins is FrameCommand)
            {
                FrameCommand cmd = ins as FrameCommand;
                if(GameSyncCtr.mIns.RenderFrameCount - cmd.m_startFrame >= cmd.FrameCount && !cmd.m_paused)
                {
                    cmd.End();
                }
            }
            else
            {
                LogMgr.LogError(ins);
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
            this._isDone = true;

            if (this.Callback != null)
            {
                this.Callback ();
            }

            
        }

        public override void Pause ()
        {
            long deltaFrame = GameSyncCtr.mIns.RenderFrameCount - this.m_startFrame;
            if(deltaFrame < FrameCount)
            {
                this._frame = this.FrameCount - deltaFrame;
                this.m_pausedStartFrame = GameSyncCtr.mIns.RenderFrameCount;
                this.m_paused =true;
            }
        }

        public override void Resume ()
        {
            if(this.m_paused)
            {
                this.m_pausedFrameCnt+=GameSyncCtr.mIns.RenderFrameCount - this.m_pausedStartFrame;
                this.m_paused =false;

                if(!this.isDone)
                    this.TryBatch();
            }
        }

        public override void AwakeFromPool ()
        {
            base.AwakeFromPool ();
            this.m_pausedStartFrame =0;
            this.m_pausedFrameCnt =0;
            this._frame = 0;
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

        protected override FrameCommand OperatorAdd (CacheCommand other)
        {
            if(this != other)
            {
                this._Add(other);
            }
            return this;
        }

        protected override FrameCommand OperatorReduce (CacheCommand other)
        {
            if(this != other)
            {
                this._Remove(other);
            }
            return this;
        }


    }
}

