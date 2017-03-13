using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.IO;

namespace KFrameWork
{
    /// <summary>
    /// assetbundle 管理器，Config中的 safemode 跑得更安全，但是开销更大，unsafe 则会麻烦点，有一些规则，比如assetname 都默认为小写
    /// </summary>
    public class ResBundleMgr  {

        private class WaitWWWTask : ITask
        {
            public bool KeepWaiting
            {
                get
                {
                    return !ResBundleMgr.mIns.isDone;
                }
            }
        }


        private static ResBundleMgr _mIns;
        public static ResBundleMgr mIns
        {
            get
            {
                if (_mIns == null)
                    _mIns = new ResBundleMgr();
                return _mIns;
            }
        }

        private BundleInfoFilter Info;

        public BundleInfoFilter BundleInformation
        {
            get
            {
                
                return this.Info;
            }
        }
        public BundleCache Cache { get; private set; }

        private string targetVerPath;

        public bool isDone { get; private set; }

        private ResBundleMgr()
        {
            this.Cache = new BundleCache();

            Start();
        }

        private void Start()
        {
            string depFileName = null;
            using (gstring.Block())
            {
                depFileName = BundlePathConvert.GetRunningPath(BundleConfig.ABVersionPath);
                targetVerPath = string.Copy(depFileName );

                AssetBundle ab = null;
                byte[] assetbytes = CreateAssetbundleBytes(out ab);
                if (assetbytes != null && assetbytes.Length > 4)
                {
                    LoadAssetbundleBytes(assetbytes);
                    ab.Unload(true);
                }
                else
                    TaskManager.CreateTask(_LoadAssetInfos()).Start();
            }
        }

        private void LoadAssetbundleBytes(byte[] abbytes)
        {
            byte[] bys = new byte[abbytes.Length];
            Array.Copy(abbytes, bys, abbytes.Length);
#if USE_TANGAB
            using (MemoryStream fs = new MemoryStream(bys))
            {
                BinaryReader br = new BinaryReader(fs);
                if (br.ReadChar() == 'A' && br.ReadChar() == 'B' && br.ReadChar() == 'D')
                {
                    if (br.ReadChar() == 'T')
                        this.Info = new BundleTextInfo();
                    else
                        this.Info = new BundleBinaryInfo();

                    fs.Position = 0;
                    this.Info.LoadFromMemory(fs);
                }
            }
#else
            throw new FrameWorkException("Missing Tang pack try use self pack bundle");
#endif

            this.isDone = true;
        }

        private byte[] CreateAssetbundleBytes(out AssetBundle ab)
        {
            ab = AssetBundle.LoadFromFile(BundlePathConvert.GetRunningPath(BundleConfig.depAssetBundlePath));
            if (ab != null)
            {
                BundlePkgAsset asset = ab.LoadAsset<BundlePkgAsset>(ab.GetAllAssetNames()[0]);
                return asset.bytes;
            }
            return null;
        }

