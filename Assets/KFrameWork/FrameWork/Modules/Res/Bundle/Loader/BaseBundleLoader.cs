//#define AB_DEBUG

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
        protected class AsyncBundleTask:ITask
        {
            public bool keep; 

            public bool KeepWaiting {
                get {
                    return keep;
                }
            }
        }

        protected string targetname;

        protected UnityEngine.Object _Bundle;

        private BundleLoadState _state;

        public BundleLoadState loadState
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
            if (this.loadState != BundleLoadState.Error && this.loadState  != BundleLoadState.Finished
                && this.loadState != BundleLoadState.Paused)
            {
                this.InvokeHandler(true, this.GetABResult());
            }
        }

        #endregion

        #region interface
        public virtual void Load(string name)
        {
            if (this.loadState == BundleLoadState.Prepared)
            {
                this.loadState = BundleLoadState.Running;
            }
        }

        public virtual void DownLoad(string url)
        {
            if (this.loadState == BundleLoadState.Prepared)
            {
                this.loadState = BundleLoadState.Running;
            }
        }

        public virtual void Pause()
        {
            if (this.loadState != BundleLoadState.Paused
                && this.loadState != BundleLoadState.Finished
                && this.loadState != BundleLoadState.Error
                && this.loadState != BundleLoadState.Stopped)
            {
                this.loadState = BundleLoadState.Paused;
            }
        }

        public virtual void Stop()
        {
            if (this.loadState != BundleLoadState.Finished && this.loadState != BundleLoadState.Error
                && this.loadState != BundleLoadState.Stopped)
            {
                this.loadState = BundleLoadState.Paused;

            }
        }

        public virtual void Resume()
        {

            if (this.loadState != BundleLoadState.Error && this.loadState != BundleLoadState.Stopped
                && this.loadState != BundleLoadState.Finished)
            {
                this.loadState = BundleLoadState.Running;

            }
        }

        public AssetBundleResult GetABResult()
        {
            AssetBundleResult result = new AssetBundleResult();
            result.MainObject = this._Bundle;
            return result;
        }
        #endregion

        #region POOL

        public static T CreateLoader<T>() where T :BaseBundleLoader,new()
        {
            T loader = null;
            if (KObjectPool.mIns != null) {
                loader = KObjectPool.mIns.Pop<T>();
            }


            if (loader == null)
                loader = new T();

            return loader;
        }

        public virtual void AwakeFromPool()
        {
            this._Bundle = null;
            this.targetname = null;
            this.loadState = BundleLoadState.Prepared;
            this.onComplete = null;
        }

        public virtual void RemovedFromPool()
        {
            this._Bundle = null;
            this.targetname = null;
            this.loadState = BundleLoadState.Prepared;
            this.onComplete = null;
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
            return this.loadState == BundleLoadState.Running ;
        }

        protected bool isPaused()
        {
            return  this.loadState == BundleLoadState.Paused;
        }

        protected AssetBundle LoadAssetBundle(string path)
        {
#if UNITY_EDITOR
            return null;
#elif UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID
        AssetBundle ab = AssetBundle.LoadFromFile(path);
        return ab;
#else
        return null;
#endif

        }

        protected UnityEngine.Object LoadFullAsset(BundlePkgInfo pkginfo)
        {

#if UNITY_EDITOR
            BundlePkgInfo info = BundleMgr.mIns.BundleInforMation.SeekInfo(pkginfo.BundleName);
            UnityEngine.Object target = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(info.EditorPath);
            if (target == null)
            {
                if (BundleConfig.SAFE_MODE)
                    throw new FrameWorkResMissingException(string.Format("Asset {0} Missing", pkginfo.BundleName));
                else
                {
                    this.loadState = BundleLoadState.Error;
                    LogMgr.LogErrorFormat("Asset {0} Missing", pkginfo.BundleName);
                }
            }
            return target;


#elif UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID
            AssetBundle ab = null;

            if(BundleMgr.mIns.Cache.Contains(pkginfo.AbFilePath))
            {
                ab =BundleMgr.mIns.Cache[pkginfo.AbFilePath].get();
            }
            else
            {
                this.LoadAssetBundle(BundlePathConvert.GetRunningPath(pkginfo.AbFilePath));
            }
            UnityEngine.Object resObject = this.CreateAssetFromAB(ab, BundlePathConvert.EditorName2AssetName(pkginfo.EditorPath));
            return resObject;
#endif
        }


        protected UnityEngine.Object CreateAssetFromAB(AssetBundle ab, string resname)
        {
            UnityEngine.Object target = ab.LoadAsset(resname);
            if (target == null)
            {
                if (BundleConfig.SAFE_MODE)
                    throw new FrameWorkResMissingException(string.Format("Asset {0} Missing", resname));
                else
                {
                    this.loadState = BundleLoadState.Error;
                    LogMgr.LogErrorFormat("Asset {0} Missing", resname);
                }
            }

            return target;
        }





    }

}
