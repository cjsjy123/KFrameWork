using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public abstract class BaseCommand<T> :CacheCommand,IPool  {

        protected bool m_bReleased =false;
        protected bool m_isBatching =false;

        protected BaseCommand()
        {
            this.GenID();
        }

        protected void _BatchCall()
        {
            if(this.RunningState == CommandState.Paused )
                return ;

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
                        this.m_isBatching =false;
                        this.Next = null;
                        MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.LateUpdate,this._SequenceCall);

                        if(this.isRunning)
                            this.SetFinished();
                    }      
                }
            }
            else
            {
                this.m_isBatching =false;
                MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.LateUpdate,this._SequenceCall);

                if (this.isRunning)
                    this.SetFinished();
            }
        }

        protected void TryBatch()
        {
            if(this.Next != null &&!this.m_isBatching)
            {
                this.m_isBatching =true;
                this.Next.Excute();
                MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.LateUpdate,this._SequenceCall);
            }
        }

        protected void _SequenceCall(int value)
        {
            this._BatchCall();
        }


        //---------------pool--------
        public virtual void RemoveToPool ()
        {
            this.GenID();
            this.m_bReleased =false;
            this.m_isBatching =false;
            this.Next = null;
        }

        protected void _Add(CacheCommand cmd)
        {
            if(cmd !=this)
            {
                CacheCommand next = this;
                while(next.Next != null)
                {
                    next = next.Next;
                }

                next.Next = cmd;
            }
        }

        protected void _Remove(CacheCommand cmd)
        {
            CacheCommand previous = null;
            CacheCommand next = this;
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
            CacheCommand previous = null;
            CacheCommand next = this;
            while(next.Next != null)
            {
                previous = next;
                next = next.Next;
                previous.Next = null;
            }
        }


        /// <summary>
        /// force 需要用户确保其他地方地方没有其引用，不然容易导致bug
        /// </summary>
        /// <param name="force"></param>
        public override void Release(bool force)
        {
            if (KObjectPool.mIns != null && force)
            {
                KObjectPool.mIns.Push(this);
            }
        }


        protected abstract T  OperatorAdd(CacheCommand other);

        protected abstract T  OperatorReduce(CacheCommand other);

        public static  T operator+(BaseCommand<T>  lft,BaseCommand<T>  rht)
        {
            return lft.OperatorAdd(rht);
        }

        public static T  operator-(BaseCommand<T>  lft,BaseCommand<T>  rht)
        {  
            return lft.OperatorReduce(rht);
        }


    }
}


