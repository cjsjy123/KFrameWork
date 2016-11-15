using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public interface IBundleRef
    {
        string name { get; }

        int DependCount { get; }

        int RefCount { get; }

        void NeedThis(IBundleRef dep);

        void LogDepends();

        void Lock(LockType tp = LockType.DontDestroy | LockType.OnlyReadNoWrite);
        void UnLock(LockType tp = LockType.DontDestroy | LockType.OnlyReadNoWrite);
        void UnLoad(bool all);

        bool LoadAsset(out UnityEngine.Object target);
        bool Instantiate(out UnityEngine.Object target, Component c = null);
    }
}


