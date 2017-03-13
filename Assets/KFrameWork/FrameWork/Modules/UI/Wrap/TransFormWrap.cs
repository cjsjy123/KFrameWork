using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using KUtils;

namespace KFrameWork
{
    public static class StaticUtils
    {
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
    }
}


