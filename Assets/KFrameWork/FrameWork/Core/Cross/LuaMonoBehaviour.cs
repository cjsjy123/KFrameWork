using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

#if TOLUA
using LuaInterface;
#endif
namespace KFrameWork
{
    public class LuaMonoBehaviour : MonoBehaviour
    {
        public bool LuaMode = false;
#if TOLUA
        private LuaTable _table;

        public LuaTable table
        {
            get
            {
                if (!LuaMode)
                    return null;

                if (_table == null && LuaClient.GetMainState() != null)
                {
                    if (string.IsNullOrEmpty(luapath))
                        luapath = TypeName + ".lua";

                    object[] os = LuaClient.GetMainState().DoFile(this.luapath);
                    if (os != null && os.Length > 0)
                    {
                        LuaTable luatable = os[0] as LuaTable;
                        if (luatable != null)
                        {
                            _table = luatable;
                        }
                        else
                            LogMgr.LogErrorFormat("Missing Luatable named:{0}", this.luapath);
                    }

                    TryAwake();
                }

                return _table;
            }
        }

        private LuaFunction awakeFunc;

        private LuaFunction startFunc;

        private LuaFunction onDestroyFunc;
#endif
        [HideInInspector]
        public string respath;
        [SerializeField]
        private string luapath ;

        private bool awakebefore = false;

        private string _typename;
        public string TypeName
        {
            get
            {
                if (_typename == null)
                {
                    _typename = GetType().Name;
                }
                return _typename;
            }
        }


        public bool HasTable()
        {
#if TOLUA
            return _table != null;
#else
            return false;
#endif
        }

        protected virtual void Awake()
        {
            if (!string.IsNullOrEmpty(respath)
#if TOLUA
                && LuaClient.Instance != null
#endif
                )
            {
                TryAwake();
            }
        }

        public void InitLua(string path)
        {
            if (awakebefore)
                return;

            if (string.IsNullOrEmpty(luapath))
            {
                this.respath = path;
                this.luapath = path.Sub2Begin('.') + ".lua";
            }

            TryAwake();
        }

        protected virtual void TryAwake()
        {
            if (awakebefore)
                return;
            awakebefore = true;
#if TOLUA
            if (table != null)
            {
                this.awakeFunc = table.GetLuaFunction("Awake");
                if (this.awakeFunc != null)
                {
                    this.awakeFunc.BeginPCall();
                    this.awakeFunc.Push(table);
                    this.awakeFunc.Push(this);
                    this.awakeFunc.PCall();
                    this.awakeFunc.EndPCall();
                }

                this.onDestroyFunc = table.GetLuaFunction("OnDestroy") ;
            }
#endif
            
        }

        protected virtual void Start()
        {
            TryAwake();
#if TOLUA
            if (this.table != null)
            {
                this.startFunc = table.GetLuaFunction("Start");
                if (this.startFunc != null)
                {
                    this.startFunc.BeginPCall();
                    this.startFunc.Push(table);
                    this.startFunc.Push(this);
                    this.startFunc.PCall();
                    this.startFunc.EndPCall();
                }
            }
#endif
        }

        protected virtual void OnDestroy()
        {
#if TOLUA
            if (this.awakeFunc != null)
            {
                this.awakeFunc.Dispose();
                this.awakeFunc = null;
            }

            if (this.startFunc != null)
            {
                this.startFunc.Dispose();
                this.startFunc = null;
            }

            if (this.onDestroyFunc != null)
            {
                this.onDestroyFunc.BeginPCall();
                this.onDestroyFunc.PCall();
                this.onDestroyFunc.EndPCall();

                this.onDestroyFunc.Dispose();
                this.onDestroyFunc = null;
            }

            if (this._table != null)
            {
                this._table.Dispose();
                this._table = null;
            }
#endif
        }

    }
}

