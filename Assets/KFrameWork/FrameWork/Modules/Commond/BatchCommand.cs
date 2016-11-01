using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace KFrameWork
{
    public class BatchCommand:BaseCommand<BatchCommand>
    {

        protected BatchCommand()
        {
            
        }

        public static BatchCommand Create(params ICommand[] cmds)
        {
            BatchCommand cmd = new BatchCommand();

            if(cmds != null && cmds.Length >0)
            {
                ICommand next = cmd;
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

        public void Add(ICommand cmd)
        {
            this._Add(cmd);
        }

        public void Remove(ICommand cmd)
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

                    if(this.Next != null)
                    {
                        this.m_isBatching =true;
                        this.Next.Excute();
                        MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.LateUpdate,this._SequenceCall);
                    }

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
        }

        public override void Resume ()
        {
            this.m_paused =false;
        }

        protected override BatchCommand OperatorAdd (ICommand other)
        {
            if(this != other)
            {
                this.Add(other);
            }
            return this;
        }

        protected override BatchCommand OperatorReduce (ICommand other)
        {
            if(this != other)
            {
                this.Remove(other);
            }
            return this;
        }
    }
}

