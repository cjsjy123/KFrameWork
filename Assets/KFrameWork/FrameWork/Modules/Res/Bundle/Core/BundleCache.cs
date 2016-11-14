
//#define AB_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{

    public class BundleCache  {

        private Dictionary<string,SharedPtr<KAssetBundle>> caches ;

        private Dictionary<string, IBundleRef> RefCaches;

        public int CacheCnt
        {
            get
            {
                return this.RefCaches.Count;
            }
        }

        public BundleCache()
        {
            this.caches = new Dictionary<string, SharedPtr<KAssetBundle>>(16);
            this.RefCaches = new Dictionary<string, IBundleRef>(16);
        }

        public void LogDebugInfo()
        {
            LogMgr.LogFormat("<color=#001111ff>Bundle In Pool Cnt is {0}</color>", this.CacheCnt);

            var keys = new List<string>(this.RefCaches.Keys);
            for (int i = 0; i < keys.Count; ++i)
            {
                string key = keys[i];
                IBundleRef bundle = this.RefCaches[key];

                LogMgr.LogFormat("<color=#00aa00ff>Root Bundle is {0} ,RefCount is {1} ,DependedCnt is {2}</color>", bundle.name,bundle.RefCount,bundle.DepndCount);

                bundle.LogDepends();
            }
        }

        public void UnLoadUnUsed(bool force)
        {
            int LimitUnLoadCnt = 10;
            if (force)
            {
                LimitUnLoadCnt = 65536;
            }

            var keys = new List<string>(this.RefCaches.Keys);

            for (int i=0; i < keys.Count;++i)
            {
                if (i < LimitUnLoadCnt)
                {
                    string key = keys[i];
                    IBundleRef bundle = this.RefCaches[key];
                    if (bundle == null || bundle.DepndCount ==0 || bundle.RefCount ==0)
                    {
                        bundle.UnLoad(true);
                    }
                }
                else
                    break;
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
                if (!this.Contains(key))
                {
                    this.RefCaches.Add(key, value);
                }
                else
                {
                    this.RefCaches[key] = value;
                }
            }
        }

        private SharedPtr<KAssetBundle> TryGetPtr(string abname, AssetBundle ab)
        {
            SharedPtr<KAssetBundle> ptr = null;
            if (this.caches.ContainsKey(abname))
            {
                ptr = this.caches[abname];
                if (ptr != null && ptr.isAlive && ptr.Equals(ab))
                {
                    return ptr;
                }
            }

            ptr = new SharedPtr<KAssetBundle>(new KAssetBundle(ab));
            this.caches[abname] = ptr;

            return ptr;

        }

        public bool Contains(string abname)
        {
            return this.caches.ContainsKey(abname) ;
        }

        public bool Contains(BundlePkgInfo pkg)
        {
            return this.caches.ContainsKey(pkg.AbFileName);
        }

        public IBundleRef TryGetValue(BundlePkgInfo pkg)
        {
            return this.TryGetValue(pkg.AbFileName);
        }

        public IBundleRef TryGetValue(string assetname )
        {
            if (this.Contains(assetname))
            {
                return this[assetname];
            }

            return null;
        }

        public IBundleRef PushAsset(BundlePkgInfo pkg, AssetBundle ab)
        {
            IBundleRef bundle = null;
            if(!this.Contains(pkg.AbFileName))
            {
                bundle = new BundleRef(this.TryGetPtr(pkg.AbFileName, ab), pkg.EditorPath);
                this.RefCaches.Add(pkg.AbFileName,bundle);
            }
            else
            {
                bundle = this[pkg.AbFileName];
            }
            return bundle;
        }

        public IBundleRef PushEditorAsset(BundlePkgInfo pkg, UnityEngine.Object ab)
        {
            IBundleRef bundle = null;
            if (!this.Contains(pkg.AbFileName))
            {
#if UNITY_EDITOR && !AB_DEBUG
                bundle = new EditorRef(ab,pkg.AbFileName);
#else
                bundle = new BundleRef(this.TryGetPtr(pkg.AbFileName, ab), pkg.EditorPath);
#endif

                this.RefCaches.Add(pkg.AbFileName, bundle);
            }
            else
            {
                bundle = this[pkg.AbFileName];
            }
            return bundle;
        }
    }
}


