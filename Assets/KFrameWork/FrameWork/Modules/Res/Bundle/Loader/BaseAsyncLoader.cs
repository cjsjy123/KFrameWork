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
            private BundlePkgInfo pkg;
            public BundlePkgInfo Info
            {
                get
                {
                    return this.pkg;
                }
            }

            public BaseAsyncLoader Loader;

            public string LoadingName { get; private set; }

            public AsyncBundleTp asyncType { get; private set; }

            public AsyncOperation currentAsync { get; private set; }

            public bool KeepWaiting
            {
                get
                {
                    if (currentAsync == null)
                    {
                        LogMgr.LogError("Current Async is Null");
                        return false;
                    }

                    if (this.asyncType == AsyncBundleTp.SCENE_LOAD && !this.currentAsync.allowSceneActivation)
                    {
                        bool ret = currentAsync.progress < 0.9f && !currentAsync.isDone;
                        int value = Mathf.FloorToInt(currentAsync.progress * 10);
                        Loader._loadedCnt = Loader.LoadedCnt + value;
                        if(ret)
                            Loader.InvokeProgressHandler(Loader.LoadedCnt, Loader.NeedLoadedCnt);

                        return ret;
                    }

                    return !currentAsync.isDone;
                }
            }

            private Queue<KeyValuePair<BundlePkgInfo,AsyncOperation>> waitQueue;

            public AsyncBundleTask(BaseAsyncLoader loader)
            {
                this.Loader = loader;
                this.waitQueue = new Queue<KeyValuePair<BundlePkgInfo, AsyncOperation>>();
            }

            public void PushOperation(BundlePkgInfo Pkginfo, AsyncOperation o)
            {
                if (this.currentAsync == null)
                {
                    this.pkg = Pkginfo;
                    this.LoadingName = Pkginfo.AbFileName;
                    this.currentAsync = o;
                    this.asyncType = this.Swithtp(this.currentAsync);
                }
                else
                {
                    this.waitQueue.Enqueue(new KeyValuePair<BundlePkgInfo, AsyncOperation>(Pkginfo, o));
                }
            }

            private AsyncBundleTp Swithtp(AsyncOperation o)
            {
                if (o is AssetBundleCreateRequest)
                {
                    return AsyncBundleTp.AB_CREATE;
                }
                else if (o is AssetBundleRequest)
                {
                    return AsyncBundleTp.AB_LOAD;
                }
                else if (o is AsyncOperation)
                {
                    return AsyncBundleTp.SCENE_LOAD;
                }
                return AsyncBundleTp.NONE;
            }

            public bool ChangeToNext()
            {
                if (waitQueue.Count > 0)
                {
                    if (KeepWaiting)
                        LogMgr.LogError("current async task hadnt finished yet");

                    KeyValuePair <BundlePkgInfo, AsyncOperation > p = this.waitQueue.Dequeue();
                    this.pkg = p.Key;
                    this.LoadingName = p.Key.AbFileName;
                    this.currentAsync = p.Value;
                    this.asyncType = this.Swithtp(this.currentAsync);

                    return true;
                }
                else
                {
                    this.pkg = null;
                    this.currentAsync = null;
                    this.LoadingName = null;
                    this.asyncType = AsyncBundleTp.NONE;
                    return false;
                }
            }
        }

        private int _loadedCnt;

        public int LoadedCnt
        {
            get
            {
                return _loadedCnt;
            }
            set
            {
                int old = _loadedCnt;
                _loadedCnt = value;

                if (old < value)
                {
                    if (Task.ChangeToNext())
                    {
                        ExcuteCmd();

                        //debug
                        if (_loadedCnt >= this.NeedLoadedCnt)
                            this.ThrowLogicError();
                    }
                    this.LoadAfterUpdateCnt();
                }  
            }
        }

        protected int NeedLoadedCnt = 0;

        protected Queue<BundlePkgInfo> LoadQueue = new Queue<BundlePkgInfo>();

        private AsyncBundleTask m_task;

        protected AsyncBundleTask Task
        {
            get
            {
                if (m_task == null)
                    m_task = new AsyncBundleTask(this);
                return m_task;
            }
        }

        public override void Load(string name)
        {
            base.Load(name);

            this.loadingpkg = this._SeekPkgInfo(name);
            if (this.loadingpkg == null)
                this.ThrowAssetMissing(name);

            ResBundleMgr.mIns.Cache.PushLoading(loadingpkg.BundleName, this);
            this.CreateLoadQueue(this.loadingpkg);
            this.StartLoad();
        }

        private void EnqueueToLoadQueue(BundlePkgInfo pkg)
        {
            if (BundleConfig.Bundle_Log)
            {
                if (LoadQueue.Contains(pkg))
                {
                    LogMgr.LogFormat("{0} 已经包含此任务 ",pkg);
                }
            }

            LoadQueue.Enqueue(pkg);
           
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
                else if (BundleConfig.Bundle_Log)
                    LogMgr.LogFormat("{0} asyncloader 状态不符", this.LoadResPath);
                else
                    break;
            }

            this.EnqueueToLoadQueue(pkginfo);
        }

        protected virtual void InitProgress()
        {
            this.NeedLoadedCnt = this.LoadQueue.Count;
            this.LoadedCnt = 0;
        }

        private void StartLoad()
        {
            InitProgress();

            if (this.LoadQueue.Count > 0)
            {
                _LoadBundle(this.LoadQueue.Dequeue());
            }
            else
            {
                LogMgr.LogErrorFormat("加载队列异常 :{0}",this.LoadResPath);
                this.LoadState = BundleLoadState.Error;
            }
        }

        private void _LoadBundle(BundlePkgInfo pkginfo)
        {
            if (ResBundleMgr.mIns.Cache.Contains(pkginfo))
            {
                if (FrameWorkConfig.Open_DEBUG)
                {
                    LogMgr.LogFormat("加载了 :{0}",pkginfo.BundleName);
                }

                this.LoadedCnt++;
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
                LogMgr.LogFormat("加载了 :{0}", bundlename);
            }
            this.LoadedCnt++;
        }

        protected void _PushTaskAndExcute(BundlePkgInfo Pkginfo, AsyncOperation operation)
        {
            Task.PushOperation(Pkginfo, operation);
            this.ExcuteCmd();
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
                if (BundleConfig.Bundle_Log)
                    LogMgr.LogFormat(":::::::Aysnc load {0}", Task.Info.BundleName);

                if (abCreateReq.assetBundle == null)
                    ThrowAssetMissing(this.LoadResPath);

                IBundleRef bundle = ResBundleMgr.mIns.Cache.PushAsset(Task.Info, abCreateReq.assetBundle);

                BundlePkgInfo pkg = this._SeekPkgInfo( abCreateReq.assetBundle.name);
                this.AppendDepends(pkg, bundle);

                ResBundleMgr.mIns.Cache.RemoveLoading(pkg.BundleName);
                if (FrameWorkConfig.Open_DEBUG)
                {
                    LogMgr.LogFormat("加载了 :{0}", pkg.BundleName);
                }
                this.LoadedCnt++;
            }
            else if (Task.asyncType == AsyncBundleTp.SCENE_LOAD)
            {
                this._AsyncOpation = Task.currentAsync;
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
                    this._Bundle = this._BundleRef.MainObject;
                    this.AppendDepends(this.loadingpkg, this._BundleRef);

                    if (isSceneLoad)
                    {
                        AsyncOperation asyncOp = GameSceneCtr.LoadSceneAsync(Path.GetFileNameWithoutExtension(this.LoadResPath));
                        asyncOp.allowSceneActivation = false;
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
                    ResBundleMgr.mIns.Cache.InvokeBundleFinishEvent(Task.Info.BundleName);
                    ResBundleMgr.mIns.Cache.RemoveLoadingBundle(Task.Info.BundleName);
                }
                this.AutoCreateBundle();
            }
        }

        private void ExcuteCmd()
        {
            WaitTaskCommand cmd = WaitTaskCommand.Create(Task, AsyncLoadFinished);
            cmd.Excute();
        }

        private void LoadAfterUpdateCnt()
        {
            InvokeProgressHandler(LoadedCnt, NeedLoadedCnt);
            if ( this.LoadQueue.Count > 0)
            {
                _LoadBundle(this.LoadQueue.Dequeue());
            }
            else if (this.LoadQueue.Count == 0)
            {
                this.CreateMain();
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
            this.NeedLoadedCnt = 0;
            this.LoadedCnt = 0;
        }

        public override void RemovedFromPool()
        {
            base.RemovedFromPool();
            this.LoadQueue.Clear();
            this.LoadQueue =null;
            this.m_task = null;
            this.NeedLoadedCnt = 0;
            this.LoadedCnt = 0;
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


