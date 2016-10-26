using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    /// <summary>
    ///FSM runner 状态逻辑运行层
    /// </summary>
    public abstract class FSMRunner:FSMRunningEvent
    {
        protected string _name;

        public string name {
            get {
                return _name;
            }
            set {
                _name =value;
            }
        }

        public virtual FSMRunningType runningType {
            get {
                return FSMRunningType.WhenEnableInvoke;
            }
        }

        private long _lastFrame;

        public long lastFrame {
            get {
                return _lastFrame;
            }
            set {
                _lastFrame =value;
            }
        }

        private float _lasttime;
        public float lasttime {
            get {
                return _lasttime;
            }
            set {
                _lasttime =value;
            }
        }

        protected bool _change;

        public bool changed
        {
            get
            {
                return _change;
            }

            set
            {
                _change =value;
            }
        }

        protected float? _delaytime;
        protected int? _delayframe;

        public float? delaytime
        {
            get
            {
                return this._delaytime;
            }
            set
            {
                if(this._delaytime != value && this.runningType == FSMRunningType.DelayTime)
                {
                    _change =true;
                }

                this._delaytime = value;
            }
        }

        public int? delayFrame
        {
            get
            {
                return this._delayframe;
            }
            set
            {
                if(this._delayframe != value && this.runningType == FSMRunningType.DelayFrame)
                {
                    _change =true;
                }

                this._delayframe = value;
            }
        }


        public abstract void FrameUpdateForLogic ();

        public abstract void DelayFrameUpdateForLogic();

        public abstract void DelayTimeUpdateForLogic ();

        public abstract void InvokeOnceWhenInit ();

        public abstract void InvokeOnceWhenEnable ();
    }
}
