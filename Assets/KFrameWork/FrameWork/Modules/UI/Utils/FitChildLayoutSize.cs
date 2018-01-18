using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FitChildLayoutSize : MonoBehaviour, ILayoutElement
{

    public LayoutGroup target;

    private RectTransform recttransfrom;

    public float flexibleHeight
    {
        get
        {
            if (target == null)
                return 0f;
            return target.flexibleHeight;
        }
    }

    public float flexibleWidth
    {
        get
        {
            if (target == null)
                return 0f;
            return target.flexibleWidth;
        }
    }

    public int layoutPriority
    {
        get
        {
            if (target == null)
                return 0;
            return target.layoutPriority;
        }
    }

    public float minHeight
    {
        get
        {
            if (target == null)
                return 0f;
            return target.minHeight;
        }
    }

    public float minWidth
    {
        get
        {
            if (target == null)
                return 0f;
            return target.minWidth;
        }
    }

    public float preferredHeight
    {
        get
        {
            if (target == null)
                return 0f;
            return target.preferredHeight;
        }
    }

    public float preferredWidth
    {
        get
        {
            if (target == null)
                return 0f;
            return target.preferredWidth;
        }
    }

    public void CalculateLayoutInputHorizontal()
    {
        if (this.recttransfrom != null)
        {

            this.recttransfrom.sizeDelta = new Vector2(this.preferredWidth , this.preferredHeight );
        }
    }

    public void CalculateLayoutInputVertical()
    {
        if (this.recttransfrom != null)
        {
            this.recttransfrom.sizeDelta = new Vector2(this.preferredWidth, this.preferredHeight );
        }
    }


    void Awake()
    {
        if (target == null)
        {
            target = this.GetComponentInChildren<LayoutGroup>();
        }

        if (target == null || this.transform.parent == null)
        {
            this.enabled = false;
            LogMgr.LogError("Missing LayoutGroup");
        }
        else
        {
            this.recttransfrom = this.transform.parent.GetComponent<RectTransform>();

        }
    }


}
