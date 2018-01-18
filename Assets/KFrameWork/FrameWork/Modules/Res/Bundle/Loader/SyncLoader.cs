using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

namespace KFrameWork
{
    public class SyncLoader : BaseBundleLoader
    {

        private int total =0;

        private int current =0;

        private Queue<BundlePkgInfo> loaderqueue = new Queue<BundlePkgInfo>();

        public override void RemoveToPool ()
        {
            base.RemoveToPool ();
            this.loaderqueue.Clear();
            this.total =0;
            this.current =0;
        }

        public override void DownLoad(string url)
        {
            
        }

        public override void Load(string name)
        {
            base.Load(name);

            loadingpkg = this._SeekPkgInfo(name);

            ResBundleMgr.mIns.Cache.PushLoading(loadingpkg.BundleName,this);

            _PushDepend(loadingpkg);

            this.total = this.loaderqueue.Count;

            StartLoad();

        }

        private void AfterDependDone()
        {
            if (this.isRunning())
            {
                this._BundleRef = this._LoadBundle(loadingpkg);
                this.AddDepends(this.loadingpkg, _BundleRef);

                this.CreateFromAsset(this._BundleRef,out this._BundleMainObject );
                this.InvokeProgressHandler(total, total);

                if (this.isRunning()) //double check
                {
                    this.LoadState = BundleLoadState.Finished;
                }
            }
        }

        private void StartLoad()
        {
            while(this.loaderqueue.Count >0)
            {
                BundlePkgInfo pkginfo = this.loaderqueue.Dequeue();
                if (this.isRunning())
                {
                    bool isasyncLoading = ResBundleMgr.mIns.Cache.ContainsLoadingBundle(pkginfo.BundleName);
                    if(isasyncLoading)
                    {
                        ResBundleMgr.mIns.Cache.PushLoadingBundle(pkginfo.BundleName,this.YiedAsyncDone);
                    }
                    else
                    {
                        this._LoadBundle(pkginfo);
                        current++;
                        this.InvokeProgressHandler(current,total);
                    }
                } 
            }

            if(current == total)
            {
                AfterDependDone();
            }
        }

        private void _PushDepend(BundlePkgInfo pkginfo)
        {
            for (int i = 0; i < pkginfo.Depends.Length; ++i)
            {
                if (this.isRunning())
                {
                    this._PushDepend(this._SeekPkgInfo( pkginfo.Depends[i]));
                }
                else if (BundleConfig.Bundle_Log)
                    LogMgr.LogFormat("{0} loader 状态不符", this.LoadResPath);
                else
                    break;
            }

            this.loaderqueue.Enqueue(pkginfo);
        }

        private void YiedAsyncDone(string key)
        {
            if(BundleConfig.Bundle_Log)
                LogMgr.LogFormat("异步通知同步任务:{0}",key);

            current++;
            this.InvokeProgressHandler(current,total);

            if(current == total)
            {
                this.AfterDependDone();
            }
            else if(current > total)
            {
                LogMgr.LogError("计数异常");
            }
        }

        private IBundleRef _LoadBundle(BundlePkgInfo pkginfo)
        {
            IBundleRef bundle = this.LoadFullAssetToMem(pkginfo);
            this.AddDepends(pkginfo, bundle);
            return bundle;
        }

        protected override void OnError()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Sync Load Asset {0} Error Frame:{1}", this.LoadResPath,GameSyncCtr.mIns.RenderFrameCount);
            
            base.OnError();
        }

        protected override void OnFinish()
        {

            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Sync Load Asset {0} Finish Frame:{1}", this.LoadResPath,GameSyncCtr.mIns.RenderFrameCount);
            
            base.OnFinish();
        }

        protected override void OnPaused()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Sync Load Asset {0} Paused Frame:{1}", this.LoadResPath,GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnResume()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Sync Load Asset {0} Resume Frame:{1}", this.LoadResPath,GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnStart()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Sync Load Asset {0} Start Frame:{1}", this.LoadResPath,GameSyncCtr.mIns.RenderFrameCount);
        }
    }
}


