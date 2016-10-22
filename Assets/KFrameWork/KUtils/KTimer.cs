using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
namespace KUtils
{

    public class KTimer
    {

        public delegate void InvokeDelegate();

        private Timer _timer;

        private InvokeDelegate _InvokeFunc;

        private long invokeTimes = 0;

        public static bool DebugLog = false;

        public KTimer()
        {
            
        }

        private void TimerSth(object o)
        {
            //if (DebugLog)
            //{
            //    LogMgr.Log("启动函数");
            //}

            if (_InvokeFunc != null)
            {
                for (int i = 0; i < this.invokeTimes; ++i)
                {
                    _InvokeFunc();
                }

            }

            //if (DebugLog)
            //{
            //    LogMgr.Log("函数同步执行完毕");
            //}

        }

        public void StartTimer(long delaytime, long mstime, InvokeDelegate invokeFunc, int times = 0)
        {
            this._InvokeFunc = invokeFunc;
            if (this._timer == null)
            {
                this._timer = new Timer(TimerSth, null, delaytime, mstime);
            }
            else
            {
                this._timer.Change(delaytime, mstime);
            }
            this.invokeTimes = 0;
        }

        public void Stop()
        {
            if (this._timer != null)
            {
                this._timer.Dispose();
                this._timer = null;
            }
            else
            {
                //LogMgr.Log("timer not initialled");
            }

        }

        public void Reset()
        {
            this.Stop();
            this._InvokeFunc = null;
        }
    }
}
