using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

namespace KFrameWork
{
    public class PreLoader : BaseBundleLoader
    {
        public override void OnPaused()
        {
            throw new NotImplementedException();
        }

        public override void OnResume()
        {
            throw new NotImplementedException();
        }

        public override void OnStart()
        {
            throw new NotImplementedException();
        }

        public override void Pause()
        {
            base.Pause();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void Resume()
        {
            base.Resume();
        }

        public override void OnError()
        {
            base.OnError();
        }

        public override void OnFinish()
        {
            base.OnFinish();
        }

        public override void Load(string name)
        {
            base.Load(name);
        }

        public override void DownLoad(string url)
        {
            base.DownLoad(url);
        }

    }
}


