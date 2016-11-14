using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

namespace KFrameWork
{
    public class AsyncLoader :BaseBundleLoader {

        private class AsyncBundleTask:ITask
        {
            public AsyncOperation custom;

            public bool KeepWaiting {
                get {
                    return !custom.isDone;
                }
            }
        }

        private int LoadedCnt =0;

        private int NeedLoadedCnt =0;

        private BundlePkgInfo _info;

        private AsyncBundleTask m_task;

        private AsyncBundleTask Task
        {
            get
            {
                if(m_task == null)
                    m_task = new AsyncBundleTask();
                return m_task;
            }
        }

        public override void OnStart ()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Async Load Asset {0} Start ", this.targetname);
        }

        public override void OnPaused ()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Async Load Asset {0} Paused ", this.targetname);
        }

        public override void OnResume ()
        {
            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Async Load Asset {0} Resumed ", this.targetname);
        }

        public override void OnError ()
        {
            base.OnError ();

            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Async Load Asset {0} Error ", this.targetname);
        }

        public override void OnFinish ()
        {
            base.OnFinish ();

            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Async Load Asset {0} Finished ", this.targetname);
        }

        public override void Load (string name)
        {
            base.Load (name);

            this._info =this._SeekPkgInfo(name);

            this.NeedLoadedCnt += this._info.Depends.Length;

            for (int i = 0; i < this._info.Depends.Length; ++i)
            {
                if (this.isRunning())
                {
                    this._AddBundleDpend(this._info.Depends[i]);

                } 
                else if (FrameWorkDebug.Open_DEBUG)
                    LogMgr.LogFormat("{0} asyncloader 状态不符",this.targetname);
                else
                    break;
            }

            if(this.isRunning())
            {
                this.LoadMain();
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
                    LogMgr.LogFormat("{0} asyncloader 状态不符",this.targetname);
                else
                    break;
            }

            if(this.isRunning())
            {
                this._LoadBundle(pkginfo);
            }
        }

        private void _LoadBundle( BundlePkgInfo pkginfo)
        {
            if(ResBundleMgr.mIns.Cache.Contains(pkginfo))
            {
                this.LoadedCnt++;
            }
            else
            {

                m_task.custom = this.LoadFullAssetToMemAsync(pkginfo);

                WaitTaskCommand cmd = WaitTaskCommand.Create(m_task,_AsyncLoad);
                cmd.ExcuteAndRelease();
            }
        }

        private void _AsyncLoad()
        {

            LoadedCnt++;

            if(LoadedCnt == NeedLoadedCnt && this.isRunning())
            {

                if(ResBundleMgr.mIns.Cache.Contains(this._info))
                {
                    this.LoadedCnt++;
                }
                else
                {
                    _LoadBundle(this._info);
                }
            }
            else if(LoadedCnt == NeedLoadedCnt+1 && this.isRunning())
            {
                this.LoadMain();
            }
            
        }

        private void LoadMain()
        {
            this._BundleRef = ResBundleMgr.mIns.Cache.TryGetValue(this._info);
            this.CreateFromAsset(this._BundleRef,out this._Bundle );

            if (this.isRunning()) //double check
            {
                this.loadState = BundleLoadState.Finished;
            }
        }

//        private void AsyncCreateAsset()
//        {
//            m_task = this._BundleRef.LoadAsset();
//
//            WaitTaskCommand cmd = WaitTaskCommand.Create(m_task,_AsyncLoad);
//            cmd.ExcuteAndRelease();
//        }


        public override void DownLoad (string url)
        {
            base.DownLoad (url);
        }

    }

}

