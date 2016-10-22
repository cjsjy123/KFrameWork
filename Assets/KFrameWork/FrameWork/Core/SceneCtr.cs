#define KDEBUG
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using KUtils;
using System.Reflection;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
using UnityEngine.SceneManagement;
#endif

namespace KFrameWork
{
    [SingleTon]
    public class SceneCtr:System.IDisposable
    {
        public static SceneCtr mIns;
        /// <summary>
        /// 属性辅助字典，0为实例化，1为销毁
        /// </summary>
        private static Dictionary<KEnum, Dictionary<int, List<FieldInfo>>> SceneOpDic;

        public static KEnum DefaultScene ;


        private bool _isLoadingLevel = false;
        public bool isLoadingLevel {
            get
            {
                return this._isLoadingLevel;
            }
            private set
            {
                _isLoadingLevel = value;
            }
        }

        private KEnum _curScene;
        private KEnum _nextScene;
        public KEnum CurScene{get {return this._curScene;}private set{this._curScene = value;}}
        public KEnum nextScene { get { return this._nextScene; } private set { this._nextScene = value; } }

        public void Dispose ()
        {
            if(SceneOpDic != null)
            {
                SceneOpDic.Clear();
                SceneOpDic = null;
            }

            DefaultScene= null;
        }

        public static void Register_SceneSingleton(KEnum e ,int destroyed,FieldInfo f)
        {
            if(SceneOpDic == null)
            {
                SceneOpDic = new Dictionary<KEnum, Dictionary<int, List<FieldInfo>>>();
            }

            if(!SceneOpDic.ContainsKey(e))
            {
                Dictionary<int, List<FieldInfo>> dic = new Dictionary<int, List<FieldInfo>>();
                SceneOpDic.Add(e,dic);
                dic.Add(destroyed,new List<FieldInfo>(){f});
            }
            else
            {
                if(!SceneOpDic[e].ContainsKey(destroyed))
                {
                    SceneOpDic[e].Add(destroyed,new List<FieldInfo>(){f});
                }
                else
                {
                    SceneOpDic[e][destroyed].Add(f);
                }
            }
            
        }

        public static void UnRegister_SceneSingleton(KEnum e ,byte destroyed,FieldInfo f)
        {
            if(SceneOpDic != null && SceneOpDic.ContainsKey(e) && SceneOpDic[e].ContainsKey(destroyed))
            {
                SceneOpDic[e][destroyed].Remove(f);
            }

        }

        public static void ClearSceneEvents()
        {
            if(SceneOpDic != null)
            {
                SceneOpDic.Clear();
                SceneOpDic = null;
            }
        }

        [SceneEnter]
        private static void EnterScene(int level)
        {
            SceneCtr.mIns.isLoadingLevel = false;
            SceneCtr.mIns.CurScene = SceneCtr.mIns.nextScene;
            SceneCtr.mIns.nextScene = null;

            LogMgr.LogFormat("场景进入 {0}", SceneCtr.mIns.CurScene);

            if(SceneOpDic != null && SceneOpDic.ContainsKey(SceneCtr.mIns.CurScene))
            {
                Dictionary<int, List<FieldInfo>> dic = SceneOpDic[SceneCtr.mIns.CurScene];
                if(dic.ContainsKey(0))
                {
                    List<FieldInfo> list = dic[0];
                    for(int i=0; i < list.Count;++i)
                    {
                        FieldInfo f = list[i];
                        #if KDEBUG
                        if(f.GetValue(null) == null  && f.IsStatic )
                        {
                            f.SetValue(null,Activator.CreateInstance(f.FieldType,true));
                        }
                        else
                        {
                            LogMgr.Log("已经实例化过一次或者为非静态字段 => "+f.Name);
                        }
                        #else
                        f.SetValue(null,Activator.CreateInstance(f.FieldType,,true)));

                        #endif
                        
                    }
                }
            }

        }

