using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;
using System.IO;

namespace KFrameWork
{
    public class SceneSyncLoader : SyncLoader
    {
        protected override void OnStart()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Scene Load Asset {0} Start at {1} ", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnPaused()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Scene Load Asset {0} Paused at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnResume()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Scene Load Asset {0} Resumed at {1} ", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnError()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Scene Load Asset {0} Error at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);

            base.OnError();
        }

        protected override void OnFinish()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("Scene Load Asset {0} Finished at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);

            this.InvokeHandler(true);

            string[] sceneSplit = this.loadingpkg.BundleName.Split('.');
            string scename = this.loadingpkg.BundleName;
            if (sceneSplit.Length > 1)
                scename = sceneSplit[0];

            GameSceneCtr.LoadScene(scename);
            this.Dispose();
        }
    }
}
