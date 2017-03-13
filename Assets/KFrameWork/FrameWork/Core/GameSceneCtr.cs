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

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace KFrameWork
{
    public struct GameSceneInfo : IEquatable<GameSceneInfo>
    {
        public int buildIndex;
        public string name;

        private bool _isValid;

        public void SetValid()
        {
            this._isValid = true;
        }

        public bool IsValid()
        {
            return _isValid;
        }

        public bool Equals(GameSceneInfo other)
        {
            if (this.buildIndex != other.buildIndex) return false;
            if (this.name != other.name) return false;
            if (this._isValid != other._isValid) return false;

            return true;
        }
    }

    [SingleTon]
    public sealed class GameSceneCtr:IDisposable
    {
        public static GameSceneCtr mIns;

        /// <summary>
        /// 属性辅助字典，0为实例化，1为销毁
        /// </summary>
        private static Dictionary<int, Dictionary<int, List<FieldInfo>>> SceneOpDic;
        /// <summary>
        /// 低频数据，采用低内存做法
        /// </summary>
        private List<GameSceneInfo> SceneList;

        public GameSceneInfo DefaultScene ;

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

        private GameSceneInfo _curScene;
        private GameSceneInfo _nextScene;
        public GameSceneInfo CurScene {get {return this._curScene;}private set{this._curScene = value;}}
        public GameSceneInfo nextScene { get { return this._nextScene; } private set { this._nextScene = value; } }

        private bool Inited = false;

        public GameSceneInfo this[int sceneIdx]
        {
            get
            {
                GameSceneInfo s;
                this.TrygetValue(sceneIdx, out s);
                return s;
            }
        }

        private GameSceneCtr()
        {
            if (MainLoop.getLoop() != null)
            {
                if (MainLoop.getLoop().HasInit)
                {
                    this.Init();
                }
                else
                {
                    MainLoop.getLoop().FrameWorkEvent += Init;
                }
            }
        }

        private void Init ()
        {
            if (Inited)
                return;

            #if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
            this.SceneList = new List<GameSceneInfo>((int)AutoScenes.END);

            for (int i = 0; i < (int)AutoScenes.END; ++i)
            {
                GameSceneInfo scene = new GameSceneInfo();
                scene.buildIndex = i;
                scene.name = ((AutoScenes)i).ToString();
                scene.SetValid();
                this.SceneList.Add(scene);
            }

            if (this.SceneList.Count > 0)
            {
            #if UNITY_EDITOR
                GameSceneInfo scene = this.SceneList.Find(p=> p.buildIndex ==EditorSceneManager.GetActiveScene().buildIndex);
                if (!scene.IsValid() || scene.name.Equals("END"))
                {
                    LogMgr.LogError("场景配置异常");
                    scene = new GameSceneInfo();
                    Scene current = EditorSceneManager.GetActiveScene();
                    scene.buildIndex = current.buildIndex;
                    scene.name = current.name;
                    scene.SetValid();
                }
                this.DefaultScene = scene;
                this.CurScene = this.DefaultScene;
            #else
            this.DefaultScene = this.SceneList[0];
            this.CurScene = this.DefaultScene;
            #endif
            }
#else
            this.SceneList = new List<GameSceneInfo>();

            for (int i = 0; i < AutoScenes.END; ++i)
            {
            GameSceneInfo scene = new GameSceneInfo();
            scene.buildIndex = i;
            scene.name = ((AutoScenes)i).ToString();
            scene.SetValid();
            this.SceneList.Add(scene);
            }

            if (this.SceneList.Count > 0)
            {
#if UNITY_EDITOR
            GameSceneInfo scene = this.SceneList.Find(p=> p.name ==EditorApplication.currentScene);
            this.DefaultScene = scene;
            this.CurScene = this.DefaultScene;
#else
            this.DefaultScene = this.SceneList[0];
            this.CurScene = this.DefaultScene;
#endif
            }
#endif

            Inited = true;
        }


        private bool TrygetValue(int id,out GameSceneInfo s)
        {
            if (this.SceneList == null)
                Init();

            for (int i = 0; i < this.SceneList.Count; ++i)
            {
                GameSceneInfo scene = this.SceneList[i];
                if (scene.IsValid() && scene.buildIndex == id)
                {
                    s = scene;
                    return true;
                }
            }

            if (!FrameWorkConfig.Open_DEBUG)
                throw new FrameWorkResNotMatchException(string.Format( "不存在此场景 ：{0}",id));

            s = default(GameSceneInfo);
            return false;
        }

        private bool TrygetValue(string name, out GameSceneInfo s)
        {
            if (this.SceneList == null)
                Init();

            for (int i = 0; i < this.SceneList.Count; ++i)
            {
                GameSceneInfo scene = this.SceneList[i];
#if UNITY_EDITOR
                if (scene.IsValid() && scene.name.IgnoreUpOrlower(name))
                {
                    s = scene;
                    return true;
                }
#else
                if (scene.IsValid() && scene.name == name)
                {
                    s = scene;
                    return true;
                }
#endif
            }

            if (!FrameWorkConfig.Open_DEBUG)
                throw new FrameWorkResNotMatchException(string.Format("不存在此场景 ：{0}", name));

            s = default(GameSceneInfo);
            return false;
        }

        public static void Register_SceneSingleton(int e ,int destroyed,FieldInfo f)
        {
            if(SceneOpDic == null)
            {
                SceneOpDic = new Dictionary<int, Dictionary<int, List<FieldInfo>>>();
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

        public void Destroy()
        {
            var en= SceneOpDic.GetEnumerator();
            while (en.MoveNext())
            {
                var valueEn = en.Current.Value.GetEnumerator();
                while (valueEn.MoveNext())
                {
                    List<FieldInfo> fs = valueEn.Current.Value;
                    for (int i = 0; i < fs.Count; ++i)
                    {
                        fs[i].SetValue(null, null);
                    }
                }
            }
            SceneOpDic.Clear();
            SceneOpDic = null;

            FrameworkAttRegister.DestroyStaticAttEvent(MainLoopEvent.OnLevelWasLoaded, typeof(GameSceneCtr), "EnterScene");
            FrameworkAttRegister.DestroyStaticAttEvent(MainLoopEvent.OnLevelLeaved, typeof(GameSceneCtr), "LeavedScene");
        }

        [SceneEnter(-10)]
        private static void EnterScene(int level)
        {
            try
            {
                if (mIns.SceneList == null)
                    mIns.Init();

                GameSceneCtr.mIns.isLoadingLevel = false;
                if (GameSceneCtr.mIns.nextScene.IsValid() == false)
                {
                    GameSceneCtr.mIns.CurScene = GameSceneCtr.mIns.DefaultScene;
                }
                else
                {
                    GameSceneCtr.mIns.CurScene = GameSceneCtr.mIns.nextScene;
                }

                LogMgr.LogFormat("场景进入 {0}", GameSceneCtr.mIns.CurScene.name);

                if (SceneOpDic != null && SceneOpDic.ContainsKey(GameSceneCtr.mIns.CurScene.buildIndex))
                {
                    Dictionary<int, List<FieldInfo>> dic = SceneOpDic[GameSceneCtr.mIns.CurScene.buildIndex];
                    if (dic.ContainsKey(0))
                    {
                        List<FieldInfo> list = dic[0];
                        for (int i = 0; i < list.Count; ++i)
                        {
                            FieldInfo f = list[i];
#if KDEBUG
                            if (f.GetValue(null) == null && f.IsStatic)
                            {
                                f.SetValue(null, Activator.CreateInstance(f.FieldType, true));
                            }
                            else
                            {
                                LogMgr.Log("已经实例化过一次或者为非静态字段 => " + f.Name);
                            }
#else
                        f.SetValue(null,Activator.CreateInstance(f.FieldType,,true)));

#endif
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
            }
        }

        [SceneLeave(-10)]
        private static void LeavedScene(int level)
        {
            try
            {
                if (!GameSceneCtr.mIns.CurScene.IsValid())
                    return;

                LogMgr.LogFormat("场景离开 {0} ", GameSceneCtr.mIns.CurScene.name);
                if (SceneOpDic != null && SceneOpDic.ContainsKey(GameSceneCtr.mIns.CurScene.buildIndex))
                {
                    Dictionary<int, List<FieldInfo>> dic = SceneOpDic[GameSceneCtr.mIns.CurScene.buildIndex];
                    if (dic.ContainsKey(1))
                    {
                        List<FieldInfo> list = dic[1];
                        for (int i = 0; i < list.Count; ++i)
                        {
                            FieldInfo f = list[i];
#if KDEBUG
                            if (f.GetValue(null) != null && f.IsStatic)
                            {
                                f.SetValue(null, null);
                            }
                            else
                            {
                                LogMgr.Log("对象从未实例化过或者为非静态字段 => " + f.Name);
                            }
#else
                        f.SetValue(null,null);
#endif

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
            }

        }

        public static void LoadScene(string name)
		{
            mIns.isLoadingLevel = true;
            GameSceneInfo s;
            mIns.TrygetValue(name, out s);
            mIns.nextScene = s;
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
            GameSceneInfo s;
            mIns.TrygetValue(name, out s);
            mIns.nextScene = s;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
            SceneManager.LoadScene(name);
#else
            Application.LoadLevel(name);
#endif
            mIns.isLoadingLevel = false;
        }



        public static void LoadLevelAddictive(int name)
        {
            mIns.isLoadingLevel = true;
            GameSceneInfo s;
            mIns.TrygetValue(name, out s);
            mIns.nextScene = s;
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
            GameSceneInfo s;
            mIns.TrygetValue(name, out s);
            mIns.nextScene = s;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6
            SceneManager.LoadScene(name,LoadSceneMode.Additive);
#else
            Application.LoadLevelAddictive(name);
#endif
            mIns.isLoadingLevel = false;
        }

        public static AsyncOperation LoadSceneAsync(string name)
        {            
            mIns.isLoadingLevel = true;
            GameSceneInfo s;
            mIns.TrygetValue(name, out s);
            mIns.nextScene = s;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6

            return SceneManager.LoadSceneAsync(name);
#else
            return Application.LoadLevelAsync(name);
#endif
		}

        public static AsyncOperation LoadSceneAsync(int name)
		{
            mIns.isLoadingLevel = true;
            GameSceneInfo s;
            mIns.TrygetValue(name, out s);
            mIns.nextScene = s;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6

            return SceneManager.LoadSceneAsync(name);
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

		public static Scene GetSceneByPath(string path)
		{

			return SceneManager.GetSceneByPath(path);
		}
#endif

        public void Dispose()
        {
            if (SceneOpDic != null)
            {
                SceneOpDic.Clear();
                SceneOpDic = null;
            }

        }

    }

}
