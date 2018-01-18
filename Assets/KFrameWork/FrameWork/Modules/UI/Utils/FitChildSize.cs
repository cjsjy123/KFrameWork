using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutGroup))]
public class FitChildSize : MonoBehaviour, ILayoutElement
{
    private LayoutGroup _group;

    public LayoutGroup group
    {
        get
        {
            if (_group == null)
                _group = this.GetComponent<LayoutGroup>();
            return _group;
        }
    }

    public float flexibleHeight
    {
        get
        {
            return group.flexibleHeight;
        }
    }

    public float flexibleWidth
    {
        get
        {
            return group.flexibleWidth;
        }
    }

    public int layoutPriority
    {
        get
        {
            return group.layoutPriority;
        }
    }

    public float minHeight
    {
        get
        {
            return group.minHeight;
        }
    }

    public float minWidth
    {
        get
        {
            return group.minWidth;
        }
    }

    public float preferredHeight
    {
        get
        {
            return group.preferredHeight;
        }
    }

    public float preferredWidth
    {
        get
        {
            return group.preferredWidth;
        }
    }

    private List<ILayoutElement> elements;

    public void CalculateLayoutInputHorizontal()
    {
        Resize(true);
    }

    public void CalculateLayoutInputVertical()
    {
        Resize(false);
    }

    public void Resize(bool isHor)
    {
        if (elements == null)
            elements = new List<ILayoutElement>();
        else
            elements.Clear();

        int cnt = this.transform.childCount;
        float totalwid = 0;
        float totalheight = 0;
        //暂时不考虑spacing padding
        //float spacedistance = 0f;
        //float exdistance = 10f;

        for (int i = 0; i < cnt; ++i)
        {
            Transform tr = this.transform.GetChild(i);
            Component component = tr.GetComponent(typeof(ILayoutElement));
            if (component != null)
            {
                ILayoutElement element = component as ILayoutElement;
                if (isHor)
                {
                    totalwid += Mathf.Max(element.minWidth, element.preferredWidth);
                    totalheight = Mathf.Max(totalheight, Mathf.Max(element.minHeight, element.preferredHeight));
                }
                else
                {
                    totalwid = Mathf.Max(totalwid,Mathf.Max(element.minWidth, element.preferredWidth));
                    totalheight += Mathf.Max(element.minHeight, element.preferredHeight);
                }
 
            }
        }

        RectTransform rect = this.transform as RectTransform;
        if (rect.anchorMin == rect.anchorMax)
        {
            rect.sizeDelta = new Vector2(totalwid, totalheight);
        }
        else
        {
            if (isHor)
            {

            }
            else
            {

            }
        }
    }
}
