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
    public abstract class CanvasLayout : AbstractLayout
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
    }
}
