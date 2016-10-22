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
    [AttributeUsage(AttributeTargets.Method)]
    public class Script_LuaLogicAttribute:Attribute
    {
        public int CMD;

        public string methodName;

        public Script_LuaLogicAttribute(int pCMD)
        {
            CMD = pCMD;
            methodName = "";

        }

        public Script_LuaLogicAttribute(int pCMD, string name)
        {
            CMD = pCMD;
            methodName = name;

        }

    }

  [AttributeUsage(AttributeTargets.Method)]
  public class Script_SharpLogicAttribute:Attribute
  {
    public int CMD;
    public Script_SharpLogicAttribute(){}

    public Script_SharpLogicAttribute(int pCMD)
    {
      CMD = pCMD;
    }

  }

}

