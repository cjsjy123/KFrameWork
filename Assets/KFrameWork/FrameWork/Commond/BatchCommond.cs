using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace KFrameWork
{
    public class BatchCommond:BaseCommond<BatchCommond>
    {

        protected BatchCommond()
        {
            
        }

        public static BatchCommond Create(params ICommond[] cmds)
        {
            BatchCommond cmd = new BatchCommond();

            if(cmds != null && cmds.Length >0)
            {
                ICommond next = cmd;
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

        public static BatchCommond Create()
        {
            BatchCommond cmd = new BatchCommond();
            return cmd;
        }

        public void Add(ICommond cmd)
        {
            this._Add(cmd);
        }

        public void Remove(ICommond cmd)
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
                        MainLoop.getLoop().RegisterLoopEvent(LoopMonoEvent.LateUpdate,this._SequenceCall);
                    }

                }
            }
            catch(System.Exception ex)
            {
                LogMgr.LogException(ex);
            }
        }

        protected override BatchCommond OperatorAdd (ICommond other)
        {

            if(this != other)
            {
                this.Add(other);
            }
            return this;
        }

        protected override BatchCommond OperatorReduce (ICommond other)
        {
            if(this != other)
            {
                this.Remove(other);
            }
            return this;
        }
    }
}

