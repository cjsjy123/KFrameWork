using UnityEngine;
using System;
using KUtils;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if TOLUA
using LuaInterface;
#endif

namespace KFrameWork
{
    public enum LayoutLoadMode
    {
        None,
        /// <summary>
        /// 单个ui,如果有就刷新UI
        /// </summary>
        SingleUI,
        /// <summary>
        /// 忽视缓存，允许多个实例
        /// </summary>
        IgnoreCache,
        /// <summary>
        /// 一旦缓存允许则从缓存加载新的,也会检查是否已经load了一个
        /// </summary>
        LoadFromCacheIfNew,
        /// <summary>
        /// 一旦缓存允许则从缓存加载新的,无视contain
        /// </summary>
        LoadFromCache,
    }

    public abstract class AbstractLayout
    {
        /// <summary>
        /// opened ui container
        /// </summary>
        protected List<GameUI> uicontainer;

        private GameObject layoutObject;

        private GameObject gameuiRoot;

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

        private bool _canDestroy;

        public bool CanDestory
        {
            get
            {
                return _canDestroy;
            }
            set
            {
                _canDestroy = value;
            }
        }

        private List<Tuple<string, Transform, AbstractParams,int>> loaderQueue;

        public AbstractLayout()
        {
            this.uicontainer = new List<GameUI>();
            this.loaderQueue = new List<Tuple<string,Transform, AbstractParams, int>>();

            this._UILayer = LayerMask.NameToLayer("UI");
        }

        public abstract void ShowUILayout();

        public abstract void HideUILayout();

        protected abstract GameUI BindToCanvas(GameObject instance, Transform Parent, AbstractParams p);

        protected abstract bool CanLoadFromCache(string respath);

        protected abstract GameUI LoadFromCache(string respath);

        protected abstract void ChangeOrder(int old, int value);

        public virtual Canvas CreateCanvas(string name)
        {
            if (gameuiRoot == null)
            {
                gameuiRoot = GameObject.Find("GameUIRoot");
            }

            if (gameuiRoot == null)
            {
                gameuiRoot = new GameObject("GameUIRoot");
                gameuiRoot.layer = LayerMask.NameToLayer("UI");
            }

            GameObject canvasObject = new GameObject(name);
            canvasObject.layer = LayerMask.NameToLayer("UI");
            Canvas uicanvas = canvasObject.AddComponent<Canvas>();
            uicanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uicanvas.worldCamera = GameCameraCtr.mIns.cameraUI;
            uicanvas.planeDistance = 100;
            uicanvas.overrideSorting = true;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1;
            scaler.referencePixelsPerUnit = 100;
            scaler.referenceResolution = new Vector2(FrameWorkConfig.DisplayUIWidth, FrameWorkConfig.DisplayUIHiehgt);

            GraphicRaycaster raycaster = canvasObject.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;

            gameuiRoot.AddInstance(canvasObject);

            return uicanvas;
        }

        private bool RemoveLoading(string name, out AbstractParams p,out Transform tr,out int mode)
        {
            for (int i = 0; i < loaderQueue.Count; ++i)
            {
                Tuple<string, Transform, AbstractParams, int> kv = this.loaderQueue[i];
                if (kv.k1 == name)
                {
                    tr = kv.k2;
                    p = kv.k3;
                    mode = kv.k4;
                    this.loaderQueue.RemoveAt(i);
                    return true;
                }
            }
            p = null;
            tr = null;
            mode = -1;
            return false;
        }

        private void CallUI(GameUI ui,AbstractParams p)
        {
            if (ui.HasEnter )
            {
                if (!ui.Visiable)
                    ui.DoVisiable();

                ui.CallRefresh(p);

                if (p != null)
                {
                    p.Release();
                    p = null;
                }
            }
            else
            {
                ui.CallEnter(p);
            }
        }