        [ScenLeave]
        private static void LeavedScene(int level)
        {
            LogMgr.LogFormat("场景离开 {0} ", SceneCtr.mIns.CurScene);
            if(SceneOpDic != null && SceneOpDic.ContainsKey(SceneCtr.mIns.CurScene))
            {
                Dictionary<int, List<FieldInfo>> dic = SceneOpDic[SceneCtr.mIns.CurScene];
                if(dic.ContainsKey(1))
                {
                    List<FieldInfo> list = dic[1];
                    for(int i=0; i < list.Count;++i)
                    {
                        FieldInfo f = list[i];
                        #if KDEBUG
                        if(f.GetValue(null) != null  && f.IsStatic )
                        {
                            f.SetValue(null,null);
                        }
                        else
                        {
                            LogMgr.Log("对象从未实例化过或者为非静态字段 => "+f.Name);
                        }
                        #else
                        f.SetValue(null,null);

                        #endif

                    }
                }
            }
        }

        public static void LoadScene(string name)
		{
            mIns.isLoadingLevel = true;
            mIns.nextScene = (KEnum)name;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
            SceneManager.LoadScene(name);
#else
            Application.LoadLevel(name);
#endif
            mIns.isLoadingLevel = false;
        }

        public static void LoadScene(int name)
		{
            mIns.isLoadingLevel = true;
            mIns.nextScene = (KEnum)name;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
            SceneManager.LoadScene(name);
#else
            Application.LoadLevel(name);
#endif
            mIns.isLoadingLevel = false;
        }

        public static void LoadScene(KEnum name)
        {
            mIns.isLoadingLevel = true;
            mIns.nextScene = name;
            #if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
            SceneManager.LoadScene((string)name);
            #else
            Application.LoadLevel(name);
            #endif
            mIns.isLoadingLevel = false;
        }

        public static void LoadLevelAddictive(int name)
        {
            mIns.isLoadingLevel = true;
            mIns.nextScene =  (KEnum)name;
            #if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
            SceneManager.LoadScene(name,LoadSceneMode.Additive);
            #else
            Application.LoadLevelAddictive(name);
            #endif
            mIns.isLoadingLevel = false;
        }

        public static void LoadLevelAddictive(string name)
        {
            mIns.isLoadingLevel = true;
            mIns.nextScene =  (KEnum)name;
            #if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
            SceneManager.LoadScene(name,LoadSceneMode.Additive);
            #else
            Application.LoadLevelAddictive(name);
            #endif
            mIns.isLoadingLevel = false;
        }

        public static void LoadLevelAddictive(KEnum name)
        {
            mIns.isLoadingLevel = true;
            mIns.nextScene = name;
            #if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
            SceneManager.LoadScene((string)name,LoadSceneMode.Additive);
            #else
            Application.LoadLevelAddictive(name);
            #endif
            mIns.isLoadingLevel = false;
        }

        public static AsyncOperation LoadSceneAsync(string name)
        {            
            mIns.isLoadingLevel = true;
            mIns.nextScene =  (KEnum)name;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6

            return SceneManager.LoadSceneAsync(name);
#else
            return Application.LoadLevelAsync(name);
#endif
		}

        public static AsyncOperation LoadSceneAsync(int name)
		{
            mIns.isLoadingLevel = true;
            mIns.nextScene =  (KEnum)name;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6

            return SceneManager.LoadSceneAsync(name);
#else
            return Application.LoadLevelAsync(name);
#endif
		}

        public static AsyncOperation LoadSceneAsync(KEnum name)
        {
            mIns.isLoadingLevel = true;
            mIns.nextScene = name;
            #if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6

            return SceneManager.LoadSceneAsync((string)name);
            #else
            return Application.LoadLevelAsync(name);
            #endif
        }

		public static bool UnLoadScene(string name)
		{
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6

			return SceneManager.UnloadScene(name);
#else
            return Application.UnloadLevel(name);
#endif
		}

		public static bool UnLoadScene(int name)
		{
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
			return SceneManager.UnloadScene(name);
#else
            return Application.UnloadLevel(name);
#endif
		}

#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
        public static Scene GetSceneByName(string name)
		{
			 return SceneManager.GetSceneByName(name);
		}

		public static Scene GetSceneByPath(string name)
		{

			return SceneManager.GetSceneByPath(name);
		}
#endif



    }

}
