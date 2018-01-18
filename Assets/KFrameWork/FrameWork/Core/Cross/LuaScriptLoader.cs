
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Reflection;
using System.Runtime.InteropServices;

#if TOLUA
using LuaInterface;
#endif

namespace KFrameWork
{
    public sealed class LuaScriptLoader :IScriptLoader
    {
#if TOLUA
        /// <summary>
        /// just for
        /// local x = class("xxxx")
        /// return x
        /// </summary>
        private static Dictionary<string,WeakReference> classFuncDictionary = new Dictionary<string, WeakReference>();

        private LuaFunction luafunc;

        private bool inited = false;

        private Script_LuaLogicAttribute attribute;

        private LuaTable Luatable;
        private string luafilepath;
#endif


        public string methodname { get; private set; }

        static LuaScriptLoader()
        {
            #if UNITY_EDITOR && TOLUA
            LogMgr.LogFormat("<color=#00B500FF>ToLua Version : {0}</color>",LuaDLL.version);
            #endif
        }
#if TOLUA
        public static LuaTable TryGetClassLuaFunction(string filename)
        {
            if (classFuncDictionary.ContainsKey(filename))
            {
                WeakReference weaktable = classFuncDictionary[filename];
                if (weaktable != null && weaktable.IsAlive)
                {
                    return weaktable.Target as LuaTable;
                }
            }
            return null;
        }
#endif

        public void Init (AbstractParams InitParams)
        {
#if TOLUA
            attribute = InitParams.ReadObject() as Script_LuaLogicAttribute;
#endif

        }

        void TryInit(Script_LuaLogicAttribute att)
        {
#if TOLUA
            if (inited)
            {
                return;
            }

            if (att != null)
            {
                this.luafilepath = att.luapath;
                this.methodname = att.methodName;
                this.Luatable = TryGetClassLuaFunction(att.luapath);
                if (this.Luatable == null)
                {
                    object[] retObj = LuaClient.GetMainState().DoFile(att.luapath.ToLower());

                    if (retObj != null && retObj.Length > 0)
                    {
                        LuaTable table = retObj[0] as LuaTable;
                        if (table != null)
                        {
                            classFuncDictionary[att.luapath] = new WeakReference(table);
                            this.Luatable = table;
                            this.luafunc = table[methodname] as LuaFunction;
                        }
                        else
                        {
                            LogMgr.LogErrorFormat("类型不匹配 :{0}", retObj[0]);
                        }
                    }
                    else
                    {
                        this.luafunc = LuaClient.GetMainState().GetFunction(methodname);
                    }
                }
                else
                {
                    this.luafunc = this.Luatable.GetLuaFunction(att.methodName);
                }
            }

            inited = true;
#endif
        }

        public AbstractParams Invoke (AbstractParams ScriptParms)
        {
#if TOLUA
            if (LuaClient.Instance == null) {
                LogMgr.Log ("未使用lua，但是程序集中包含了带有lua目标的函数的注册");
                return null;
            }

            TryInit(this.attribute);

            if (this.luafunc == null)
            {
                this.luafunc = LuaClient.GetMainState().GetFunction(luafilepath.Split('.')[0]+"."+ this.methodname);
                if (this.luafunc == null)
                {
                    //try get function from global
                    this.luafunc = LuaClient.GetMainState().GetFunction(this.methodname);
                }
            }

            if (this.luafunc != null) {
                int oldTop = this.luafunc.BeginPCall ();
                //retry
                if (Luatable == null)
                {
                    this.Luatable = TryGetClassLuaFunction(this.luafilepath);
                }
                    
                if(Luatable != null)
                {
                    this.luafunc.Push(Luatable);
                }
                
                this._LuaScriptParmsCall (ScriptParms);
                this.luafunc.PCall ();
                AbstractParams retparams = this._LuaRetCall(oldTop);

                this.luafunc.EndPCall ();
                return retparams;
            }
#else
            LogMgr.LogError("Lua Client Missing Cant Invoke Lua Function");
#endif

            return null;
        }

#if TOLUA
        private void _LuaScriptParmsCall (AbstractParams ScriptParms)
        {
            if (ScriptParms != null) {
                ScriptParms.ResetReadIndex ();

                for (int i = 0; i < ScriptParms.ArgCount; ++i) {
                    int tp = ScriptParms.GetArgIndexType (i);
                    if (tp == (int)ParamType.INT) {
                        this.luafunc.Push (ScriptParms.ReadInt ());
                    } else if (tp == (int)ParamType.BOOL) {
                        this.luafunc.Push (ScriptParms.ReadBool ());
                    } else if (tp == (int)ParamType.SHORT) {
                        this.luafunc.Push (ScriptParms.ReadShort ());
                    } else if (tp == (int)ParamType.FLOAT) {
                        this.luafunc.Push (ScriptParms.ReadFloat ());
                    } else if (tp == (int)ParamType.DOUBLE) {
                        this.luafunc.Push (ScriptParms.ReadDouble ());
                    } else if (tp == (int)ParamType.STRING) {
                        this.luafunc.Push (ScriptParms.ReadString ());
                    } else if (tp == (int)ParamType.OBJECT) {
                        this.luafunc.Push (ScriptParms.ReadObject ());
                    } else if (tp == (int)ParamType.UNITYOBJECT) {
                        this.luafunc.Push (ScriptParms.ReadUnityObject ());
                    } else if (tp == (int)ParamType.LONG) {
                        this.luafunc.Push (ScriptParms.ReadLong ());
                    }
                }
            }
        }

