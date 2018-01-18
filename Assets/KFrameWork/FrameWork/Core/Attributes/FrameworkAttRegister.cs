using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using KUtils;

namespace KFrameWork
{
    public sealed class FrameworkAttRegister :BaseAttributeRegister
    {
        private bool m_binit;
        public bool Inited
        {
            get
            {
                return this.m_binit;
            }
        }


        public FrameworkAttRegister()
        {
            
        }

        public static void DestroyStaticAttEvent(MainLoopEvent loopevent, Type tp, string methodname)
        {
            MethodInfo method = tp.GetMethod(methodname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method != null)
            {
                MainLoop.getInstance().UnRegisterStaticEvent(loopevent, method);
            }
            else
            {
                LogMgr.LogErrorFormat("cant found {0} in type :{1}", methodname,tp);
            }
        }

        public static void DestroyAttEvent(MainLoopEvent loopevent, Type tp, string methodname)
        {
            MethodInfo method = tp.GetMethod(methodname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method != null)
            {
                MainLoop.getInstance().UnRegisterLoopEvent(loopevent, method);
            }
            else
            {
                LogMgr.LogErrorFormat("cant found {0} in type :{1}", methodname,tp);
            }
        }

        public void LoadFromCache(RegisterCachedTypes types)
        {
            try
            {
                AttributeRegister.Register(this);
                this.StartFromCache(types);
                this.End();
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
            }
        }

        public void LoadFromCache(string path)
        {
            try
            {
                AttributeRegister.Register(this);
                this.StartFromCache(path);
                this.End();
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
            }
        }
#if UNITY_EDITOR
        public void SaveToCache(string path)
        {
            try
            {
                AttributeRegister.Register(this);
                this.StartToCache(this.GetType(), path);
                this.End();
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
            }
        }
#endif

        public void Initialite()
        {
            try
            {
                AttributeRegister.Register(this);
                this.Start(this.GetType());
                this.End();
                this.m_binit = true;
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
                this.m_binit =false;
            }

        }

        public void InitFrameWorkModuldes(MainLoop loop)
        {
            
            if(loop.OpenFps)
                loop.gameObject.AddComponent<FPS>();

            GameSyncCtr.mIns.StartSync();

            if (loop.OpenLua)
            {
#if TOLUA
                new LuaResLoader();
                loop.gameObject.AddComponent(typeof(GameLuaClient));
#endif
            }

        }

    }
}

