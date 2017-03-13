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
    [SingleTon]
    public class GameUIControl
    {
        public static GameUIControl mIns;

        private SimpleDictionary<string, AbstractLayout> layouts = new SimpleDictionary<string, AbstractLayout>();

        private EventSystem eventsystem;

        public AbstractLayout this[string name]
        {
            get
            {
                if (!this.layouts.ContainsKey(name))
                {
                    LogMgr.LogErrorFormat("尚未注册的layout :{0}", name);
                    return null;
                }
                   
                return this.layouts[name];
            }
        }

        public AbstractLayout GetLayout(string name)
        {
            return this[name];
        }

        private void CreateEventSystems()
        {
            if (eventsystem != null)
                return;

            EventSystem esys = GameObject.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                GameObject eventObject = new GameObject("EventSystem");
                esys = eventObject.AddComponent<EventSystem>();
                eventObject.AddComponent<StandaloneInputModule>();
            }

            eventsystem = esys;
        }

        public void AddLayout(AbstractLayout lay)
        {
            if (!layouts.ContainsKey(lay.LayoutName))
            {
                layouts.Add(lay.LayoutName, lay);
            }
            else
            {
                LogMgr.LogErrorFormat("{0} 为已经注册的Layout",lay.LayoutName);
            }
        }

        public void RemoveLayot(string name)
        {
            this.layouts.Remove(name);
        }

        public void RemoveLayout(AbstractLayout lay)
        {
            layouts.Remove(lay);
        }

        public void ShowUILayout(string layoutname)
        {
            AbstractLayout layout = this[layoutname];
            if (layout != null)
            {
                layout.ShowUILayout();
            }
        }

        public void HideUILayout(string layoutname)
        {
            AbstractLayout layout = this[layoutname];
            if (layout != null)
            {
                layout.HideUILayout();
            }
        }

        /// <summary>
        /// not include all of the layouts
        /// </summary>
        public void ClearUI()
        {
            List<AbstractLayout> lays = this.layouts.ReadOnlyValues;

            for (int i = 0; i < lays.Count; ++i)
            {
                if(lays[i].CanDestory)
                    lays[i].Clear();
            }
        }

        public void ClearUILayouts()
        {
            List<AbstractLayout> lays = this.layouts.Values;

            this.layouts.Clear();
            for (int i = 0; i < lays.Count; ++i)
            {
                AbstractLayout lay = lays[i];
                if (!lay.CanDestory)
                {
                    this.layouts.Add(lay.LayoutName, lay);
                }
            }

            ListPool.TryDespawn(lays);
        }

        [SceneEnter]
        private static void ListenSceneEnter(int lv)
        {
            mIns.CreateEventSystems();
        }

        [SceneLeave]
        private static void ListenerScene(int lv)
        {
            mIns.eventsystem = null;
            mIns.ClearUI();
        }

        public void Destroy()
        {
            List<AbstractLayout> lays = this.layouts.ReadOnlyValues;
            for (int i = 0; i < lays.Count; ++i)
            {
                AbstractLayout lay = lays[i];
                lay.Release();
                lay.Clear();
            }

            this.layouts.Clear();

            FrameworkAttRegister.DestroyStaticAttEvent(MainLoopEvent.OnLevelLeaved, typeof(GameUIControl), "ListenerScene");
            FrameworkAttRegister.DestroyStaticAttEvent(MainLoopEvent.OnLevelWasLoaded, typeof(GameUIControl), "ListenSceneEnter");
        }
    }
}
