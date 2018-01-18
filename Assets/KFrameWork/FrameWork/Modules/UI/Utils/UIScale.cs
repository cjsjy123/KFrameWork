using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KFrameWork;
using UnityEngine.UI;


public class UIScale : MonoBehaviour {


    public Vector2 uiscale = Vector2.one;

    private Vector2 prescale = Vector2.one;

    void Start()
    {

    }

    public void SetScale(Vector2 targetScale)
    {
        if (targetScale == prescale)
            return;

        Vector2 nowscale = new Vector2( targetScale.x / prescale.x, targetScale.y / prescale.y);

        //pick layout && only pick actives, otherwise maybe affect some pool
        GridLayoutGroup[] grids = this.GetComponentsInChildren<GridLayoutGroup>();
        for(int i =0; i < grids.Length;++i)
        {
            Vector2 size = grids[i].cellSize;
            size.Scale(nowscale);

            grids[i].cellSize = size;
        }

        //pick Font && only pick actives, otherwise maybe affect some pool
        Text[] texts = this.GetComponentsInChildren<Text>();
        for(int i =0; i < texts.Length;++i)
        {
            int size = texts[i].fontSize;
            //y affect text visiable or not
            size = Mathf.RoundToInt(size * nowscale.y);
            texts[i].fontSize = size;
        }

        RectTransform[] recttransforms = this.GetComponentsInChildren<RectTransform>();
        Dictionary<RectTransform, Vector2> dic = new Dictionary<RectTransform, Vector2>();

        for (int i = 0; i < recttransforms.Length; ++i)
        {
            RectTransform recttrans = recttransforms[i];
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = recttrans.worldToLocalMatrix;
            Vector3[] m_Corners = new Vector3[4];
            recttrans.GetWorldCorners(m_Corners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);

            dic[recttrans] = bounds.size;
        }

        for (int i =0; i < recttransforms.Length;++i)
        {
            RectTransform recttrans = recttransforms[i];
            if (i == 0)
                continue;

            Vector2 pos = recttrans.anchoredPosition;
            Vector2 deltasize = recttrans.sizeDelta;

            //Vector2 offsetMin = recttrans.offsetMin;
            //Vector2 offsetMax = recttrans.offsetMax;

            Vector2 uisize = recttrans.rect.size;// dic[recttrans];
            //Vector2 uideltasize = new Vector2(nowscale.x * uisize.x,nowscale.y * uisize.y) - uisize;
            //Debug.Log(uideltasize);

            //recttrans.offsetMin = offsetMin - uideltasize / 2;// new Vector2(uideltasize .x /2);
            //recttrans.offsetMax = offsetMax + uideltasize / 2;

            if (recttrans.anchorMin == recttrans.anchorMax)
            {
                deltasize.Scale(uiscale);
            }
            else
            {
                if (recttrans.anchorMin.x == recttrans.anchorMax.x)
                {
                    deltasize = new Vector2(deltasize.x * nowscale.x, deltasize.y);
                }
                else
                {
                    deltasize.x -= uisize.x * (uiscale.x - 1);
                    // pos.y *= nowscale.y;
                }

                if (recttrans.anchorMin.y == recttrans.anchorMax.y)
                {
                    deltasize = new Vector2(deltasize.x, deltasize.y * nowscale.y);
                }
                else
                {
                    deltasize.y -= uisize.y * (uiscale.y - 1);
                   // pos.x *= nowscale.x;
                }
            }

            recttrans.sizeDelta = deltasize;
            recttrans.anchoredPosition = pos;
        }
        prescale = targetScale;
    }

}
