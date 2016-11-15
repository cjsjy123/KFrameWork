using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public enum CommandState
    {
        PrePared,
        Running,
        Paused,
        Stoped,
        Finished,
    }


    public abstract class BaseCommand<T> :CacheCommand,IPool  {
        protected bool m_bExcuted =false;
        protected bool m_bReleased =false;
        protected bool m_isBatching =false;

        [FrameWokAwakeAttribute]
        public static void Preload(int value)
        {
            if(CMDCache == null)
                CMDCache = new Dictionary<int, Queue<CacheCommand>>(16);
        }


        protected BaseCommand()
        {
            this.GenID();
        }

        protected static U Spawn<U>(int CMD_ID)  where U:BaseCommand<T>
        {
            if(CMDCache != null && CMDCache.ContainsKey(CMD_ID) && CMDCache[CMD_ID].Count >0)
            {
                CacheCommand Top = CMDCache[CMD_ID].Peek();
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

        protected void _BatchCall()
        {
            if(this.m_paused )
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
                        this._isDone = true;
                        this.m_isBatching =false;
                        this.Next = null;
                        MainLoop.getLoop().UnRegisterLoopEvent(MainLoopEvent.LateUpdate,this._SequenceCall);
                    }      
                }
            }
            else
            {
                this._isDone = true;
                this.m_isBatching =false;
                MainLoop.getLoop().UnRegisterLoopEvent(MainLoopEvent.LateUpdate,this._SequenceCall);
            }
        }

        protected void TryBatch()
        {
            if(this.Next != null &&!this.m_isBatching)
            {
                this.m_isBatching =true;
                this.Next.Excute();
                MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.LateUpdate,this._SequenceCall);
            }
        }

        protected void _SequenceCall(int value)
        {
            this._BatchCall();
        }


        //---------------pool--------
        public virtual void AwakeFromPool ()
        {
            this.GenID();
            this.m_paused =false;
            this.m_bExcuted =false;
            this.m_bReleased =false;
            this.m_isBatching =false;
            this.Next = null;
            this._isDone = false;
        }

        public virtual void RemovedFromPool ()
        {
            this.m_paused = false;
            this.m_bExcuted = false;
            this.m_bReleased = false;
            this.m_isBatching = false;
            this.Next = null;
            this._isDone = false;
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

        protected override void Reset()
        {
            base.Reset();
            this.m_bExcuted = false;
            this.m_bReleased = false;
            this.m_isBatching = false;
        }

        public override void Excute()
        {
            this.m_bExcuted = true;
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


