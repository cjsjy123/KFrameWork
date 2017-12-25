using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System;

namespace KFrameWork
{
    /// <summary>
    /// it will restart next frame
    /// </summary>
    [Node(false,"KFrame Work Base/ReStart")]
    public class ReStartState : FSMElement
    {
        public override string GetID
        {
            get
            {
                return "ReStartState";
            }
        }

        public override string Title
        {
            get
            {
                return "重启结点";
            }
        }

        public override Vector2 DefaultSize
        {
            get
            {
                return new Vector2(250, 200);
            }
        }

        [SerializeField]
        private bool NeedRestart = false;

        private NodeCanvas startCanvas;

        protected override bool UpdateFrame(long frameCnt)
        {
            if (this.NeedRestart )
            {
                if (this.startCanvas == null)
                {
                    LogMgr.LogError("没有找到初始化所需的canvas");
                }
                else
                {
                    if (FSMCtr.mIns != null)
                        FSMCtr.mIns.ReStart(this);

                    return true;
                }

            }
            return false;
        }

        public override void ResetValues()
        {
            NeedRestart = false;
        }

        protected override void DrawFSMGUI()
        {
            var old = GUI.skin.box.normal.background;
            GUI.skin.box.normal.background = GreenTexture;
            if(GUILayout.Button("Restart"))
            {
                if (FSMCtr.mIns != null)
                    FSMCtr.mIns.ReStart(this);
            }
            GUI.skin.box.normal.background = old;
            //GUI.skin.box.normal.background = old;
        }

        protected override void OnEnter()
        {
            if (this.OpenBaseLog)
                LogMgr.LogFormat("Restart OnEnter at:{0}",GameSyncCtr.mIns.RenderFrameCount);

            
            if (startCanvas == null && FSMCtr.mIns != null)
            {
                FSMCtr.mIns.TryGetCanvas(this, out startCanvas);
            }
        }

        protected override void OnExit()
        {
            if(this.OpenBaseLog)
                LogMgr.LogFormat("Restart OnExit at:{0}", GameSyncCtr.mIns.RenderFrameCount);

            NeedRestart = false;
        }

        protected override bool Select(FSMElement element)
        {
            return true;
        }

        public override void OnCanvasFinished()
        {
            if (FSMCtr.mIns != null && this.InputValue.connections.Count == 0 && nodecanvas != null)
            {
                FSMCtr.mIns.ReStart(this.nodecanvas);
            }
        }

        protected override void From(FSMElement element)
        {

        }
    }

}

