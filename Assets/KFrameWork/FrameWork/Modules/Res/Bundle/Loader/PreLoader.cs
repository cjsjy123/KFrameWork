using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

namespace KFrameWork
{
    public class PreLoader : BaseAsyncLoader
    {
        public override void DownLoad(string url)
        {
            base.DownLoad(url);
        }

        protected override void OnStart()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("PreLoad Load Asset {0} Start at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnPaused()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("PreLoad Load Asset {0} Paused at v{1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnResume()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("PreLoad Load Asset {0} Resumed at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnError()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("PreLoad Load Asset {0} Error at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);

            base.OnError();

        }

        protected override void OnFinish()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("PreLoad Asset {0} Finished at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);

            base.OnFinish();
            this.Dispose();
        }

        protected override void CreateMain()
        {
            this._BundleRef = ResBundleMgr.mIns.Cache.TryGetValue(this.loadingpkg);
            if (this._BundleRef == null)
                this.ThrowLogicError(loadingpkg.BundleName);

            this._FinishAndRelease();
        }

        protected override void LoadMainAsyncRequest(AssetBundleRequest request)
        {
            this._BundleMainObject = request.asset;
        }

        protected override void LoadSceneAssetFinish()
        {

        }
    }
}


