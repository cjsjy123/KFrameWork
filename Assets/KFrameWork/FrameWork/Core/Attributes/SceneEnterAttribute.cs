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
        public SceneEnterAttribute(int p)
        {
            this.Priority = p;
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
        public SceneLeaveAttribute(int p)
        {
            this.Priority = p;
        }

        public SceneLeaveAttribute()
        {
            this.Priority = 0;
        }
    }
}


