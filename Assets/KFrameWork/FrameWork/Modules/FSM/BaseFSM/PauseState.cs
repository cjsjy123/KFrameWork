using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using System;

namespace KFrameWork
{
    [Node(false, "KFrame Work Base/Pause")]
    public class PauseState : FSMElement
    {
        public override string GetID
        {
            get
            {
                return "PauseState";
            }
        }

        public override void ResetValues()
        {
            
        }

        protected override void DrawFSMGUI()
        {
           
        }

        protected override void From(FSMElement element)
        {
            
        }

        protected override void OnEnter()
        {
            if (this.OpenBaseLog)
                LogMgr.LogFormat("Pause OnEnter at:{0}", GameSyncCtr.mIns.RenderFrameCount);

            if (FSMCtr.mIns != null)
            {
                FSMCtr.mIns.PauseFSM(this);
            }
            else
            {
                LogMgr.LogError("Missing FSMCtr");
            }
        }

        protected override void OnExit()
        {
            if (this.OpenBaseLog)
                LogMgr.LogFormat("Pause OnExit at:{0}", GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override bool Select(FSMElement element)
        {
            return true;
        }

        protected override bool UpdateFrame(long frameCnt)
        {
            return false;
        }
    }
}


