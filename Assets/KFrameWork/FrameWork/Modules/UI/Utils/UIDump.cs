using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using UnityEngine.UI;

namespace KFrameWork
{
    [ExecuteInEditMode]
    public class UIDump : MonoBehaviour
    {
        public int RenderQueue;

        public int UIRender;

        public string UISortLayer;
        [Range(0, 5000)]
        public int canvasrenderQueue;

        [Range(0,5000)]
        public int UISortOrder;

        public Vector3 worldPos;

        public Vector3 LocalPos;

        private Canvas m_canvas;

        private Renderer m_render;

        private CanvasRenderer canvasrender;

        void Awake()
        {
            m_canvas = this.GetComponent<Canvas>();
            m_render = this.GetComponent<Renderer>();
            canvasrender = this.GetComponent<CanvasRenderer>();
            if (m_canvas != null)
            {
                UISortLayer = m_canvas.sortingLayerName;
                UISortOrder = m_canvas.sortingOrder;
                UIRender = m_canvas.renderOrder;
            }

            if (canvasrender != null)
            {
                Material mat = canvasrender.GetMaterial();
                if (mat != null)
                {
                    this.canvasrenderQueue = mat.renderQueue;
                }
            }

            LocalPos = this.transform.localPosition;
            worldPos = this.transform.position;
        }

        void Update()
        {
            LocalPos = this.transform.localPosition;
            worldPos = this.transform.position;

            if (m_canvas != null)
            {
                UISortLayer = m_canvas.sortingLayerName;
                if (UISortOrder != m_canvas.sortingOrder)
                {
                    m_canvas.sortingOrder = UISortOrder;
                }

                UISortOrder = m_canvas.sortingOrder;
                UIRender = m_canvas.renderOrder;
            }

            if (canvasrender != null)
            {
                Material mat = canvasrender.GetMaterial();
                if (mat != null)
                {
                    if (this.canvasrenderQueue != mat.renderQueue)
                        mat.renderQueue = this.canvasrenderQueue;
                    this.canvasrenderQueue = mat.renderQueue;
                }
            }


            if (m_render != null && m_render.sharedMaterial !=null)
            {
                RenderQueue = m_render.sharedMaterial.renderQueue;
            }
        }
    }
}


