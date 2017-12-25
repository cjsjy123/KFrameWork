using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public sealed class ScriptCommand :BaseCommand<ScriptCommand>
    {
        private int _CMD;

        public int CMD
        {
            get
            {

                return _CMD;
            }
        }

        private AbstractParams _Gparams;

        public AbstractParams CallParams
        {
            get
            {
                if (_Gparams == null)
                    _Gparams = GenericParams.Create();
                return _Gparams;
            }
            set
            {
                if (_Gparams != value)
                {
                    if (_Gparams != null)
                        _Gparams.Release();
                    _Gparams = value;
                }
            }
        }

        public bool HasCallParams
        {
            get
            {
                return _Gparams != null;
            }
        }
        /// <summary>
        /// runtime注册时候需要的参数(c#动态注册时一个对象-》一个对象类型=》对象方法---- lua则是文件名和方法)
        /// </summary>
        private AbstractParams _Initparams;
        public AbstractParams InitParams
        {
            get
            {
                if (_Initparams == null)
                    _Initparams = GenericParams.Create();
                return _Initparams;
            }
        }

        public bool HasInitParams
        {
            get
            {
                return _Initparams != null;
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

        /// <summary>
        /// 脚本对象
        /// </summary>
        public ScriptTarget target = ScriptTarget.Unknown;

        public void SetCallParams(AbstractParams p)
        {
            this._Gparams =p;
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
            ScriptCommand cmd = KObjectPool.mIns.Pop<ScriptCommand>();
            if (cmd == null)
            {
                cmd = new ScriptCommand();
               
            }

            cmd._CMD = CMD_ID;

            if (!cmd.HasCallParams)
            {
                if (argcount > 0 && argcount < 4)
                {
                    cmd._Gparams = SimpleParams.Create(argcount);
                }
                else if (argcount > 3)
                {
                    cmd._Gparams = GenericParams.Create(argcount);
                }
            }

            return cmd;
        }

        public void ExcuteAndRelease()
        {
            this.Excute();
            this.Release(false);
        }

        public override void Release(bool force)
        {
            if (this._state == CommandState.Running)
            {
                this._state = CommandState.Finished;
                if (FrameWorkConfig.Open_DEBUG)
                    LogMgr.LogFormat("********* Cmd Finished  :{0}", this);

                RunningList.Remove(this);

                this.TryBatch();

                base.Release(force);
            }     
        }

        public override void Excute ()
        {
            try
            {
                if (!this.isRunning)
                {
                    base.Excute();
                    ScriptLogicCtr.mIns.PushCommand (this);
                } 

            }
            catch(System.Exception ex)
            {
                LogMgr.LogException(ex);
            }

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

        public override void RemoveToPool()
        {
            base.RemoveToPool();
            if (this._Gparams != null)
            {
                //KObjectPool.mIns.Push(this._Gparams);
                this._Gparams = null;
            }

            if (this._Initparams != null)
            {
                KObjectPool.mIns.Push(this._Initparams);
                this._Initparams = null;
            }

            if (this._RParams != null )
            {
                KObjectPool.mIns.Push(this._RParams);
                this._RParams = null;
            }

            this.target = ScriptTarget.Unknown;
        }

        public override void RemovedFromPool()
        {
            base.RemovedFromPool();
            this._Gparams = null;
            this._RParams = null;
            this._Initparams = null;
        }

    }
}


