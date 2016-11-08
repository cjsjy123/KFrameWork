using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptInitOrderAttribute : Attribute {
        public int Order =0;

        public ScriptInitOrderAttribute(int order)
        {
            this.Order = order;
        }
    }
}

