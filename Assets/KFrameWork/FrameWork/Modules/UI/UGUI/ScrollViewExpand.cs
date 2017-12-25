using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using KUtils;

namespace KFrameWork
{

	public class ScrollViewExpand : ScrollRect {

        private RectTransform rect;

        public RectTransform rectTransform
        {
            get
            {
                if (rect == null)
                    rect = GetComponent<RectTransform>();
                return rect;
            }
        }

        private CanvasRenderer _canvasrender;

        public CanvasRenderer canvasrender
        {
            get
            {
                if (_canvasrender == null)
                    _canvasrender = GetComponent<CanvasRenderer>();
                return _canvasrender;
            }
        }

    }
}


