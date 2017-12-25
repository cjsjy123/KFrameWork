using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using System.Reflection;
using System;

namespace KFrameWork
{
    public class GameDebugger : MonoBehaviour
    {
        public GameDebugger mIns;
        private static List<FieldInfo> references = new List<FieldInfo>();
        public bool openLogDir = false;
        //最多显示多少条日志到屏幕
        public int showLogSize = 100;
        //设置过滤显示到屏幕的关键字，多个关键字用 | 隔开
        public string filterString = "";
        //存储显示到屏幕上的日志
        private List<string> logList;

        private bool showFull = false;
        private Vector2 scrollpos;

        private bool ShowLog = false;
        void Awake()
        {
            mIns = this;

            Type[] types =  typeof(MainLoop).Assembly.GetTypes();

            for (int i = 0; i < types.Length; ++i)
            {
                Type type = types[i];
                if (type == typeof(GameDebugger) || type.IsEnum)
                    continue;
                FieldInfo[] fs = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public| BindingFlags.DeclaredOnly);
                for (int j = 0; j < fs.Length; ++j)
                {
                    FieldInfo f = fs[j];
                    if(f.FieldType.IsClass && !f.IsLiteral && !f.IsInitOnly)
                    {
                        references.Add(f);
                    }
                }

            }
        }

        void Start()
        {
            logList = new List<string>();
            Application.logMessageReceived += LogCallBack;
            Application.logMessageReceivedThreaded += LogCallBack;
        }

        void LogCallBack(string logString, string stackTrace, LogType type)
        {
            //设置过滤条件，将指定类型、包含某些字符串的日志保存到屏幕日志窗器中
            bool show = false;
            //置过滤条件:指定类型
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning)
            {
                show = true;
            }
            //置过滤条件:包含指定关键字，多个关键字用 | 隔开
            foreach (string str in filterString.Split('|'))
            {
                if (logString.Contains(str))
                {
                    show = true;
                    break;
                }
            }
            if (show)
            {
                logList.Add(logString);
                if (logList.Count > showLogSize)
                {
                    logList.RemoveAt(0);
                }
            }
        }

        void OnGUI()
        {
            if (ShowLog)
            {
                if (!showFull)
                    GUILayout.BeginArea(new Rect(0, 100, 300, Screen.height - 120));
                else
                    GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

                scrollpos = GUILayout.BeginScrollView(scrollpos);
                for (int i = 0; i < logList.Count; ++i)
                {
                    if (!showFull)
                        GUILayout.Label(logList[i]);
                    else
                        GUILayout.Label(logList[i], GUILayout.Width(Screen.width - 20));
                }
                GUILayout.EndScrollView();

                GUILayout.EndArea();
            }

            if (GUILayout.Button("Dump Bundle"))
            {
                ResBundleMgr.mIns.Cache.LogDebugInfo();
            }

            if (GUILayout.Button("Dump cmd"))
            {
                CacheCommand.LogInfo();
            }

            if (GUILayout.Button("showFull"))
            {
                showFull = !showFull;
            }

            if (GUILayout.Button("Show Log"))
            {
                this.ShowLog = !this.ShowLog;
            }
        }

        //void OnDestroy()
        //{

        //    try
        //    {
        //        LogMgr.Log("---------static references --------- checker");

        //        int counter = 0;

        //        for (int i = 0; i < references.Count; ++i)
        //        {
        //            FieldInfo field = references[i];
        //            if (field != null && !field.DeclaringType.ContainsGenericParameters)
        //            {
        //                System.Object o = field.GetValue(null);
        //                if (o != null)
        //                {
        //                    LogMgr.LogFormat("static refence not clear :{0}>>>>> From Type :{1} >>>>>>fieldType:{2}>>>>name:{3}", o, field.DeclaringType, field.FieldType,field.Name);
        //                    counter++;
        //                }
        //            }
        //        }

        //        LogMgr.LogFormat("--------------Not Clear Count is :{0} -------------", counter);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogMgr.LogError(ex);
        //    }

        //}

    }
}


