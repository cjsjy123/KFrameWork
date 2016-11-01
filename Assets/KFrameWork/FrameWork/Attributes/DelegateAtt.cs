using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace KFrameWork
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DelegateAttribute : Attribute {

        public MainLoopEvent   e;

        private DelegateAttribute()
        {
            
        }

        public DelegateAttribute(MainLoopEvent ev)
        {
            this.e =ev;
        }
    }
}


