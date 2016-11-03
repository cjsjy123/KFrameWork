using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace KFrameWork
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DelegateMethodAttribute : Attribute {

        public MainLoopEvent   e;

        public string name;

        public Type tp;

        private DelegateMethodAttribute()
        {
            
        }

        public DelegateMethodAttribute(MainLoopEvent ev,string pname,Type passtp)
        {
            this.e =ev;
            this.name = pname;
            this.tp = passtp;
        }
    }


}


