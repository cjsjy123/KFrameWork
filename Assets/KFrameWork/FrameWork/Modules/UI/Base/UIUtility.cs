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
        private static Vector3 GetUIAlignPos(UIAlign align, Canvas canvas, Camera camera)
        {
            bool isOverLay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;

            float ScreenHight = isOverLay ? canvas.pixelRect.height : Screen.height; //canvas.pixelRect.height;
            float ScreenWidth = isOverLay ? canvas.pixelRect.width : Screen.width;// canvas.pixelRect.width;

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

            Vector3 worldpos;
            RectTransform canvasrect = canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasrect, ScreenPos, isOverLay ? null : camera, out worldpos);
            //WorldPos = camera.ScreenToWorldPoint(ScreenPos.AppendUIDepth(canvas));
            return worldpos;
        }

        private static void UGUIAutoAlign(this GameObject target, UIAlign align = UIAlign.CENTER)
        {
            Camera camera = null;
            Canvas canvas = target.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                LogMgr.LogError("Cant found Canvas");
                return;
            }
            else if (canvas.worldCamera == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                LogMgr.LogError("Camera is Null");
                return;
            }

            RectTransform trans = target.GetComponent<RectTransform>();
            if (trans == null)
            {
                LogMgr.LogError("missing recttransform");
                return;
            }

            camera = canvas.worldCamera;
            //contentSize = new Vector2(contentSize.x * selfSX * canvas.transform.localScale.x, contentSize.y * selfSY * canvas.transform.localScale.y);
            if (align == UIAlign.CENTER)
            {
                trans.anchorMin = new Vector2(0.5f, 0.5f);
                trans.anchorMax = new Vector2(0.5f, 0.5f);
                trans.pivot = new Vector2(0.5f, 0.5f);
                trans.position = GetUIAlignPos(align, canvas, camera);
            }
            else if (align == UIAlign.CENTER_TOP)
            {
                trans.anchorMin = new Vector2(0.5f, 1f);
                trans.anchorMax = new Vector2(0.5f, 1f);
                trans.pivot = new Vector2(0.5f, 1f);
                trans.position = GetUIAlignPos(align, canvas, camera);
            }
            else if (align == UIAlign.CENTER_DOWN)
            {
                trans.anchorMin = new Vector2(0.5f, 0f);
                trans.anchorMax = new Vector2(0.5f, 0f);
                trans.pivot = new Vector2(0.5f, 0f);
                trans.position = GetUIAlignPos(align, canvas, camera);
            }
            else if (align == UIAlign.LEFT_DOWN)
            {
                trans.anchorMin = new Vector2(0f, 0f);
                trans.anchorMax = new Vector2(0f, 0f);
                trans.pivot = new Vector2(0f, 0f);
                trans.position = GetUIAlignPos(align, canvas, camera);
            }
            else if (align == UIAlign.LEFT_TOP)
            {
                trans.anchorMin = new Vector2(0f, 1f);
                trans.anchorMax = new Vector2(0f, 1f);
                trans.pivot = new Vector2(0f, 1f);
                trans.position = GetUIAlignPos(align, canvas, camera);
            }
            else if (align == UIAlign.RIGHT_TOP)
            {
                trans.anchorMin = new Vector2(1f, 1f);
                trans.anchorMax = new Vector2(1f, 1f);
                trans.pivot = new Vector2(1f, 1f);
                trans.position = GetUIAlignPos(align, canvas, camera);
            }
            else if (align == UIAlign.RIGHT_DOWN)
            {
                trans.anchorMin = new Vector2(1f, 0f);
                trans.anchorMax = new Vector2(1f, 0f);
                trans.pivot = new Vector2(1f, 0f);
                trans.position = GetUIAlignPos(align, canvas, camera);
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

            if (canvas == null || !canvas.isRootCanvas)
                return;

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            float logWidth = Mathf.Log(screenSize.x / scaler.referenceResolution.x, 2);
            float logHeight = Mathf.Log(screenSize.y / scaler.referenceResolution.y, 2);
            float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, scaler.matchWidthOrHeight);
            float scaleFactor = Mathf.Pow(2, logWeightedAverage);

            float aspect = screenSize.x / screenSize.y;
            float canvasWidth = screenSize.x / scaleFactor;
            float canvasHeight = screenSize.y / scaleFactor;
            float initialAspect = (float)canvasWidth / canvasHeight;
            float height = (initialAspect < aspect) ? canvasWidth / aspect : canvasHeight;

            float targetvalue = 2f / height * canvas.worldCamera.orthographicSize;

            RectTransform recttransform = canvas.GetComponent<RectTransform>();
            recttransform.sizeDelta = new Vector2(canvasWidth, canvasHeight);
            //canvas.scaleFactor = targetvalue;
            canvas.transform.localScale = targetvalue.ToVector3();
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

