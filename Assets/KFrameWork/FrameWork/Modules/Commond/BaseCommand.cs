using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{

    public abstract class CacheCommand:ICommand
    {
        protected static Dictionary<int,Queue<ICommand>> CMDCache ;

        public abstract void Release (bool force);

        public abstract void Excute ();

        protected int? _CMD;

        public int? CMD
        {
            get
            {

                return _CMD;
            }
        }

        protected AbstractParams _Gparams ;

        public AbstractParams CallParms
        {
            get
            {
                if(_Gparams == null)
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

        private ICommand _Next;
        public ICommand Next
        {
            get
            {
                return _Next;
            }

            set
            {
                this._Next = value;
            }
        }

        protected bool _isDone;
        /// <summary>
        /// 命令是否完成，请确保当完成的时候，其被移除掉，不占用引用
        /// </summary>
        /// <value><c>true</c> if is done; otherwise, <c>false</c>.</value>
        public bool isDone
        {
            get
            {
                return this._isDone;
            }
        }

        protected AbstractParams _RParams;

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
    }

    public abstract class BaseCommand<T> :CacheCommand,KUtils.IPool where T:BaseCommand<T> {

       

        protected bool m_bExcuted =false;
        protected bool m_bReleased =false;
        protected bool m_isBatching =false;

        [FrameWokAwakeAttribute]
        public static void Preload(int value)
        {
            if(CMDCache == null)
                CMDCache = new Dictionary<int, Queue<ICommand>>(16);
        }


        protected static U Spawn<U>(int CMD_ID)  where U:BaseCommand<T>
        {
            if(CMDCache != null && CMDCache.ContainsKey(CMD_ID) && CMDCache[CMD_ID].Count >0)
            {
                ICommand Top = CMDCache[CMD_ID].Peek();
                if(Top is U)
                {
                    U cmd = CMDCache[CMD_ID].Dequeue() as U;
                    cmd.m_bReleased =false;
                    cmd.m_bExcuted =false;
                    cmd._isDone =false;
                    cmd.m_isBatching=false;
                    cmd.Next = null;
                    return cmd;
                }
            }
            return null;
        }

        protected virtual void _BatchCall()
        {
            if(this.Next != null)
            {
                if(this.Next.isDone )
                { 
                    if(this.Next.Next != null)
                    {
                        this.Next = this.Next.Next;
                    }
                    else
                    {
                        this._isDone = true;
                        this.m_isBatching =false;
                        MainLoop.getLoop().UnRegisterLoopEvent(LoopMonoEvent.LateUpdate,this._SequenceCall);
                    }      
                }
            }
            else
            {
                this._isDone = true;
                this.m_isBatching =false;
                MainLoop.getLoop().UnRegisterLoopEvent(LoopMonoEvent.LateUpdate,this._SequenceCall);
            }
        }

        protected virtual void _SequenceCall(int value)
        {
            this._BatchCall();
        }
        //---------------pool--------
        public virtual void AwakeFromPool (){}
        public virtual void RemovedFromPool (){}

        public virtual void ReleaseToPool ()
        {
            this._isDone = false;
            this.m_bExcuted =false;
            this.Next = null;
            this.m_isBatching =false;

        }


        public override void Release(bool force)
        {
            if(!this.CMD.HasValue || this.m_bReleased || !this.isDone)
                return ;

            this.m_bReleased =false;
            this.m_bExcuted =false;

//            if(CMDCache == null)
//                CMDCache = new Dictionary<int, Queue<ICommand>>(16);

            if(CMDCache.ContainsKey(this._CMD.Value))
            {
                if(this.HasCallParams )
                {
                    this._Gparams.ResetReadIndex();
                }
                CMDCache[this.CMD.Value].Enqueue(this);
            }
            else
            {
                if(this.HasCallParams )
                {
                    this._Gparams.ResetReadIndex();
                }

                Queue<ICommand> queue = new Queue<ICommand>(4);
                queue.Enqueue(this);
                CMDCache.Add(this._CMD.Value,queue);
            }
        }

        protected void _Add(ICommand cmd)
        {
            if(cmd !=this)
            {
                ICommand next = this;
                while(next.Next != null)
                {
                    next = next.Next;
                }

                next.Next = cmd;
            }
        }

        protected void _Remove(ICommand cmd)
        {
            ICommand previous = null;
            ICommand next = this;
            while(next.Next != null)
            {
                if(next == cmd)
                {
                    previous.Next = next.Next;
                    next.Release(true);
                    break;
                }
                previous = next;
                next = next.Next;
            }
        }

        protected void _Clear()
        {
            ICommand previous = null;
            ICommand next = this;
            while(next.Next != null)
            {
                previous = next;
                next = next.Next;
                previous.Next = null;
            }
        }

        public override void Excute()
        {
            m_bExcuted = true;
        }

        public void ExcuteAndRelease()
        {
            this.Excute();
            this.Release(false);
        }

        protected abstract T  OperatorAdd(ICommand other);

        protected abstract T  OperatorReduce(ICommand other);

        public static  T operator+(BaseCommand<T>  lft,BaseCommand<T>  rht)
        {
            //UNABLE TO DO LIKE THIS
//            if(lft == rht)
//                return lft;
//
            return lft.OperatorAdd(rht);
        }

        public static T  operator-(BaseCommand<T>  lft,BaseCommand<T>  rht)
        {
            //UNABLE TO DO LIKE THIS
//            if(lft == rht)
//                return lft;
//            
            return lft.OperatorReduce(rht);
        }


    }
}


