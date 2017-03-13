//#define TOLUA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Reflection;
#if TOLUA
using LuaInterface;
#endif


namespace KFrameWork
{
    /// <summary>
    /// 限制静态，可以使得不修改原始的Script_SharpLogicAttribute注册的方法，直接切换为Script_LuaLogicAttribute属性对应的方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Script_LuaLogicAttribute : Attribute
    {
        public int CMD;

        public string luapath;

        public string methodName;

        public Script_LuaLogicAttribute(int pCMD, string path, string name)
        {
            luapath = path;
            CMD = pCMD;
            methodName = name;
        }

    }
    /// <summary>
    /// script 静态方法属性，供scriptcommd调用，静态可以无实例gc
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Script_SharpLogicAttribute : Attribute
    {
        public int CMD;
        public Script_SharpLogicAttribute() { }

        public Script_SharpLogicAttribute(int pCMD)
        {
            CMD = pCMD;
        }

    }

}

