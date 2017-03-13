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
    /// <summary>
    /// 集中在一个canvas下面
    /// </summary>
    public abstract class CanvasCacheLayout : Cachelayout
    {
        public override LayoutLoadMode DefaultMode
        {
            get
            {
                return LayoutLoadMode.LoadFromCacheIfNew;
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

        protected override GameUI BindToCanvas(GameObject instance, Transform Parent, AbstractParams p)
        {
            if (canvas == null)
            {
                canvas = CreateCanvas(LayoutName);
                canvas.overrideSorting = true;
                canvas.sortingOrder = this.Order;
                //if (FrameWorkConfig.Open_DEBUG)
                //    canvas.TryAddComponent<UIDump>();
                CanvasCreated(canvas);
            }

            if (Parent == null)
                this.canvas.AddInstance(instance);
            else
                instance.SetTheParent(Parent);

            instance.transform.position = new Vector3(0, 0, -999999);
            GameUI ui = instance.GetComponent<GameUI>();
            ui.ParentLayout = this;
            return ui;
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
    public abstract class Cachelayout : AbstractLayout
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

        protected override bool CanLoadFromCache(string respath)
        {
            for (int i = uicaches.Count -1; i>=0; --i)
            {
                WeakReference weak = uicaches[i];
                if (weak != null && weak.IsAlive)
                {
                    GameUI weakui = weak.Target as GameUI;
                    if (weakui.loadpath.Equals(respath))
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
                    if (weakui.loadpath.Equals(respath))
                    {
                        weakui.DoVisiable();
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

        private void TryAddToCache(GameUI gameui)
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

            this.uicaches.Add(new WeakReference(gameui));
        }

        public override void CloseUI(string respath, AbstractParams p = null)
        {
            for (int i = this.uicontainer.Count - 1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];
                if (gameui.loadpath.Equals(respath))
                {
                    this.uicontainer.RemoveAt(i);
                    gameui.CallExit(p);

                    if (p != null)
                        p.Release();

                    gameui.DoInVisiable();
                    TryAddToCache(gameui);
                    break;
                }
            }
        }

        public override void CloseUI(GameUI ui, AbstractParams p)
        {
            for (int i = this.uicontainer.Count - 1; i >= 0; i--)
            {
                GameUI gameui = this.uicontainer[i];
                if (gameui == ui)
                {
                    this.uicontainer.RemoveAt(i);
                    break;
                }
            }

            ui.CallExit(p);
            ui.DoInVisiable();
            if (p != null)
                p.Release();
            TryAddToCache(ui);
        }

        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < uicaches.Count; ++i)
            {
                WeakReference weak = uicaches[i];
                if (weak != null && weak.IsAlive )
                {
                    GameUI gameui = weak.Target as GameUI;
                    GameObject.Destroy(gameui);
                }
            }

            uicaches.Clear();

            if (CachePool != null)
            {
                GameObject.Destroy(CachePool);
                CachePool = null;
            }
        }
    }
}


