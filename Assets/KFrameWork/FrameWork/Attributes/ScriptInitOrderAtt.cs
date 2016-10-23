using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptInitOrderAtt : Attribute {
        public int Order =0;

        public ScriptInitOrderAtt(int order)
        {
            this.Order = order;
        }
    }
}

