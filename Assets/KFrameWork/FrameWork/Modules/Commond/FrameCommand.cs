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

        private Action<FrameCommand> Callback;


        private FrameCommand()
        {
            
        }

        [FrameWorkStartAttribute]
        private static void PreLoad(int v)
        {
            for(int i=0;i < FrameWorkConfig.Preload_ParamsCount;++i)
            {
                KObjectPool.mIns.Push(new FrameCommand());
            }
        }

        public static FrameCommand Create(int delayFrame,Action<FrameCommand> cbk)
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

                    this.m_startFrame = GameSyncCtr.mIns.RenderFrameCount;

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

        [DelegateMethodAttribute(MainLoopEvent.BeforeUpdate,"methodID",typeof(FrameCommand))]
        private static void _ConfirmFrameDone(System.Object ins, int value)
        {
            try
            {
                if(ins is FrameCommand)
                {
                    FrameCommand cmd = ins as FrameCommand;
                    if(GameSyncCtr.mIns.RenderFrameCount - cmd.m_startFrame >= cmd.FrameCount && cmd.RunningState != CommandState.Paused)
                    {
                        cmd.End();
                    }

                    if(GameSyncCtr.mIns.NeedReCalculateFrameCnt)
                    {
                        long delta = GameSyncCtr.mIns.RenderFrameCount - cmd.m_startFrame;
                        cmd._frame -= delta;
                        cmd.m_startFrame =0;
                    }
                }
                else
                {
                    LogMgr.LogError(ins);
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
            return false;
        }

        public override void Cancel()
        {
            base.Cancel();

            if (!MainLoop.getInstance().UnRegisterCachedAction(MainLoopEvent.BeforeUpdate, methodID, this))
            {
                LogMgr.LogError("删除失败");
            }
        }

        protected override void SetFinished()
        {
            if (!MainLoop.getInstance().UnRegisterCachedAction(MainLoopEvent.BeforeUpdate, methodID, this))
            {
                LogMgr.LogError("删除失败");
            }
            if (this.Callback != null)
            {
                this.Callback (this);
            }

            base.SetFinished();
        }

        public override void Pause ()
        {
            long deltaFrame = GameSyncCtr.mIns.RenderFrameCount - this.m_startFrame;
            if(deltaFrame < FrameCount && this.RunningState == CommandState.Running)
            {
                this._frame = this.FrameCount - deltaFrame;
                this.m_pausedStartFrame = GameSyncCtr.mIns.RenderFrameCount;
            }
        }

        public override void Resume ()
        {
            if(this.RunningState == CommandState.Paused)
            {
                base.Resume();
                this.m_pausedFrameCnt+=GameSyncCtr.mIns.RenderFrameCount - this.m_pausedStartFrame;

                if(!this.isDone)
                    this.TryBatch();
            }
        }

        public override void RemoveToPool ()
        {
            base.RemoveToPool ();
            this.m_pausedStartFrame =0;
            this.m_pausedFrameCnt =0;
            this._frame = 0;
            this.Callback = null;
        }

        public override void Release (bool force)
        {
            if( (this.isDone || force) && !this.m_bReleased)
            {
                this.m_bReleased =true;

                if(!this.isDone)
                    LogMgr.LogWarning("命令未执行完毕，就被销毁了");

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

