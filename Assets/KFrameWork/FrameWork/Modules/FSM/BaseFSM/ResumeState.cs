using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using System;

namespace KFrameWork
{
    [Node(false, "KFrame Work Base/Resume")]
    public class ResumeState : FSMElement
    {
        public override string GetID
        {
            get
            {
                return "ResumeState";
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
                LogMgr.LogFormat("Resume OnEnter at:{0}", GameSyncCtr.mIns.RenderFrameCount);

            if (FSMCtr.mIns != null)
            {
                FSMCtr.mIns.ResumeFSM(this);
            }
            else
            {
                LogMgr.LogError("Missing FSMCtr");
            }
        }

        protected override void OnExit()
        {
            if (this.OpenBaseLog)
                LogMgr.LogFormat("Resume OnExit at:{0}", GameSyncCtr.mIns.RenderFrameCount);
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

