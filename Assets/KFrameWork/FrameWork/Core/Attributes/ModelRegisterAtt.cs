using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{

    [AttributeUsage( AttributeTargets.Method)]
    public class ModelRegisterAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ModelRegisterClassAttribute : TypeInitAttribute
    {
        public ModelRegisterClassAttribute() : base(-9999999)
        {
            
        }

        public ModelRegisterClassAttribute(int order) : base(order)
        {
            
        }
    }
}