        /// <summary>
        /// 符合的全部刷新
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="p"></param>
        private List<GameUI> RefreshUI(string respath,Transform trans, AbstractParams p)
        {
            List<GameUI> uilist = TryGetUIByPathParent(respath,trans);

            for (int i = 0; i < uilist.Count; ++i)
            {
                GameUI ui = uilist[i];
                CallUI(ui, p);
            }

            return uilist;
        }
        /// <summary>
        /// 根据UID 刷新
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private GameUI RefreshUI(int UID, AbstractParams p )
        {
            GameUI gameui = TryGetUIByUID(UID);
            if (gameui != null)
            {
                CallUI(gameui, p);
            }

            return gameui;
        }
        /// <summary>
        /// 从缓存中加载
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="Parent"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private GameUI CallCacheUI(string respath, Transform Parent , AbstractParams p )
        {
            GameUI ui = LoadFromCache(respath);
            if (ui != null)
            {
                BindToCanvas(ui.gameObject, Parent, p);
                CallUI(ui, p);
                AddtoContioner(ui, Parent);
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
        private GameUI SyncLoadUI(string respath, Transform Parent , AbstractParams p )
        {
            GameObject instance = ResBundleMgr.mIns.Load(respath).SimpleInstance();
            if (instance == null)
                return null;

            GameUI ui = BindToCanvas(instance, Parent, p);
            if (ui != null)
                ui.loadpath = respath;
            ui.CallEnter(p);
            AddtoContioner(ui, Parent);
            return ui;
        }
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="Parent"></param>
        /// <param name="p"></param>
        private void AsyncLoadUI(string respath, Transform Parent, LayoutLoadMode mode , AbstractParams p )
        {
            this.loaderQueue.Add(new Tuple<string, Transform, AbstractParams, int>(respath,Parent,p,(int)mode));
            if (!ResBundleMgr.mIns.Cache.ContainsLoading(respath))
            {
                ResBundleMgr.mIns.LoadAsync(respath, AsyncDone);
            }
        }

        public GameUI OpenUI(string respath, LayoutLoadMode mode)
        {
            return OpenUI(respath, null, mode,null);
        }

        public GameUI OpenUI(string respath, AbstractParams p)
        {
            return OpenUI(respath, null, this.DefaultMode, p);
        }

        public GameUI OpenUI(string respath, AbstractParams p, LayoutLoadMode mode)
        {
            return OpenUI(respath, null, mode, p);
        }

        public GameUI OpenUI(string respath, Transform Parent, AbstractParams p)
        {
            return OpenUI(respath, Parent, this.DefaultMode, p);
        }

        /// <summary>
        /// 同步打开UI
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="Parent"></param>
        /// <param name="mode"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public GameUI OpenUI(string respath,Transform Parent =null,LayoutLoadMode mode = LayoutLoadMode.None, AbstractParams p = null)
        {
            if (mode == LayoutLoadMode.None)
            {
                mode = this.DefaultMode;
            }

            bool contains = this.ContainsUIByParent(respath, Parent);
            if (mode == LayoutLoadMode.SingleUI)
            {
                if (contains)
                {
                    List<GameUI> uilist = RefreshUI(respath, Parent, p);
                    GameUI ui = uilist[0];
                    ListPool.TryDespawn(uilist);

                    return ui;
                }
                else
                {
                    return SyncLoadUI(respath, Parent, p);
                }
            }
            else if (mode == LayoutLoadMode.IgnoreCache)
            {
                return SyncLoadUI(respath, Parent, p);
            }
            else if (mode == LayoutLoadMode.LoadFromCache)
            {
                if (this.CanLoadFromCache(respath))
                {
                    return this.CallCacheUI(respath, Parent, p);
                }
                else
                {
                    return SyncLoadUI(respath, Parent, p);
                }
            }
            else if (mode == LayoutLoadMode.LoadFromCacheIfNew)
            {
                if (contains)
                {
                    List<GameUI> uilist = RefreshUI(respath, Parent, p);
                    GameUI ui = uilist[0];
                    ListPool.TryDespawn(uilist);

                    return ui;
                }
                else if (this.CanLoadFromCache(respath))
                {
                    return this.CallCacheUI(respath, Parent, p);
                }
                else
                {
                    return SyncLoadUI(respath, Parent, p);
                }
            }

            throw new FrameWorkException("加载模式异常");
        }

        public GameUI OpenUIAsync(string respath, AbstractParams p, LayoutLoadMode mode)
        {
            return OpenUIAsync(respath, null, mode, p);
        }

        public GameUI OpenUIAsync(string respath, LayoutLoadMode mode)
        {
            return OpenUIAsync(respath, null, mode,null);
        }

        public GameUI OpenUIAsync(string respath, AbstractParams p )
        {
            return OpenUIAsync(respath,null,this.DefaultMode,p);
        }

        public GameUI OpenUIAsync(string respath,Transform Parent, AbstractParams p)
        {
            return OpenUIAsync(respath, Parent, this.DefaultMode, p);
        }

        /// <summary>
        /// 异步打开UI 但是返回值并不一定有值
        /// </summary>
        /// <param name="respath"></param>
        /// <param name="Parent"></param>
        /// <param name="mode"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public GameUI OpenUIAsync(string respath, Transform Parent = null, LayoutLoadMode mode = LayoutLoadMode.None, AbstractParams p = null)
        {
            if (mode == LayoutLoadMode.None)
            {
                mode = this.DefaultMode;
            }
            bool contains = this.ContainsUIByParent(respath,Parent);
            if (mode == LayoutLoadMode.SingleUI)
            {
                if (contains)
                {
                    List<GameUI> uilist = RefreshUI(respath, Parent, p);
                    GameUI ui = uilist[0];
                    ListPool.TryDespawn(uilist);

                    return ui;
                }
                else
                {
                    this.AsyncLoadUI(respath, Parent, mode, p);
                    return null;
                }
            }
            else if (mode == LayoutLoadMode.IgnoreCache)
            {
                this.AsyncLoadUI(respath, Parent, mode, p);
                return null;
            }
            else if (mode == LayoutLoadMode.LoadFromCache)
            {
                if (this.CanLoadFromCache(respath))
                {
                    return this.CallCacheUI(respath, Parent, p);
                }
                else
                {
                    return SyncLoadUI(respath, Parent, p);
                }
            }
            else if (mode == LayoutLoadMode.LoadFromCacheIfNew)
            {
                if (contains)
                {
                    List<GameUI> uilist = RefreshUI(respath, Parent, p);
                    GameUI ui = uilist[0];
                    ListPool.TryDespawn(uilist);

                    return ui;
                }
                else if (this.CanLoadFromCache(respath))
                {
                    return this.CallCacheUI(respath, Parent, p);
                }
                else
                {
                    this.AsyncLoadUI(respath, Parent, mode, p);
                    return null;
                }
            }
            throw new FrameWorkException("加载模式异常");
        }

        private void AsyncDone(bool ret, AssetBundleResult result)
        {
            if (ret)
            {
                AbstractParams p = null;
                Transform Parent = null;
                int Mode;
                while (this.RemoveLoading(result.LoadPath, out p,out Parent,out Mode))
                {
                    if ((LayoutLoadMode)Mode != LayoutLoadMode.IgnoreCache && this.ContainsUIByParent(result.LoadPath, Parent))
                    {
                        ListPool.TryDespawn( RefreshUI(result.LoadPath, Parent, p));
                    }
                    else
                    {
                        GameObject instance = result.SimpleInstance();
                        GameUI ui = BindToCanvas(instance, Parent, p);
                        if (ui != null)
                            ui.loadpath = result.LoadPath;
                        ui.CallEnter(p);
                        AddtoContioner(ui, Parent);
                    }
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
                    parentUi.ParentLayout.uicontainer.TryAdd(ui);
                }
                //else ignore
            }
        }

        public virtual void CloseUI(string respath, AbstractParams p = null)
        {
            for (int i = this.uicontainer.Count-1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];
                if (gameui.loadpath.Equals(respath))
                {
                    this.uicontainer.RemoveAt(i);
                    gameui.CallExit(p);

                    if (p != null)
                        p.Release();

                    CacheCommand.CanCelAllBy(gameui);
                    GameObject.Destroy(gameui);
                    return;
                }
            }
        }

        public virtual void CloseUI(GameUI ui, AbstractParams p = null)
        {
            for (int i = this.uicontainer.Count -1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];
                if (gameui == ui)
                {
                    this.uicontainer.RemoveAt(i);
                    break;
                }
            }

            ui.CallExit(p);

            if (p != null)
                p.Release();
            CacheCommand.CanCelAllBy(ui);
            GameObject.Destroy(ui);
        }

