using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    [SingleTon]
    public sealed class Schedule  {

        public static Schedule mIns;

        private struct ScheduleData:IEquatable<ScheduleData>
        {

            public float Delta;
            public Action Callback;

            private int _totalCnt;
            private float _time;
            public float InvokeTime
            {
                get
                {
                    return this._time + this.Delta *(this._totalCnt - this.RepeatCount);
                }
            }

            private int _Repeat;
            public int RepeatCount
            {
                get
                {
                    return this._Repeat;
                }
                set
                {
                    this._Repeat =value;
                }
            }

            public ScheduleData(float invoke,int repeat ,float delta,Action cbk)
            {
                this._totalCnt =repeat;
                this._time = invoke;
                this._Repeat = repeat;
                this.Delta = delta;
                this.Callback= cbk;
            }
                

            public bool Equals (ScheduleData other)
            {
                if(this.InvokeTime != other.InvokeTime) return false;
                if(this.RepeatCount != other.RepeatCount) return false;
                if(this.Delta != other.Delta) return false;
                if(this.Callback != other.Callback) return false;

                return true;
            }
        }


        private LinkedList<ScheduleData> scheduleList = new LinkedList<ScheduleData>();

//        private Queue<LinkedListNode<ScheduleData>> removeList = new Queue<LinkedListNode<ScheduleData>>(4);
//
        public Schedule()
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.BeforeUpdate,UpdateSchedule);
        }
            
        public void ScheduleInvoke(float delay,Action callback)
        {
            if(delay < 0f)
            {
                LogMgr.LogErrorFormat("参数错误 {0}",delay.ToString());
                return;
            }

            float invoketime = Time.realtimeSinceStartup + delay;
            if(this.scheduleList.Count >0)
            {
                for(var firstPriorityNode = this.scheduleList.First;firstPriorityNode != null;firstPriorityNode = firstPriorityNode.Next )
                {
                    if(firstPriorityNode.Value.InvokeTime > invoketime)
                    {
                        this.scheduleList.AddBefore(firstPriorityNode,new ScheduleData(invoketime,1,0f,callback));
                        break;
                    }
                }
            }
            else
            {
                this.scheduleList.AddLast(new ScheduleData(invoketime,1,0f,callback));
            }

        }

        public void UnScheduleInvoke(Action callback)
        {
            for(var firstPriorityNode = this.scheduleList.First;firstPriorityNode != null;firstPriorityNode = firstPriorityNode.Next )
            {
                if(firstPriorityNode.Value.Callback == callback)
                {
                    this.scheduleList.Remove(firstPriorityNode);
                    break;
                }
            }

        }
        /// <summary>
        /// 延迟delay秒之后每隔delta秒执行callback，执行次数为repeat
        /// </summary>
        /// <param name="delay">Delay.</param>
        /// <param name="delta">Delta.</param>
        /// <param name="Repeat">Repeat.</param>
        /// <param name="callback">Callback.</param>
        public void ScheduleRepeatInvoke(float delay,float delta,int Repeat,Action callback)
        {
            if(Repeat == 0 || delta <0f || delay <0f)
            {
                LogMgr.LogErrorFormat("参数错误 {0} {1} {2}",delay.ToString(),delta.ToString(),Repeat.ToString());
                return;
            }

            float invoketime = Time.realtimeSinceStartup + delay;
            if(this.scheduleList.Count >0)
            {
                bool inserted =false;
                for(var firstPriorityNode = this.scheduleList.First;firstPriorityNode != null;firstPriorityNode = firstPriorityNode.Next )
                {
                    if(firstPriorityNode.Value.InvokeTime > invoketime)
                    {
                        this.scheduleList.AddBefore(firstPriorityNode,new ScheduleData(invoketime,Repeat,delta,callback));
                        inserted =true;
                        break;
                    }
                }

                if(!inserted)
                {
                    this.scheduleList.AddLast(new ScheduleData(invoketime,Repeat,delta,callback));
                }
            }
            else
            {
                this.scheduleList.AddLast(new ScheduleData(invoketime,Repeat,delta,callback));
            }
        }

        private void UpdateSchedule(int value)
        {

            float now = Time.realtimeSinceStartup;
            for(var firstPriorityNode = this.scheduleList.First;firstPriorityNode != null; )
            {
                ScheduleData data = firstPriorityNode.Value;
                if(data.InvokeTime < now)
                {
                    if(data.RepeatCount >0)
                    {
                        data.Callback();
                        data.RepeatCount --;
                        if(data.RepeatCount ==0)
                        {  
                            LinkedListNode<ScheduleData> n = firstPriorityNode.Next;
                            this.scheduleList.Remove(firstPriorityNode);
                            firstPriorityNode = n;
                            continue;
                        }
                        else
                        {
                            firstPriorityNode.Value = data;
                        }
                    }
                    else
                    {
                        data.Callback();
                    }

                }

                firstPriorityNode = firstPriorityNode.Next;
            }

//            while(this.removeList.Count >0)
//            {
//                this.scheduleList.Remove(this.removeList.Dequeue());
//            }

        }

    }
}


