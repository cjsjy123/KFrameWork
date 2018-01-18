using UnityEngine;
using System;
using KUtils;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Text;

#if TOLUA
using LuaInterface;
#endif

namespace KFrameWork
{
    public enum LayoutLoadMode
    {
        None,
        /// <summary>
        /// 刷新检查
        /// </summary>
        ContainCheck = 1,
        /// <summary>
        /// 缓存检查
        /// </summary>
        CacheCheck = 2,
        /// <summary>
        /// 忽视缓存，允许多个实例
        /// </summary>
        LoadDirect = 4,
        /// <summary>
        /// 单个ui,如果有就刷新UI
        /// </summary>
        SingleUI  = ContainCheck | LoadDirect,

        /// <summary>
        /// 一旦缓存允许则从缓存加载新的,也会检查是否已经load了一个,然后进行刷新
        /// </summary>
        FullLoad = ContainCheck | CacheCheck  | LoadDirect,
        /// <summary>
        /// 一旦缓存允许则从缓存加载新的,无视contain
        /// </summary>
        LoadNewIfCache = CacheCheck | LoadDirect,
    }

    public abstract class AbstractLayout
    {
        /// <summary>
        /// opened ui container
        /// </summary>
        protected List<GameUI> uicontainer;

        protected GameObject gameuiRoot;

        public virtual int Size
        {
            get
            {
                if (this.uicontainer != null)
                    return this.uicontainer.Count;
                return 0;
            }
        }

        public virtual string LayoutName
        {
            get
            {
                return "EmptyLayout";
            }
        }
        /// <summary>
        /// 默认的ui加载模式
        /// </summary>
        public virtual LayoutLoadMode DefaultMode
        {
            get
            {
                return LayoutLoadMode.SingleUI;
            }
        }

        protected string _sotringLayer;
        public virtual string sortingLayer
        {
            get
            {
                if (string.IsNullOrEmpty(_sotringLayer))
                {
                    bool exist = false;
                    for (int i = 0; i < SortingLayer.layers.Length; ++i)
                    {
                        if (SortingLayer.layers[i].name.Equals("GameUI"))
                        {
                            exist = true;
                            break;
                        }
                    }

                    if (exist)
                    {
                        this._sotringLayer = "GameUI";
                    }
                    else
                    {
                        this._sotringLayer = "Default";
                    }
                }

                return _sotringLayer;
            }
        }

        protected int _UILayer;
        public virtual int UILayer
        {
            get
            {
                return _UILayer;
            }
        }

        private int renderOrder;
        public int Order
        {
            get
            {
                return this.renderOrder;
            }
            set
            {
                if (this.renderOrder != value)
                {
                    ChangeOrder(this.renderOrder, value);
                    this.renderOrder = value;
                }
            }
        }


        private List<Tuple<string, Transform, AbstractParams>> loaderQueue;

        private List<GameUI> removeList;

        private bool _enable3D = false;

        public bool enable3D
        {
            get
            {
                return _enable3D;
            }
            set
            {
                if (_enable3D != value)
                {
                    _enable3D = value;
                    this.UpdateForPropertys();
                }
            }
        }

        private bool _enable2D = false;

        public bool enable2D
        {
            get
            {
                return _enable2D;
            }
            set
            {
                if (_enable2D != value)
                {
                    _enable2D = value;
                    this.UpdateForPropertys();
                }
            }
        }

        private class UISceneAliveListener:MonoBehaviour
        {
            void OnDestroy()
            {
#if UNITY_EDITOR
                GameUIControl.mIns.Dump();
#endif
                GameUIControl.mIns.ClearUI(true);
            }
        }

        public AbstractLayout()
        {
            this.uicontainer = new List<GameUI>();
            this.loaderQueue = new List<Tuple<string, Transform, AbstractParams>>();
            this.removeList = new List<GameUI>();

            this._UILayer = LayerMask.NameToLayer("UI");
        }

