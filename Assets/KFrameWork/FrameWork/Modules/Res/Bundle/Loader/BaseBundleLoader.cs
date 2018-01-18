
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KFrameWork
{

    public abstract class BaseBundleLoader : PoolCls<BaseBundleLoader>, IPool, IDisposable
    {
        private static uint Counter = 0;
        protected bool isABMode
        {
            get
            {
#if UNITY_EDITOR 
                if (Application.isPlaying && MainLoop.getInstance().OpenABMode)
                    return true;
                return false;
#elif UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID 
                return true;
#endif
            }
        }

        protected bool isSceneLoad
        {
            get
            {
                return this is SceneAsyncLoader || this is SceneSyncLoader;
            }
        }

        public uint UID { get; private set; }

        protected string LoadResPath;

        protected BundlePkgInfo loadingpkg;

        protected UnityEngine.Object _BundleMainObject;

        protected IBundleRef _BundleRef;

        protected SceneOperation _AsyncOpation;

        protected GameObject PreParedGameObject;

        protected GameObject LoaderInsObject;
#if UNITY_EDITOR
        private Stopwatch _timer = new Stopwatch();
#endif
        private BundleLoadState _state;

        public BundleLoadState LoadState
        {
            get
            {
                return this._state;
            }
            protected set
            {
                BundleLoadState old = this._state;
                this._state = value;
                if (old == BundleLoadState.Prepared && value == BundleLoadState.Running)
                {
#if UNITY_EDITOR
                    if (FrameWorkConfig.Open_DEBUG)
                    {
                        _timer.Reset();
                        _timer.Start();
                    }

#endif
                    this.OnStart();
                }

                if (old == BundleLoadState.Running && value == BundleLoadState.Paused)
                    this.OnPaused();

                if (old == BundleLoadState.Paused && value == BundleLoadState.Running)
                    this.OnResume();

                if (old == BundleLoadState.Running && value == BundleLoadState.Finished)
                {
#if UNITY_EDITOR
                    string filename = loadingpkg != null ? this.loadingpkg.BundleName : "";
#endif
                    this.OnFinish();
                    this.OnProgressHandler = null;
#if UNITY_EDITOR
                    if (FrameWorkConfig.Open_DEBUG)
                    {
                        _timer.Stop();
                        LogMgr.LogFormat("{0} cost {1}", filename, _timer.Elapsed.TotalSeconds.ToString());
                        if (_timer.Elapsed.TotalSeconds > 0.5f)
                        {
                            LogMgr.LogError("cost large");
                        }
                    }

#endif
                }

                if (old != BundleLoadState.Error && value == BundleLoadState.Error)
                {
#if UNITY_EDITOR
                    string filename = loadingpkg != null ? this.loadingpkg.BundleName : "";
#endif
                    this.OnError();
                    this.OnProgressHandler = null;

#if UNITY_EDITOR
                    if (FrameWorkConfig.Open_DEBUG)
                    {
                        _timer.Stop();
                        LogMgr.LogFormat("{0} cost {1}", filename, _timer.Elapsed.TotalSeconds.ToString());
                        if (_timer.Elapsed.TotalSeconds > 0.5f)
                        {
                            LogMgr.LogError("cost large");
                        }
                    }
#endif
                }
            }
        }

        public Action<bool, AssetBundleResult> onComplete;

        public Action<BaseBundleLoader, int, int> OnProgressHandler;

        #region events
        protected abstract void OnStart();
        /// <summary>
        /// 通常在异步加载中
        /// </summary>
        protected abstract void OnPaused();

        protected abstract void OnResume();

        protected virtual void OnError()
        {
            this.InvokeHandler(false);
        }

        protected virtual void OnFinish()
        {
            if (LoaderInsObject == null && this.PreParedGameObject != null)
            {
                if (this._BundleMainObject is GameObject)
                {
                    GameObject gameobject = GameObject.Instantiate(this._BundleMainObject) as GameObject;
                    this.LoaderInsObject = PreParedGameObject.AddInstance(gameobject);
                }
                else
                {
                    LogMgr.LogErrorFormat("{0} 类型错误非gameobject ", _BundleMainObject);
                }
            }

            this.InvokeHandler(true);
        }

        #endregion

        public BaseBundleLoader()
        {
            this.UID = ++Counter;
        }

        #region interface
        public virtual void Load(string name)
        {
            this.LoadResPath = name;
            if (this.LoadState == BundleLoadState.Prepared)
            {
                this.LoadState = BundleLoadState.Running;
            }
        }

        public virtual void Load(string name, GameObject Parent)
        {
            this.PreParedGameObject = Parent;
            this.Load(name);
        }

        public virtual void DownLoad(string url)
        {
            if (this.LoadState == BundleLoadState.Prepared)
            {
                this.LoadState = BundleLoadState.Running;
            }
        }

        public virtual void Pause()
        {
            if (this.LoadState != BundleLoadState.Paused
                && this.LoadState != BundleLoadState.Finished
                && this.LoadState != BundleLoadState.Error
                && this.LoadState != BundleLoadState.Stopped)
            {
                this.LoadState = BundleLoadState.Paused;
            }
        }

        public virtual void Stop()
        {
            if (this.LoadState != BundleLoadState.Finished && this.LoadState != BundleLoadState.Error
                && this.LoadState != BundleLoadState.Stopped)
            {
                this.LoadState = BundleLoadState.Stopped;
            }
        }

        public virtual void Resume()
        {
            if (this.LoadState != BundleLoadState.Error && this.LoadState != BundleLoadState.Stopped
                && this.LoadState != BundleLoadState.Finished)
            {
                this.LoadState = BundleLoadState.Running;
            }
        }

        public AssetBundleResult GetABResult()
        {
            AssetBundleResult result = new AssetBundleResult(this._BundleRef, this._BundleMainObject, this.PreParedGameObject, this.LoaderInsObject, this.LoadResPath);
            result.SceneAsyncResult = this._AsyncOpation as SceneOperation;
            return result;
        }

        public virtual void Dispose()
        {

            if (KObjectPool.mIns != null)
            {
                KObjectPool.mIns.Push(this);
            }
        }

        #endregion

        #region POOL

        public static T CreateLoader<T>() where T : BaseBundleLoader, new()
        {
            T t = TrySpawn<T>();
            if (t != null)
                t.UID = ++Counter;
            return t;
        }

        public virtual void RemoveToPool()
        {
            this._BundleMainObject = null;
            this._BundleRef = null;
            this.loadingpkg = null;
            this.LoadResPath = null;
            this.LoadState = BundleLoadState.Prepared;
            this.onComplete = null;
            this.LoaderInsObject = null;
            this.PreParedGameObject = null;
            this._AsyncOpation = null;
        }

        #endregion

        protected void InvokeHandler(bool bvalue)
        {
            ResBundleMgr.mIns.Cache.RemoveLoading(this.LoadResPath);

            if (this.onComplete != null)
            {
                this.onComplete(bvalue, GetABResult());
            }
        }

        protected void InvokeProgressHandler(int current, int total)
        {
            //LogMgr.LogError("cur "+ current+" t "+ total +" res "+ LoadResPath);
            if (this.OnProgressHandler != null)
            {
                this.OnProgressHandler(this, current, total);
            }
        }

        protected bool isRunning()
        {
            return this.LoadState == BundleLoadState.Running;
        }

        protected bool isPaused()
        {
            return this.LoadState == BundleLoadState.Paused;
        }

        protected BundlePkgInfo _SeekPkgInfo(string name)
        {
            BundlePkgInfo pkginfo = ResBundleMgr.mIns.BundleInformation.SeekInfo(name);

            if (pkginfo == null)
            {
                this.LoadState = BundleLoadState.Error;
                throw new FrameWorkException(string.Format("Not Found {0}", name), ExceptionType.Higher_Excetpion);
            }

            return pkginfo;
        }

        protected AssetBundle LoadAssetBundle(string path)
        {
            if (MainLoop.getInstance().OpenABMode)
            {
#if UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID
                AssetBundle ab = KAssetBundle.LoadFromFile(path);
                return ab;
#else 
                return null;
#endif
            }
            else
            {
                return null;
            }
        }

        protected IBundleRef LoadFullAssetToMem(BundlePkgInfo pkginfo)
        {
            IBundleRef bundle = null;
            if (ResBundleMgr.mIns.Cache.Contains(pkginfo))
            {
                bundle = ResBundleMgr.mIns.Cache.TryGetValue(pkginfo);
                return bundle;
            }

#if UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID 
            if (GameApplication.isPlaying && MainLoop.getInstance().OpenABMode)
            {
                AssetBundle ab = null;
                using (gstring.Block())
                {
                    ab = this.LoadAssetBundle(BundlePathConvert.GetRunningPath(pkginfo.AbFileName));
                }

                if (ab == null)
                    this.ThrowAssetMissing(this.LoadResPath);

                bundle = ResBundleMgr.mIns.Cache.PushAsset(pkginfo, ab);
                return bundle;
            }
            else
#endif
            {
                UnityEngine.Object target = AssetDatabaseLoad(pkginfo.EditorPath);
                if (target == null)
                {
                    if (BundleConfig.SAFE_MODE)
                    {
                        this.ThrowAssetMissing(pkginfo.BundleName);
                    }
                    else
                    {
                        this.LoadState = BundleLoadState.Error;
                        LogMgr.LogErrorFormat("Asset {0} Missing", pkginfo.BundleName);
                    }
                }
                else
                {
#if UNITY_EDITOR
                    bundle = ResBundleMgr.mIns.Cache.PushEditorAsset(pkginfo, target);
#endif
                }
                return bundle;
            }
        }

        private UnityEngine.Object AssetDatabaseLoad(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
#else
            return null;
#endif
        }

        protected AssetBundleCreateRequest LoadFullAssetToMemAsync(BundlePkgInfo pkginfo)
        {
            if (Application.isPlaying && MainLoop.getInstance())
            {
                AssetBundleCreateRequest abRequst = null;
                using (gstring.Block())
                {
                    abRequst = KAssetBundle.LoadFromFileAsync(BundlePathConvert.GetRunningPath(pkginfo.AbFileName));
                }

                return abRequst;
            }
            else
            {
                return null;
            }
        }

        protected bool CreateFromAsset(IBundleRef bundle, out UnityEngine.Object asset)
        {
            if (isSceneLoad)
            {
                asset = null;
                return true;
            }

            bool ret = bundle.LoadAsset(out asset);
            if (!ret)
            {
                this.ThrowAssetMissing(this.LoadResPath);
            }

            return ret;
        }

        protected void AddDepends(BundlePkgInfo pkginfo, IBundleRef bundle)
        {
            if (!isABMode)
                return;

            if (bundle != null)
            {
                for (int i = 0; i < pkginfo.Depends.Length; ++i)
                {
                    IBundleRef depbund = ResBundleMgr.mIns.Cache.TryGetValue(pkginfo.Depends[i]);
                    if (depbund != null)
                    {
                        bundle.NeedThis(depbund);
                    }
                    else
                    {
                        this.ThrowBundleMissing(pkginfo.Depends[i]);
                    }
                }
            }
            else
            {
                this.ThrowBundleMissing(pkginfo.BundleName);
            }
        }

        protected void ThrowLogicError(string info = null)
        {
            ResBundleMgr.mIns.Cache.RemoveLoading(this.LoadResPath);
            this.LoadState = BundleLoadState.Error;
            throw new FrameWorkException(string.Format("{0} 逻辑异常", info), ExceptionType.Higher_Excetpion);
        }

        protected void ThrowBundleMissing(string resname)
        {
            ResBundleMgr.mIns.Cache.RemoveLoading(resname);
            ResBundleMgr.mIns.Cache.RemoveLoading(this.LoadResPath);
            if (BundleConfig.SAFE_MODE)
                throw new FrameWorkResMissingException(string.Format("Bundle {0} Missing", resname));
            else
            {
                this.LoadState = BundleLoadState.Error;
                LogMgr.LogErrorFormat("Bundle {0} Missing", resname);
            }
        }

        protected void ThrowAssetMissing(string resname)
        {
            ResBundleMgr.mIns.Cache.RemoveLoading(resname);
            ResBundleMgr.mIns.Cache.RemoveLoading(this.LoadResPath);
            if (BundleConfig.SAFE_MODE)
                throw new FrameWorkResMissingException(string.Format("Asset =》{0} Missing", resname));
            else
            {
                this.LoadState = BundleLoadState.Error;
                LogMgr.LogErrorFormat("Asset =》{0} Missing", resname);
            }
        }
    }

}
