using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    [AttributeUsage( AttributeTargets.Class)]
    public class NetAutoRegisterAttribute : Attribute
    {
        public int id;

        public NetAutoRegisterAttribute(int arg)
        {
            this.id = arg;
        }
    }
}

