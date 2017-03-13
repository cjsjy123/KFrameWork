using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{

    public class WaitTrueTask : ITask
    {
        private Func<bool> waittask;

        public bool KeepWaiting
        {
            get
            {
                return waittask();
            }
        }

        public WaitTrueTask(Func<bool> func)
        {
            waittask = func;
        }
    }

    public class WaitFalseTask : ITask
    {
        private Func<bool> waittask;

        public bool KeepWaiting
        {
            get
            {
                return !waittask();
            }
        }

        public WaitFalseTask(Func<bool> func)
        {
            waittask = func;
        }
    }
}
