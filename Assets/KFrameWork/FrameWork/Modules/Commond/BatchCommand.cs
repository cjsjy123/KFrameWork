using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace KFrameWork
{
    public sealed class BatchCommand:BaseCommand<BatchCommand>
    {

        private BatchCommand()
        {
            
        }

        public static BatchCommand Create(params CacheCommand[] cmds)
        {
            BatchCommand cmd = new BatchCommand();

            if(cmds != null && cmds.Length >0)
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
            BatchCommand cmd = new BatchCommand();
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

        public void Clear()
        {
            this._Clear();
        }

        public override void Excute ()
        {
            try
            {
                if(!this.m_bExcuted)
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

        public override void Stop ()
        {
            if(this.m_isBatching && this.Next != null)
                this.Next.Stop();
            
            MainLoop.getLoop().UnRegisterLoopEvent(MainLoopEvent.LateUpdate,this._SequenceCall);
        }

        public override void Pause ()
        {
            this.m_paused =true;
            CacheCommand nextCmd = this.Next ;
            while(nextCmd != null)
            {
                nextCmd.Pause();
                nextCmd = nextCmd.Next;
            }
        }

        public override void Resume ()
        {
            
            CacheCommand nextCmd = this.Next ;
            while(nextCmd != null)
            {
                nextCmd.Resume();
                nextCmd = nextCmd.Next;
            }

            this.m_paused =false;

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