        private AbstractParams _LuaRetCall (int oldTop)
        {
            AbstractParams retparams = null;

            LuaState luastate =this.luafunc.GetLuaState ();
            int pos =oldTop ;
            while (!luastate .LuaIsNil(pos) && !luastate.lua_isnone(pos)) {
                pos++;

                if (retparams == null) {
                    retparams = GenericParams.Create ();
                }

                if (luastate.lua_isboolean(pos)) {
                    retparams.WriteBool (this.luafunc.CheckBoolean ());
                } else if (luastate.lua_islightuserdata(pos)) {
                    /**
                     * 因为目前TOlua的版本不开放获取intptr的途径，无法自己判断lua类型，所以需要修改代码：在luastate中加入：
                     *         public LuaValueType GetValueType(int Pos)
                                {
                                    return LuaDLL.tolua_getvaluetype(L,Pos);
                                }

                                public LuaTypes GetLuaType(int Pos)
                                {
                                    return LuaDLL.lua_type(L,Pos);
                                }
                     * */
                    LuaValueType type = luastate.GetValueType(pos);
                    if (type == LuaValueType.Bounds) {
                        retparams.WriteObject (this.luafunc.CheckBounds ());
                    } else if (type == LuaValueType.Color) {
                        retparams.WriteObject (this.luafunc.CheckColor ());
                    } else if (type == LuaValueType.Vector2) {
                        retparams.WriteObject (this.luafunc.CheckVector2 ());
                    } else if (type == LuaValueType.Vector3) {
                        retparams.WriteObject (this.luafunc.CheckVector3 ());
                    } else if (type == LuaValueType.Vector4) {
                        retparams.WriteObject (this.luafunc.CheckVector4 ());
                    } else if (type == LuaValueType.Quaternion) {
                        retparams.WriteObject (this.luafunc.CheckQuaternion ());
                    } else if (type == LuaValueType.Ray || type == LuaValueType.RaycastHit) {
                        retparams.WriteObject (this.luafunc.CheckRay ());
                    } else if (type == LuaValueType.LayerMask) {
                        retparams.WriteObject (this.luafunc.CheckLayerMask ());
                    } else if (type == LuaValueType.None) {
                        retparams.WriteObject (this.luafunc.CheckObject (typeof(System.Object)));
                    } else {
                        LogMgr.LogErrorFormat ("不支持的类型 {0}", type);
                    }
                    //Touch ------

                } else if (luastate.LuaIsNumber(pos)) {
                    retparams.WriteDouble (this.luafunc.CheckNumber ());
                } else if (luastate.LuaIsString(pos)) {
                    retparams.WriteString (this.luafunc.CheckString ());
                } else if (luastate.lua_istable(pos)) {
                    retparams.WriteObject (this.luafunc.CheckLuaTable ());
                } else if (luastate.lua_isfunction(pos)) {
                    retparams.WriteObject (this.luafunc.CheckLuaFunction ());
                } else if (luastate.LuaIsUserData(pos)) {
                    /**
                     * 因为目前TOlua的版本不开放获取intptr的途径，无法自己判断lua类型，所以需要修改代码：在luastate中加入：
                     *         public LuaValueType GetValueType(int Pos)
                                {
                                    return LuaDLL.tolua_getvaluetype(L,Pos);
                                }

                                public LuaTypes GetLuaType(int Pos)
                                {
                                    return LuaDLL.lua_type(L,Pos);
                                }
                     * */
                    LuaValueType type = luastate.GetValueType(pos);

                    if (type == LuaValueType.Bounds) {
                        retparams.WriteObject (this.luafunc.CheckBounds ());
                    } else if (type == LuaValueType.Color) {
                        retparams.WriteObject (this.luafunc.CheckColor ());
                    } else if (type == LuaValueType.Vector2) {
                        retparams.WriteObject (this.luafunc.CheckVector2 ());
                    } else if (type == LuaValueType.Vector3) {
                        retparams.WriteObject (this.luafunc.CheckVector3 ());
                    } else if (type == LuaValueType.Vector4) {
                        retparams.WriteObject (this.luafunc.CheckVector4 ());
                    } else if (type == LuaValueType.Quaternion) {
                        retparams.WriteObject (this.luafunc.CheckQuaternion ());
                    } else if (type == LuaValueType.Ray || type == LuaValueType.RaycastHit) {
                        retparams.WriteObject (this.luafunc.CheckRay ());
                    } else if (type == LuaValueType.LayerMask) {
                        retparams.WriteObject (this.luafunc.CheckLayerMask ());
                    } else if (type == LuaValueType.None) {
                        retparams.WriteObject (this.luafunc.CheckObject (typeof(System.Object)));
                    } else {
                        LogMgr.LogErrorFormat ("不支持的类型 {0}", type);
                    }
                    //Touch ------

                } else if (luastate.lua_isthread(pos)) {
                    retparams.WriteObject (this.luafunc.CheckLuaThread ());
                }

            }

            return retparams;
        }
#endif

        public void Reset ()
        {
#if TOLUA
            if (this.luafunc != null) {
                this.luafunc.Dispose ();
                this.luafunc = null;
                Luatable.Dispose();
                Luatable = null;
            }
#endif
        }
    }
}


