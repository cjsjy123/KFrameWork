using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace KFrameWork
{
    public enum UIPluginType
    {
        UGUI,
        NGUI,
        FGUI,
    }

    public static class UIUtility
    {
        public const UIPluginType defaultType = UIPluginType.UGUI;

        #region ugui

        public static void SetGridLayoutPadding(this GridLayoutGroup grid, Vector4 padding)
        {
            grid.padding = new RectOffset((int)padding.x, (int)padding.y, (int)padding.z, (int)padding.w);
        }

        public static void SetGridLayout(this GridLayoutGroup grid, Vector2 size, Vector2 space, int constvalue,int csize)
        {
            grid.cellSize = size;
            grid.spacing = space;
            grid.constraint = (GridLayoutGroup.Constraint)constvalue;

            if (grid.constraint != GridLayoutGroup.Constraint.Flexible)
            {
                grid.constraintCount = csize;
            }

        }

        public static void RemoveAllListner(this Button btn)
        {
            btn.onClick.RemoveAllListeners();
        }

        public static UILisenter GetListener(this GameObject go)
        {
            //gc will be better (interface)
            UILisenter listener = go.GetComponent<UILisenter>();
            if (listener == null)
                listener = go.AddComponent<UILisenter>();
            return listener;
        }

        public static UILisenter GetListener(this Transform go)
        {

            //gc  will be better (interface)
            UILisenter listener = go.gameObject.GetComponent<UILisenter>();
            if (listener == null)
                listener = go.gameObject.AddComponent<UILisenter>();
            return listener;
        }

        public static UILisenter GetListener(this UIBehaviour go)
        {
            //gc  will be better (interface)
            UILisenter listener = go.gameObject.GetComponent<UILisenter>();
            if (listener == null)
                listener = go.gameObject.AddComponent<UILisenter>();
            return listener;
        }

        public static Vector2 GetposInParentCanvas(this RectTransform recttransform,Canvas canvas)
        {
            RectTransform canvastransform = (RectTransform)canvas.transform;
            Vector3 screenpos = Vector3.zero;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                screenpos = recttransform.position;
            }
            else
                screenpos = canvas.worldCamera.WorldToScreenPoint(recttransform.position);

            Vector2 localpos;
            bool ret =RectTransformUtility.ScreenPointToLocalPointInRectangle(canvastransform, screenpos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out localpos);
            if (!ret)
            {
                LogMgr.LogErrorFormat("{0} not in Rect", recttransform);
            }

            if (!recttransform.pivot.x.FloatEqual(0.5f) || !recttransform.pivot.y.FloatEqual(0.5f))
            {
                float dx = recttransform.pivot.x - 0.5f;
                float dy = recttransform.pivot.y - 0.5f;
                Vector2 size = recttransform.rect.size;
                localpos[0] -= dx * size.x;
                localpos[1] -= dy * size.y;
            }

            return localpos;
        }

        public static void SetGameInsetAndSize(this RectTransform recttransform, float wid, float height)
        {
            Vector2 realsize = recttransform.GetUIRealSize();
            recttransform.SetInsetAndSizeFromParentEdge( RectTransform.Edge.Left , wid, realsize.x);
            recttransform.SetInsetAndSizeFromParentEdge( RectTransform.Edge.Bottom , height, realsize.y);
        }

        public static Vector2 GetUIRealSize(this RectTransform recttransform)
        {

            if (recttransform.anchorMax == recttransform.anchorMin)
            {
                return recttransform.sizeDelta;
            }
            if (recttransform.anchorMax.x == recttransform.anchorMin.x)
            {
                return new Vector2(recttransform.sizeDelta.x, Math.Abs(recttransform.sizeDelta.y));
            }
            else if (recttransform.anchorMax.y == recttransform.anchorMin.y)
            {
                return new Vector2(Math.Abs(recttransform.sizeDelta.x), recttransform.sizeDelta.y);
            }
            else {
                return new Vector2(Math.Abs(recttransform.sizeDelta.x), Math.Abs(recttransform.sizeDelta.y));
            }
        }

        public static Vector2 GetUIPixelSize(this RectTransform recttransform)
        {
            return recttransform.rect.size;
        }

        public static void SetFitScreen(this RectTransform recttransfrom)
        {
            recttransfrom.anchorMin = Vector2.zero;
            recttransfrom.anchorMax = Vector2.one;
            recttransfrom.offsetMin = Vector2.zero;
            recttransfrom.offsetMax = Vector2.zero;
        }

        private static void SetUIAlignPos(RectTransform rect, UIAlign align, Canvas canvas, Camera camera)
        {
            bool isOverLay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;

            float ScreenHight = canvas.pixelRect.height; //canvas.pixelRect.height;
            float ScreenWidth = canvas.pixelRect.width;// canvas.pixelRect.width;

            //Vector3 WorldPos;
            Vector2 ScreenPos = Vector2.zero;
            switch (align)
            {
                case UIAlign.CENTER:
                    {
                        ScreenPos = new Vector2(ScreenWidth / 2, ScreenHight / 2);

                        break;
                    }
                case UIAlign.CENTER_TOP:
                    {
                        ScreenPos = new Vector2(ScreenWidth / 2, ScreenHight);

                        break;
                    }
                case UIAlign.CENTER_DOWN:
                    {
                        ScreenPos = new Vector2(ScreenWidth / 2, 0);

                        break;
                    }
                case UIAlign.LEFT_DOWN:
                    {
                        ScreenPos = new Vector2(0, 0);
                        break;
                    }
                case UIAlign.LEFT_TOP:
                    {
                        ScreenPos = new Vector2(0, ScreenHight);
                        break;
                    }
                case UIAlign.RIGHT_DOWN:
                    {
                        ScreenPos = new Vector2(ScreenWidth, 0);
                        break;
                    }
                case UIAlign.RIGHT_TOP:
                    {
                        ScreenPos = new Vector2(ScreenWidth, ScreenHight);
                        break;
                    }
            }
            RectTransform canvasrect = canvas.GetComponent<RectTransform>();

            Vector3 worldpos;

            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasrect, ScreenPos, isOverLay ? null : camera, out worldpos);
            rect.position = worldpos;
            //return worldpos;
            //Vector2 pos;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasrect, ScreenPos, isOverLay ? null : camera, out pos);

            //rect.anchoredPosition = pos;
        }

        private static void UGUIAutoAlign(this GameObject target, UIAlign align = UIAlign.CENTER)
        {
            Camera camera = null;
            Canvas canvas = target.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                LogMgr.LogErrorFormat ("Cant found Canvas :{0}", target);
#if UNITY_EDITOR
                target.PauseGame();
#endif
                return;
            }
            else if (canvas.worldCamera == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                LogMgr.LogErrorFormat("Camera is Null:{0", target);
                return;
            }

            RectTransform trans = target.GetComponent<RectTransform>();
            if (trans == null)
            {
                LogMgr.LogErrorFormat("missing recttransform :{0}",target);
                return;
            }

            camera = canvas.worldCamera;

            //bool islandscape = FrameWorkConfig.DisplayUIWidth > FrameWorkConfig.DisplayUIHiehgt;
            //Vector2 size = trans.sizeDelta;
            //Vector2 screensize = new Vector2(Screen.width,Screen.height);

            if (align == UIAlign.CENTER)
            {
                //if (islandscape)
                //{
                //    trans.anchorMin = new Vector2(0,0.5f);
                //    trans.anchorMax = new Vector2(1, 0.5f);
                //    trans.offsetMin = new Vector2();
                //}
                //else
                //{
                //    trans.anchorMin = new Vector2(0.5f, 0f);
                //    trans.anchorMax = new Vector2(0.5f, 1f);
                //}
                ////trans.anchorMin = new Vector2(0.5f, 0.5f);
                ////trans.anchorMax = new Vector2(0.5f, 0.5f);
                //trans.pivot = new Vector2(0.5f, 0.5f);
                SetUIAlignPos(trans,align, canvas, camera);
            }
            else if (align == UIAlign.CENTER_TOP)
            {
                //if (islandscape)
                //{
                //    trans.anchorMin = new Vector2(0, 0.5f);
                //    trans.anchorMax = new Vector2(1, 0.5f);
                //}
                //else
                //{
                //    trans.anchorMin = new Vector2(0.5f, 0f);
                //    trans.anchorMax = new Vector2(0.5f, 1f);
                //}
                ////trans.anchorMin = new Vector2(0.5f, 1f);
                ////trans.anchorMax = new Vector2(0.5f, 1f);
                //trans.pivot = new Vector2(0.5f, 1f);
                SetUIAlignPos(trans, align, canvas, camera);
            }
            else if (align == UIAlign.CENTER_DOWN)
            {
                //if (islandscape)
                //{
                //    trans.anchorMin = new Vector2(0, 0.5f);
                //    trans.anchorMax = new Vector2(1, 0.5f);
                //}
                //else
                //{
                //    trans.anchorMin = new Vector2(0.5f, 0f);
                //    trans.anchorMax = new Vector2(0.5f, 1f);
                //}
                ////trans.anchorMin = new Vector2(0.5f, 0f);
                ////trans.anchorMax = new Vector2(0.5f, 0f);
                //trans.pivot = new Vector2(0.5f, 0f);
                SetUIAlignPos(trans, align, canvas, camera);
            }
            else if (align == UIAlign.LEFT_DOWN)
            {
                //if (islandscape)
                //{
                //    trans.anchorMin = new Vector2(0, 0.5f);
                //    trans.anchorMax = new Vector2(1, 0.5f);
                //}
                //else
                //{
                //    trans.anchorMin = new Vector2(0.5f, 0f);
                //    trans.anchorMax = new Vector2(0.5f, 1f);
                //}
                ////trans.anchorMin = new Vector2(0f, 0f);
                ////trans.anchorMax = new Vector2(0f, 0f);
                //trans.pivot = new Vector2(0f, 0f);
                SetUIAlignPos(trans, align, canvas, camera);
            }
            else if (align == UIAlign.LEFT_TOP)
            {
                //if (islandscape)
                //{
                //    trans.anchorMin = new Vector2(0, 0.5f);
                //    trans.anchorMax = new Vector2(1, 0.5f);
                //}
                //else
                //{
                //    trans.anchorMin = new Vector2(0.5f, 0f);
                //    trans.anchorMax = new Vector2(0.5f, 1f);
                //}
                ////trans.anchorMin = new Vector2(0f, 1f);
                ////trans.anchorMax = new Vector2(0f, 1f);
                //trans.pivot = new Vector2(0f, 1f);
                SetUIAlignPos(trans, align, canvas, camera);
            }
            else if (align == UIAlign.RIGHT_TOP)
            {
                //if (islandscape)
                //{
                //    trans.anchorMin = new Vector2(0, 0.5f);
                //    trans.anchorMax = new Vector2(1, 0.5f);
                //}
                //else
                //{
                //    trans.anchorMin = new Vector2(0.5f, 0f);
                //    trans.anchorMax = new Vector2(0.5f, 1f);
                //}
                ////trans.anchorMin = new Vector2(1f, 1f);
                ////trans.anchorMax = new Vector2(1f, 1f);
                //trans.pivot = new Vector2(1f, 1f);
                SetUIAlignPos(trans, align, canvas, camera);
            }
            else if (align == UIAlign.RIGHT_DOWN)
            {
                //if (islandscape)
                //{
                //    trans.anchorMin = new Vector2(0, 0.5f);
                //    trans.anchorMax = new Vector2(1, 0.5f);
                //}
                //else
                //{
                //    trans.anchorMin = new Vector2(0.5f, 0f);
                //    trans.anchorMax = new Vector2(0.5f, 1f);
                //}
                ////trans.anchorMin = new Vector2(1f, 0f);
                ////trans.anchorMax = new Vector2(1f, 0f);
                //trans.pivot = new Vector2(1f, 0f);
                SetUIAlignPos(trans, align, canvas, camera);
            }
        }

        #endregion

        public static void AutoAlign(this Component tr, UIAlign align = UIAlign.CENTER, UIPluginType uitype = defaultType)
        {
            tr.gameObject.AutoAlign(align, uitype);
        }

        public static void AutoAlign(this GameObject tr, UIAlign align = UIAlign.CENTER, UIPluginType uitype = defaultType)
        {
            if (uitype == UIPluginType.UGUI)
            {
                tr.UGUIAutoAlign(align);
            }
        }

        /// <summary>
        ///使得worldui的缩放参数和大小参数和screen 一致
        /// </summary>
        /// <param name="canvas"></param>
        public static void CalcalCanvasSceenSize(this Canvas canvas)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Vector2 designedResolution = Vector2.zero;
            float matchWidthOrHeight = 1f;
            if (scaler == null)
            {
                designedResolution = new Vector2(FrameWorkConfig.DisplayUIWidth,FrameWorkConfig.DisplayUIHiehgt);
            }
            else
            {
                matchWidthOrHeight = scaler.matchWidthOrHeight;
                designedResolution = scaler.referenceResolution;
            }

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            float logWidth = Mathf.Log(screenSize.x / designedResolution.x, 2);
            float logHeight = Mathf.Log(screenSize.y / designedResolution.y, 2);
            float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, matchWidthOrHeight);
            float scaleFactor = Mathf.Pow(2, logWeightedAverage);

            float aspect = screenSize.x / screenSize.y;
            float canvasWidth = screenSize.x / scaleFactor;
            float canvasHeight = screenSize.y / scaleFactor;
            float initialAspect = (float)canvasWidth / canvasHeight;
            float height = (initialAspect < aspect) ? canvasWidth / aspect : canvasHeight;

            float targetvalue = 2f / height * canvas.worldCamera.orthographicSize;

            RectTransform recttransform = canvas.GetComponent<RectTransform>();
            recttransform.sizeDelta = new Vector2(canvasWidth, canvasHeight);
            //canvas.transform.SetWorldScale(targetvalue.ToVector3());

            //canvas.scaleFactor = targetvalue;
            Transform canvasParent = canvas.transform.parent;
            if (canvasParent != null)
            {
                canvas.transform.localScale = canvasParent.worldToLocalMatrix * targetvalue.ToVector3();
            }
            else
            {
                canvas.transform.localScale = targetvalue.ToVector3();
            }
        }


        public static List<GameObject> CreateVertGridScroll(this GameObject gameobject, string masksprite, out GameObject scrollObj, Vector2 viewSize, float delta, GameObject prefab, int cnt, UIAlign align = UIAlign.CENTER, int collimit = -1, int rowlimit = -1, UIPluginType uitype = defaultType)
        {
            if (uitype == UIPluginType.UGUI)
            {
                GameObject scrollgameobject = gameobject.TryGetGameObject("Scroll");
                scrollObj = scrollgameobject;

                float totaly = 0f;
                if (prefab != null)
                {
                    totaly = (prefab.GetComponent<RectTransform>().sizeDelta.y + delta) * cnt;
                }

                ScrollViewExpand rect = scrollgameobject.TryAddComponent<ScrollViewExpand>();
                rect.vertical = true;
                rect.horizontal = false;
                rect.movementType = ScrollRect.MovementType.Elastic;
                rect.inertia = true;
                //rect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                //rect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                rect.GetComponent<RectTransform>().sizeDelta = viewSize;

                GameObject viewPortgameobject = scrollgameobject.TryGetGameObject("Viewport");
                GameObject Gridgameobject = viewPortgameobject.TryGetGameObject("Grid");

                Mask viewmask = viewPortgameobject.TryAddComponent<Mask>();
                viewmask.showMaskGraphic = false;

                ImageExpand viewimage = viewPortgameobject.TryAddComponent<ImageExpand>();
                viewimage.raycastTarget = true;
                viewimage.type = Image.Type.Sliced;
                viewimage.fillCenter = true;

                if (SpriteAtlasMgr.mIns != null)
                    SpriteAtlasMgr.mIns.ChangeSprite(viewimage, masksprite);

                GridLayoutGroup group = Gridgameobject.TryAddComponent<GridLayoutGroup>();
                group.startCorner = GridLayoutGroup.Corner.UpperLeft;
                group.startAxis = GridLayoutGroup.Axis.Vertical;
                group.spacing = new Vector2(0, delta);

                RectTransform grouprect = group.GetComponent<RectTransform>();
                grouprect.sizeDelta = new Vector2(0, totaly);
                grouprect.anchorMin = new Vector2(0, 1);
                grouprect.anchorMax = new Vector2(1, 1);
                grouprect.pivot = new Vector2(0, 1);

                if (collimit != -1 && collimit >= 0)
                {
                    group.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    group.constraintCount = collimit;
                }
                else if (rowlimit != -1 && rowlimit >= 0)
                {
                    group.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    group.constraintCount = rowlimit;
                }

                if (align == UIAlign.CENTER)
                {
                    group.childAlignment = TextAnchor.MiddleCenter;
                }
                else if (align == UIAlign.CENTER_DOWN)
                {
                    group.childAlignment = TextAnchor.LowerCenter;
                }
                else if (align == UIAlign.CENTER_TOP)
                {
                    group.childAlignment = TextAnchor.UpperCenter;
                }
                else if (align == UIAlign.LEFT_DOWN)
                {
                    group.childAlignment = TextAnchor.LowerLeft;
                }
                else if (align == UIAlign.LEFT_TOP)
                {
                    group.childAlignment = TextAnchor.UpperLeft;
                }
                else if (align == UIAlign.RIGHT_DOWN)
                {
                    group.childAlignment = TextAnchor.LowerRight;
                }
                else if (align == UIAlign.RIGHT_TOP)
                {
                    group.childAlignment = TextAnchor.UpperRight;
                }

                List<GameObject> gameObjectList = null;
                if (cnt > 0)
                {
                    gameObjectList = ListPool.TrySpawn<List<GameObject>>();
                    for (int i = 0; i < cnt; ++i)
                    {
                        GameObject childgameobject = GameObject.Instantiate(prefab);
                        if (i == 0)
                        {
                            RectTransform childrect = childgameobject.GetComponent<RectTransform>();
                            if (childrect != null)
                                group.cellSize = childrect.sizeDelta;
                        }
                        Gridgameobject.AddInstance(childgameobject);
                        gameObjectList.Add(childgameobject);
                    }
                }

                RectTransform viewrecttransform = viewPortgameobject.GetComponent<RectTransform>();
                viewrecttransform.anchorMin = Vector2.zero;
                viewrecttransform.anchorMax = new Vector2(1, 1);
                viewrecttransform.pivot = new Vector2(0, 1);
                viewrecttransform.sizeDelta = Vector2.zero;

                rect.content = grouprect;
                rect.viewport = viewrecttransform;
                return gameObjectList;
            }
            scrollObj = null;
            return null;
        }


        public static List<GameObject> CreateHorGridScroll(this GameObject gameobject, string masksprite, out GameObject scrollObj, Vector2 viewSize, float delta, GameObject prefab, int cnt, UIAlign align = UIAlign.CENTER, int collimit = -1, int rowlimit = -1, UIPluginType uitype = defaultType)
        {
            if (uitype == UIPluginType.UGUI)
            {
                GameObject scrollgameobject = gameobject.TryGetGameObject("Scroll");
                scrollObj = scrollgameobject;
                float totalx = 0f;
                if (prefab != null)
                {
                    totalx = (prefab.GetComponent<RectTransform>().sizeDelta.x + delta) * cnt;
                }

                ScrollViewExpand rect = scrollgameobject.TryAddComponent<ScrollViewExpand>();
                rect.vertical = false;
                rect.horizontal = true;
                rect.movementType = ScrollRect.MovementType.Elastic;
                rect.inertia = true;
                //rect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                //rect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                rect.GetComponent<RectTransform>().sizeDelta = viewSize;

                GameObject viewPortgameobject = scrollgameobject.TryGetGameObject("Viewport");
                GameObject Gridgameobject = viewPortgameobject.TryGetGameObject("Grid");

                Mask viewmask = viewPortgameobject.TryAddComponent<Mask>();
                viewmask.showMaskGraphic = false;

                ImageExpand viewimage = viewPortgameobject.TryAddComponent<ImageExpand>();
                viewimage.raycastTarget = true;
                viewimage.type = Image.Type.Sliced;
                viewimage.fillCenter = true;

                if (SpriteAtlasMgr.mIns != null)
                    SpriteAtlasMgr.mIns.ChangeSprite(viewimage, masksprite);

                GridLayoutGroup group = Gridgameobject.TryAddComponent<GridLayoutGroup>();
                group.spacing = new Vector2(delta, 0);
                group.startCorner = GridLayoutGroup.Corner.UpperLeft;
                group.startAxis = GridLayoutGroup.Axis.Horizontal;

                RectTransform grouprect = group.GetComponent<RectTransform>();
                grouprect.sizeDelta = new Vector2(totalx, 0);
                grouprect.anchorMin = new Vector2(0, 1);
                grouprect.anchorMax = new Vector2(1, 1);
                grouprect.pivot = new Vector2(0, 1);

                if (collimit != -1 && collimit >= 0)
                {
                    group.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    group.constraintCount = collimit;
                }
                else if (rowlimit != -1 && rowlimit >= 0)
                {
                    group.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    group.constraintCount = rowlimit;
                }

                if (align == UIAlign.CENTER)
                {
                    group.childAlignment = TextAnchor.MiddleCenter;
                }
                else if (align == UIAlign.CENTER_DOWN)
                {
                    group.childAlignment = TextAnchor.LowerCenter;
                }
                else if (align == UIAlign.CENTER_TOP)
                {
                    group.childAlignment = TextAnchor.UpperCenter;
                }
                else if (align == UIAlign.LEFT_DOWN)
                {
                    group.childAlignment = TextAnchor.LowerLeft;
                }
                else if (align == UIAlign.LEFT_TOP)
                {
                    group.childAlignment = TextAnchor.UpperLeft;
                }
                else if (align == UIAlign.RIGHT_DOWN)
                {
                    group.childAlignment = TextAnchor.LowerRight;
                }
                else if (align == UIAlign.RIGHT_TOP)
                {
                    group.childAlignment = TextAnchor.UpperRight;
                }

                List<GameObject> gameObjectList = null;
                if (cnt > 0)
                {
                    gameObjectList = ListPool.TrySpawn<List<GameObject>>();
                    for (int i = 0; i < cnt; ++i)
                    {
                        GameObject childgameobject = GameObject.Instantiate(prefab);
                        if (i == 0)
                        {
                            RectTransform childrect = childgameobject.GetComponent<RectTransform>();
                            if (childrect != null)
                                group.cellSize = childrect.sizeDelta;
                        }
                        Gridgameobject.AddInstance(childgameobject);
                        gameObjectList.Add(childgameobject);
                    }
                }

                RectTransform viewrecttransform = viewPortgameobject.GetComponent<RectTransform>();
                viewrecttransform.anchorMin = Vector2.zero;
                viewrecttransform.anchorMax = new Vector2(1, 1);
                viewrecttransform.pivot = new Vector2(0, 1);
                viewrecttransform.sizeDelta = Vector2.zero;

                rect.content = grouprect;
                rect.viewport = viewrecttransform;
                return gameObjectList;

            }
            scrollObj = null;
            return null;
        }
    }
}

