using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    /// <summary>
    /// 资源引用，只负责由此bundle 加载出来的实例对象，tip:因为存在unload false，所以之前存在的对象将不收到此bundle管理，
    /// </summary>
    public class BundleRef : IBundleRef
    {
        public const float deltatime = 30f;

        private KAssetBundle Res;
        /// <summary>
        /// 这里完全依赖mono的gc来做引用检查
        /// </summary>
        private List<WeakReference> refs;

        private List<IBundleRef> depends;

        public string name { get; private set; }

        public string LoadName { get; private set; }

        /// <summary>
        /// 被依赖数
        /// </summary>
        /// <value>The depnd count.</value>
        public int DependCount
        {
            get
            {
                return this.depends.Count;
            }
        }
        /// <summary>
        /// 实例引用
        /// </summary>
        /// <value>The reference count.</value>
        public int InstanceRefCount
        {
            get
            {
                this.UpdateRefs();

                return this.refs.Count;
            }
        }
        /// <summary>
        /// 自引用计数
        /// </summary>
        public int SelfRefCount { get; private set; }

        public bool SupportAsync
        {
            get
            {
                return true;
            }
        }

        public string filename { get; private set; }

        public UnityEngine.Object MainObject { get; private set; }

        private float CreateTime;

        private BundleRef()
        {
            this.UpdateTime();
        }

        public static BundleRef  Create(KAssetBundle ab,string abname, string loadname,string bundlename)
        {
            BundleRef bundle = new BundleRef();

            bundle.Res = ab;

            bundle.depends = new List<IBundleRef>();
            bundle.refs = new List<WeakReference>();

            if (ab.isStreamedSceneAssetBundle)
            {
                bundle.LoadName = System.IO.Path.GetFileNameWithoutExtension(ab.GetAllScenePaths()[0]);
            }
            else
                bundle.LoadName = BundlePathConvert.EditorName2AssetName(loadname);

            bundle.name = bundlename;
            bundle.filename = abname;
            return bundle;
        }

        void UpdateTime()
        {
            CreateTime = GameSyncCtr.mIns.FrameWorkTime;
        }

        public static bool  operator  == (BundleRef left,BundleRef right)
        {
            bool leftempty = left.Res == null || left.Res.isEmpty();
            bool rightempty = right.Res == null || left.Res.isEmpty();
            if (leftempty == rightempty)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(BundleRef left, BundleRef right)
        {
            bool leftempty = left.Res == null || left.Res.isEmpty();
            bool rightempty = right.Res == null || left.Res.isEmpty();
            if (leftempty != rightempty)
            {
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public void NeedThis(IBundleRef dep)
        {
            for (int i = 0; i < this.depends.Count; ++i)
            {
                IBundleRef Ibundle = this.depends[i];
                if (Ibundle != null && Ibundle.Equals(dep))
                {
                    return;
                }
            }
            dep.Retain();
            this.depends.Add(dep);
        }

        public void LogDepends()
        {
            //this.UpdateRefs();

            for (int i = 0; i < this.depends.Count; ++i)
            {
                IBundleRef bundle = this.depends[i];
                LogMgr.LogFormat("<color=#ff0ff0ff>[{0}] Need This Bundle [{1}] ,its RefCount is {2},its SelfRefCnt is {3} ,its DependedCnt is {4}</color>", this.name, bundle.name, bundle.InstanceRefCount, bundle.SelfRefCount, bundle.DependCount);
            }
        }


        private bool CheckRes()
        {
            if (this.Res == null)
            {
                if (BundleConfig.SAFE_MODE)
                {
                    throw new FrameWorkResMissingException(string.Format("Asset Bundle {0} Missing", this.LoadName));
                }
                else
                {
                    LogMgr.LogErrorFormat("Asset Bundle {0} Missing", this.LoadName);
                    return false;
                }
            }
            return true;
        }

        public AssetBundleRequest LoadAssetAsync()
        {
            if (!CheckRes())
            {
                return null;
            }
            else
            {
                return Res.AsyncLoad(this.LoadName);
            }
        }

        public bool LoadAsset(out UnityEngine.Object target )
        {
            if (!this.CheckRes())
            {
                target = null;
                return false;
            }

            target = Res.Load(this.LoadName);
            return true;
        }

        public bool LoadAsset(string abname, out UnityEngine.Object target)
        {
            if (!this.CheckRes())
            {
                target = null;
                return false;
            }

            target = Res.Load(abname);
            return true;
        }

        public bool LoadAllAssets(out UnityEngine.Object[] target)
        {
            if (!this.CheckRes())
            {
                target = null;
                return false;
            }

            target = Res.LoadAll<UnityEngine.Object>();
            return true;
        }

        public string[] GetAllAssetNames()
        {
            KAssetBundle ab = Res;
            if (ab == null)
            {
                return null;
            }
            return ab.GetAllAssetNames();
        }

        public bool Instantiate(out UnityEngine.Object target , Component c = null)
        {
            if (!this.CheckRes())
            {
                target = null;
                return false;
            }

            UnityEngine.Object prefab = null;
            if (this.MainObject != null)
            {
                prefab = this.MainObject;
            }
            else
            {
                LoadAsset(out prefab);
            }
            target = InstantiateWithBundle(prefab, c);
            return true;
        }

        public UnityEngine.Object InstantiateWithBundle(UnityEngine.Object prefab, Component c = null)
        {
            if (prefab == null)
            {
                if (BundleConfig.SAFE_MODE)
                    throw new FrameWorkArgumentException("prefab is Null");
                else
                {
                    LogMgr.LogError("prefab is Null");
                    return null;
                }
            }
            this.MainObject = prefab;
            if (prefab is GameObject)
            {
                UnityEngine.Object ins = GameObject.Instantiate(prefab);
                this.Retain(ins);
                return ins;
            }
            else
            {
                if (c == null)
                    throw new FrameWorkException("If Target isnt Gameobejct ,you should Pass with a component ");

                this.Retain(c);
            }
            return prefab;

        }

        void UpdateRefs()
        {
            for (int i = this.refs.Count - 1; i >= 0; i--)
            {
                WeakReference r = this.refs[i];
                if (r == null || !r.IsAlive || r.Target == null)
                {
                    this.refs.RemoveAt(i);
                }
            }
        }

        public void UnLoad(bool all)
        {
            if (this.Res != null && this.DependCount == 0 )
            {
                if (!all && GameSyncCtr.mIns.FrameWorkTime - this.CreateTime < deltatime)
                {
                    LogMgr.LogErrorFormat("Reject the {0} Asset  {1} Desotry  ", this.name, all);
                    return ;
                }
                LogMgr.LogErrorFormat("{0} Asset Will {1} Desotry  ", this.name, all);
                //if (FrameWorkConfig.Open_DEBUG)


                ResBundleMgr.mIns.Cache.Remove(this.filename);
                this.Res.Unload(all);
                this.Dispose();
            }
        }

        public UnityEngine.Object SimpleInstantiate()
        {
            UnityEngine.Object o;
            this.Instantiate(out o);
            return o;
        }


        public void Retain()
        {
            this.SelfRefCount++;
            this.UpdateTime();
        }

        public void Retain(UnityEngine.Object o)
        {
            for (int i = 0; i < this.refs.Count; ++i)
            {
                WeakReference weakptr = this.refs[i];
                if (weakptr != null && weakptr.IsAlive && weakptr.Target.Equals(o))
                {
                    return;
                }
            }

            this.refs.Add(new WeakReference(o));
            this.UpdateTime();
        }

        public void Release()
        {
            this.SelfRefCount--;
        }

        public void Release(UnityEngine.Object o)
        {
            for (int i = 0; i < this.refs.Count; ++i)
            {
                WeakReference weakptr = this.refs[i];
                if (weakptr != null && weakptr.IsAlive && weakptr.Target.Equals(o))
                {
                    this.refs.RemoveAt(i);
                    return;
                }
            }
        }

        private void Dispose()
        {

            for (int i = 0; i < this.depends.Count; ++i)
            {
                this.depends[i].Release();
            }

            this.refs.Clear();
            this.depends.Clear();
            this.LoadName = null;
            this.Res = null;
            this.refs = null;
            this.depends = null;
        }
    }
}