        public abstract void ShowUILayout();

        public abstract void HideUILayout();

        public abstract bool isShow();

        public abstract void AskCanvas();

        protected abstract GameUI BindToCanvas(GameObject instance, Transform Parent, AbstractParams p);

        protected abstract bool CanLoadFromCache(string respath);

        protected abstract void DestroyUI(GameUI ui);

        protected abstract GameUI LoadFromCache(string respath);

        protected abstract void ChangeOrder(int old, int value);

        protected abstract void UpdateForPropertys();

        protected virtual Camera getUICamera()
        {
            return Camera.main;
        }

        protected virtual Canvas CreateCanvas(string name)
        {
            if (gameuiRoot == null)
            {
                gameuiRoot = GameObject.Find("GameUIRoot");
            }

            if (gameuiRoot == null)
            {
                gameuiRoot = new GameObject("GameUIRoot");
                gameuiRoot.layer = LayerMask.NameToLayer("UI");
                gameuiRoot.AddComponent<UISceneAliveListener>();
            }

            //set gameobject
            GameObject canvasObject = new GameObject(name);
            canvasObject.layer = UILayer;
            //set canvas
            Canvas uicanvas = canvasObject.AddComponent<Canvas>();
            uicanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uicanvas.worldCamera = getUICamera();
            uicanvas.planeDistance = 100;
            uicanvas.overrideSorting = true;
            uicanvas.sortingLayerName = this.sortingLayer;

            //set scaler
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1;
            scaler.referencePixelsPerUnit = 100;
            scaler.referenceResolution = new Vector2(FrameWorkConfig.DisplayUIWidth, FrameWorkConfig.DisplayUIHiehgt);
            //set raycaster
            GraphicRaycaster raycaster = canvasObject.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;

            gameuiRoot.AddInstance(canvasObject);

            return uicanvas;
        }

        private bool RemoveLoading(string name, out AbstractParams p, out Transform tr)
        {
            for (int i = 0; i < loaderQueue.Count; ++i)
            {
                Tuple<string, Transform, AbstractParams> kv = this.loaderQueue[i];
                if (kv.k1 == name)
                {
                    tr = kv.k2;
                    p = kv.k3;
                    this.loaderQueue.RemoveAt(i);
                    return true;
                }
            }
            p = null;
            tr = null;
            return false;
        }

        private void CallUI(GameUI ui, AbstractParams p)
        {
            if (ui.HasEnter)
            {
                ui.CallRefresh(p);
            }
            else
            {
                ScriptCommand cmd = ScriptCommand.Create((int)FrameWorkCmdDefine.UICallEnter);
                if (p != null)
                    cmd.CallParams = p;
                cmd.CallParams.InsertObject(0, ui);
                cmd.target = ScriptTarget.Sharp;
                cmd.Excute();
            }
        }

