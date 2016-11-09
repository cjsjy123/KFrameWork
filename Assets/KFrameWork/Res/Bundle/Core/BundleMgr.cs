using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    [SingleTon]
    public class BundleMgr  {

        public static BundleMgr mIns ;

        public UnityEngine.Object Load(string pathname)
        {
            return null;
        }

        public void LoadAsync(string pathname,Action<string,UnityEngine.Object> callback)
        {

        }

    }
}

