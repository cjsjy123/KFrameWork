using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

namespace KFrameWork
{
    public class AsyncLoader :BaseAsyncLoader {

        protected override void OnStart()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Async Load Asset {0} Start at {1} ", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnPaused()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Async Load Asset {0} Paused at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnResume()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Async Load Asset {0} Resumed at {1} ", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnError()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Async Load Asset {0} Error at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
            
            base.OnError();
        }

        protected override void OnFinish()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Async Load Asset {0} Finished at {1}", this.LoadResPath,GameSyncCtr.mIns.RenderFrameCount);

            base.OnFinish();
            this.Dispose();
        }

        protected override void CreateMain()
        {
            //LogMgr.LogError(loadingpkg.BundleName);
            this._BundleRef = ResBundleMgr.mIns.Cache.TryGetValue(this.loadingpkg);

            if (this._BundleRef == null)
                this.ThrowLogicError(loadingpkg.BundleName);

            if (this._BundleRef.MainObject == null)
            {
                CreateMainAsset();
            }
            else
            {
                this._BundleMainObject = this._BundleRef.MainObject;
                this._FinishAndRelease();
            }
        }

        private void CreateMainAsset()
        {
            if (!this._BundleRef.SupportAsync && this._BundleRef.MainObject == null)
            {
                this._BundleRef = LoadFullAssetToMem(this.loadingpkg);
                this._BundleRef.LoadAsset(out _BundleMainObject);
                this._FinishAndRelease();
            }
            else if(this._BundleRef.MainObject != null)
            {
                this._BundleRef = LoadFullAssetToMem(this.loadingpkg);
                this._BundleMainObject = this._BundleRef.MainObject;
                this._FinishAndRelease();
            }
            else
            {
                ///需要异步预实例化
                this._PushTaskAndExcute(this.loadingpkg, this._BundleRef.LoadAssetAsync());
            }
        }

        protected override void LoadMainAsyncRequest(AssetBundleRequest request)
        {
            this._BundleMainObject = request.asset;
            this._FinishAndRelease();
        }

        public override void DownLoad (string url)
        {
            base.DownLoad (url);
        }

        protected override void LoadSceneAssetFinish()
        {
            
        }
    }

}

