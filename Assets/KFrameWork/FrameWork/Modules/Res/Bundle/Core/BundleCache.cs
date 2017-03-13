
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{

    public class BundleCache  {
        /// <summary>
        /// loader层检测 for async
        /// </summary>
        private Dictionary<string, BaseBundleLoader> LoadingCaches;
        /// <summary>
        /// loading 的bundle 层检测 for aysnc
        /// </summary>
        private Dictionary<string,Action<string>> loadingBundleCaches;
        /// <summary>
        /// ab 引用缓存
        /// </summary>
        private Dictionary<string,SharedPtr<KAssetBundle>> ptrcaches ;
        /// <summary>
        /// bundle(上层类) 引用缓存
        /// </summary>
        private Dictionary<string, IBundleRef> RefCaches;

        /// <summary>
        /// bundle 引用数量
        /// </summary>
        public int CacheCnt
        {
            get
            {
                return this.RefCaches.Count;
            }
        }

        public IBundleRef this[string key]
        {
            get
            {
                try
                {
                    return this.RefCaches[key];
                }
                catch (Exception ex)
                {
                    LogMgr.LogErrorFormat("{0} Cause {1} ", key, ex);
                    return null;
                }

            }
            set
            {
                this.RefCaches[key] = value;
            }
        }


        public BundleCache()
        {
            this.ptrcaches = new Dictionary<string, SharedPtr<KAssetBundle>>(16);
            this.RefCaches = new Dictionary<string, IBundleRef>(16);
            this.LoadingCaches = new Dictionary<string, BaseBundleLoader>(4);
            this.loadingBundleCaches = new Dictionary<string, Action<string>>(4);
        }

        public void LogDebugInfo()
        {
            LogMgr.LogFormat("<color=#0000ffff>----------Bundle In Pool Cnt is {0}-------------</color>", this.CacheCnt);

            var keys = new List<string>(this.RefCaches.Keys);
            for (int i = 0; i < keys.Count; ++i)
            {
                string key = keys[i];
                IBundleRef bundle = this.RefCaches[key];
                string bundlename = bundle.name;
                BundlePkgInfo info = ResBundleMgr.mIns.BundleInformation.SeekInfo(bundle.name);
                if (info != null)
                    bundlename = info.AbFileName;

                LogMgr.LogFormat("<color=#00aa00ff>Root Bundle is {0}:{1} ,InsRefCount is {2} ,DependedCnt is {3} ,SelfRefCount:{4}</color>", bundle.name, bundlename, bundle.InstanceRefCount, bundle.DependCount, bundle.SelfRefCount);
                bundle.LogDepends();
            }
            LogMgr.Log("<color=#0000ffff>----------Bundle In Pool Log End-------------</color>");
        }

        public void UnLoadUnUsed(bool force)
        {
            int LimitUnLoadCnt = 10;
            if (force)
            {
                LimitUnLoadCnt = 65536;
            }

            var keys = new List<string>(this.RefCaches.Keys);

            for (int i = 0; i < keys.Count; ++i)
            {
                if (i < LimitUnLoadCnt)
                {
                    string key = keys[i];
                    IBundleRef bundle = this.RefCaches[key];
                    if (bundle == null  || (bundle.InstanceRefCount == 0 && bundle.SelfRefCount ==0 ))
                    {
                        bundle.UnLoad(true);
                    }
                }
                else
                    break;
            }

        }

        private SharedPtr<KAssetBundle> TryGetPtr(string abname, AssetBundle ab)
        {
            SharedPtr<KAssetBundle> ptr = null;
            if (this.ptrcaches.ContainsKey(abname))
            {
                ptr = this.ptrcaches[abname];
                if (ptr != null )
                {
                    if (!ptr.isAlive)
                    {
                        ptr.Restore(new KAssetBundle(ab));
                    }
                    else if (ptr.isAlive && ptr.get().Equals(ab))
                    {
                        return ptr;
                    }
                    else if (ptr.isAlive && !ptr.get().Equals(ab))
                    {
                        throw new FrameWorkException("assetbundle 资源引用异常");
                    }

                    return ptr;
                }
            }

            ptr = new SharedPtr<KAssetBundle>(new KAssetBundle(ab));
            this.ptrcaches[abname] = ptr;

            return ptr;

        }

        /// <summary>
        /// 移除引用的bundle
        /// </summary>
        /// <param name="bundlename"></param>
        /// <returns></returns>
        public bool Remove(string bundlename)
        {
            bool ret= this.RefCaches.Remove(bundlename) ;
            if (FrameWorkConfig.Open_DEBUG)
            {
                LogMgr.LogFormat("Bundle Will Remove From this Pool :{0} ,{1}",bundlename,ret?"True":"False");
            }
            return ret;
        }

        /// <summary>
        /// 是否存在这个bundle
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public bool Contains(string abname)
        {
            return this.RefCaches.ContainsKey(abname) ;
        }
        /// <summary>
        /// 是否存在这个bundle
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public bool Contains(BundlePkgInfo pkg)
        {
            return this.RefCaches.ContainsKey(pkg.AbFileName);
        }

        /// <summary>
        /// 是否这个主体资源正在加载
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsLoading(string key)
        {
            return this.LoadingCaches.ContainsKey(key);
        }
        /// <summary>
        /// 是否这个资源bundle正在加载
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsLoadingBundle(string key)
        {
            return this.loadingBundleCaches.ContainsKey(key) ;
        }
        /// <summary>
        /// 尝试获取引用的bundle
        /// </summary>
        /// <param name="assetname"></param>
        /// <returns></returns>
        public IBundleRef TryGetValue(BundlePkgInfo pkg)
        {
            return this.TryGetValue(pkg.AbFileName);
        }
        /// <summary>
        /// 尝试获取引用的bundle
        /// </summary>
        /// <param name="assetname"></param>
        /// <returns></returns>
        public IBundleRef TryGetValue(string assetname )
        {
            IBundleRef bundle;
            this.RefCaches.TryGetValue(assetname, out bundle);
            return bundle;
        }
        /// <summary>
        /// 获取正在加载的主体资源
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public BaseBundleLoader GetLoading(string key)
        {
            BaseBundleLoader loader =null;
            if (this.LoadingCaches.TryGetValue(key, out loader))
            {
                return loader;
            }
            return loader;
        }
        /// <summary>
        /// 触发加载的bundle的事件
        /// </summary>
        /// <param name="key"></param>
        public void InvokeBundleFinishEvent(string key)
        {
            if (this.ContainsLoadingBundle(key))
            {
                Action<string> cbk = this.loadingBundleCaches[key];
                if (cbk != null)
                    cbk(key);
            }
        }
        /// <summary>
        /// 为主体资源添加加载记录
        /// </summary>
        /// <param name="key"></param>
        /// <param name="loader"></param>
        /// <returns></returns>
        public bool PushLoading(string key, BaseBundleLoader loader)
        {
            if (!this.LoadingCaches.ContainsKey(key))
            {
                this.LoadingCaches.Add(key,loader);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 为主体资源移除记录
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveLoading(string key)
        {
            return this.LoadingCaches.Remove(key);
        }
        /// <summary>
        /// 添加加载的bundle的记录
        /// </summary>
        /// <param name="key"></param>
        /// <param name="loadercallback"></param>
        /// <returns></returns>
        public bool PushLoadingBundle(string key, Action<string> loadercallback =null)
        {
            if (!this.loadingBundleCaches.ContainsKey(key))
            {
                this.loadingBundleCaches.Add(key, loadercallback);
                return true;
            }
            else
            {
                if(this.loadingBundleCaches[key] != null)
                    this.loadingBundleCaches[key] += loadercallback;
                else
                    this.loadingBundleCaches[key] = loadercallback;
                return false;
            }
        }
        /// <summary>
        /// 移除正在加载的bundle
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveLoadingBundle(string key)
        {
            return this.loadingBundleCaches.Remove(key);
        }
        /// <summary>
        /// 添加bundle
        /// </summary>
        /// <param name="pkg"></param>
        /// <param name="ab"></param>
        /// <returns></returns>
        public IBundleRef PushAsset(BundlePkgInfo pkg, AssetBundle ab)
        {
            IBundleRef bundle = null;
            if(!this.Contains(pkg.AbFileName))
            {
                bundle = BundleRef.Create(this.TryGetPtr(pkg.AbFileName, ab),pkg.AbFileName, pkg.EditorPath, pkg.BundleName);
                this.RefCaches.Add(pkg.AbFileName,bundle);
            }
            else
            {
                bundle = this[pkg.AbFileName];
            }
            return bundle;
        }
#if UNITY_EDITOR
        public IBundleRef PushEditorAsset(BundlePkgInfo pkg, UnityEngine.Object ab)
        {
            IBundleRef bundle = null;
            if (!this.Contains(pkg.AbFileName))
            {
                bundle = new EditorRef(ab,pkg.AbFileName,pkg.EditorPath);
                this.RefCaches.Add(pkg.AbFileName, bundle);
            }
            else
            {
                bundle = this[pkg.AbFileName];
            }
            return bundle;
        }
#endif
    }
}


