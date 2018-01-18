using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.IO;

namespace KFrameWork
{
    public abstract class BaseAsyncLoader : BaseBundleLoader
    {
        protected enum AsyncBundleTp
        {
            NONE,
            AB_CREATE,
            AB_LOAD,
            SCENE_LOAD,
        }

        protected class AsyncBundleTask : ITask
        {
            private BundlePkgInfo _currentPkg;
            public BundlePkgInfo CurrentLoadInfo
            {
                get
                {
                    return this._currentPkg;
                }
            }

            private BaseAsyncLoader Loader;

            public AsyncBundleTp asyncType { get; private set; }

            public object currentAsync { get; private set; }

            public bool KeepWaiting
            {
                get
                {
                    if (currentAsync == null)
                    {
                        LogMgr.LogError("Current Async is Null");
                        return false;
                    }

                    if (this.asyncType == AsyncBundleTp.SCENE_LOAD )
                    {
                        SceneOperation asyncop = currentAsync as SceneOperation;
                        if (asyncop.HasLoad)
                        {
                            Loader.InvokeProgressHandler(10, 10);
                            return false;
                        }
                        else
                        {
                            bool ret = asyncop.progress < 0.9f && !asyncop.isDone;
                            int value = Mathf.FloorToInt(asyncop.progress * 10);
                            if(ret)
                                Loader.InvokeProgressHandler(value, 10);
                            else
                                Loader.InvokeProgressHandler(10, 10);

                            return ret;
                        }
                    }
                    else
                    {
                        AsyncOperation asyncop = currentAsync as AsyncOperation;
                        return !asyncop.isDone;
                    } 
                }
            }

            public AsyncBundleTask(BaseAsyncLoader asyncLoader)
            {
                this.Loader = asyncLoader;
            }

            public void PushOperation(BundlePkgInfo Pkginfo, object o)
            {
                this._currentPkg = Pkginfo;
                this.currentAsync = o;
                this.asyncType = this.Swithtp(this.currentAsync);
            }

            private AsyncBundleTp Swithtp(object o)
            {
                if (o is AssetBundleCreateRequest)
                {
                    return AsyncBundleTp.AB_CREATE;
                }
                else if (o is AssetBundleRequest)
                {
                    return AsyncBundleTp.AB_LOAD;
                }
                else if (o is SceneOperation)
                {
                    return AsyncBundleTp.SCENE_LOAD;
                }
                return AsyncBundleTp.NONE;
            }
        }

        protected Queue<BundlePkgInfo> LoadQueue = new Queue<BundlePkgInfo>();

        protected Dictionary<string, IBundleRef> LoadedRefs = new Dictionary<string, IBundleRef>() ;

        private AsyncBundleTask m_task;

        protected AsyncBundleTask Task
        {
            get
            {
                if (m_task == null)
                {
                    m_task = new AsyncBundleTask(this);
                } 
                return m_task;
            }
        }

        private int Needed = 0;

        public override void Load(string name)
        {
            base.Load(name);

            LoadedRefs.Clear();

            this.loadingpkg = this._SeekPkgInfo(name);
            if (this.loadingpkg == null)
                this.ThrowAssetMissing(name);

            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("准备加载 :{0} :abname :{1}", this.loadingpkg.BundleName, this.loadingpkg.AbFileName);

            ResBundleMgr.mIns.Cache.PushLoading(loadingpkg.BundleName, this);

            this.CreateLoadQueue(loadingpkg);

            this.Needed = this.LoadQueue.Count;

            this.RefreshLoaded();

            this.RunNextTask();
        }

        private void CreateLoadQueue(BundlePkgInfo pkginfo)
        {
            for (int i = 0; i < pkginfo.Depends.Length; ++i)
            {
                if (this.isRunning())
                {
                    BundlePkgInfo pkg = this._SeekPkgInfo(pkginfo.Depends[i]);
                    this.CreateLoadQueue(pkg);
                }
                else
                    LogMgr.LogFormat("{0} asyncloader 状态不符", this.LoadResPath);
            }

            LoadQueue.Enqueue(pkginfo);
        }

        private void RefreshLoaded()
        {
            if (this.Needed == this.LoadedRefs.Count)
                return;

            SetLoadRef(loadingpkg);

            //self
            IBundleRef selfBundle = ResBundleMgr.mIns.Cache.TryGetValue(this.loadingpkg);
            if (selfBundle != null)
            {
                LoadedRefs[loadingpkg.AbFileName] = selfBundle;
            }
        }

        private void SetLoadRef(BundlePkgInfo pkginfo)
        {
            for (int i = 0; i < pkginfo.Depends.Length; ++i)
            {
                BundlePkgInfo pkg = this._SeekPkgInfo(pkginfo.Depends[i]);
                IBundleRef refBundle = ResBundleMgr.mIns.Cache.TryGetValue(pkg);
                if (refBundle != null)
                {
                    LoadedRefs[pkg.AbFileName] = refBundle;
                }
                //set child pkg
                SetLoadRef(pkg);
            }
        }

        private void _LoadBundle(BundlePkgInfo pkginfo)
        {
            if (LoadedRefs.ContainsKey(pkginfo.AbFileName))
            {
                if (FrameWorkConfig.Open_DEBUG)
                {
                    LogMgr.LogFormat("缓存中存在直接加载 :{0} => {1}", pkginfo.BundleName, pkginfo.AbFileName);
                }
                this.RunNextTask();
            }
            else if (!isABMode && this.isRunning())
            {
                this.AutoCreateBundle();
            }
            else if (this.isRunning())
            {
                if (!ResBundleMgr.mIns.Cache.ContainsLoadingBundle(pkginfo.BundleName))
                {
                    this._PushTaskAndExcute(pkginfo, this.LoadFullAssetToMemAsync(pkginfo));
                    ResBundleMgr.mIns.Cache.PushLoadingBundle(pkginfo.BundleName);
                }   
                else
                    ResBundleMgr.mIns.Cache.PushLoadingBundle(pkginfo.BundleName, this.AttheSametimeUpdate);
            }
        }

