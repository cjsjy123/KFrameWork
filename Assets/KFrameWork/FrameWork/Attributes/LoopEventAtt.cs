using System;
using UnityEngine;

namespace KFrameWork
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LoopEventAttribute:Attribute
    {
        public LoopMonoEvent ev = LoopMonoEvent.Awake; 
        
        public LoopEventAttribute(){}

        public LoopEventAttribute(LoopMonoEvent e)
        {
            this.ev = e;
        }

    }
}



