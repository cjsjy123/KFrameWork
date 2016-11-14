using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.IO;

namespace KFrameWork
{
    [SingleTon]
    public class ResBundleMgr  {

        private class WaitWWWTask : ITask
        {
            public bool KeepWaiting
            {
                get
                {
                    return !ResBundleMgr.mIns.Inited;
                }
            }

            public void Done()
            {

            }
        }

        public static ResBundleMgr mIns ;

        private BundleInfoFilter Info;

        public BundleInfoFilter BundleInforMation
        {
            get
            {
                
                return this.Info;
            }
        }

        private BundleCache _cache ;

        public BundleCache Cache
        {
            get
            {
                return this._cache;
            }
        }

        private bool _Inited;
        public bool Inited
        {
            get
            {
                return _Inited;
            }
        }

        private ResBundleMgr()
        {
            this.Info = new BundleBinaryInfo();
            this._cache = new BundleCache();
            using (gstring.Block())
            {
                string depFileName = BundlePathConvert.getBundleStreamPath(BundleConfig.ABVersionPath);
                MainLoop.getLoop().StartCoroutine(_LoadAssetInfos(depFileName));
            } 
        }

        private IEnumerator _LoadAssetInfos(string depFileName)
        {
            if (File.Exists(depFileName))
            {
                FileStream fs = new FileStream(depFileName, FileMode.Open, FileAccess.Read);
                this.Info.LoadFromMemory(fs);
                fs.Close();
                this._Inited = true;
            }
            else
            {
                WWW w = new WWW(BundlePathConvert.GetWWWPath(depFileName));
                yield return w;

                if (w.error == null)
                {
                    this.Info.LoadFromMemory(new MemoryStream(w.bytes));
                    this._Inited = true;
                }
                else
                {
                    Debug.LogError(string.Format("{0} not exist!", depFileName));
                }
            }
        }

        public static void YieldInited(Action DoneEvent)
        {
            if (!ResBundleMgr.mIns.Inited)
            {
                WaitTaskCommand cmd = WaitTaskCommand.Create(new WaitWWWTask(), DoneEvent);
                cmd.ExcuteAndRelease();
            }
            else
            {
                DoneEvent();
            }
        }

        public static void UnLoadUnused(bool force =false)
        {
            
        }

        public void PreLoad(string pathname)
        {

        }



        /// <summary>
        /// 如果不开启safe模式的话，用户需小写文件名
        /// </summary>
        /// <param name="pathname"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
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

        public void LoadAsync(string pathname, GameObject parent)
        {
            
        }

        public void LoadAsync(string pathname, Action<bool, AssetBundleResult> callback)
        {
            if (BundleConfig.SAFE_MODE)
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
        private GameObject _LoadGameobject(string pathname, GameObject parent)
        {
            SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();

            loader.Load(pathname);

            AssetBundleResult result = loader.GetABResult();
            UnityEngine.Object res = null;
            if (result.MainObject.Instantiate(out res))
            {
                if (res is GameObject)
                {
                    GameObject ins = res as GameObject;
                    ins.BindParent(parent);
                    return ins;

                }
                else
                {
                    if (BundleConfig.SAFE_MODE)
                        throw new FrameWorkResNotMatchException(string.Format("{0} Type Is Not Gameobject", result.MainObject));
                    else
                        return null;
                }
            }

            return null;

        }

//        private GameObject _LoadGameobjectAsync(bool Done,)
//        {
// 
//
//        }

        private void _SimpleLoad(string pathname, Action<bool, AssetBundleResult> callback)
        {
            SyncLoader loader = BaseBundleLoader.CreateLoader<SyncLoader>();
            loader.onComplete += callback;
            loader.Load(pathname);
        }

        private void _SimpleAysncLoad(string pathname, Action<bool, AssetBundleResult> callback)
        {
            AsyncLoader loader = BaseBundleLoader.CreateLoader<AsyncLoader>();
            loader.onComplete += callback;
            loader.Load(pathname);
        }
        #endregion

    }
}

