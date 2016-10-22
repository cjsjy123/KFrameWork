using System;
using UnityEngine;

namespace KFrameWork
{

    [AttributeUsage(AttributeTargets.Method)]
    public class SceneEnterAttribute :Attribute
    {
        public SceneEnterAttribute(){}
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ScenLeaveAttribute :Attribute
    {
        public ScenLeaveAttribute(){}
    }
}


