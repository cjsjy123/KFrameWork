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
    #if TOLUA
    [ScriptInitOrderAttribute(-8000)]
    public class GameLuaClient : LuaClient
    {
        public bool isDone = false;

        public static GameLuaClient getInstance()
        {
            if (Instance is GameLuaClient)
            {
                return Instance as GameLuaClient;
            }

            return null;
        }

        protected override void LoadLuaFiles()
        {
            if (ResBundleMgr.mIns != null && ResBundleMgr.mIns.isDone)
            {
                base.LoadLuaFiles();
#if UNITY_EDITOR
                if (GameConfig.Open_DEBUG || MainLoop.getInstance().OpenLuaLOG)
                    this.luaState.LogGC = true;
#endif

                isDone = true;
            }
        }

    }
#endif
}


