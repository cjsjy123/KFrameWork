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
    [ScriptInitOrder(-8000)]
    public class GameLuaClient : LuaClient
    {
        protected override void LoadLuaFiles()
        {
            if (ResBundleMgr.mIns != null && ResBundleMgr.mIns.isDone && !HasDone)
            {
                base.LoadLuaFiles();
            }
            else
                MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.BeforeUpdate, this.SelfUpdate);
        }

        public override void Destroy()
        {
            base.Destroy();
            MainLoop.getLoop().UnRegisterLoopEvent(MainLoopEvent.BeforeUpdate, this.SelfUpdate);
        }


        void SelfUpdate(int v)
        {
            if (ResBundleMgr.mIns != null && ResBundleMgr.mIns.isDone && !HasDone)
            {
                base.LoadLuaFiles();
            }
        }
    }
#endif
}


