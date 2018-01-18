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

        private BundleInfoFilter Info = null;

        public BundleInfoFilter BundleInformation
        {
            get
            {
                
                return this.Info;
            }
        }
        public BundleCache Cache { get; private set; }

        public string targetVerPath { get; private set; }

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
                {
                    LogMgr.LogError("Res Init Error");
                }
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
                    {
                        if (this.Info == null)
                        {
                            this.Info = new BundleTextInfo();
                        }
                    }
                    else
                    {
                        if (this.Info == null)
                        {
                            this.Info = new BundleBinaryInfo();
                        }
                    }

                    fs.Position = 0;
                    this.Info.LoadFromMemory(fs);
                }
            }
            this.isDone = true;
#else
            throw new FrameWorkException("Missing Tang pack try use self pack bundle");
#endif
    
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

        public void Reload()
        {
            this.Cache.Reload();
            isDone = false;
            this.Start();
        }


        public static void UnLoadUnused(bool force =false)
        {
            if (mIns != null)
            {
                mIns.Cache.UnLoadUnUsed(force);
            }
        }
        /// <summary>
        /// 搜查并删除
        /// </summary>
        /// <param name="name"></param>
        /// <param name="force"></param>
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
                ABLoaderDispatcher.PreLoadForAssets(pathname);
            }
            else
            {
                try
                {
                    ABLoaderDispatcher.PreLoadForAssets(pathname);
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
                return ABLoaderDispatcher.SyncLoadForAssets(pathname);
            }
            else
            {
                try
                {
                    return ABLoaderDispatcher.SyncLoadForAssets(pathname);
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
            return this.Load( pathname, parent == null? null: parent.gameObject);
        }

        public GameObject Load(string pathname, Transform parent)
        {
            return this.Load(pathname, parent == null ? null : parent.gameObject);
        }

        /// <summary>
        /// 直接加载，并不检查实例对象,并不建议这样使用
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        public AssetBundleResult Load(string pathname)
        {
            return ABLoaderDispatcher.SyncLoadForAssetsWithResult(pathname, null);
        }


        public GameObject Load(string pathname, GameObject parent)
        {
            /// 通过异常阻断下面逻辑执行
            if (!BundleConfig.SAFE_MODE)
            {
                return ABLoaderDispatcher.SyncLoadForGameObjects(pathname, parent);
            }
            else
            {
                try
                {
                    return ABLoaderDispatcher.SyncLoadForGameObjects(pathname, parent);
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
                ABLoaderDispatcher.SyncLoad(pathname, callback);
            }
            else
            {
                try
                {
                    ABLoaderDispatcher.SyncLoad(pathname, callback);
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

            if (!BundleConfig.SAFE_MODE)
            {
                ABLoaderDispatcher.LoadSceneAsync(pathname, OnProgressHandler, oncomplete);
            }
            else
            {
                try
                {
                    ABLoaderDispatcher.LoadSceneAsync(pathname, OnProgressHandler, oncomplete);
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

        public void LoadScene(KEnum scene)
        {
            LoadScene((string)scene);
        }

        public void LoadScene(string pathname)
        {

            if (!BundleConfig.SAFE_MODE)
            {
                ABLoaderDispatcher.LoadScene(pathname);
            }
            else
            {
                try
                {
                    ABLoaderDispatcher.LoadScene(pathname);
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

        public void LoadAsync(string pathname, Component parent, Action<bool, AssetBundleResult> onCompelete = null)
        {
            this.LoadAsync(pathname,parent !=null? parent.gameObject:null, onCompelete);
        }

        public void LoadAsync(string pathname, GameObject parent, Action<bool, AssetBundleResult> onCompelete = null)
        {
            if (!BundleConfig.SAFE_MODE)
            {
                ABLoaderDispatcher.AsyncLoadForGameObject(pathname,parent, onCompelete);
            }
            else
            {
                try
                {
                    ABLoaderDispatcher.AsyncLoadForGameObject(pathname, parent, onCompelete);
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
                ABLoaderDispatcher.AsyncLoadForAssets(pathname, callback);
            }
            else
            {
                try
                {
                    ABLoaderDispatcher.AsyncLoadForAssets(pathname, callback);
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
    }
}

