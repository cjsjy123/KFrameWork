using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;


namespace KFrameWork
{
    public sealed class FSMRuningComponet:MonoBehaviour
    {

        private FSMElement Origin;

        private float _pausedrecordtime;

        private float _AppPausdrecordtime;

        private float _pausedApptme;

        private float _pausedtime;

        private bool _first =true;

        private float currenttime;

        private List<FSMRunningEvent> delayTimeList = new List<FSMRunningEvent>(8);
        private List<FSMRunningEvent> delayFrameList = new List<FSMRunningEvent>(8);

        void OnEnable()
        {
            if(_first)
            {
                _first =false;
            }  
            else
            {
                _pausedtime += Time.realtimeSinceStartup -_pausedrecordtime;
            }
        }

        void OnDisable()
        {
            _pausedrecordtime = Time.realtimeSinceStartup;
        }

        public T CreateElement<T>(KEnum state, FSMRunningEvent runner) where T:FSMElement,new()
        {
            T e = new T();
            e.Initiate(state,runner,this);
            if(Origin  == null)
                this.Origin =e;
            return e;
        }

        public void RemoveRunner(FSMRunningEvent runner )
        {
            if(runner != null)
            {
                if(runner.runningType == FSMRunningType.DelayFrame)
                {
                    this.delayFrameList.Remove(runner);
                }
                else if(runner.runningType == FSMRunningType.DelayTime)
                {
                    this.delayTimeList.Remove(runner);
                }
            }
        }


        private int _delayFrameSort(FSMRunningEvent left,FSMRunningEvent right)
        {
            return left.delayFrame.Value - right.delayFrame.Value;
        }

        private int _delaytimeSort(FSMRunningEvent left,FSMRunningEvent right)
        {
            float r =left.delaytime.Value - right.delaytime.Value;
            if(r >0)
            {
                return 1;
            }
            else if(r.FloatEqual(0f))
            {
                return 0;
            }
            return -1;
        }

        private void _adjustTimeContainer(FSMRunningEvent runner)
        {
            if(runner.changed)
            {
                runner.lasttime = this.currenttime;
                if(this.delayTimeList.Contains(runner))
                {
                    this.delayTimeList.Sort(_delaytimeSort);
                }
                else
                {
                    this.delayTimeList.Add(runner);
                    int origion =-1;
                    FSMRunningEvent temp = null;
                    FSMRunningEvent next = null;
                    for(int i=0; i < this.delayTimeList.Count-1;++i)
                    {
                        if(origion !=-1)
                        {
                            temp = this.delayTimeList[i+1];
                            this.delayTimeList[i+1] = next;
                            next = temp;
                        }
                        else
                        {
                            FSMRunningEvent element= this.delayTimeList[i];
                            if(element.delaytime.Value > runner.delaytime.Value)
                            {
                                origion = i;
                                next = this.delayTimeList[i+1];
                                this.delayTimeList[i+1] = this.delayTimeList[i];
                            }
                        }
                    }

                    if(origion != -1)
                        this.delayTimeList[origion] = runner;

                    
                }

                runner.changed =false;
            }

        }

        private void _adjustFrameContainer(FSMRunningEvent runner)
        {
            if(runner.changed)
            {
                runner.lastFrame =GameSyncCtr.mIns.RenderFrameCount;
                if(this.delayFrameList.Contains(runner))
                {
                    this.delayFrameList.Sort(_delayFrameSort);
                }
                else
                {
                    this.delayFrameList.Add(runner);
                    int origion =-1;
                    FSMRunningEvent temp = null;
                    FSMRunningEvent next = null;
                    for(int i=0; i < this.delayFrameList.Count-1;++i)
                    {
                        if(origion !=-1)
                        {
                            temp = this.delayFrameList[i+1];
                            this.delayFrameList[i+1] = next;
                            next = temp;
                        }
                        else
                        {
                            FSMRunningEvent element= this.delayFrameList[i];
                            if(element.delayFrame.Value > runner.delayFrame.Value)
                            {
                                origion = i;
                                next = this.delayFrameList[i+1];
                                this.delayFrameList[i+1] = this.delayFrameList[i];
                            }
                        }
                    }

                    this.delayFrameList[origion] = runner;
                }

                runner.changed =false;
            }
        }

        private void _invokeElement(FSMElement element)
        {
            if(element != null && element.Active )
            {
                if(element.Runner != null )
                {
                    switch(element.Runner.runningType)
                    {
                    case FSMRunningType.DelayFrame:
                        {
                            this._adjustFrameContainer(element.Runner);
                            break;
                        }
                    case FSMRunningType.DelayTime:
                        {
                            this._adjustTimeContainer(element.Runner);
                            break;

                        }
                    case FSMRunningType.Frame:
                        {
                            element.Runner.FrameUpdateForLogic();
                            break;
                        }
                    }
                }
                else if(element.Runner == null)
                {
                    LogMgr.LogErrorFormat("dont contain a runner on {0}",element);
                }

                List<FSMElement> list = element.Children;
                for(int i =0; i <list.Count;++i)
                {
                    this._invokeElement(list[i]);
                }

            }
            else if(FrameWorkConfig.Open_DEBUG && element != null && !element.Active)
            {
                LogMgr.LogErrorFormat("inactive in {0} ",element);
            }
        }

        private int _CalInvokeTimes(FSMRunningEvent e)
        {
            if(e.runningType == FSMRunningType.DelayTime)
            {
                float delta = this.currenttime - e.lasttime;
                int num = (int)(delta / e.delaytime.Value);
                return num;
            }
            else if(e.runningType == FSMRunningType.DelayFrame)
            {
                if(e.delayFrame ==0)
                    return 0;
                long delta = GameSyncCtr.mIns.RenderFrameCount - e.lastFrame;
                int num = (int)(delta /e.delayFrame);
                return num;
                   
            }
            return 0;
        }

        private void _delayInvoke(FSMRunningEvent e)
        {
            int num = this._CalInvokeTimes(e);
            for(int i=0;i < num;++i)
            {
                e.DelayTimeUpdateForLogic();
            }

            if(num >0)
            {
                if(e.runningType == FSMRunningType.DelayTime)
                {
                    e.lasttime = this.currenttime;
                }
                else if(e.runningType  == FSMRunningType.DelayFrame)
                {
                    e.lastFrame = GameSyncCtr.mIns.RenderFrameCount;
                }
            }
        }

        void Update()
        {
            this.currenttime = Time.realtimeSinceStartup - this._pausedtime -this._pausedApptme;

            _invokeElement(this.Origin);

            for(int i=0; i < this.delayTimeList.Count;++i)
            {
                this._delayInvoke(this.delayTimeList[i]);
            }

            for(int i=0; i < this.delayFrameList.Count;++i)
            {
                this._delayInvoke( this.delayTimeList[i]);
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if(pauseStatus)
            {
                _AppPausdrecordtime = Time.realtimeSinceStartup;
            }
            else
            {
                this._pausedApptme += Time.realtimeSinceStartup - _AppPausdrecordtime;
            }
        }

    }
}

