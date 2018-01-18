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
            this.layouts.RemoveKey(name);
        }

        public void RemoveLayout(AbstractLayout lay)
        {
            layouts.RemoveValue(lay);
        }

        public bool HasLayout(string name)
        {
            return this.layouts.ContainsKey(name);
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

        public void CreateALLLayoutForScene()
        {
            List<AbstractLayout> values =  layouts.Values;
            for (int i = 0; i < values.Count; ++i)
            {
                values[i].AskCanvas();
                values[i].HideUILayout();
            }
        }

        /// <summary>
        /// not include all of the layouts
        /// </summary>
        public void ClearUI(bool all)
        {
            List<AbstractLayout> lays = this.layouts.ReadOnlyValues;

            for (int i = 0; i < lays.Count; ++i)
            {
                lays[i].Clear(all);
            }
        }

        public void ClearUILayouts()
        {
            List<AbstractLayout> lays = this.layouts.Values;

            this.layouts.Clear();
            for (int i = 0; i < lays.Count; ++i)
            {
                AbstractLayout lay = lays[i];
                this.layouts.Add(lay.LayoutName, lay);
            }

            ListPool.TryDespawn(lays);
        }

        public void Dump()
        {
            List<AbstractLayout> lays = this.layouts.Values;

            for (int i = 0; i < lays.Count; ++i)
            {
                AbstractLayout lay = lays[i];
                lay.Dump();
            }
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
        }

        public void Destroy()
        {
            List<AbstractLayout> lays = this.layouts.ReadOnlyValues;
            for (int i = 0; i < lays.Count; ++i)
            {
                AbstractLayout lay = lays[i];
                lay.Release();
                lay.Clear(true);
            }

            this.layouts.Clear();

            FrameworkAttRegister.DestroyStaticAttEvent(MainLoopEvent.OnLevelLeaved, typeof(GameUIControl), "ListenerScene");
            FrameworkAttRegister.DestroyStaticAttEvent(MainLoopEvent.OnLevelWasLoaded, typeof(GameUIControl), "ListenSceneEnter");
        }


    }
}
