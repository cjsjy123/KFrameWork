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

        public override void OnStart()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("PreLoad Load Asset {0} Start at {1}", this.targetname, GameSyncCtr.mIns.RenderFrameCount);
        }

        public override void OnPaused()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("PreLoad Load Asset {0} Paused at v{1}", this.targetname, GameSyncCtr.mIns.RenderFrameCount);
        }

        public override void OnResume()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("PreLoad Load Asset {0} Resumed at {1}", this.targetname, GameSyncCtr.mIns.RenderFrameCount);
        }

        public override void OnError()
        {
            base.OnError();

            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("PreLoad Load Asset {0} Error at {1}", this.targetname, GameSyncCtr.mIns.RenderFrameCount);
        }

        public override void OnFinish()
        {
            base.OnFinish();

            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("PreLoad Asset {0} Finished at {1}", this.targetname, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void CreateMain()
        {
            this._BundleRef = ResBundleMgr.mIns.Cache.TryGetValue(this._info);

            if (this.isRunning()) //double check
            {
                this.LoadState = BundleLoadState.Finished;
            }
        }

        protected override void LoadMainAsyncRequest(AssetBundleRequest request)
        {

        }
    }
}


