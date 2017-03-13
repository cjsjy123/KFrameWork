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
    public abstract class GameUI : MonoBehaviour
    {
        private static int uidCounter = 0;
        #region lua
#if Advance
        [Inspect]
#endif
        public bool LuaMode = false;
#if Advance
        [Inspect]
#endif
        [SerializeField]
        private string LuaFilePath;

#if TOLUA
        public LuaTable table;

        private LuaFunction awakeFunc;

        private LuaFunction startFunc;

        private LuaFunction enterFunc;

        private LuaFunction refreshFunc;

        private LuaFunction exitFunc;

        private LuaFunction releaseFunc;

        private LuaFunction onDestroyFunc;
#endif
#endregion

        public RectTransform rectTransform;

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

        private bool m_bvisible;
        #if Advance
        [Inspect, ReadOnly]
#endif
        public bool Visiable
        {
            get
            {
                return m_bvisible;
            }
            private set
            {
                m_bvisible = value;
            }
        }
        private bool enterBefore = false;

        public bool HasEnter
        {
            get
            {
                return enterBefore;
            }
        }

        private AbstractParams enterParams;
#if Advance
        [Inspect, ReadOnly]
#endif
        /// <summary>
        /// 资源加载路径
        /// </summary>
        public string loadpath;
        #if Advance
        [Inspect, ReadOnly]
#endif
        /// <summary>
        /// 所归属的layout
        /// </summary>
        public AbstractLayout ParentLayout;

        private bool needToZero = true;
        private bool startBefore = false;

        private Canvas _canvas;
        public Canvas m_canvas
        {
            get
            {
                if (_canvas == null)
                    _canvas = this.GetComponentInParent<Canvas>();
                return _canvas;
            }
        }

        /// <summary>
        /// 如果自行加载非自动管理的资源，需手动管理引用
        /// </summary>
        private List<IBundleRef> managed;

        protected virtual void Awake()
        {
            m_bvisible = this.gameObject.activeSelf;

            GameUI[] uilist = this.GetComponentsInChildren<GameUI>(true);
            for (int i = 0; i < uilist.Length; ++i)
            {
                if(uilist[i] != this)
                    uilist[i].needToZero = false;
            }

#if !TOLUA
            LuaMode =false;
#endif

            if (LuaMode)
            {
#if TOLUA
                if (string.IsNullOrEmpty(LuaFilePath)  )
                {
                    string newpath = this.GetType().Name + ".lua.bytes";
                    BundlePkgInfo pkg = ResBundleMgr.mIns.BundleInformation.SeekInfo(newpath);
                    if (pkg == null)
                    {
                        LogMgr.LogErrorFormat("LuaFilePath is Null :{0}", this);
                        this.LuaMode = false;
                    }
                    else
                        LuaFilePath = pkg.BundleName;
                }
                
                if(LuaMode)
                {
                    object[] os = LuaClient.GetMainState().DoFile(this.LuaFilePath);
                    if (os != null && os.Length > 0)
                    {
                        LuaTable luatable = os[0] as LuaTable;
                        if (luatable != null)
                            this.table = luatable;
                        else
                            LogMgr.LogErrorFormat("Missing Luatable named:{0}",this.LuaFilePath);
                    }
                }

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

                    this.onDestroyFunc = table["OnDestroy"] as LuaFunction;
                }
#endif
            }
        }

        protected virtual void Start()
        {
#if TOLUA
            if (table != null)
            {
                this.startFunc = table["Start"] as LuaFunction;
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
            this.ParentLayout = null;
            this.enterParams = null;
            this.enterBefore = false;
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

            if (this.onDestroyFunc != null)
            {
                this.onDestroyFunc.BeginPCall();
                this.onDestroyFunc.PCall();
                this.onDestroyFunc.EndPCall();

                this.onDestroyFunc.Dispose();
                this.onDestroyFunc = null;
            }

            if (this.table != null)
            {
                this.table.Dispose();
                this.table = null;
            }
#endif

            if (managed != null)
            {
                for (int i = 0; i < managed.Count; ++i)
                {
                    managed[i].Release();
                }

                managed.Clear();
                managed = null;
            }
        }

        public void CallEnter(AbstractParams p)
        {
            if (enterParams != null)
            {
                LogMgr.LogFormat("{0} enter params will refresh :{1}",this,p);
                enterParams.Release();
            }

            enterParams = p;
            if(!HasEnter)
            {
                if (!startBefore)
                    FrameCommand.Create(NextFrame,2).Excute();
                else
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

            this.rectTransform = this.GetComponent<RectTransform>();
            this.uidepth = this.rectTransform.GetSiblingIndex();

            if (!HasEnter)
            {
                if (this.ParentLayout != null)
                    this.gameObject.layer = this.ParentLayout.UILayer;

                if (needToZero)
                    this.transform.localPosition = Vector3.zero;

                if (LuaMode)
                {
#if TOLUA
                    if (table != null)
                    {
                        if (this.enterFunc == null)
                            this.enterFunc = table["OnEnter"] as LuaFunction;

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

                            if (enterParams != null && enterParams.ArgCount > 0)
                            {
                                InitParams(enterParams);
                            }
                        }

                    }
#endif

                    if (enterParams != null)
                        enterParams.Release();
                }
                else
                {
                    this.OnEnter(enterParams);

                    if (enterParams != null && enterParams.ArgCount > 0)
                    {
                        InitParams(enterParams);
                        enterParams.Release();
                    }
                }

                enterParams = null;
                enterBefore = true;
            }

            startBefore = true;
        }

#region events

       

        /// <summary>
        /// only enterParams has value that will called by script
        /// </summary>
        /// <param name="uiParams"></param>
        protected virtual void InitParams(AbstractParams uiParams)
        {

        }

        public virtual void CallRefresh(AbstractParams p)
        {
            if (LuaMode)
            {
#if TOLUA
                if (table != null)
                {
                    if (this.refreshFunc == null)
                        this.refreshFunc = table["Refresh"] as LuaFunction;
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

        public virtual void CallExit(AbstractParams p)
        {
            this.enterBefore = false;
            this._UID = 0;

            if (LuaMode)
            {
#if TOLUA
                if (table != null)
                {
                    if (this.exitFunc == null)
                        this.exitFunc = table["OnExit"] as LuaFunction;
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
        }

        public virtual void CallRelease()
        {
            if (LuaMode)
            {
#if TOLUA
                if (table != null)
                {
                    if(this.releaseFunc == null)
                        this.releaseFunc = table["Release"] as LuaFunction;

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

        //public ImageExpand CreatePanel(string imagename)
        //{
        //    GameObject bg = new GameObject("Background");
        //    this.transform.parent.AddInstance(bg);
        //    ImageExpand image = bg.AddComponent<ImageExpand>();
        //    image.type = Image.Type.Sliced;
        //    image.fillCenter = true;
        //    image.color = Color.white;
        //    image.raycastTarget = false;
        //    SpriteAtlasMgr.mIns.ChangeSprite(image, imagename);

        //    RectTransform imageRect = bg.TryAddComponent<RectTransform>();
        //    imageRect.anchorMin = Vector2.zero;
        //    imageRect.anchorMax = Vector2.one;
        //    imageRect.anchoredPosition = Vector2.zero;
        //    imageRect.sizeDelta = Vector2.zero;

        //    int targetdepth = Mathf.Max(0, this.UIDepth - 1);
        //    imageRect.SetSiblingIndex(targetdepth);

        //    return image;
        //}

        //public TextExpand CreateCenterText(string textname, Vector2 size, int fontsize)
        //{
        //    GameObject textGameobject = new GameObject("CenterText");
        //    this.transform.AddInstance(textGameobject);

        //    TextExpand text = textGameobject.AddComponent<TextExpand>();

        //    text.font = ResBundleMgr.mIns.LoadAsset(GameConfig.mIns.Font70) as Font;
        //    text.text = textname;
        //    text.fontSize = fontsize;
        //    text.alignment = TextAnchor.MiddleCenter;

        //    IBundleRef bundle = ResBundleMgr.mIns.Cache.TryGetValue(GameConfig.mIns.Font70);
        //    if (bundle != null)
        //    {
        //        if (managed == null)
        //            managed = new List<IBundleRef>();

        //        bundle.Retain();
        //        managed.Add(bundle);
        //    }

        //    RectTransform textRect = text.TryAddComponent<RectTransform>();
        //    textRect.sizeDelta = size;
        //    textRect.AutoAlign();

        //    return text;
        //}


        public AbstractParams NotifyToLua(int luaCmd, AbstractParams p)
        {
            ScriptCommand cmd = ScriptCommand.Create(luaCmd);
            cmd.SetCallParams(p);
            cmd.Excute();

            AbstractParams ret = cmd.ReturnParams;
            cmd.Release(false);
            return ret;
        }

        /// <summary>
        /// 执行操作使得UI显示
        /// </summary>
        public void DoVisiable()
        {
            if (!Visiable)
            {
                //this.OnBecomeVisable();
                Visiable = true;
                if (this.isActiveAndEnabled)
                {
                    this.transform.localPosition = Vector3.zero;
                }
                else
                {
                    LogMgr.LogWarningFormat("{0} use Active Property", this);
                    this.gameObject.SetActive(true);
                }
            }

        }
        /// <summary>
        /// 执行操作使得UI隐藏
        /// </summary>
        public void DoInVisiable()
        {
            if (Visiable)
            {
                //this.OnBecomeInVisable();
                Visiable = false;
                if (GameCameraCtr.mIns != null)
                {
                    Transform canvasTransform = m_canvas.worldCamera.transform;
                    this.transform.position = canvasTransform.position - canvasTransform.forward *1000;
                }
                else
                {
                    LogMgr.LogWarningFormat("{0} use Active Property", this);
                    this.gameObject.SetActive(false);
                }
            }
        }
#endregion
    }
}


