using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public abstract class BaseAsyncLoader : BaseBundleLoader
    {
        protected enum AsyncBundleTp
        {
            NONE,
            AB_CREATE,
            AB_LOAD,
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

            private string _loadingname;

            public string LoadingName
            {
                get
                {
                    return this._loadingname;
                }
            }

            private AsyncBundleTp _tp;

            public AsyncBundleTp asyncType
            {
                get
                {
                    return this._tp;
                }
            }

            private AsyncOperation _currentAsync;

            public AsyncOperation currentAsync
            {
                get
                {
                    return this._currentAsync;
                }
            }

            public bool KeepWaiting
            {
                get
                {
                    if (_currentAsync == null)
                    {
                        LogMgr.LogError("Current Async is Null");
                        return false;
                    }

                    return !_currentAsync.isDone;
                }
            }

            private Queue<KeyValuePair<BundlePkgInfo,AsyncOperation>> waitQueue;

            public AsyncBundleTask()
            {
                this.waitQueue = new Queue<KeyValuePair<BundlePkgInfo, AsyncOperation>>();
            }

            public void PushOperation(BundlePkgInfo Pkginfo, AsyncOperation o)
            {
                if (this.currentAsync == null)
                {
                    this.pkg = Pkginfo;
                    this._loadingname = Pkginfo.AbFileName;
                    this._currentAsync = o;
                    this._tp = this.Swithtp(this._currentAsync);
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
                else if ( o is AssetBundleRequest)
                {
                    return AsyncBundleTp.AB_LOAD;
                }
                return AsyncBundleTp.NONE;
            }

            public bool ChangeToNext()
            {
                if (waitQueue.Count > 0)
                {
                    if (KeepWaiting)
                    {
                        LogMgr.LogError("current async task hadnt finished yet");
                    }
                    KeyValuePair <BundlePkgInfo, AsyncOperation > p = this.waitQueue.Dequeue();
                    this.pkg = p.Key;
                    this._loadingname = p.Key.AbFileName;
                    this._currentAsync = p.Value;
                    this._tp = this.Swithtp(this._currentAsync);

                    return true;
                }
                else
                {
                    this.pkg = null;
                    this._currentAsync = null;
                    this._loadingname = null;
                    this._tp = AsyncBundleTp.NONE;
                    return false;
                }
            }
        }

        private int LoadedCnt = 0;

        private int NeedLoadedCnt = 0;

        protected BundlePkgInfo _info;

        private AsyncBundleTask m_task;

        protected AsyncBundleTask Task
        {
            get
            {
                if (m_task == null)
                    m_task = new AsyncBundleTask();
                return m_task;
            }
        }

        private WaitTaskCommand cmd;

        public override void Load(string name)
        {
            base.Load(name);

            this._info = this._SeekPkgInfo(name);

            this.NeedLoadedCnt += this._info.Depends.Length;

            for (int i = 0; i < this._info.Depends.Length; ++i)
            {
                if (this.isRunning())
                {
                    this._AddBundleDpend(this._info.Depends[i]);

                }
                else if (FrameWorkDebug.Open_DEBUG)
                    LogMgr.LogFormat("{0} asyncloader 状态不符", this.targetname);
                else
                    break;
            }

        }

        private void _AddBundleDpend(string name)
        {
            BundlePkgInfo pkginfo = this._SeekPkgInfo(name);

            this.NeedLoadedCnt += pkginfo.Depends.Length;

            for (int i = 0; i < pkginfo.Depends.Length; ++i)
            {
                if (this.isRunning())
                {
                    this._AddBundleDpend(pkginfo.Depends[i]);
                }
                else if (FrameWorkDebug.Open_DEBUG)
                    LogMgr.LogFormat("{0} asyncloader 状态不符", this.targetname);
                else
                    break;
            }

            if (this.isRunning())
            {
                this._LoadBundle(pkginfo);
            }
        }

        protected virtual void _LoadBundle(BundlePkgInfo pkginfo)
        {
            if (ResBundleMgr.mIns.Cache.Contains(pkginfo))
            {
                this.LoadedCnt++;
                this.LoadAfterUpdateCnt();
            }
            else if(!ResBundleMgr.mIns.Cache.ContainsLoading(pkginfo.AbFileName))
            {
                Task.PushOperation(pkginfo, this.LoadFullAssetToMemAsync(pkginfo));

                ResBundleMgr.mIns.Cache.PushLoading(Task.LoadingName);

                if (cmd == null)
                {
                    cmd = WaitTaskCommand.Create(Task, _AsyncLoad);
                    cmd.Excute();
                }
                else
                {
                    if (cmd.RunningState != CommandState.Running)
                        cmd.Excute();
                }
            }
        }

        private void _AutoCreate()
        {
            if (Task.asyncType == AsyncBundleTp.AB_LOAD)
            {
                AssetBundleRequest abReq = Task.currentAsync as AssetBundleRequest;
                if (LoadedCnt == NeedLoadedCnt)
                {
                    this.LoadMainAsyncRequest(abReq);
                }
            }
            else if (Task.asyncType == AsyncBundleTp.AB_CREATE)
            {
                AssetBundleCreateRequest abCreateReq = Task.currentAsync as AssetBundleCreateRequest;
                AssetBundle ab = abCreateReq.assetBundle;

                ResBundleMgr.mIns.Cache.PushAsset(Task.Info, ab);
            }
            else
            {
                if (BundleConfig.SAFE_MODE)
                {
                    throw new FrameWorkResNotMatchException(string.Format("不被支持的异步类型 {0}",Task.currentAsync));
                }
                else
                {
                    LogMgr.LogErrorFormat("不被支持的异步类型 {0}", Task.currentAsync);
                }
            }
        }

        private void _AsyncLoad()
        {
            ResBundleMgr.mIns.Cache.RemoveLoading(Task.LoadingName);

            if (this.isRunning())
            {
                this._AutoCreate();

                bool needNext = Task.ChangeToNext();

                this.LoadedCnt++;

                if (needNext)
                {
                    if (cmd.RunningState != CommandState.Running)
                        cmd.Excute();
                    else////debug
                    {
                        this.LoadState = BundleLoadState.Error;
                        throw new FrameWorkException("逻辑异常", ExceptionType.Higher_Excetpion);
                    }

                    //debug
                    if (LoadedCnt >= this.NeedLoadedCnt)
                    {
                        this.LoadState = BundleLoadState.Error;
                        throw new FrameWorkException("逻辑异常", ExceptionType.Higher_Excetpion);
                    }
                }

                this.LoadAfterUpdateCnt();

            }
        }

        private void LoadAfterUpdateCnt()
        {
            if (LoadedCnt == NeedLoadedCnt)
            {
                if (ResBundleMgr.mIns.Cache.Contains(this._info))
                {
                    this.CreateMain();
                    this.PushTaskToPool();
                }
                else
                {
                    _LoadBundle(this._info);
                }
            }
            else if (LoadedCnt == NeedLoadedCnt + 1)
            {
                this.CreateMain();
                this.PushTaskToPool();
            }
        }


        protected void PushTaskToPool()
        {
            if (cmd != null)
            {
                cmd.Release(true);
                cmd = null;
            }
        }

        public override void AwakeFromPool()
        {
            base.AwakeFromPool();
            this.m_task = null;
            this.NeedLoadedCnt = 0;
            this.LoadedCnt = 0;
            this._info = null;
            this.cmd = null;
        }

        public override void RemovedFromPool()
        {
            base.RemovedFromPool();
            this.m_task = null;
            this.NeedLoadedCnt = 0;
            this.LoadedCnt = 0;
            this._info = null;
            this.cmd = null;
        }

        protected abstract void CreateMain();

        protected abstract void LoadMainAsyncRequest(AssetBundleRequest request);

    }
}


