using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DropDownExpand:Dropdown
{
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
