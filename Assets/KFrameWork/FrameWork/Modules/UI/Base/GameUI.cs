using UnityEngine;
using System;
using KUtils;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if Advance
using AdvancedInspector;
#endif

#if TOLUA
using LuaInterface;
#endif

namespace KFrameWork
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class GameUI : LuaMonoBehaviour
    {
        private static int uidCounter = 0;
        #region lua
#if Advance
        [Inspect]
#endif
        
#if TOLUA
        private LuaFunction enterFunc;

        private LuaFunction refreshFunc;

        private LuaFunction exitFunc;

        private LuaFunction releaseFunc;

#endif
        #endregion
#if Advance
        [Inspect, ReadOnly]
#endif
        [NonSerialized]
        public Transform BindParent;
#if Advance
        [Inspect, ReadOnly]
#endif
        [NonSerialized]
        private RectTransform _rect;
        public RectTransform rectTransform
        {
            get
            {
                if (_rect == null)
                    _rect = this.GetComponent<RectTransform>();
                return _rect;
            }
        }
        [NonSerialized]
        private int uidepth;
#if Advance
        [Inspect, ReadOnly]
#endif
        public int UIDepth
        {
            get
            {
                return uidepth;
            }
            set
            {
                if (uidepth != value)
                {
                    uidepth = value;
                    this.rectTransform.SetSiblingIndex(uidepth);
                }
            }
        }
        [NonSerialized]
        private int _UID;
        #if Advance
        [Inspect, ReadOnly]
#endif
        public int Uid
        {
            get
            {
                if (_UID == 0)
                    _UID = ++uidCounter;
                return _UID;
            }
        }
#if Advance
        [Inspect, ReadOnly]
#endif
        public bool Visiable
        {
            get
            {
                return this.isActiveAndEnabled && this.transform.localScale != Vector3.zero; //this.rectTransform.IsVisibleFrom(this.m_canvas.worldCamera);
            }
        }
        [NonSerialized]
        private bool enterBefore = false;
#if Advance
        [Inspect, ReadOnly]
#endif
        public bool HasEnter
        {
            get
            {
                return enterBefore;
            }
        }
        [NonSerialized]
        private AbstractParams enterParams;

#if Advance
        [Inspect, ReadOnly]
#endif
        [NonSerialized]
        /// <summary>
        /// 所归属的layout
        /// </summary>
        public AbstractLayout ParentLayout;
        [NonSerialized]
        private Canvas _canvas;

        public Canvas m_canvas
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = this.GetComponentInParent<Canvas>();
                }   
                return _canvas;
            }
        }

        public void OpenUI(AbstractParams p = null)
        {
            if (this.ParentLayout != null)
            {
                this.ParentLayout.OpenUI(this, p);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}",this);
            }
        }

        public GameUI OpenUI(string path, AbstractParams p = null)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUI(path, p);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUI(string respath, LayoutLoadMode mode)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUI(respath, mode);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUI(string respath, AbstractParams p, LayoutLoadMode mode)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUI(respath,p, mode);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUI(string respath, Transform Parent)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUI(respath, Parent);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUI(string respath, Transform Parent, AbstractParams p)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUI(respath, Parent,p);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUI(string respath, Transform Parent, AbstractParams p, LayoutLoadMode mode)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUI(respath, Parent, p, mode);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }

            return null;
        }


        //
        public GameUI OpenUIAsync(string path, AbstractParams p = null)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUIAsync(path, p);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUIAsync(string respath, LayoutLoadMode mode)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUIAsync(respath, mode);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUIAsync(string respath, AbstractParams p, LayoutLoadMode mode)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUIAsync(respath, p, mode);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUIAsync(string respath, Transform Parent)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUIAsync(respath, Parent);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUIAsync(string respath, Transform Parent, AbstractParams p)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUIAsync(respath, Parent, p);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }
            return null;
        }

        public GameUI OpenUIAsync(string respath, Transform Parent, AbstractParams p, LayoutLoadMode mode)
        {
            if (this.ParentLayout != null)
            {
                return this.ParentLayout.OpenUIAsync(respath, Parent, p, mode);
            }
            else
            {
                LogMgr.LogErrorFormat("Missing Layout {0}", this);
            }

            return null;
        }

        protected override void TryAwake()
        {
           // m_bvisible = this.gameObject.activeSelf;
            if (LuaMode)
            {
                base.TryAwake();
            }
        }

        protected override void OnDestroy()
        {
            
            this.enterParams = null;
            this.enterBefore = false;
#if TOLUA

            if (this.enterFunc != null)
            {
                this.enterFunc.Dispose();
                this.enterFunc = null;
            }

            if (this.exitFunc != null)
            {
                this.exitFunc.Dispose();
                this.exitFunc = null;
            }

            if (this.refreshFunc != null)
            {
                this.refreshFunc.Dispose();
                this.refreshFunc = null;
            }

            if (this.releaseFunc != null)
            {
                this.releaseFunc.Dispose();
                this.releaseFunc = null;
            }

#endif

            if (this.ParentLayout != null)
            {
                this.ParentLayout.CloseChildrenFromThis(this);
            }
            this.ParentLayout = null;

            base.OnDestroy();
        }

        private void CallEnter(AbstractParams p)
        {
            if (enterParams != null)
            {
                LogMgr.LogFormat("{0} enter params will refresh :{1} => {2}",this,enterParams,p);
                enterParams.Release();
            }

            enterParams = p;
            if(!HasEnter)
            {
                NextFrame(null);
            }
        }

        /// <summary>
        /// 脱离ugui 的uibehaviour 和graphic 类的canvas rebuild ，直接等待下一帧，确保canvas的初始化完成
        /// </summary>
        void NextFrame(FrameCommand cmd)
        {
            if (this == null)
            {
                CacheCommand.CanCelAllBy(this);
                return;
            }

            this.uidepth = this.rectTransform.GetSiblingIndex();

            if (!HasEnter)
            {
                if (this.ParentLayout != null)
                    this.gameObject.layer = this.ParentLayout.UILayer;

                GameUI[] uiarray = this.GetComponentsInChildren<GameUI>(true);
                for (int i = 0; i < uiarray.Length; ++i)
                {
                    GameUI ui = uiarray[i];
                    if (ui != null && ui.ParentLayout == null)
                    {
                        ui.ParentLayout = this.ParentLayout;
                    }
                }

                if (LuaMode)
                {
#if TOLUA
                    if (table != null)
                    {
                        if (this.enterFunc == null)
                            this.enterFunc = table.GetLuaFunction("OnEnter");

                        if (this.enterFunc != null)
                        {
                            this.enterFunc.BeginPCall();
                            this.enterFunc.Push(table);
                            this.enterFunc.Push(enterParams);
                            this.enterFunc.PCall();
                            this.enterFunc.EndPCall();
                        }
                        else
                        {
                            this.OnEnter(enterParams);
                        }

                    }
                    else {
#if UNITY_EDITOR
                        LogMgr.LogErrorFormat("找不到对应的{0} 的luatable path is :{1}", this, this.respath);
#endif
                    }
#endif

                    //if (enterParams != null)
                    //    enterParams.Release();
                }
                else
                {
                    this.OnEnter(enterParams);
                }

                enterParams = null;
                enterBefore = true;
            }
        }


