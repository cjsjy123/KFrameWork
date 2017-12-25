using System;
using UnityEngine;

namespace KFrameWork
{
    /// <summary>
    /// EnterID
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SceneEnterAttribute :Attribute
    {
        public int Priority = 0;
        public SceneEnterAttribute(int priority)
        {
            this.Priority = priority;
        }

        public SceneEnterAttribute()
        {
            this.Priority = 0;
        }
    }
    /// <summary>
    /// LeaveID
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SceneLeaveAttribute :Attribute
    {
        public int Priority = 0;
        public SceneLeaveAttribute(int priority)
        {
            this.Priority = priority;
        }

        public SceneLeaveAttribute()
        {
            this.Priority = 0;
        }
    }
}


