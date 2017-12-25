using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;
using System.IO;

namespace KFrameWork
{
    public class SceneAsyncLoader : BaseAsyncLoader
    {

        protected override void OnStart()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("SceneAsync Load Asset {0} Start at {1} ", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnPaused()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("SceneAsync Load Asset {0} Paused at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnResume()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("SceneAsync Load Asset {0} Resumed at {1} ", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
        }

        protected override void OnError()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("SceneAsync Load Asset {0} Error at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
            
            base.OnError();
        }

        protected override void OnFinish()
        {
            if (BundleConfig.Bundle_Log)
                LogMgr.LogFormat("SceneAsync Load Asset {0} Finished at {1}", this.LoadResPath, GameSyncCtr.mIns.RenderFrameCount);
            
            this.InvokeHandler(true);

            if (this.onComplete == null && this.OnProgressHandler == null)
            {
                SceneOperation sceneop = this._AsyncOpation as SceneOperation;
                sceneop.EnableScene();
            }
            this.Dispose();
        }

        protected override void CreateMain()
        {
            this._BundleRef = ResBundleMgr.mIns.Cache.TryGetValue(this.loadingpkg);
            if (this._BundleRef == null)
                this.ThrowLogicError(loadingpkg.BundleName);
            else
                this.LoadScene();
        }

        private void LoadScene()
        {
            if (!this.isABMode)
            {
                SceneOperation asyOp = GameSceneCtr.LoadSceneAsync(Path.GetFileNameWithoutExtension(this.loadingpkg.BundleName));
                asyOp.DisableScene();
                this._AsyncOpation = asyOp;

                this._PushTaskAndExcute(this.loadingpkg, asyOp);
            }
            else
            {
                ///需要异步预实例化
                SceneOperation asyOp = GameSceneCtr.LoadSceneAsync(this._BundleRef.LoadName);
                asyOp.DisableScene();

                this._AsyncOpation = asyOp;
                this._PushTaskAndExcute(this.loadingpkg, asyOp);
            }

        }

        protected override void LoadMainAsyncRequest(AssetBundleRequest request)
        {
            this._BundleMainObject = request.asset;

            if (!this.isABMode)
            {
                GameSceneCtr.LoadScene(this.loadingpkg.BundleName);
                this._FinishAndRelease();
            }
            else
            {
                ///需要异步预实例化
                this._PushTaskAndExcute(this.loadingpkg, GameSceneCtr.LoadSceneAsync(this.loadingpkg.BundleName));
            }
        }

        protected override void LoadSceneAssetFinish()
        {
            this._FinishAndRelease();
        }
    }

}

