using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    ///// <summary>
    ///// 脚本执行顺序定义属性
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Class)]
    //public class ScriptInitOrderAttribute : Attribute {
    //    public int Order =0;

    //    public ScriptInitOrderAttribute(int order)
    //    {
    //        this.Order = order;
    //    }
    //}
    /// <summary>
    /// time 定义属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TimeSetAttribute : Attribute
    {
        public float fixedstep ;

        public TimeSetAttribute(float time)
        {
            this.fixedstep = time;
        }
    }
    /// <summary>
    /// tag定义属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TagSetAttribute : Attribute
    {
        public List<string> tags;

        public TagSetAttribute(params string[] ts)
        {
            this.tags = new List<string>();
            this.tags.AddRange(ts);
        }
    }
    /// <summary>
    /// layer定义属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LayerSetAttribute : Attribute
    {
        public List<string>  layers;

        public LayerSetAttribute(params string[] ts)
        {
            this.layers = new List<string>();
            this.layers.AddRange(ts);
        }
    }

    /// <summary>
    /// 脚本宏定义属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptMarcoDefineAttribute : Attribute
    {
        public List<string> names;

        public ScriptMarcoDefineAttribute(params string[] s)
        {
            this.names = new List<string>(s);
        }
    }

}