        private bool HasMode(LayoutLoadMode mode,LayoutLoadMode comparemode)
        {
            int left = (int)mode;
            int right = (int)comparemode;
            if ((left & right) == right)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 符合的全部刷新
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="p"></param>
        private List<GameUI> RefreshUI(string respath, Transform trans, AbstractParams p)
        {
            this.ComparedValue = respath;
            this.ComparedTrans = trans;
            List<GameUI> uilist = GetUI(PathTransCompareUI);

            for (int i = 0; i < uilist.Count; ++i)
            {
                GameUI ui = uilist[i];
                CallUI(ui, p);
            }

            return uilist;
        }

        /// <summary>
        /// 从缓存中加载
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="Parent"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private GameUI CallCacheUI(string respath, Transform Parent, AbstractParams p)
        {
            GameUI ui = LoadFromCache(respath);
            if (ui != null)
            {
                BindToCanvas(ui.gameObject, Parent, p);
                AddtoContioner(ui, Parent);

                ui.DoVisiable();
                CallUI(ui, p);
            }
            return ui;
        }
        /// <summary>
        /// 同步加载
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="Parent"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private GameUI SyncLoadUI(string respath, Transform Parent, AbstractParams p)
        {
            GameObject instance = ResBundleMgr.mIns.Load(respath).SimpleInstance();
            if (instance == null)
                return null;

            GameUI ui = BindToCanvas(instance, Parent, p);
            if (ui != null)
                ui.InitLua(respath);
            AddtoContioner(ui, Parent);
            CallUI(ui, p);
            return ui;
        }
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="Parent"></param>
        /// <param name="p"></param>
        private void AsyncLoadUI(string respath, Transform Parent,  AbstractParams p)
        {
            this.loaderQueue.Add(new Tuple<string, Transform, AbstractParams>(respath, Parent, p));
            if (!ResBundleMgr.mIns.Cache.ContainsLoading(respath))
            {
                ResBundleMgr.mIns.LoadAsync(respath, AsyncDone);
            }
        }

        public virtual GameUI OpenUI(string respath)
        {
            return OpenUI(respath, null, null, this.DefaultMode);
        }

        public virtual GameUI OpenUI(string respath, LayoutLoadMode mode)
        {
            return OpenUI(respath, null, null, mode);
        }

        public virtual GameUI OpenUI(string respath, AbstractParams p)
        {
            return OpenUI(respath, null, p, this.DefaultMode);
        }

        public virtual GameUI OpenUI(string respath, AbstractParams p, LayoutLoadMode mode)
        {
            return OpenUI(respath, null, p, mode);
        }

        public virtual GameUI OpenUI(string respath, Transform Parent)
        {
            return OpenUI(respath, Parent, null, this.DefaultMode);
        }

        public virtual GameUI OpenUI(string respath, Transform Parent, AbstractParams p)
        {
            return OpenUI(respath, Parent, p, this.DefaultMode);
        }

        public virtual GameUI OpenUI(GameUI ui, AbstractParams p = null)
        {
            ui.InitLua(ui.TypeName);
            CallUI(ui, p);
            return ui;
        }
        /// <summary>
        /// 同步打开UI
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="Parent"></param>
        /// <param name="mode"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual GameUI OpenUI(string respath, Transform Parent, AbstractParams p, LayoutLoadMode mode)
        {
            if (mode == LayoutLoadMode.None)
            {
                mode = this.DefaultMode;
            }
            GameUI ui = null;
            if (this.HasMode(mode, LayoutLoadMode.ContainCheck) && this.ContainsLoad(respath,Parent,p,out ui))
            {
                return ui;
            }

            if (this.HasMode(mode, LayoutLoadMode.CacheCheck) && this.CanLoadFromCache(respath))
            {
                ui = CallCacheUI(respath,Parent,p);
                return ui;
            }

            if (this.HasMode(mode, LayoutLoadMode.LoadDirect))
            {
                return SyncLoadUI(respath,Parent,p);
            }

            LogMgr.LogErrorFormat("{0} mode error",respath);
            return null;
        }

        public virtual GameUI OpenUIAsync(string respath)
        {
            return OpenUIAsync(respath, null, null, DefaultMode);
        }

        public virtual GameUI OpenUIAsync(string respath, Transform Parent, AbstractParams p)
        {
            return OpenUIAsync(respath, Parent, p, DefaultMode);
        }

        public virtual GameUI OpenUIAsync(string respath, AbstractParams p, LayoutLoadMode mode)
        {
            return OpenUIAsync(respath, null, p, mode);
        }

        public virtual GameUI OpenUIAsync(string respath, LayoutLoadMode mode)
        {
            return OpenUIAsync(respath, null, null, mode);
        }

        public virtual GameUI OpenUIAsync(string respath, AbstractParams p)
        {
            return OpenUIAsync(respath, null, p, this.DefaultMode);
        }

        public virtual GameUI OpenUIAsync(string respath, Transform Parent)
        {
            return OpenUIAsync(respath, Parent, null, this.DefaultMode);
        }


        /// <summary>
        /// 异步打开UI 但是返回值并不一定有值
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="Parent"></param>
        /// <param name="mode"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual GameUI OpenUIAsync(string respath, Transform Parent, AbstractParams p, LayoutLoadMode mode)
        {
            if (mode == LayoutLoadMode.None)
            {
                mode = this.DefaultMode;
            }

            GameUI ui = null;
            if (this.HasMode(mode, LayoutLoadMode.ContainCheck) && this.ContainsLoad(respath, Parent, p, out ui))
            {
                return ui;
            }

            if (this.HasMode(mode, LayoutLoadMode.CacheCheck) && this.CanLoadFromCache(respath))
            {
                ui = CallCacheUI(respath, Parent, p);
                return ui;
            }

            if (this.HasMode(mode, LayoutLoadMode.LoadDirect))
            {
                AsyncLoadUI(respath, Parent, p);
                return null;
            }
            LogMgr.LogErrorFormat("{0} mode error", respath);
            return null;
        }

        private bool ContainsLoad(string respath, Transform Parent, AbstractParams p,out GameUI gameui)
        {
            List<GameUI> uilist = RefreshUI(respath, Parent, p);
            if (uilist.Count > 0)
            {
                if (uilist.Count > 1)
                {
                    LogMgr.LogWarningFormat("{0} contains multi ui elements at the {1}", respath, Parent);
                }

                gameui = uilist[0];
                ListPool.TryDespawn(uilist);

                return true;
            }
            gameui = null;
            return false;
        }

        
        private void AsyncDone(bool ret, AssetBundleResult result)
        {
            if (ret)
            {
                AbstractParams p = null;
                Transform Parent = null;
                while (this.RemoveLoading(result.LoadPath, out p,out Parent))
                {
                    GameObject instance = result.SimpleInstance();
                    GameUI ui = BindToCanvas(instance, Parent, p);

                    if (ui != null)
                        ui.InitLua(result.LoadPath);

                    AddtoContioner(ui, Parent);
                    CallUI(ui, p);
                }
            }
        }

        private void AddtoContioner(GameUI ui, Transform Parent)
        {
            //if Parent isnt null ,显式的指定对象的可能parent在其他layout下面，不应该受到uicontioner的控制
            if (Parent == null)
                this.uicontainer.Add(ui);
            else
            {
                GameUI parentUi = Parent.GetComponent<GameUI>();
                if (parentUi != null && parentUi.ParentLayout != null)
                {
                    parentUi.ParentLayout.uicontainer.Add(ui);
                }
                else
                {
                    this.uicontainer.Add(ui);
                }
                //else ignore
            }
        }

        public void CloseChildrenFromThis(GameUI ui)
        {
            for (int i = this.uicontainer.Count - 1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];

                if (gameui != null && gameui.BindParent != null && gameui.BindParent == ui)
                {
                    CloseAtIndex(gameui, i);
                }
            }

            RealRemove();
        }