        public virtual void Release()
        {
            for (int i = this.uicontainer.Count - 1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];
                gameui.CallRelease();
            }
        }

        public virtual void Clear()
        {
            //also stop loading

            if (this.CanDestory == false)
                return;

            for (int i = this.uicontainer.Count - 1; i >= 0; i--)
            {
                GameUI ui = this.uicontainer[i];
                if (ui != null)
                {
                    CacheCommand.CanCelAllBy(ui);
                    GameObject.Destroy(ui);
                }
            }

            this.uicontainer.Clear();
        }


        public GameUI GetUIByPathWithFirst(string path)
        {
            List<GameUI> list = TryGetUIByPath(path);

            GameUI ui = null;
            if (list.Count > 0)
                ui = list[0];

            ListPool.TryDespawn(list);
            return ui;
        }

        public GameUI TryGetUIByUID(int uid)
        {
            for (int i = 0; i < this.uicontainer.Count; ++i)
            {
                GameUI ui = this.uicontainer[i];
                if (ui.Uid.Equals(uid))
                {
                    return ui;
                }
            }

            return null;
        }

        public List<GameUI> TryGetUIByPathParent(string path,Transform transform)
        {
            List<GameUI> list = ListPool.TrySpawn<List<GameUI>>();

            for (int i = 0; i < this.uicontainer.Count; ++i)
            {
                GameUI ui = this.uicontainer[i];
                bool parentenable = (transform != null && ui.transform.parent == transform) || transform == null;
                if (ui.loadpath.Equals(path) && parentenable)
                {
                    list.Add(ui);
                }
            }
            return list;
        }

