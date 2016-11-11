using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.IO;

namespace KFrameWork
{
    [SingleTon]
    public class BundleMgr  {

        public static BundleMgr mIns ;

        private BundleInfoFilter Info;

        public BundleInfoFilter BundleInforMation
        {
            get
            {
                return this.Info;
            }
        }

        private BundleMgr()
        {
            Info = new BundleInfo();
            MainLoop.getLoop().StartCoroutine(_LoadAssetInfos());
        }

        private IEnumerator _LoadAssetInfos()
        {
            string depFileName = BundlePathConvert.getBundleStreamPath(BundleConfig.ABVersionPath);

            if (File.Exists(depFileName))
            {
                FileStream fs = new FileStream(depFileName, FileMode.Open, FileAccess.Read);
                this.Info.LoadFromMemory(fs);
                fs.Close();
            }
            else
            {
                WWW w = new WWW(BundlePathConvert.GetWWWPath(depFileName));
                yield return w;

                if (w.error == null)
                {
                    this.Info.LoadFromMemory(new MemoryStream(w.bytes));
                }
                else
                {
                    Debug.LogError(string.Format("{0} not exist!", depFileName));
                }
            }
        }

        public void PreLoad(string pathname)
        {

        }

        public GameObject Load(string pathname, Component parent)
        {
            return this.Load(parent: parent.gameObject, pathname: pathname);
        }

        public GameObject Load(string pathname, Transform parent)
        {
            return this.Load( parent: parent.gameObject, pathname: pathname);
        }

        public GameObject Load(string pathname, GameObject parent)
        {
            /// 通过异常阻断下面逻辑执行
            if (BundleConfig.SAFE_MODE)
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
            if (BundleConfig.SAFE_MODE)
            {
                this._SimpleLoad(pathname, callback);
            }
            else
            {
                try
                {
                    this._SimpleLoad(pathname, callback);
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

        }

        #region private
        private GameObject _LoadGameobject(string pathname, GameObject parent)
        {
            SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();

            loader.Load(pathname);

            AssetBundleResult result = loader.GetABResult();

            if (result.MainObject is GameObject)
            {
                GameObject prefab = result.MainObject as GameObject;
                return prefab.InstancePrefab(parent);

            }
            else
            {
                if (BundleConfig.SAFE_MODE)
                    throw new FrameWorkResNotMatchException(string.Format("{0} Type Is Not Gameobject", result.MainObject));
                else
                    return null;
            }
        }

        private void _SimpleLoad(string pathname, Action<bool, AssetBundleResult> callback)
        {
            SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();
            loader.onComplete += callback;
            loader.Load(pathname);
        }
        #endregion

    }
}

