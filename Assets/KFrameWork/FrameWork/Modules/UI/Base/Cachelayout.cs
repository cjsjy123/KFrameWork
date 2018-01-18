using UnityEngine;
using System;
using KUtils;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace KFrameWork
{
    public abstract class DontDestoryLayout : BaseLayout
    {

        protected Canvas canvas;

        protected override void ChangeOrder(int old, int value)
        {
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = value;
            }
        }

        public override bool isShow()
        {
            if (canvas == null)
                return false;
            return canvas.isActiveAndEnabled;
        }

        public override void AskCanvas()
        {
            if (canvas == null)
            {
                canvas = CreateCanvas(LayoutName);
                CanvasCreated(canvas);

                canvas.overrideSorting = true;
                canvas.sortingOrder = this.Order;
                canvas.pixelPerfect = true;
            }
        }

        protected override Canvas CreateCanvas(string name)
        {
            if (gameuiRoot == null)
            {
                gameuiRoot = GameObject.Find("GlobalUIROOT");
            }

            if (gameuiRoot == null)
            {
                gameuiRoot = new GameObject("GlobalUIROOT");
                gameuiRoot.layer = LayerMask.NameToLayer("UI");
                GameObject.DontDestroyOnLoad(gameuiRoot);
            }
            //set gameobject
            GameObject canvasObject = new GameObject(name);
            canvasObject.layer = LayerMask.NameToLayer("UI");
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

        protected override GameUI BindToCanvas(GameObject instance, Transform Parent, AbstractParams p)
        {
            AskCanvas();

            if (instance != null)
            {
                if (!this.isShow())
                {
                    this.ShowUILayout();
                }

                if (Parent == null)
                {
                    this.canvas.AddInstance(instance, false);
                }
                else
                    Parent.AddInstance(instance);
                //set ui
                GameUI ui = instance.GetComponentInChildren<GameUI>();
                if (ui == null)
                    LogMgr.LogErrorFormat("{0} is Null ", instance);
                else
                {
                    ui.ParentLayout = this;
                    ui.BindParent = Parent;
                }
                return ui;
            }

            return null;
        }

        protected abstract void CanvasCreated(Canvas canvas);

        public override void ShowUILayout()
        {
            canvas.gameObject.SetActive(true);
        }

        public override void HideUILayout()
        {
            canvas.gameObject.SetActive(false);
        }

        protected override bool CanLoadFromCache(string respath)
        {
            return false;
        }

        protected override GameUI LoadFromCache(string respath)
        {
            return null;
        }

        protected override void UpdateForPropertys()
        {
            GraphicRaycaster.BlockingObjects blocking = GraphicRaycaster.BlockingObjects.None;
            if (this.enable2D && this.enable3D)
            {
                blocking = GraphicRaycaster.BlockingObjects.All;
            }
            else if (this.enable3D && !this.enable2D)
            {
                blocking = GraphicRaycaster.BlockingObjects.ThreeD;
            }
            else if (this.enable2D && !this.enable3D)
            {
                blocking = GraphicRaycaster.BlockingObjects.TwoD;
            }

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            raycaster.blockingObjects = blocking;
        }
    }

    /// <summary>
    /// 集中在一个canvas下面
    /// </summary>
    public abstract class CanvasCacheLayout : Cachelayout
    {
        public override LayoutLoadMode DefaultMode
        {
            get
            {
                return LayoutLoadMode.FullLoad;
            }
        }

        protected Canvas canvas;

        protected override void ChangeOrder(int old, int value)
        {
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = value;
            }
        }

        public override bool isShow()
        {
            if (canvas == null)
                return false;

            return canvas.isActiveAndEnabled;
        }

        public override void AskCanvas()
        {
            if (canvas == null)
            {
                canvas = CreateCanvas(LayoutName);
                CanvasCreated(canvas);

                canvas.overrideSorting = true;
                canvas.sortingOrder = this.Order;
                canvas.pixelPerfect = true;

                UpdateForPropertys();
            }
        }

        protected override void UpdateForPropertys()
        {
            GraphicRaycaster.BlockingObjects blocking = GraphicRaycaster.BlockingObjects.None;
            if (this.enable2D && this.enable3D)
            {
                blocking = GraphicRaycaster.BlockingObjects.All;
            }
            else if (this.enable3D && !this.enable2D)
            {
                blocking = GraphicRaycaster.BlockingObjects.ThreeD;
            }
            else if (this.enable2D && !this.enable3D)
            {
                blocking = GraphicRaycaster.BlockingObjects.TwoD;
            }

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            raycaster.blockingObjects = blocking;
        }

        protected override GameUI BindToCanvas(GameObject instance, Transform Parent, AbstractParams p)
        {
            AskCanvas();

            if (instance != null)
            {
                if (!this.isShow())
                {
                    this.ShowUILayout();
                }

                if (Parent == null)
                {
                    this.canvas.AddInstance(instance, false);
                }
                else
                    Parent.AddInstance(instance);
                //set ui
                GameUI ui = instance.GetComponentInChildren<GameUI>();
                if (ui == null)
                    LogMgr.LogErrorFormat("{0} is Null ", instance);
                else
                {
                    ui.ParentLayout = this;
                    ui.BindParent = Parent;
                }
                return ui;
            }

            return null;
        }

        protected abstract void CanvasCreated(Canvas canvas);

        public override void ShowUILayout()
        {
            canvas.gameObject.SetActive(true);
        }

        public override void HideUILayout()
        {
            canvas.gameObject.SetActive(false);
        }

    }
    /// <summary>
    /// 集中在一个canvas下面
    /// </summary>
    public abstract class Cachelayout : BaseLayout
    {
        public override LayoutLoadMode DefaultMode
        {
            get
            {
                return LayoutLoadMode.SingleUI;
            }
        }

        private static GameObject CachePool;

        protected List<WeakReference> uicaches = new List<WeakReference>();

        protected override void DestroyUI(GameUI ui)
        {
            TryAddToCache(ui);
        }

        protected override bool CanLoadFromCache(string respath)
        {
            for (int i = uicaches.Count -1; i>=0; --i)
            {
                WeakReference weak = uicaches[i];
                if (weak != null && weak.IsAlive)
                {
                    GameUI weakui = weak.Target as GameUI;
                    if (weakui.respath != null && weakui.respath.Equals(respath))
                        return true;
                }
                else
                {
                    uicaches.RemoveAt(i);
                }
            }

            return false;
        }

        protected override GameUI LoadFromCache(string respath)
        {
            for (int i = uicaches.Count - 1; i >= 0; --i)
            {
                WeakReference weak = uicaches[i];
                if (weak != null && weak.IsAlive)
                {
                    GameUI weakui = weak.Target as GameUI;
                    if (weakui.respath != null &&  weakui.respath.Equals(respath))
                    {
                        if (FrameWorkConfig.Open_DEBUG)
                            LogMgr.LogFormat("{0} pop from uipool", weakui);
                        uicaches.RemoveAt(i);
                        return weakui;
                    }
                }
                else
                {
                    uicaches.RemoveAt(i);
                }
            }
            return null;
        }

        protected void TryAddToCache(GameUI gameui)
        {
            for (int i = uicaches.Count - 1; i >= 0; --i)
            {
                WeakReference weak = uicaches[i];
                if (weak != null && weak.IsAlive &&  weak.Target.Equals(gameui))
                {
                    return;
                }
                else if(weak.IsAlive == false)
                {
                    uicaches.RemoveAt(i);
                }
            }

            if (CachePool == null)
            {
                CachePool = new GameObject("UIPool");
            }
            if(FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("{0} insert to uipool",gameui);
            gameui.transform.SetTheParent(CachePool);
            this.uicaches.Add(new WeakReference(gameui));
        }

        public override void Clear(bool all)
        {
            base.Clear(all);
            for (int i = 0; i < uicaches.Count; ++i)
            {
                WeakReference weak = uicaches[i];
                if (weak != null && weak.IsAlive )
                {
                    GameUI gameui = weak.Target as GameUI;
                    gameui.DestorySelf();
                }
            }

            uicaches.Clear();

            if (CachePool != null)
            {
                CachePool.DestorySelf();
                CachePool = null;
            }
        }
    }
}