#region events

        public void CallRefresh(AbstractParams p)
        {
            if (!HasEnter)
            {
                this.CallEnter(p);
                return;
            }

            if (LuaMode)
            {
#if TOLUA
                if (table != null)
                {
                    if (this.refreshFunc == null)
                        this.refreshFunc = table.GetLuaFunction("Refresh");
                    if (refreshFunc != null)
                    {
                        this.refreshFunc.BeginPCall();
                        this.refreshFunc.Push(table);
                        this.refreshFunc.Push(p);
                        this.refreshFunc.PCall();
                        this.refreshFunc.EndPCall();
                    }
                    else
                    {
                        this.Refresh(p);
                    }
                }
#endif
            }
            else
            {
                this.Refresh(p);
            }
        }

        [Script_SharpLogic((int)FrameWorkCmdDefine.UICallExit)]
        private static void staticExit(AbstractParams p)
        {
            if (p != null)
            {
                GameUI ui = p.ReadObject() as GameUI;
                ui.CallExit(p);
            }
            else
            {
                LogMgr.LogError("gameui staticExit params is Null ");
            }
        }

        [Script_SharpLogic((int)FrameWorkCmdDefine.UICallEnter)]
        private static void staticEnter(AbstractParams p)
        {
            if (p != null)
            {
                System.Object o = p.ReadObject() ;
                GameUI ui = o as GameUI;
                if (ui == null)
                {
                    LogMgr.LogError("really??");
                }
                ui.CallEnter(p);
            }
            else
            {
                LogMgr.LogError("gameui staticEnter params is Null ");
            }
        }

        [Script_SharpLogic((int)FrameWorkCmdDefine.UICallRelease)]
        private static void staticRelease(AbstractParams p)
        {
            if (p != null)
            {
                GameUI ui = p.ReadObject() as GameUI;
                ui.CallRelease();
            }
            else
            {
                LogMgr.LogError("gameui staticRelease params is Null ");
            }
        }

        private void CallExit(AbstractParams p)
        {
            this._UID = 0;
            this.enterBefore = false;

            if (LuaMode)
            {
#if TOLUA
                if (table != null)
                {
                    if (this.exitFunc == null)
                        this.exitFunc = table.GetLuaFunction("OnExit");
                    if (exitFunc != null)
                    {
                        this.exitFunc.BeginPCall();
                        this.exitFunc.Push(table);
                        this.exitFunc.Push(p);
                        this.exitFunc.PCall();
                        this.exitFunc.EndPCall();
                    }
                    else
                    {
                        this.OnExit(p);
                    }
                }
#endif
            }
            else
            {
                this.OnExit(p);
            }

            this.DoInVisiable();

        }

        private void CallRelease()
        {
            if (LuaMode)
            {
#if TOLUA
                if (table != null)
                {
                    if(this.releaseFunc == null)
                        this.releaseFunc = table.GetLuaFunction("Release");

                    if (releaseFunc != null)
                    {
                        this.releaseFunc.BeginPCall();
                        this.releaseFunc.Push(table);
                        this.releaseFunc.PCall();
                        this.releaseFunc.EndPCall();
                    }
                    else
                    {
                        Release();
                    }
                }
#endif
            }
            else
            {
                this.Release();
            }
        }

        /// <summary>
        /// enter不能直接调用，必须确保该对象上以及子对象上的元素全部被canvas build结束，否则做的一切transform修改可能在之后被canvas重置
        /// </summary>
        /// <param name="p"></param>
        protected abstract void OnEnter(AbstractParams p);

        protected abstract void Refresh(AbstractParams p);

        protected abstract void Release();

        protected abstract void OnExit(AbstractParams p);
        #endregion

        #region utils
        /// <summary>
        /// 矩形中下位置唯一原来的worldpos处
        /// </summary>
        public void UIStandAtPosition()
        {
            this.rectTransform.anchoredPosition = this.rectTransform.anchoredPosition.UpdateY(this.rectTransform.sizeDelta.y / 2);
        }

        public void CloseSelf()
        {
            if (this.ParentLayout != null)
            {
                this.ParentLayout.CloseUI(this);
            }
            else
            {
                LogMgr.LogErrorFormat("Havent layout :{0}",this);
                this.CallExit(null);
                Destroy(this.gameObject);
            }
        }

        public AbstractParams NotifyToLua(int luaCmd, AbstractParams p)
        {
            ScriptCommand cmd = ScriptCommand.Create(luaCmd);
            cmd.SetCallParams(p);
            cmd.Excute();

            AbstractParams ret = cmd.ReturnParams;
            cmd.Release(false);
            return ret;
        }
