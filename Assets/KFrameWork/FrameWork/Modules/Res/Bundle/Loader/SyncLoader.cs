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

            BundlePkgInfo pkginfo = this._SeekPkgInfo(name);

            for (int i = 0; i < pkginfo.Depends.Length; ++i)
            {
                if (this.isRunning())
                {
                    this._AddBundleDpend(pkginfo.Depends[i]);
                }
                else if (FrameWorkDebug.Open_DEBUG)
                    LogMgr.LogFormat("{0} loader 状态不符", this.targetname);
                else
                    break;
            }

            if (this.isRunning())
            {
                this._BundleRef = this._LoadBundle(pkginfo);
                this.CreateFromAsset(this._BundleRef,out this._Bundle );

                if (this.isRunning()) //double check
                {
                    this.LoadState = BundleLoadState.Finished;
                }
            }

        }


        private void _AddBundleDpend(string name)
        {
            BundlePkgInfo pkginfo = this._SeekPkgInfo(name);

            for (int i = 0; i < pkginfo.Depends.Length; ++i)
            {
                if (this.isRunning())
                {
                    this._AddBundleDpend(pkginfo.Depends[i]);
                }
                else if (FrameWorkDebug.Open_DEBUG)
                    LogMgr.LogFormat("{0} loader 状态不符", this.targetname);
                else
                    break;
            }

            if (this.isRunning())
            {
                this._LoadBundle(pkginfo);
            }
                
        }


        private IBundleRef _LoadBundle(BundlePkgInfo pkginfo)
        {

            IBundleRef bundle = this.LoadFullAssetToMem(pkginfo);

            if (bundle != null)
            {
                for (int i = 0; i < pkginfo.Depends.Length; ++i)
                {
                   IBundleRef depbund =  ResBundleMgr.mIns.Cache.TryGetValue(pkginfo.Depends[i]);
                    if (depbund != null)
                    {
                        depbund.NeedThis(bundle);
                    }
                    else
                    {
                        this.ThrowBundleMissing(pkginfo.Depends[i]);
                    }
                }
            }
            else
            {
                this.ThrowBundleMissing(pkginfo.BundleName);
            }

            return bundle;
        }


        public override void OnError()
        {
            base.OnError();

            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Sync Load Asset {0} Error ", this.targetname);
        }

        public override void OnFinish()
        {
            base.OnFinish();

            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Sync Load Asset {0} Finish ", this.targetname);
        }

        public override void OnPaused()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Sync Load Asset {0} Paused ", this.targetname);
        }

        public override void OnResume()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Sync Load Asset {0} Resume ", this.targetname);
        }

        public override void OnStart()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Sync Load Asset {0} Start ", this.targetname);
        }
    }
}


