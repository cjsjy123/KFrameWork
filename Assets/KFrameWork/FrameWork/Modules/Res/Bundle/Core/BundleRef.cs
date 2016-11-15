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
    public class BundleRef : IBundleRef, IPool, IDisposable
    {
        private string LoadName;

        private string ABfilename;

        private SharedPtr<KAssetBundle> Res;
        /// <summary>
        /// 这里完全依赖mono的gc来做引用检查
        /// </summary>
        private List<WeakReference> refs;

        private List<IBundleRef> depends;

        public string name
        {
            get
            {
                return this.LoadName;
            }
        }

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
        public int RefCount
        {
            get
            {
                this.UpdateRefs();

                return this.refs.Count;
            }
        }

        private BundleRef()
        {

        }

        public static BundleRef  Create(SharedPtr<KAssetBundle> ab,string abname, string loadname)
        {
            BundleRef bundle = null;
            if (KObjectPool.mIns != null)
            {
                bundle = KObjectPool.mIns.Pop<BundleRef>();
            }

            if(bundle == null)
            {
                bundle = new BundleRef();
            }

            bundle.Res = ab;
            bundle.Res.AddRef();

            bundle.depends = new List<IBundleRef>();
            bundle.refs = new List<WeakReference>();
            bundle.LoadName = BundlePathConvert.EditorName2AssetName(loadname);
            bundle.ABfilename = abname;
            return bundle;
        }

        public void NeedThis(IBundleRef dep)
        {
            for (int i = 0; i < this.depends.Count; ++i)
            {
                IBundleRef weakptr = this.depends[i];
                if (weakptr != null && weakptr.Equals(dep))
                {
                    return;
                }
            }

            this.depends.Add(dep);
        }

        public void LogDepends()
        {
            //this.UpdateRefs();

            for (int i = 0; i < this.depends.Count; ++i)
            {
                IBundleRef bundle = this.depends[i];
                LogMgr.LogFormat("<color=#ff0ff0ff>[{0}] Need This Bundle [{1}] ,RefCount is {2} ,DependedCnt is {3}</color>", bundle.name,this.name, bundle.RefCount, bundle.DependCount);
            }
        }

        public void Lock(LockType tp = LockType.DontDestroy | LockType.OnlyReadNoWrite)
        {
            if (this.Res != null)
            {
                this.Res.Lock(tp);
            }
        }

        public void UnLock(LockType tp = LockType.DontDestroy | LockType.OnlyReadNoWrite)
        {
            if (this.Res != null)
            {
                this.Res.UnLock(tp);
            }
        }



        public bool LoadAsset(out UnityEngine.Object target )
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
                    target = null;
                    return false;
                }
            }

            target = Res.get().Load(this.LoadName);
            return true;
        }

        public bool Instantiate(out UnityEngine.Object target , Component c = null)
        {
            target = null;

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

            UnityEngine.Object prefab = Res.get().Load(this.LoadName);

            if (prefab == null)
                return false;

            if (prefab is GameObject)
            {
                UnityEngine.Object ins = GameObject.Instantiate(prefab);
                this.refs.Add(new WeakReference(ins));
                target = ins;
                return true;
            }
            else
            {
                if (c == null)
                    throw new FrameWorkException("If Target isnt Gameobejct ,you should Pass with a component ");

                target = prefab;
                this.refs.Add(new WeakReference(c));
            }

            return true;
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

            for (int i = this.depends.Count - 1; i >= 0; i--)
            {
                IBundleRef r = this.depends[i];
                if (r == null || (r.DependCount == 0 && r.RefCount ==0))
                {
                    this.depends.RemoveAt(i);
                }
            }

        }

        public void UnLoad(bool all)
        {
            if (this.Res != null && this.Res.isLock(LockType.DontDestroy))
            {
                LogMgr.LogErrorFormat("this is an DontDestory Object with name {0}", this.LoadName);
                return;
            }

            if (this.Res != null && this.DependCount == 0)
            {
                ///
                if (this.RefCount != 0)
                {
                    all = false;
                }

                if (this.Res.isLock(LockType.DontDestroy))
                {
                    LogMgr.LogErrorFormat("{0} is an DontDestrory Object Should Be Unlock first", this.LoadName);
                }
                else
                {
                    if (FrameWorkDebug.Open_DEBUG)
                        LogMgr.LogFormat("{0} Asset Will {1} Desotry  ", this.LoadName, all);

                    this.Res.get().Unload(all);
                    ResBundleMgr.mIns.Cache.Remove(this.ABfilename);
                    this.Dispose();
                }

            }
        }

        public void Dispose()
        {
            this.LoadName = null;
            this.Res.RemoveRef();
            if (this.Res.Count != 0)
                LogMgr.LogErrorFormat("Res RefCount Error : {0}",this.Res.Count);

            this.Res = null;
            this.refs.Clear();
            this.depends.Clear();
        }

        public void AwakeFromPool()
        {
            this.LoadName = null;
            this.Res.RemoveRef();
            if (this.Res.Count != 0)
                LogMgr.LogErrorFormat("Res RefCount Error : {0}", this.Res.Count);

            this.Res = null;
            this.refs.Clear();
            this.depends.Clear();
        }

        public void RemovedFromPool()
        {
            this.LoadName = null;
            this.Res.RemoveRef();
            if (this.Res.Count != 0)
                LogMgr.LogErrorFormat("Res RefCount Error : {0}", this.Res.Count);

            this.Res = null;
            this.refs.Clear();
            this.depends.Clear();
        }
    }
}


