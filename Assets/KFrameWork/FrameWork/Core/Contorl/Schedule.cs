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

            public readonly long InvokeTime;

            public readonly int totalCnt;

            private float deltatime;
            public int RepeatCount { get; private set; }

            public ScheduleData(long invokeframe,int repeat ,float delta, object callvalue,Action<object,int> cbk)
            {
                this.InvokeTime = invokeframe;
                this.RepeatCount = repeat;
                this.Delta = delta;
                this.deltatime = -1f;
                this.Callback= cbk;
                this.callparams = callvalue;
                this.Schedule_Type =  ScheduleType.DeltaTime;
                this.DeltaFrame = -1;
                this.recordFrame = -1;
                this.totalCnt = this.RepeatCount;
            }

            public ScheduleData(long invokeframe, int repeat, long deltaFrame, object callvalue, Action<object, int> cbk)
            {
                this.InvokeTime = invokeframe;
                this.RepeatCount = repeat;
                this.Delta = -1;
                this.deltatime = -1f;
                this.Callback = cbk;
                this.callparams = callvalue;
                this.Schedule_Type = ScheduleType.DeltaFrame;
                this.DeltaFrame = deltaFrame;
                this.recordFrame = -1;
                this.totalCnt = this.RepeatCount;
            }


            public void CallEvent()
            {
                if (this.Schedule_Type == ScheduleType.DeltaFrame)
                {
                    if (this.DeltaFrame > 0  && recordFrame >0 )
                    {
                        bool frameenable = (GameSyncCtr.mIns.RenderFrameCount - this.recordFrame) % this.DeltaFrame == 0;
                        if (this.RepeatCount < 0 && frameenable)
                        {
                            if (this.Callback != null)
                                this.Callback(this.callparams, this.RepeatCount);
                        }
                        else if (frameenable)
                        {
                            this.RepeatCount--;
                            if (this.Callback != null)
                                this.Callback(this.callparams, this.RepeatCount);
                        }
                    }
                    else
                    {
                        this.RepeatCount--;
                        this.recordFrame = GameSyncCtr.mIns.RenderFrameCount;
                        if (this.Callback != null)
                            this.Callback(this.callparams, this.RepeatCount);
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
                            this.deltatime -= GameSyncCtr.mIns.RenderDeltaTime;
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
                            this.deltatime -= GameSyncCtr.mIns.RenderDeltaTime;
                        }
                    }
                }
            }
        }

        private List<ScheduleData> scheduleList = new List<ScheduleData>();

        public Schedule()
        {
            if(MainLoop.getInstance()!= null)
                MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.BeforeUpdate,UpdateSchedule);
        }

        public void Destroy()
        {
            if (MainLoop.getInstance() != null)
                MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.BeforeUpdate, UpdateSchedule);
            scheduleList.Clear();
        }
        /// <summary>
        /// just one invoke
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="o"></param>
        /// <param name="callback"></param>
        public void ScheduleInvoke(float delay,object o,Action<object,int> callback)
        {
            if(delay < 0f)
            {
                LogMgr.LogErrorFormat("参数错误 {0}",delay.ToString());
                return;
            }

            long invoketime = GameSyncCtr.mIns.RenderFrameCount + Mathf.RoundToInt( delay * GameSyncCtr.mIns.FrameRate);

            bool insert = false;
            for (int i = 0; i < scheduleList.Count; ++i)
            {
                ScheduleData data = scheduleList[i];
                if (data.InvokeTime > invoketime)
                {
                    this.scheduleList.Insert(i, new ScheduleData(invoketime, 1,0f, o, callback));
                    insert = true;
                    break;
                }
            }


            if (!insert)
            {
                this.scheduleList.Add(new ScheduleData(invoketime, 1, 0f, o, callback));
            }
        }

        public void UnScheduleInvoke(Action<object,int> callback)
        {
            if (scheduleList == null)
                return;

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

            long invoketime = GameSyncCtr.mIns.RenderFrameCount + Mathf.RoundToInt(delay * GameSyncCtr.mIns.FrameRate);
            if (this.scheduleList.Count >0)
            {
                bool inserted =false;
                for(int i = scheduleList.Count - 1; i >= 0; --i)
                {
                    ScheduleData data = scheduleList[i];
                    if (data.InvokeTime > invoketime)
                    {
                        this.scheduleList.Insert(i,new ScheduleData(invoketime,Repeat,delta, o,callback));
                        inserted =true;
                        break;
                    }
                }

                if(!inserted)
                {
                    this.scheduleList.Add(new ScheduleData(invoketime,Repeat,delta, o, callback));
                }
            }
            else
            {
                this.scheduleList.Add(new ScheduleData(invoketime,Repeat,delta, o, callback));
            }
        }

        public void ScheduleRepeatFrameInvoke(int delay, int deltaFrame, int Repeat, object o, Action<object, int> callback)
        {
            if (Repeat == 0 || deltaFrame < 0f )
            {
                LogMgr.LogErrorFormat("参数错误 {0} {1} {2}", delay.ToString(), deltaFrame.ToString(), Repeat.ToString());
                return;
            }

            long invoketime = GameSyncCtr.mIns.RenderFrameCount + delay;
            bool inserted = false;
            for (int i = scheduleList.Count - 1; i >= 0; --i)
            {
                ScheduleData data = scheduleList[i];
                if (data.InvokeTime > invoketime)
                {
                    this.scheduleList.Insert(0, new ScheduleData(invoketime, Repeat,deltaFrame,o,callback));
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                this.scheduleList.Add(new ScheduleData(invoketime, Repeat, deltaFrame, o, callback));
            }
        }

        private void UpdateSchedule(int value)
        {
            List<ScheduleData> listv = ListPool.TrySpawn<List<ScheduleData>>();
            listv.AddRange(this.scheduleList);
            for (int i = 0; i <  listv.Count; ++i)
            {
                ScheduleData data = listv[i];

                if(data.InvokeTime <= GameSyncCtr.mIns.RenderFrameCount)
                {
                    data.CallEvent();

                    if (data.RepeatCount == 0)
                    {
                        this.scheduleList.Remove(data);
                    }
                }
            }

            ListPool.TryDespawn(listv);
        }

    }
}


