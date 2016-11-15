
//#define AB_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{

    public class BundleCache  {

        private Dictionary<string,SharedPtr<KAssetBundle>> ptrcaches ;

        private Dictionary<string, IBundleRef> RefCaches;

        private List<string> _LoadingList;

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
            this._LoadingList = new List<string>(16);
        }

        public void LogDebugInfo()
        {
            LogMgr.LogFormat("<color=#0000ffff>----------Bundle In Pool Cnt is {0}-------------</color>", this.CacheCnt);

            var keys = new List<string>(this.RefCaches.Keys);
            for (int i = 0; i < keys.Count; ++i)
            {
                string key = keys[i];
                IBundleRef bundle = this.RefCaches[key];

                LogMgr.LogFormat("<color=#00aa00ff>Root Bundle is {0} ,RefCount is {1} ,DependedCnt is {2}</color>", bundle.name, bundle.RefCount, bundle.DependCount);

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
                    if (bundle == null || bundle.DependCount == 0 || bundle.RefCount == 0)
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
                if (ptr != null  && ptr.Equals(ab))
                {
                    if (!ptr.isAlive)
                        ptr.Restore(new KAssetBundle(ab));

                    return ptr;
                }
            }

            ptr = new SharedPtr<KAssetBundle>(new KAssetBundle(ab));
            this.ptrcaches[abname] = ptr;

            return ptr;

        }

        public bool ContainsLoading(string loadpath)
        {
            return this._LoadingList.Contains(loadpath);
        }

        public void PushLoading(string loadpath)
        {
            if (!this.ContainsLoading(loadpath))
            {
                this._LoadingList.Add(loadpath);
            }
        }

        public void RemoveLoading(string loadpath)
        {
            this._LoadingList.Remove(loadpath);
        }

        public bool Remove(string bundlename)
        {
            bool ret= this.RefCaches.Remove(bundlename) ;
            if (FrameWorkDebug.Open_DEBUG)
            {
                LogMgr.LogFormat("Bundle Will Remove From this Pool :{0} ,{1}",bundlename,ret?"True":"False");
            }
            return ret;
        }

        public bool Contains(string abname)
        {
            return this.RefCaches.ContainsKey(abname) ;
        }

        public bool Contains(BundlePkgInfo pkg)
        {
            return this.RefCaches.ContainsKey(pkg.AbFileName);
        }

        public IBundleRef TryGetValue(BundlePkgInfo pkg)
        {
            return this.TryGetValue(pkg.AbFileName);
        }

        public IBundleRef TryGetValue(string assetname )
        {
            IBundleRef bundle;
            this.RefCaches.TryGetValue(assetname, out bundle);
            return bundle;
        }


        public IBundleRef PushAsset(BundlePkgInfo pkg, AssetBundle ab)
        {
            IBundleRef bundle = null;
            if(!this.Contains(pkg.AbFileName))
            {
                bundle = BundleRef.Create(this.TryGetPtr(pkg.AbFileName, ab),pkg.AbFileName, pkg.EditorPath);
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


