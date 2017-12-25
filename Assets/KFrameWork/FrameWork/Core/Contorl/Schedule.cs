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

        private enum ScheduleType
        {
            DeltaTime,
            DeltaFrame,
        }

        private class ScheduleData
        {
            public ScheduleType Schedule_Type;

            public long DeltaFrame;

            public float Delta;

            private object callparams;

            public Action<object,int> Callback;

            public long recordFrame { get; private set; }

            public readonly float InvokeTime;

            public long TargetFrame
            {
                get
                {
                    if (GameSyncCtr.mIns.FrameWorkTime > this.InvokeTime && totalCnt >0)
                    {
                        return this.recordFrame + this.DeltaFrame * totalCnt;
                    }
                    return this.recordFrame;
                }
            }

            public readonly int totalCnt;

            private float deltatime;

            public int RepeatCount { get; private set; }

            public ScheduleData(float invoke,int repeat ,float delta,long deltaFrame, ScheduleType schType, object callvalue,Action<object,int> cbk)
            {
                this.InvokeTime = invoke;
                this.RepeatCount = repeat;
                this.Delta = delta;
                this.deltatime = Delta;
                this.Callback= cbk;
                this.callparams = callvalue;
                this.Schedule_Type = schType;
                this.DeltaFrame = deltaFrame;
                this.recordFrame = -1;
                this.totalCnt = this.RepeatCount;
            }

            public void StartFrameCounter()
            {
                if(this.recordFrame == -1)
                    this.recordFrame = GameSyncCtr.mIns.RenderFrameCount;
            }

            public void CallEvent()
            {
                if (this.Schedule_Type == ScheduleType.DeltaFrame)
                {
                    if (this.DeltaFrame > 0  )
                    {
                        if (this.RepeatCount < 0 && (GameSyncCtr.mIns.RenderFrameCount -this.recordFrame  ) % this.DeltaFrame == 0)
                        {
                            if (this.Callback != null)
                                this.Callback(this.callparams, this.RepeatCount);
                        }
                        else if ((this.TargetFrame - GameSyncCtr.mIns.RenderFrameCount) % this.DeltaFrame == 0)
                        {
                            this.RepeatCount--;
                            if (this.Callback != null)
                                this.Callback(this.callparams, this.RepeatCount);
                        }
                    }
                }
                else if (this.Schedule_Type == ScheduleType.DeltaTime)
                {
                    if (this.RepeatCount > 0)
                    {
                        if (this.deltatime < 0f)
                        {
                            this.deltatime = this.Delta;
                            this.RepeatCount--;
                            if (this.Callback != null)
                                this.Callback(this.callparams, this.RepeatCount);
                        }
                        else
                        {
                            this.deltatime -= Time.deltaTime;
                        }
                    }
                    else if (this.RepeatCount < 0)
                    {
                        if (this.deltatime < 0f)
                        {
                            this.deltatime = this.Delta;
                            if (this.Callback != null)
                                this.Callback(this.callparams, this.RepeatCount);
                        }
                        else
                        {
                            this.deltatime -= Time.deltaTime;
                        }
                    }
                }
            }
        }

        private List<ScheduleData> scheduleList = new List<ScheduleData>();

        public Schedule()
        {
            if(MainLoop.getLoop()!= null)
                MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.BeforeUpdate,UpdateSchedule);
        }

        public void Destroy()
        {
            if (MainLoop.getLoop() != null)
                MainLoop.getLoop().UnRegisterLoopEvent(MainLoopEvent.BeforeUpdate, UpdateSchedule);
            scheduleList.Clear();
            scheduleList = null;
        }
            
        public void ScheduleInvoke(float delay,object o,Action<object,int> callback)
        {
            if(delay < 0f)
            {
                LogMgr.LogErrorFormat("参数错误 {0}",delay.ToString());
                return;
            }

            float invoketime = GameSyncCtr.mIns.FrameWorkTime + delay;

            bool insert = false;
            for (int i = 0; i < scheduleList.Count; ++i)
            {
                ScheduleData data = scheduleList[i];
                if (data.InvokeTime > invoketime)
                {
                    this.scheduleList.Insert(i, new ScheduleData(invoketime, 1, 0f, 0, ScheduleType.DeltaTime, o, callback));
                    insert = true;
                    break;
                }
            }


            if (!insert)
            {
                this.scheduleList.Add(new ScheduleData(invoketime, 1, 0f, 0, ScheduleType.DeltaTime, o, callback));
            }
        }

        public void UnScheduleInvoke(Action<object,int> callback)
        {
            for(int i = scheduleList.Count -1; i >= 0; --i)
            {
                ScheduleData data = scheduleList[i];
                if (data.Callback == callback)
                {
                    scheduleList.RemoveAt(i);
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
        public void ScheduleRepeatInvoke(float delay,float delta,int Repeat,object o,Action<object,int> callback)
        {
            if(Repeat == 0 || delta <0f || delay <0f)
            {
                LogMgr.LogErrorFormat("参数错误 {0} {1} {2}",delay.ToString(),delta.ToString(),Repeat.ToString());
                return;
            }

            float invoketime = GameSyncCtr.mIns.FrameWorkTime + delay;
            if(this.scheduleList.Count >0)
            {
                bool inserted =false;
                for(int i = scheduleList.Count - 1; i >= 0; --i)
                {
                    ScheduleData data = scheduleList[i];
                    if (data.InvokeTime > invoketime)
                    {
                        this.scheduleList.Insert(i,new ScheduleData(invoketime,Repeat,delta,0, ScheduleType.DeltaTime, o,callback));
                        inserted =true;
                        break;
                    }
                }

                if(!inserted)
                {
                    this.scheduleList.Add(new ScheduleData(invoketime,Repeat,delta, 0, ScheduleType.DeltaTime, o, callback));
                }
            }
            else
            {
                this.scheduleList.Add(new ScheduleData(invoketime,Repeat,delta, 0, ScheduleType.DeltaTime, o, callback));
            }
        }

        public void ScheduleRepeatFrameInvoke(float delay, int deltaFrame, int Repeat, object o, Action<object, int> callback)
        {
            if (Repeat == 0 || deltaFrame < 0f )
            {
                LogMgr.LogErrorFormat("参数错误 {0} {1} {2}", delay.ToString(), deltaFrame.ToString(), Repeat.ToString());
                return;
            }

            float invoketime = GameSyncCtr.mIns.FrameWorkTime + delay;
            bool inserted = false;
            for (int i = scheduleList.Count - 1; i >= 0; --i)
            {
                ScheduleData data = scheduleList[i];
                if (data.InvokeTime > invoketime)
                {
                    this.scheduleList.Insert(0, new ScheduleData(invoketime, Repeat, 0, deltaFrame, ScheduleType.DeltaFrame, o, callback));
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                this.scheduleList.Add(new ScheduleData(invoketime, Repeat, 0, deltaFrame, ScheduleType.DeltaFrame, o, callback));
            }
        }

        private void UpdateSchedule(int value)
        {
            float now = GameSyncCtr.mIns.FrameWorkTime;

            List<ScheduleData> listv = ListPool.TrySpawn<List<ScheduleData>>();
            listv.AddRange(this.scheduleList);
            for (int i = 0; i <  listv.Count; ++i)
            {
                ScheduleData data = listv[i];

                if(data.InvokeTime < now )
                {
                    if (data.Schedule_Type == ScheduleType.DeltaTime)
                    {
                        data.CallEvent();

                        if (data.RepeatCount == 0)
                        {
                            this.scheduleList.Remove(data);
                        }
                    }
                    else if (data.Schedule_Type == ScheduleType.DeltaFrame)
                    {
                        data.StartFrameCounter();
                        data.CallEvent();
                        if (data.TargetFrame <= GameSyncCtr.mIns.RenderFrameCount)
                        {
                            this.scheduleList.Remove(data);
                        }
                    }
                }
            }

            ListPool.TryDespawn(listv);

        }

    }
}


