using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
#if USE_TANGAB
using Tangzx;
using Tangzx.ABSystem;

public class TangBundleRef : IBundleRef
{
    public int DependCount
    {
        get
        {
            return assetInfo.data.dependencies.Length;
        }
    }

    public string filename
    {
        get
        {
            return assetInfo.data.fullName;
        }
    }

    public int InstanceRefCount
    {
        get
        {
            return assetInfo.references.Count;
        }
    }

    public string LoadName
    {
        get
        {
            return assetInfo.data.shortName;
        }
    }

    public UnityEngine.Object MainObject
    {
        get
        {
            return assetInfo.mainObject;
        }
    }

    public string name
    {
        get
        {
            return assetInfo.bundleName;
        }
    }

    public int SelfRefCount
    {
        get
        {
            return assetInfo.refCount;
        }
    }

    public bool SupportAsync
    {
        get
        {
            return true;
        }
    }

    private AssetBundleInfo assetInfo;

    public TangBundleRef(AssetBundleInfo info)
    {
        assetInfo = info;
    }

    public string[] GetAllAssetNames()
    {
        return assetInfo.GetAllAssetNames();
    }

    public bool Instantiate(out UnityEngine.Object target, Component c = null)
    {
        target = assetInfo.Instantiate();
        return true;
    }

    public UnityEngine.Object InstantiateWithBundle(UnityEngine.Object prefab, Component c = null)
    {
        return assetInfo.Instantiate();
    }

    public bool LoadAllAssets(out UnityEngine.Object[] target)
    {
        throw new NotImplementedException();
    }

    public bool LoadAsset(out UnityEngine.Object target)
    {
        target = assetInfo.LoadAsset<UnityEngine.Object>(null, GetAllAssetNames()[0]);
        return true;
    }

    public bool LoadAsset(string abname, out UnityEngine.Object target)
    {
        target = assetInfo.LoadAsset<UnityEngine.Object>(null, abname);
        return true;
    }

    public AssetBundleRequest LoadAssetAsync()
    {
        throw new NotImplementedException();
    }

    public void Lock(LockType tp = LockType.END)
    {
        
    }

    public void LogDepends()
    {
        
    }

    public void NeedThis(IBundleRef dep)
    {
        
    }

    public void Release()
    {
        
    }

    public void Release(UnityEngine.Object o)
    {
       
    }

    public void Retain()
    {
        
    }

    public void Retain(UnityEngine.Object o)
    {
        
    }

    public UnityEngine.Object SimpleInstantiate()
    {
        return assetInfo.Instantiate();
    }

    public void UnLoad(bool all)
    {
        
    }

    public void UnLock(LockType tp = LockType.END)
    {
       
    }
}
#endif