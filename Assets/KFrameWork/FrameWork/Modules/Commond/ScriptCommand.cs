using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace KFrameWork
{
    public sealed class ScriptCommand :BaseCommand<ScriptCommand>
    {

        protected ScriptCommand()
        {
            
        }

        /// <summary>
        /// 创建脚本命令，传入命令号，传入参数个数可以进行更好的优化处理
        /// </summary>
        /// <param name="CMD_ID">CM d I.</param>
        /// <param name="argCount">Argument count.</param>
        public static ScriptCommand Create(int CMD_ID,int argcount =-1)
        {
            ScriptCommand cmd = Spawn<ScriptCommand>(CMD_ID);
            if(cmd == null)
            {
                cmd = new ScriptCommand();
                cmd._CMD = CMD_ID;
                if(!cmd.HasCallParams)
                {
                    if(argcount >0 && argcount <4)
                    {
                        cmd._Gparams = SimpleParams.Create(argcount);
                    }
                    else if(argcount>3)
                    {
                        cmd._Gparams = GenericParams.Create(argcount);
                    }
                }
            }

            return cmd;
        }

        public override void Excute ()
        {
            try
            {
                if (CMD != null && !this.m_bExcuted)
                {
                    base.Excute();
                    ScriptLogicCtr.mIns.PushCommand (this);
                } 
                else if(!this.m_bExcuted) {
                    LogMgr.LogError ("命令号未设置");
                }
            }
            catch(System.Exception ex)
            {
                LogMgr.LogException(ex);
            }

        }

        public override void Release (bool force)
        {
            ///也有可能用户主动调用，
            this._isDone =true;

            if(!this.m_isBatching && this.Next != null)
            {
                this.m_isBatching =true;
                this.Next.Excute();
                //MainLoop.getLoop().RegisterLoopEvent(LoopMonoEvent.LateUpdate,this._SequenceCall);
            }
//

            base.Release (force);
        }

        public override void Stop ()
        {
            this._isDone =true;
        }

        public override void Pause ()
        {
            this.m_paused =true;
        }

        public override void Resume ()
        {
            this.m_paused =false;
        }

        protected override ScriptCommand OperatorAdd (ICommand other)
        {
            if(this != other)
            {
                this._Add(other);
            }
            return this;
        }

        protected override ScriptCommand OperatorReduce (ICommand other)
        {
            if(this != other)
            {
                this._Remove(other);
            }
            return this;

        }

    }
}