        public List<GameUI> TryGetUIByPath(string path)
        {
            List<GameUI> list = ListPool.TrySpawn<List<GameUI>>();

            for (int i = 0; i < this.uicontainer.Count; ++i)
            {
                GameUI ui = this.uicontainer[i];
                if (ui.loadpath.Equals(path))
                {
                    list.Add(ui);
                }
            }
            return list;
        }

        public bool ContainsUI(int uid)
        {
            for (int i = 0; i < this.uicontainer.Count; ++i)
            {
                if (this.uicontainer[i].Uid == uid)
                    return true;
            }

            return false;
        }

        private bool ContainsUIByParent(string respath,Transform tr)
        {
            for (int i = 0; i < this.uicontainer.Count; ++i)
            {
                bool pathenable = this.uicontainer[i].loadpath.Equals(respath);
                if (pathenable && tr == null)
                    return true;
                else if (pathenable && tr != null && this.uicontainer[i].transform.parent.Equals(tr))
                    return true;
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
                if (this.uicontainer[i].loadpath.Equals(respath))
                    return true;
            }

            return false;
        }

        public bool ContainsUI(GameUI ui)
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

        public virtual Vector3 ConvertV3Screen(Vector2 pos)
        {
            return new Vector3(pos.x,pos.y,100f);
        }
    }
}

