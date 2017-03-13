using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace KFrameWork
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DelegateMethodAttribute : Attribute {

        public MainLoopEvent   Invokeopportunity;

        public string name;

        public Type tp;

        private DelegateMethodAttribute()
        {
            
        }

        public DelegateMethodAttribute(MainLoopEvent ev,string pname,Type passtp)
        {
            this.Invokeopportunity =ev;
            this.name = pname;
            this.tp = passtp;
        }
    }


}


