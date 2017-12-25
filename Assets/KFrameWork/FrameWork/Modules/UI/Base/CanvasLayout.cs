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
    /// just a single canvas
    /// </summary>
    public abstract class CanvasLayout : BaseLayout
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

                this.UpdateForPropertys();
            }
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
                    this.canvas.AddInstance(instance);
                else
                    Parent.AddInstance(instance);

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

        protected override bool CanLoadFromCache(string respath)
        {
            return false;
        }

        protected override GameUI LoadFromCache(string respath)
        {
            return null;
        }


        public override void ShowUILayout()
        {
            canvas.gameObject.SetActive(true);
        }

        public override void HideUILayout()
        {
            canvas.gameObject.SetActive(false);
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
}
