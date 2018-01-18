using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
#if USE_TANGAB
using Tangzx.ABSystem;
#endif

public static class ABLoaderDispatcher  {

    private enum ABLoaderMode
    {
        Self,
        ABTang,
    }

    private static ABLoaderMode Mode = ABLoaderMode.Self;

#if USE_TANGAB
    private static GameObject _TangRoot;
    private static AssetBundleManager manager;
    private static GameObject TangRoot
    {
        get
        {
            if (TangRoot == null)
            {
                _TangRoot = new GameObject("TangRoot");
                GameObject.DontDestroyOnLoad(_TangRoot);

                manager = _TangRoot.AddComponent<AssetBundleManager>();
                manager.Init(null);
                return _TangRoot;
            }
            return _TangRoot;
        }
    }
#endif

    public static void PreLoadForAssets(string pathname)
    {
        if (Mode == ABLoaderMode.Self)
        {
            if (!ResBundleMgr.mIns.Cache.ContainsLoading(pathname))
            {
                PreLoader loader = BaseBundleLoader.CreateLoader<PreLoader>();
                if (loader == null)
                    loader = new PreLoader();

                loader.Load(pathname);
            }
            else
            {
                LogMgr.LogErrorFormat("{0} is loading ", pathname);
            }
        }
        else if (Mode == ABLoaderMode.ABTang)
        {
#if USE_TANGAB

#endif
        }

    }

    public static void SyncLoad(string pathname, Action<bool, AssetBundleResult> callback)
    {
        if (Mode == ABLoaderMode.Self)
        {
            if (!ResBundleMgr.mIns.Cache.ContainsLoading(pathname))
            {
                SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();
                if (loader == null)
                    loader = new SyncLoader();

                if (callback != null)
                    loader.onComplete += callback;
                loader.Load(pathname);

                loader.Dispose();
            }
            else
            {
                LogMgr.LogErrorFormat("{0} is loading ", pathname);
            }
        }
        else if (Mode == ABLoaderMode.ABTang)
        {
#if USE_TANGAB
            BundlePkgInfo info = ResBundleMgr.mIns.BundleInformation.SeekInfo(pathname);
            string resultpath = info.EditorPath.Replace("/", ".");
            manager.Load(resultpath,(result)=>
            {
                if (callback != null)
                    callback(true,Info2Result(result));
            });
#endif
        }

    }
#if USE_TANGAB
    private static AssetBundleResult Info2Result(AssetBundleInfo info)
    {
        return new AssetBundleResult(new TangBundleRef(info), info.mainObject,null, null, info.bundleName);

    }
#endif

    public static UnityEngine.Object SyncLoadForAssets(string pathname)
    {
        if (Mode == ABLoaderMode.Self)
        {
            if (!ResBundleMgr.mIns.Cache.ContainsLoading(pathname))
            {
                SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();
                if (loader == null)
                    loader = new SyncLoader();

                loader.Load(pathname);

                UnityEngine.Object result = loader.GetABResult().LoadedObject;
                loader.Dispose();

                return result;
            }
            else
            {
                LogMgr.LogErrorFormat("{0} is loading ", pathname);
            }
        }
        else if (Mode == ABLoaderMode.ABTang)
        {
#if USE_TANGAB
            BundlePkgInfo info = ResBundleMgr.mIns.BundleInformation.SeekInfo(pathname);
            string resultpath = info.EditorPath.Replace("/", ".");
            manager.Load(resultpath);
#endif
        }

        return null;
    }

    public static GameObject SyncLoadForGameObjects(string pathname, GameObject parent)
    {
        if (Mode == ABLoaderMode.Self)
        {
            if (!ResBundleMgr.mIns.Cache.ContainsLoading(pathname))
            {
                SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();

                if (loader == null)
                    loader = new SyncLoader();

                loader.Load(pathname, parent);

                AssetBundleResult result = loader.GetABResult();
                loader.Dispose();

                GameObject target = result.InstancedObject;
                if (target == null)
                    target = result.SimpleInstance();

                return target;
            }
            else
            {
                LogMgr.LogErrorFormat("{0} is loading ",pathname);
            }
        }
        else if (Mode == ABLoaderMode.ABTang)
        {

#if USE_TANGAB

#endif

        }

        return null;
    }

