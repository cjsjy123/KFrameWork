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

        private Action<WaitTaskCommand> CallBack;

        [FrameWorkStartAttribute]
        private static void PreLoad(int v)
        {
            for (int i = 0; i < FrameWorkConfig.Preload_ParamsCount; ++i)
            {
                KObjectPool.mIns.Push(new WaitTaskCommand());
            }
        }

        private WaitTaskCommand()
        {
            
        }

        public static WaitTaskCommand Create(ITask task ,Action<WaitTaskCommand> DoneEvent = null)
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
            Command.CallBack =DoneEvent;

            return Command;
        }

        public override void Excute ()
        {
            try
            {

                if(!this.isRunning)
                {
                    base.Excute();
                    //if(FrameWorkConfig.Open_DEBUG)
                    //    LogMgr.LogFormat("{0} ID:{1} start . ::::Task :{2}",this,this.UID,this.m_task);
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

        public static void Destroy()
        {
            MainLoop.getInstance().UnRegisterCachedAction(MainLoopEvent.BeforeUpdate, methodID);
        }

        [DelegateMethodAttribute(MainLoopEvent.BeforeUpdate,"methodID",typeof(WaitTaskCommand))]
        private static void _ConfirmFrameDone(System.Object o, int value)
        {
            if(o is WaitTaskCommand)
            {
                WaitTaskCommand cmd = o as WaitTaskCommand;
                if( (cmd.m_task == null || !cmd.m_task.KeepWaiting) && cmd.RunningState != CommandState.Paused)
                {
                    //if(FrameWorkConfig.Open_DEBUG)
                    //    LogMgr.LogFormat("{0} ID:{1} finished . ::::Task :{2}",cmd,cmd.UID,cmd.m_task);
                    
                    cmd.End();
                }
            }
            else
            {
                LogMgr.LogError(o);
            }
        }

        public ITask GetTask()
        {
            return this.m_task;
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
            if (this.CallBack != null)
            {
                Delegate d = this.CallBack as Delegate;
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
            MainLoop.getInstance().UnRegisterCachedAction(MainLoopEvent.BeforeUpdate, methodID, this);
        }

        protected override void SetFinished()
        {
            MainLoop.getInstance().UnRegisterCachedAction(MainLoopEvent.BeforeUpdate,methodID,this);

            if (this.CallBack != null)
                this.CallBack(this);

            base.SetFinished();
        }

        public override void Resume ()
        {
            base.Resume();
            if(this.isDone)
                this.TryBatch();
        }

        public override void RemoveToPool ()
        {
            base.RemoveToPool ();
            this.m_task = null;
            this.CallBack = null;
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


