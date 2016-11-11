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
        public override void DownLoad(string url)
        {
            
        }

        public override void Load(string name)
        {
            base.Load(name);
            BundlePkgInfo pkginfo = BundleMgr.mIns.BundleInforMation.TrygetInfo(name);

            if (string.IsNullOrEmpty(pkginfo.realpath))
            {
                this.loadState = BundleLoadState.Error;
                throw new FrameWorkException(string.Format( "Not Found {0}",name),ExceptionType.Higher_Excetpion);
            }



            for (int i = 0; i < pkginfo.Depends.Length; ++i)
            {
                if (this.isRunning())
                    this.Load(pkginfo.Depends[i]);
                else
                    break;
            }

            if(this.isRunning())
                this._LoadBundle(pkginfo);
        }

        #region private
        private void _LoadBundle(BundlePkgInfo pkginfo)
        {
            this._Bundle = this.LoadFullAsset(pkginfo);

            if (this.isRunning())
            {
                this.loadState = BundleLoadState.Finished;
            }
        }

        #endregion
        public override void OnError()
        {
            base.OnError();
        }

        public override void OnFinish()
        {
            base.OnFinish();
        }

        public override void OnPaused()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Load Asset {0} Paused ", this.targetname);
        }

        public override void OnResume()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Load Asset {0} Resume ", this.targetname);
        }

        public override void OnStart()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Load Asset {0} Start ", this.targetname);
        }
    }
}