    public static AssetBundleResult SyncLoadForAssetsWithResult(string pathname, Action<bool, AssetBundleResult> callback)
    {
        if (Mode == ABLoaderMode.Self)
        {
            if (!ResBundleMgr.mIns.Cache.ContainsLoading(pathname))
            {
                SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();
                if (loader == null)
                    loader = new SyncLoader();

                if (callback != null)
                    loader.onComplete += callback;
                loader.Load(pathname);
                //get result
                AssetBundleResult result = loader.GetABResult();
                loader.Dispose();
                return result;
            }
            else
            {
                LogMgr.LogErrorFormat("{0} is loading ", pathname);
            }
        }
        else if (Mode == ABLoaderMode.ABTang)
        {
#if USE_TANGAB

#endif
        }

        return default(AssetBundleResult);

    }

    public static void AsyncLoadForAssets(string pathname, Action<bool, AssetBundleResult> callback)
    {
        if (Mode == ABLoaderMode.Self)
        {
            if (!ResBundleMgr.mIns.Cache.ContainsLoading(pathname))
            {
                AsyncLoader loader = BaseBundleLoader.CreateLoader<AsyncLoader>();
                if (loader == null)
                    loader = new AsyncLoader();

                if (callback != null)
                    loader.onComplete += callback;
                loader.Load(pathname);
            }
            else
            {
                BaseBundleLoader baseLoader = ResBundleMgr.mIns.Cache.GetLoading(pathname);

                if (callback != null)
                    baseLoader.onComplete += callback;
            }
        }
        else if (Mode == ABLoaderMode.ABTang)
        {

#if USE_TANGAB


#endif
        }

    }

    public static void AsyncLoadForGameObject(string pathname, GameObject parnet, Action<bool, AssetBundleResult> callback)
    {
        if (Mode == ABLoaderMode.Self)
        {
            if (!ResBundleMgr.mIns.Cache.ContainsLoading(pathname))
            {
                AsyncLoader loader = BaseBundleLoader.CreateLoader<AsyncLoader>();
                if (loader == null)
                    loader = new AsyncLoader();

                if (callback != null)
                    loader.onComplete += callback;
                loader.Load(pathname,parnet);
            }
            else
            {
                BaseBundleLoader baseLoader = ResBundleMgr.mIns.Cache.GetLoading(pathname);

                if (callback != null)
                    baseLoader.onComplete += callback;
            }
        }
        else if (Mode == ABLoaderMode.ABTang)
        {
#if USE_TANGAB

#endif
        }

    }

    public static void LoadScene(string name)
    {
        SceneSyncLoader loader = BaseBundleLoader.CreateLoader<SceneSyncLoader>();
        if (loader == null)
            loader = new SceneSyncLoader();

        loader.Load(name);
    }

    public static void LoadSceneAsync(string name, Action<BaseBundleLoader, int, int> OnProgressHandler, Action<bool, AssetBundleResult> oncomplete)
    {
        if (ResBundleMgr.mIns.Cache.ContainsLoading(name))
        {
            BaseBundleLoader loader = ResBundleMgr.mIns.Cache.GetLoading(name);

            if (oncomplete != null)
                loader.onComplete += oncomplete;

            if (OnProgressHandler != null)
                loader.OnProgressHandler += OnProgressHandler;
        }
        else
        {
            SceneAsyncLoader loader = BaseBundleLoader.CreateLoader<SceneAsyncLoader>();
            if (loader == null)
                loader = new SceneAsyncLoader();

            if (oncomplete != null)
                loader.onComplete += oncomplete;

            if (OnProgressHandler != null)
                loader.OnProgressHandler += OnProgressHandler;

            loader.Load(name);
        }
    }
}
