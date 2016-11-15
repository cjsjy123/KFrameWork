#define AB_DEBUG

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

    public abstract class BaseBundleLoader:IPool
    {

        protected string targetname;

        protected UnityEngine.Object _Bundle;

        protected IBundleRef _BundleRef;

        public GameObject PreParedGameObject;

        private GameObject LoaderInsObject;

        private BundleLoadState _state;

        public BundleLoadState LoadState
        {
            get
            {
                return this._state;
            }
            protected set
            {
                if (this._state == BundleLoadState.Prepared && value == BundleLoadState.Running)
                    this.OnStart();

                if (this._state == BundleLoadState.Running && value == BundleLoadState.Paused)
                    this.OnPaused();

                if (this._state == BundleLoadState.Paused && value == BundleLoadState.Running)
                    this.OnResume();

                if (this._state == BundleLoadState.Running && value == BundleLoadState.Finished)
                    this.OnFinish();

                if (this._state != BundleLoadState.Error && value == BundleLoadState.Error)
                    this.OnError();

                this._state = value;
            }
        }


        public Action<bool, AssetBundleResult> onComplete;

        

        #region events
        public abstract void OnStart();
        /// <summary>
        /// 通常在异步加载中
        /// </summary>
        public abstract void OnPaused();

        public abstract void OnResume();

        public virtual void OnError()
        {
            this.InvokeHandler(false, default(AssetBundleResult));

        }

        public virtual void OnFinish()
        {
            if (this.LoadState != BundleLoadState.Error && this.LoadState  != BundleLoadState.Finished
                && this.LoadState != BundleLoadState.Paused)
            {
                this.PreparedTryIns();
                this.InvokeHandler(true, this.GetABResult());
            }
        }

        #endregion

        #region interface
        public virtual void Load(string name)
        {
            this.targetname = name;
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
                this.LoadState = BundleLoadState.Paused;

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
            AssetBundleResult result = new AssetBundleResult();
            result.MainObject = this._BundleRef ;
            result.InstancedObject = this.LoaderInsObject;
            return result;
        }

        protected void PreparedTryIns()
        {
            if (this.PreParedGameObject != null)
            {
                UnityEngine.Object res = null;

                if (this._BundleRef.Instantiate(out res))
                {
                    if (res is GameObject)
                    {
                        this.LoaderInsObject = res as GameObject;
                        this.LoaderInsObject.BindParent(this.PreParedGameObject);
                    }
                    else
                    {
                        if (BundleConfig.SAFE_MODE)
                            throw new FrameWorkResNotMatchException(string.Format("{0} Type Is Not Gameobject", targetname));
                        else
                            LogMgr.LogErrorFormat("Not Gameobject cant be instanced as gameobject :{0}",targetname);
                    }
                }
            }
        }

        #endregion

        #region POOL

        public static T CreateLoader<T>() where T :BaseBundleLoader,new()
        {
            T loader = null;
            if (KObjectPool.mIns != null) {
                loader = KObjectPool.mIns.Pop<T>();
            }

            //if (loader == null)
            //    loader = new T();

            return loader;
        }

        public virtual void AwakeFromPool()
        {
            this._Bundle = null;
            this._BundleRef = null;
            this.targetname = null;
            this.LoadState = BundleLoadState.Prepared;
            this.onComplete = null;
            this.LoaderInsObject = null;
            this.PreParedGameObject = null;
        }

        public virtual void RemovedFromPool()
        {
            this._Bundle = null;
            this._BundleRef = null;
            this.targetname = null;
            this.LoadState = BundleLoadState.Prepared;
            this.onComplete = null;
            this.LoaderInsObject = null;
            this.PreParedGameObject = null;
        }

        #endregion

        protected void InvokeHandler(bool bvalue, AssetBundleResult result)
        {
            if (this.onComplete != null)
            {
                this.onComplete(bvalue,result);
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
            BundlePkgInfo pkginfo = ResBundleMgr.mIns.BundleInforMation.SeekInfo(name);

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
                this.ThrowAssetMissing(this.targetname);

            bundle = ResBundleMgr.mIns.Cache.PushAsset(pkginfo, ab);
            return bundle;
#endif
        }



        protected AssetBundleCreateRequest LoadFullAssetToMemAsync(BundlePkgInfo pkginfo)
        {
            if (ResBundleMgr.mIns.Cache.Contains(pkginfo))
            {
                return null;
            }
            else
            {
                AssetBundleCreateRequest abRequst = null;
                using (gstring.Block())
                {
                    abRequst = KAssetBundle.LoadFromFileAsync( BundlePathConvert.GetRunningPath(pkginfo.AbFileName));
                }

                return abRequst;
            }
        }

        protected bool CreateFromAsset(IBundleRef bundle, out UnityEngine.Object asset)
        {
            bool ret = bundle.LoadAsset(out asset);
            if (!ret)
            {
                this.ThrowAssetMissing(this.targetname);
            }

            return ret;
        }

        protected void ThrowBundleMissing(string resname)
        {
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