        private IEnumerator _LoadAssetInfos()
        {
            if (File.Exists(targetVerPath))
            {
#if USE_TANGAB
                using (FileStream fs = new FileStream(targetVerPath, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader br = new BinaryReader(fs);
                    if (br.ReadChar() == 'A' && br.ReadChar() == 'B' && br.ReadChar() == 'D')
                    {
                        if (br.ReadChar() == 'T')
                            this.Info = new BundleTextInfo();
                        else
                            this.Info = new BundleBinaryInfo();

                        fs.Position = 0;
                        this.Info.LoadFromMemory(fs);
                    }
                    br.Close();
                    this.isDone = true;
                }
#else
                throw new FrameWorkException("Missing Tang pack try use self pack bundle");
#endif
            }
            else
            {
                WWW w = new WWW(BundlePathConvert.GetWWWPath(targetVerPath));
                yield return w;
                if (w.error == null)
                {
                    LoadAssetbundleBytes(w.bytes);
                }
                else
                {
                    LogMgr.LogError(string.Format("{0} not exist! info:{1}", targetVerPath, w.error));
                }
            }

            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.Log("bundle info Finish");
        }

        public static void YieldInited(Action<WaitTaskCommand> DoneEvent)
        {
            if (mIns == null || !ResBundleMgr.mIns.isDone)
            {
                WaitTaskCommand cmd = WaitTaskCommand.Create(new WaitWWWTask(), DoneEvent);
                cmd.Excute();
            }
            else
            {
                DoneEvent(null);
            }
        }

        public static void UnLoadUnused(bool force =false)
        {
            if (mIns != null)
            {
                mIns.Cache.UnLoadUnUsed(force);
            }
        }

        public static void SeekUnLoad(string name, bool force = false)
        {
            if (mIns != null)
            {
                BundlePkgInfo info = mIns.Info.SeekInfo(name);
                if (info != null)
                {
                    IBundleRef bundle = mIns.Cache.TryGetValue(info);
                    if (bundle != null)
                    {
                        bundle.UnLoad(force);
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("The Bundle which your seek isnt in the Pool Or Loaded Yield :{0}", name);
                    }
                }
                else
                {
                    LogMgr.LogErrorFormat("Not Found your Seek Info :{0}",name);
                }
            }
        }

        public void PreLoad(string pathname)
        {
            if (!BundleConfig.SAFE_MODE)
            {
                this._SimplePreLoad(pathname);
            }
            else
            {
                try
                {
                    this._SimplePreLoad(pathname);
                }
                catch (FrameWorkException ex)
                {
                    LogMgr.LogException(ex);
                    ex.RaiseExcption();
                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                }
            }
        }

        public UnityEngine.Object LoadAsset(string pathname)
        {
            if (!BundleConfig.SAFE_MODE)
            {
                return this._LoadAsset(pathname);
            }
            else
            {
                try
                {
                    return this._LoadAsset(pathname);
                }
                catch (FrameWorkException ex)
                {
                    LogMgr.LogException(ex);
                    ex.RaiseExcption();
                    return null;
                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 如果不开启safe模式的话，用户需小写文件名
        /// </summary>
        /// <param name="pathname"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject Load(string pathname, Component parent)
        {
            return this.Load( pathname, parent.gameObject);
        }

        public GameObject Load(string pathname, Transform parent)
        {
            return this.Load(pathname,  parent.gameObject);
        }

        /// <summary>
        /// 直接加载，并不检查实例对象
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        public AssetBundleResult Load(string pathname)
        {
            BaseBundleLoader loader= this._SimpleLoad(pathname, null);
            AssetBundleResult result = loader.GetABResult();

            loader.Dispose();
            return result;
        }


        public GameObject Load(string pathname, GameObject parent)
        {
            /// 通过异常阻断下面逻辑执行
            if (!BundleConfig.SAFE_MODE)
            {
                return this._LoadGameobject(pathname, parent);
            }
            else
            {
                try
                {
                    return this._LoadGameobject(pathname, parent);
                }
                catch (FrameWorkException ex)
                {
                    LogMgr.LogException(ex);
                    ex.RaiseExcption();
                    return null;
                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                    return null;
                }
            }
        }

        public void Load(string pathname, Action<bool, AssetBundleResult> callback )
        {
            if (!BundleConfig.SAFE_MODE)
            {
                BaseBundleLoader loader = this._SimpleLoad(pathname, callback);
                loader.Dispose();
            }
            else
            {
                try
                {
                    BaseBundleLoader loader = this._SimpleLoad(pathname, callback);
                    loader.Dispose();
                }
                catch (FrameWorkException ex)
                {
                    LogMgr.LogException(ex);
                    ex.RaiseExcption();

                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                }
            }
        }

        public void LoadSceneAsync(KEnum scene, Action<bool, AssetBundleResult> oncomplete)
        {
            LoadSceneAsync(scene, null, oncomplete);
        }

        public void LoadSceneAsync(KEnum scene, Action<BaseBundleLoader, int, int> OnProgressHandler)
        {
            LoadSceneAsync(scene, OnProgressHandler,null);
        }

        public void LoadSceneAsync(KEnum scene)
        {
            LoadSceneAsync(scene, null,null);
        }

        public void LoadSceneAsync(string pathname, Action<BaseBundleLoader, int, int> OnProgressHandler,Action<bool,AssetBundleResult> oncomplete)
        {
            if (Cache.ContainsLoading(pathname))
            {
                BaseBundleLoader loader = Cache.GetLoading(pathname);

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

                loader.Load(pathname);
            }
        }

        public void LoadAsync(string pathname, Component parent, Action<bool, AssetBundleResult> onCompelete = null)
        {
            this.LoadAsync(pathname,parent !=null? parent.gameObject:null, onCompelete);
        }

        //public void LoadAsync(string pathname, Transform parent, Action<bool, AssetBundleResult> onCompelete = null)
        //{
        //    this.LoadAsync(pathname, parent != null ? parent.gameObject : null, onCompelete);
        //}

        public void LoadAsync(string pathname, GameObject parent, Action<bool, AssetBundleResult> onCompelete = null)
        {
            if (!BundleConfig.SAFE_MODE)
            {
                this._LoadGameobjectAsync(pathname,parent, onCompelete);
            }
            else
            {
                try
                {
                    this._LoadGameobjectAsync(pathname, parent, onCompelete);
                }
                catch (FrameWorkException ex)
                {
                    LogMgr.LogException(ex);
                    ex.RaiseExcption();
                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                }
            }
        }

        public void LoadAsync(string pathname, Action<bool, AssetBundleResult> callback)
        {
            if (!BundleConfig.SAFE_MODE)
            {
                this._SimpleAysncLoad(pathname, callback);
            }
            else
            {
                try
                {
                    this._SimpleAysncLoad(pathname, callback);
                }
                catch (FrameWorkException ex)
                {
                    LogMgr.LogException(ex);
                    ex.RaiseExcption();
                }
                catch (Exception ex)
                {
                    LogMgr.LogException(ex);
                }
            }
        }

#region private

        private UnityEngine.Object _LoadAsset(string pathname)
        {
            SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();

            if (loader == null)
                loader = new SyncLoader();

            loader.Load(pathname);

            AssetBundleResult result = loader.GetABResult();
            loader.Dispose();

            return result.LoadedObject;
        }


        private GameObject _LoadGameobject(string pathname, GameObject parent)
        {
            SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();

            if (loader == null)
                loader = new SyncLoader();

            loader.PreParedGameObject = parent;
            loader.Load(pathname);

            AssetBundleResult result = loader.GetABResult();
            loader.Dispose();

            GameObject target = result.InstancedObject;
            if(target == null)
                target = result.SimpleInstance();

            return target;
        }

        private BaseBundleLoader _LoadGameobjectAsync(string pathname,GameObject parnet,Action<bool,AssetBundleResult> onCompelete =null)
        {
            if (Cache.ContainsLoading(pathname))
            {
                BaseBundleLoader baseLoader= Cache.GetLoading(pathname);

                if (onCompelete != null)
                    baseLoader.onComplete += onCompelete;
                return baseLoader;
            }

            AsyncLoader loader = BaseBundleLoader.CreateLoader<AsyncLoader>();
            if (loader == null)
                loader = new AsyncLoader();

            if(onCompelete != null)
                loader.onComplete += onCompelete;
            loader.PreParedGameObject = parnet;
            loader.Load(pathname);
            return loader;
        }

        private BaseBundleLoader _SimplePreLoad(string pathname)
        {
            if (Cache.ContainsLoading(pathname))
            {
                BaseBundleLoader baseLoader = Cache.GetLoading(pathname);
                return baseLoader;
            }

            PreLoader loader = BaseBundleLoader.CreateLoader<PreLoader>();
            if (loader == null)
                loader = new PreLoader();

            loader.Load(pathname);
            return loader;
        }

        private BaseBundleLoader _SimpleLoad(string pathname, Action<bool, AssetBundleResult> callback)
        {
            SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();
            if (loader == null)
                loader = new SyncLoader();
            
            if(callback != null)
                loader.onComplete += callback;
            loader.Load(pathname);
            return loader;
        }

        private BaseBundleLoader _SimpleAysncLoad(string pathname, Action<bool, AssetBundleResult> callback)
        {
            if (Cache.ContainsLoading(pathname))
            {
                BaseBundleLoader baseLoader = Cache.GetLoading(pathname);

                if (callback != null)
                    baseLoader.onComplete += callback;
                return baseLoader;
            }

            AsyncLoader loader = BaseBundleLoader.CreateLoader<AsyncLoader>();
            if (loader == null)
                loader = new AsyncLoader();
            
            if(callback != null)
                loader.onComplete += callback;
            loader.Load(pathname);
            return loader;
        }
#endregion

    }
}

