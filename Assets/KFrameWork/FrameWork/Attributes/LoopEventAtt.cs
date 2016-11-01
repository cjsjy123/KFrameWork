using System;
using UnityEngine;

namespace KFrameWork
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LoopEventAttribute:Attribute
    {
        public MainLoopEvent ev = MainLoopEvent.Awake; 
        
        public LoopEventAttribute(){}

        public LoopEventAttribute(MainLoopEvent e)
        {
            this.ev = e;
        }

    }
}



