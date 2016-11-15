using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public sealed class WaitTaskCommand : BaseCommand<WaitTaskCommand> {

        private static readonly int methodID =0;

        private ITask m_task;

        private Action m_Done;

        [FrameWokAwakeAttribute]
        private static void PreLoad(int v)
        {
            for (int i = 0; i < FrameWorkDebug.Preload_ParamsCount; ++i)
            {
                KObjectPool.mIns.Push(new WaitTaskCommand());
            }
        }

        private WaitTaskCommand()
        {
            
        }

        public static WaitTaskCommand Create(ITask task ,Action DoneEvent = null)
        {
            if(task == null)
                throw new FrameWorkResMissingException("Missing Task");

            WaitTaskCommand Command = null;
            if(KObjectPool.mIns != null)
            {
                Command =KObjectPool.mIns.Pop<WaitTaskCommand>();
            }

            if(Command == null)
                Command = new WaitTaskCommand();

            Command.m_task =task;
            Command.m_Done =DoneEvent;

            return Command;
        }

        public override void Excute ()
        {
            try
            {

                if(!this.m_bExcuted)
                {
                    base.Excute();
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

        [DelegateMethodAttribute(MainLoopEvent.LateUpdate,"methodID",typeof(WaitTaskCommand))]
        private static void _ConfirmFrameDone(System.Object o, int value)
        {
            if(o is WaitTaskCommand)
            {
                WaitTaskCommand cmd = o as WaitTaskCommand;
                if( (cmd.m_task == null || !cmd.m_task.KeepWaiting) && !cmd.m_paused)
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
            this.Reset();
            this._isDone = true;

            if (this.m_Done != null)
                this.m_Done();
        }

        public override void Pause ()
        {
            this.m_paused =true;
        }

        public override void Resume ()
        {
            this.m_paused =false;
            if(this.isDone)
                this.TryBatch();
        }

        public override void AwakeFromPool ()
        {
            base.AwakeFromPool ();
            this.m_task = null;
            this.m_Done = null;
        }

        public override void RemovedFromPool ()
        {
            base.RemovedFromPool ();
            this.m_task = null;
            this.m_Done = null;
        }

        protected override WaitTaskCommand OperatorAdd (CacheCommand other)
        {
            if(this != other)
            {
                this._Add(other);
            }
            return this;
        }

        protected override WaitTaskCommand OperatorReduce (CacheCommand other)
        {
            if(this != other)
            {
                this._Remove(other);
            }
            return this;
        }
    }
}