        private void AttheSametimeUpdate(string bundlename)
        {
            if (FrameWorkConfig.Open_DEBUG)
            {
                BundlePkgInfo pkg = this._SeekPkgInfo(bundlename);
                LogMgr.LogFormat(":::::其他地方加载了 :{0} => {1}", pkg.BundleName, pkg.AbFileName);
            }

            this.RefreshLoaded();

            this.RunNextTask();
        }

        protected void _PushTaskAndExcute(BundlePkgInfo Pkginfo, object operation)
        {

            Task.PushOperation(Pkginfo, operation);

            if (FrameWorkConfig.Open_DEBUG)
            {
                LogMgr.LogFormat("开始异步任务 :{0} => {1} =>", Pkginfo.BundleName, Pkginfo.AbFileName, Task.asyncType);
            }

            WaitTaskCommand cmd = WaitTaskCommand.Create(Task, AsyncLoadFinished);
            cmd.Excute();
        }

        private void AutoCreateBundle()
        {
            if (Task.asyncType == AsyncBundleTp.AB_LOAD)
            {
                AssetBundleRequest abReq = Task.currentAsync as AssetBundleRequest;
                this.LoadMainAsyncRequest(abReq);
            }
            else if (Task.asyncType == AsyncBundleTp.AB_CREATE)
            {
                AssetBundleCreateRequest abCreateReq = Task.currentAsync as AssetBundleCreateRequest;
                if (abCreateReq.assetBundle == null)
                    ThrowAssetMissing(this.LoadResPath);

                IBundleRef bundle = ResBundleMgr.mIns.Cache.PushAsset(Task.CurrentLoadInfo, abCreateReq.assetBundle);

                BundlePkgInfo pkg = this._SeekPkgInfo( abCreateReq.assetBundle.name);
                this.AddDepends(pkg, bundle);

                ResBundleMgr.mIns.Cache.RemoveLoading(pkg.BundleName);
                if (FrameWorkConfig.Open_DEBUG)
                {
                    LogMgr.LogFormat("成功加载了 :{0} => {1} => {2} => RefCnt :{3} =>SelfRefCnt:{4}",this.UID, pkg.BundleName,pkg.AbFileName,bundle.InstanceRefCount, bundle.SelfRefCount);
                }
                this.RefreshLoaded();
            }
            else if (Task.asyncType == AsyncBundleTp.SCENE_LOAD)
            {
                this.LoadSceneAssetFinish();
            }
            else
            {
                if (isABMode)
                {
                    if (BundleConfig.SAFE_MODE)
                    {
                        throw new FrameWorkResNotMatchException(string.Format("不被支持的异步类型 {0}", Task.currentAsync));
                    }
                    else
                    {
                        LogMgr.LogError("不被支持的异步类型 ");
                        this.LoadState = BundleLoadState.Error;
                    }
                }
                else
                {
                    this._BundleRef = LoadFullAssetToMem(this.loadingpkg);
                    this._BundleMainObject = this._BundleRef.MainObject;
                    this.AddDepends(this.loadingpkg, this._BundleRef);

                    if (isSceneLoad)
                    {
                        SceneOperation asyncOp = GameSceneCtr.LoadSceneAsync(Path.GetFileNameWithoutExtension(this.LoadResPath));
                        asyncOp.DisableScene();
                        this._AsyncOpation = asyncOp;

                        this._PushTaskAndExcute(this.loadingpkg, asyncOp);
                    }
                    else
                    {
                        this._FinishAndRelease();
                    }
                }
            }
        }

        private void AsyncLoadFinished(WaitTaskCommand cmd)
        {
            if (this.isRunning())
            {
                if (isABMode)
                {
                    ResBundleMgr.mIns.Cache.InvokeBundleFinishEvent(Task.CurrentLoadInfo.BundleName);
                    ResBundleMgr.mIns.Cache.RemoveLoadingBundle(Task.CurrentLoadInfo.BundleName);
                }

                AutoCreateBundle();

                if (this.isRunning())
                {
                    RunNextTask();
                }
            }
        }

        private void RunNextTask()
        {
            if(!isSceneLoad)
                InvokeProgressHandler(this.LoadedRefs.Count,Needed);

            //may exception 
            if (this.LoadedRefs.Count == Needed)
            {
                this.CreateMain();
            }
            else if (this.LoadedRefs.Count > Needed)
            {
                LogMgr.LogError("加载异常");
                this.LoadState = BundleLoadState.Error;
            }
            else if (this.LoadQueue.Count > 0)
            {
                _LoadBundle(this.LoadQueue.Dequeue());
            }
        }

        protected  void _FinishAndRelease()
        {

            if (this.isRunning()) //double check
            {
                this.LoadState = BundleLoadState.Finished;
            }
        }

        public override void RemoveToPool()
        {
            base.RemoveToPool();
            this.LoadQueue.Clear();
            this.m_task = null;
            this.LoadedRefs.Clear();
        }

        /// <summary>
        /// 创建bundleref
        /// </summary>
        protected abstract void CreateMain();
        /// <summary>
        /// 创建bundle
        /// </summary>
        /// <param name="request"></param>
        protected abstract void LoadMainAsyncRequest(AssetBundleRequest request);

        protected abstract void LoadSceneAssetFinish();

    }
}


