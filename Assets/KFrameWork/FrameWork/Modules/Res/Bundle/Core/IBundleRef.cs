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

        string filename { get; }

        string LoadName { get; }

        int DependCount { get; }

        int InstanceRefCount { get; }

        int SelfRefCount { get; }

        UnityEngine.Object MainObject { get; }

        bool SupportAsync { get; }

        void NeedThis(IBundleRef dep);

        void Retain();

        void Retain(UnityEngine.Object o);

        void Release();

        void Release(UnityEngine.Object o);

        void LogDepends();

        void UnLoad(bool all);

        string[] GetAllAssetNames();
        bool LoadAllAssets(out UnityEngine.Object[] target);
        bool LoadAsset(out UnityEngine.Object target);
        bool LoadAsset(string abname, out UnityEngine.Object target);
        AssetBundleRequest LoadAssetAsync();

        bool Instantiate(out UnityEngine.Object target, Component c = null);
        UnityEngine.Object SimpleInstantiate();
        UnityEngine.Object InstantiateWithBundle(UnityEngine.Object prefab, Component c = null);
    }
}