#if UNITY_EDITOR

        protected virtual void OnDrawGizmos()
        {
            if (m_canvas != null && this.Visiable) //&& ShowUIBounds)
            {
                Gizmos.color = Color.yellow;
                DrawWire(this.rectTransform);

                UIBehaviour[] uis = this.GetComponentsInChildren<UIBehaviour>();
                for (int i = 0; i < uis.Length; ++i)
                {
                    if (uis[i] != rectTransform)
                    {
                        DrawWire(uis[i].transform as RectTransform);
                    }
                }

            }
        }

        void DrawWire(RectTransform uirecttansform)
        {
            Vector3[] objectCorners = new Vector3[4];
            uirecttansform.GetWorldCorners(objectCorners);

            Gizmos.DrawLine(objectCorners[0], objectCorners[1]);
            Gizmos.DrawLine(objectCorners[1], objectCorners[2]);
            Gizmos.DrawLine(objectCorners[2], objectCorners[3]);
            Gizmos.DrawLine(objectCorners[3], objectCorners[0]);
        }
#endif

        /// <summary>
        /// 执行操作使得UI显示
        /// </summary>
        public void DoVisiable()
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        /// <summary>
        /// 执行操作使得UI隐藏
        /// </summary>
        public void DoInVisiable()
        {
            rectTransform.localScale = Vector3.zero;
        }
#endregion
    }
}