        /// <summary>
        /// 删除第一个符合条件的
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="p"></param>
        public virtual void CloseUI(string respath,Transform Parent,  AbstractParams p = null)
        {
            for (int i = this.uicontainer.Count - 1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];
                ///可能会出现都在最外面，parent is null，只是删除了第一个对象，所以最好持有对象然后删除
                if (CompareUI(gameui, respath, Parent))
                {
                    CloseAtIndex(gameui, i,false, p);
                    break;
                }
            }

            RealRemove();
        }
        /// <summary>
        /// 删除第一个符合条件的
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="p"></param>
        public virtual void CloseUI(string respath, AbstractParams p = null)
        {
            for (int i = this.uicontainer.Count-1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];
                if (CompareUI(gameui,respath))
                {
                    CloseAtIndex(gameui, i,false, p);
                    break;
                }
            }
            RealRemove();
        }

        public void CloseUI(GameObject ui, AbstractParams p = null)
        {
            GameUI gameui = ui.GetComponent<GameUI>();
            if (gameui != null)
            {
                CloseUI(gameui,p);
            }
            RealRemove();
        }

        public void CloseUI(Transform ui, AbstractParams p = null)
        {
            GameUI gameui = ui.GetComponent<GameUI>();
            if (gameui != null)
            {
                CloseUI(gameui, p);
            }
            RealRemove();
        }

        public virtual void CloseUI(GameUI ui, AbstractParams p = null)
        {
            for (int i = this.uicontainer.Count -1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];
                if (gameui == ui)
                {
                    CloseAtIndex(gameui, i, false, p);
                    break;
                }
            }
            RealRemove();
        }

        public virtual void Release()
        {
            for (int i = this.uicontainer.Count - 1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];
                ScriptCommand cmd= ScriptCommand.Create((int)FrameWorkCmdDefine.UICallRelease,1);
                cmd.target = ScriptTarget.Sharp;
                cmd.CallParams.WriteObject(gameui);
                cmd.Excute();
            }
        }

        public virtual void Clear(bool all)
        {
            for (int i = this.uicontainer.Count - 1; i >= 0; i--)
            {
                GameUI ui = this.uicontainer[i];
                if (ui != null)
                {
                    if (!all)
                    {
                        CacheCommand.CanCelAllBy(ui);
                    }
                    else
                    {
                        this.CloseAtIndex(ui, i,all);
                    }
                }
            }

            this.removeList.Clear();
            this.uicontainer.Clear();
        }


        public virtual List<GameUI> GetUI(Func<GameUI,bool> func)
        {
            List<GameUI> list = ListPool.TrySpawn<List<GameUI>>();

            for (int i = 0; i < this.uicontainer.Count; ++i)
            {
                GameUI ui = this.uicontainer[i];
                if (func(ui))
                {
                    list.Add(ui);
                }
            }
            return list;
        }

        protected virtual bool ContainsUIByParent(string respath, Transform tr)
        {
            for (int i = this.uicontainer.Count -1; i >= 0; --i)
            {
                if (uicontainer[i] == null)
                {
                    uicontainer.RemoveAt(i);
                    continue;
                }

                bool pathenable = UIByParentEnable(uicontainer[i], respath, tr);
                if (pathenable)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 忽视父对象不同
        /// </summary>
        /// <param name="respath"></param>
        /// <returns></returns>
        public bool ContainsUI(string respath)
        {
            for (int i = 0; i < this.uicontainer.Count; ++i)
            {
                bool pathenable = UIByParentEnable(uicontainer[i], respath, null);
                if (pathenable)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool ContainsUI(GameUI ui)
        {
            for (int i = 0; i < this.uicontainer.Count; ++i)
            {
                if (this.uicontainer[i] == ui)
                {
                    return true;
                }
            }
            return false;
        }

        #region utility


        /// <summary>
        /// for compare
        /// </summary>
        private string ComparedValue;

        private Transform ComparedTrans;

        private bool PathCompareUI(GameUI gameui)
        {
            return gameui != null && !string.IsNullOrEmpty(ComparedValue) && gameui.respath != null && gameui.respath.Equals(ComparedValue);
        }

        private bool PathTransCompareUI(GameUI gameui)
        {
            return gameui != null && gameui.respath != null && gameui.respath.Equals(ComparedValue)  && (gameui.BindParent == null || (gameui.BindParent != null && gameui.BindParent == ComparedTrans));
        }

        public GameUI GetUIByPathWithFirst(string respath)
        {
            ComparedValue = respath;
            List<GameUI> result=GetUI(PathCompareUI);
            if (result.Count > 0)
            {
                GameUI ui = result[0];
                ListPool.TryDespawn(result);
                return ui;
            }
            ListPool.TryDespawn(result);
            return null;
        }

        public List<GameUI> GetUI(string respath)
        {
            ComparedValue = respath;
            return GetUI(PathCompareUI);
        }

        protected bool CompareUI(GameUI ui, string respath)
        {
            return ui != null && ui.respath != null  && ui.respath.Equals(respath);
        }

        protected bool CompareUI(GameUI ui, string respath, Transform trans)
        {
            return ui != null && ui.respath != null && ui.respath.Equals(respath) && (ui.BindParent == null || trans == null || (ui.BindParent != null && ui.BindParent == trans));
        }

        protected bool UIByParentEnable(GameUI ui, string respath, Transform tr)
        {
            bool pathenable = ui.respath.Equals(respath);
            if (pathenable && tr == null)
                return true;
            else if (pathenable && tr != null && ui.BindParent.Equals(tr))
                return true;

            return false;
        }

        protected void CloseAtIndex(GameUI gameui, int index,bool force =false , AbstractParams p = null)
        {
            if (!removeList.Contains(gameui) )
            {
                //check children
                GameUI[] childrenUI = gameui.GetComponentsInChildren<GameUI>();
                for (int i = 0; i < childrenUI.Length; ++i)
                {
                    if (childrenUI[i] != gameui)
                    {
                        for (int j = 0; j < uicontainer.Count; ++j)
                        {
                            if (uicontainer[j] == childrenUI[i])
                            {
                                CloseAtIndex(uicontainer[j], j, force);
                            }
                        }
                    }
                }

                ScriptCommand cmd = ScriptCommand.Create((int)FrameWorkCmdDefine.UICallExit);
                if (p != null)
                    cmd.CallParams = p;
                cmd.target = ScriptTarget.Sharp;
                cmd.CallParams.InsertObject(0, gameui);
                cmd.Excute();

                if (p != null)
                    p.Release();

                CacheCommand.CanCelAllBy(gameui);

                if(force)
                {
                    gameui.DestorySelf();
                }
                else
                {
                    DestroyUI(gameui); 
                }

                removeList.Add(gameui);
            }
        }

        protected void RealRemove()
        {
            //int cnt = 0;
            for (int i = 0; i < this.removeList.Count; ++i)
            {
                GameUI ui = removeList[i];
                for (int j = uicontainer.Count - 1; j >= 0; j--)
                {
                    GameUI gameui = uicontainer[j];
                    if (ui == gameui)
                    {
                        uicontainer.RemoveAt(j);
                    }
                }
            }

            removeList.Clear();
        }

        bool isNull(GameUI ui)
        {
            return ui == null;
        }

        public virtual void Dump()
        {
#if UNITY_EDITOR
            uicontainer.RemoveAll(isNull);
            if (this.uicontainer.Count > 0)
            {
                StringBuilder stringbuilder = new StringBuilder(256);
                stringbuilder.AppendFormat("------------------- {0} Start -------------------\n", this.LayoutName);
                
                for (int i = this.uicontainer.Count -1; i >= 0; --i)
                {
                    GameUI gameui = this.uicontainer[i];
                    if (gameui != null)
                    {
                        stringbuilder.AppendFormat("Gameui Name:[{0}] UID:[{1}] Visiable:[{2}] Layout:[{3}] BindParent:[{4}] LuaPath:[{5}]\n\n", gameui, gameui.Uid, gameui.Visiable, gameui.ParentLayout, gameui.BindParent, gameui.respath);
                    }
                    else
                    {
                        uicontainer.RemoveAt(i);
                    }
                }

                stringbuilder.AppendFormat("---------------------- UIContainer Cnt:{0} --------------------\n\n", this.uicontainer.Count);
                stringbuilder.AppendFormat("------------------- {0} End -------------------\n", this.LayoutName);
                LogMgr.Log(stringbuilder.ToString());
            }
#endif

        }

        #endregion
    }
}

