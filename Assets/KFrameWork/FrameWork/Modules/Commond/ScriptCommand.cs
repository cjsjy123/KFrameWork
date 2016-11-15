using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace KFrameWork
{
    public sealed class ScriptCommand :BaseCommand<ScriptCommand>
    {
        private int? _CMD;

        public int? CMD
        {
            get
            {

                return _CMD;
            }
        }

        private AbstractParams _Gparams;

        public AbstractParams CallParms
        {
            get
            {
                if (_Gparams == null)
                    _Gparams = GenericParams.Create();
                return _Gparams;
            }
        }

        public bool HasCallParams
        {
            get
            {
                return _Gparams != null;
            }
        }

        private AbstractParams _RParams;

        /// <summary>
        /// 当有返回值得时候用户请自行dispose
        /// </summary>
        /// <value>The return parameters.</value>
        public AbstractParams ReturnParams
        {
            get
            {
                return _RParams;
            }
            set
            {
                _RParams = value;
            }
        }

        private ScriptCommand()
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

        public override void Release(bool force)
        {
            ///也有可能用户主动调用，
            this._isDone = true;

            if (!this.m_isBatching && this.Next != null)
            {
                this.m_isBatching = true;
                this.Next.Excute();
            }

            if (!this.CMD.HasValue || this.m_bReleased || !this.isDone)
                return;

            this.m_bReleased = false;
            this.m_bExcuted = false;

            if (CMDCache.ContainsKey(this._CMD.Value))
            {
                if (this.HasCallParams)
                {
                    this._Gparams.ResetReadIndex();
                }
                CMDCache[this.CMD.Value].Enqueue(this);
            }
            else
            {
                if (this.HasCallParams)
                {
                    this._Gparams.ResetReadIndex();
                }

                Queue<CacheCommand> queue = new Queue<CacheCommand>(4);
                queue.Enqueue(this);
                CMDCache.Add(this._CMD.Value, queue);
            }
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
            if(this.isDone)
                this.TryBatch();
        }

        protected override ScriptCommand OperatorAdd (CacheCommand other)
        {
            if(this != other)
            {
                this._Add(other);
            }
            return this;
        }

        protected override ScriptCommand OperatorReduce (CacheCommand other)
        {
            if(this != other)
            {
                this._Remove(other);
            }
            return this;

        }

        public override void AwakeFromPool()
        {
            base.AwakeFromPool();
            this._CMD = null;
            this._Gparams = null;
            this._RParams = null;
        }

        public override void RemovedFromPool()
        {
            base.RemovedFromPool();
            this._CMD = null;
            this._Gparams = null;
            this._RParams = null;
        }

    }
}


