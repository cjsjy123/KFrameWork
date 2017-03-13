using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public sealed class BatchCommand:BaseCommand<BatchCommand>
    {

        private BatchCommand()
        {
            
        }

        public static BatchCommand Create(params CacheCommand[] cmds)
        {
            BatchCommand cmd = KObjectPool.mIns.Pop<BatchCommand>();
            if (cmd == null)
                cmd = new BatchCommand();

            if (cmds != null && cmds.Length >0)
            {
                CacheCommand next = cmd;
                for(int i =0; i < cmds.Length;++i)
                {
                    if(cmds[i] != cmd)
                    {
                        next.Next = cmds[i];
                        next = next.Next;
                    }
                }
            }

            return cmd;
        }

        public static BatchCommand Create()
        {
            BatchCommand cmd = KObjectPool.mIns.Pop<BatchCommand>();
            if (cmd == null)
                cmd = new BatchCommand();
            return cmd;
        }

        public void Add(CacheCommand cmd)
        {
            this._Add(cmd);
        }

        public void Remove(CacheCommand cmd)
        {
            this._Remove(cmd);
        }

        public override void Cancel()
        {
            this._Clear();
            base.Cancel();
        }

        public override void Excute ()
        {
            try
            {
                if(!this.isRunning)
                {
                    base.Excute();

                    this.TryBatch();
                }
            }
            catch(System.Exception ex)
            {
                LogMgr.LogException(ex);
            }
        }

        public override void Release(bool force)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("********* Cmd Finished  :{0}", this);

            base.Release(force);
        }

        public override void Pause ()
        {
            CacheCommand nextCmd = this.Next ;
            while(nextCmd != null)
            {
                nextCmd.Pause();
                nextCmd = nextCmd.Next;
            }

            base.Pause();
        }

        public override void Resume ()
        {
            
            CacheCommand nextCmd = this.Next ;
            while(nextCmd != null)
            {
                nextCmd.Resume();
                nextCmd = nextCmd.Next;
            }

            base.Resume();
            if(this.isDone)
                this.TryBatch();
        }

        protected override BatchCommand OperatorAdd (CacheCommand other)
        {
            if(this != other)
            {
                this.Add(other);
            }
            return this;
        }

        protected override BatchCommand OperatorReduce (CacheCommand other)
        {
            if(this != other)
            {
                this.Remove(other);
            }
            return this;
        }

    }
}

