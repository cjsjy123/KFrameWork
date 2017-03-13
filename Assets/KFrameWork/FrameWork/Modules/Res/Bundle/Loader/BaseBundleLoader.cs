
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KFrameWork
{

    public abstract class BaseBundleLoader : PoolCls<BaseBundleLoader>, IPool, IDisposable
    {
        private static uint Counter =0;
        protected bool isABMode
        {
            get
            {
#if UNITY_EDITOR && !AB_DEBUG
                return false;
#elif UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || AB_DEBUG
                return true;
#endif
            }
        }

        protected bool isSceneLoad
        {
            get
            {
                return this is SceneAsyncLoader;
            }
        }

        public uint UID { get; private set; }

        protected string LoadResPath;

        protected BundlePkgInfo loadingpkg;

        protected UnityEngine.Object _Bundle;

        protected IBundleRef _BundleRef;

        private GameObject HadInstance;

        protected AsyncOperation _AsyncOpation;

        public GameObject PreParedGameObject;

        protected GameObject LoaderInsObject;

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
                    this.OnStart();

                if (old == BundleLoadState.Running && value == BundleLoadState.Paused)
                    this.OnPaused();

                if (old == BundleLoadState.Paused && value == BundleLoadState.Running)
                    this.OnResume();

                if (old == BundleLoadState.Running && value == BundleLoadState.Finished)
                {
                    this.OnFinish();
                    this.OnProgressHandler = null;
                }

                if (old != BundleLoadState.Error && value == BundleLoadState.Error)
                {
                    this.OnError();
                    this.OnProgressHandler = null;
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
            if(HadInstance == null && this.PreParedGameObject != null)
            {
                if (this._Bundle is GameObject)
                {
                    GameObject gameobject = GameObject.Instantiate( this._Bundle) as GameObject;
                    this.HadInstance = PreParedGameObject.AddInstance(gameobject);
                }
                else
                {
                    LogMgr.LogErrorFormat("{0} 类型错误非gameobject ", _Bundle);
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
            AssetBundleResult result = new AssetBundleResult(this._BundleRef,this._Bundle,this.PreParedGameObject,this.HadInstance,this.LoadResPath);
            result.SceneAsyncResult = this._AsyncOpation;
            return result;
        }

        public virtual void Dispose()
        {

            if (KObjectPool.mIns != null)
            {
                KObjectPool.mIns.Push(this);
            }
        }

        //protected virtual void PreparedTryIns()
        //{
        //    if (this.PreParedGameObject != null)
        //    {
        //        UnityEngine.Object res = null;

        //        if (this._BundleRef.Instantiate(out res))
        //        {
        //            if (res is GameObject)
        //            {
        //                this.LoaderInsObject = res as GameObject;
        //                this.PreParedGameObject.AddInstance(this.LoaderInsObject);
        //            }
        //            else
        //            {
        //                if (BundleConfig.SAFE_MODE)
        //                    throw new FrameWorkResNotMatchException(string.Format("{0} Type Is Not Gameobject", LoadResPath));
        //                else
        //                    LogMgr.LogErrorFormat("Not Gameobject cant be instanced as gameobject :{0}",LoadResPath);
        //            }
        //        }
        //    }
        //}

#endregion

#region POOL

        public static T CreateLoader<T>() where T :BaseBundleLoader,new()
        {
            T t = TrySpawn<T>();
            if(t != null)
                t.UID = ++Counter;
            return t;
        }

        public virtual void RemoveToPool()
        {
            this._Bundle = null;
            this._BundleRef = null;
            this.loadingpkg =null;
            this.LoadResPath = null;
            this.LoadState = BundleLoadState.Prepared;
            this.onComplete = null;
            this.LoaderInsObject = null;
            this.PreParedGameObject = null;
        }

        public virtual void RemovedFromPool()
        {
            this._Bundle = null;
            this._BundleRef = null;
            this.LoadResPath = null;
            this.LoadState = BundleLoadState.Prepared;
            this.onComplete = null;
            this.LoaderInsObject = null;
            this.PreParedGameObject = null;
        }

#endregion

        protected void InvokeHandler(bool bvalue)
        {
            if (ResBundleMgr.mIns.Cache.ContainsLoading(this.LoadResPath))
            {
                ResBundleMgr.mIns.Cache.RemoveLoading(this.LoadResPath);
            }

            if (this.onComplete != null)
            {
                this.onComplete(bvalue,GetABResult());
            }
        }

        protected void InvokeProgressHandler( int current,int total)
        {
            //LogMgr.LogError("cur "+ current+" t "+ total +" res "+ LoadResPath);
            if (this.OnProgressHandler != null)
            {
                this.OnProgressHandler(this,current, total);
            }
        }

        protected bool isRunning()
        {
            return this.LoadState == BundleLoadState.Running ;
        }

        protected bool isPaused()
        {
            return  this.LoadState == BundleLoadState.Paused;
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
#if UNITY_EDITOR && !AB_DEBUG
            return null;
#elif UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || AB_DEBUG
            AssetBundle ab = KAssetBundle.LoadFromFile(path);
            return ab;
#else
            return null;
#endif
        }

        protected IBundleRef LoadFullAssetToMem(BundlePkgInfo pkginfo)
        {
            IBundleRef bundle = null;
            if (ResBundleMgr.mIns.Cache.Contains(pkginfo))
            {
                bundle = ResBundleMgr.mIns.Cache.TryGetValue(pkginfo);
                return bundle;
            }

#if UNITY_EDITOR && !AB_DEBUG
            //BundlePkgInfo info = BundleMgr.mIns.BundleInforMation.SeekInfo(pkginfo.BundleName);
            UnityEngine.Object target = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(pkginfo.EditorPath);
            if (target == null)
            {
                if (BundleConfig.SAFE_MODE)
                    throw new FrameWorkResMissingException(string.Format("Asset {0} Missing", pkginfo.BundleName));
                else
                {
                    this.LoadState = BundleLoadState.Error;
                    LogMgr.LogErrorFormat("Asset {0} Missing", pkginfo.BundleName);
                }
            }
            else
            {
                bundle = ResBundleMgr.mIns.Cache.PushEditorAsset(pkginfo, target);
            }
            return bundle;


#elif UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || AB_DEBUG
            AssetBundle ab = null;
            using (gstring.Block())
            {
                ab = this.LoadAssetBundle(BundlePathConvert.GetRunningPath(pkginfo.AbFileName));
            }

            if (ab == null)
                this.ThrowAssetMissing(this.LoadResPath);

            bundle = ResBundleMgr.mIns.Cache.PushAsset(pkginfo, ab);
            return bundle;
#endif
        }

        protected AssetBundleCreateRequest LoadFullAssetToMemAsync(BundlePkgInfo pkginfo)
        {
#if UNITY_EDITOR && !AB_DEBUG
            return null;
#elif UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || AB_DEBUG
            AssetBundleCreateRequest abRequst = null;
            using (gstring.Block())
            {
                abRequst = KAssetBundle.LoadFromFileAsync(BundlePathConvert.GetRunningPath(pkginfo.AbFileName));
            }

            return abRequst;
#endif
        }

        protected bool CreateFromAsset(IBundleRef bundle, out UnityEngine.Object asset)
        {
            bool ret = bundle.LoadAsset(out asset);
            if (!ret)
            {
                this.ThrowAssetMissing(this.LoadResPath);
            }

            return ret;
        }

        protected void AppendDepends(BundlePkgInfo pkginfo, IBundleRef bundle)
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

        protected void ThrowLogicError()
        {
            this.LoadState = BundleLoadState.Error;
            throw new FrameWorkException("逻辑异常", ExceptionType.Higher_Excetpion);
        }

        protected void ThrowBundleMissing(string resname)
        {
            //BundlePkgInfo info = this._SeekPkgInfo(resname);
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
            if (BundleConfig.SAFE_MODE)
                throw new FrameWorkResMissingException(string.Format("Asset {0} Missing", resname));
            else
            {
                this.LoadState = BundleLoadState.Error;
                LogMgr.LogErrorFormat("Asset {0} Missing", resname);
            }
        }
    }

}
