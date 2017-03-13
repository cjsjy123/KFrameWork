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
        private class AsyncSetAssetTask : ITask
        {
            public AsyncOperation _currentAsync;
            public bool KeepWaiting
            {
                get
                {
                    return !_currentAsync.isDone;
                }
            }

            public AsyncSetAssetTask(AsyncOperation o)
            {
                this._currentAsync = o;
            }
        }

        public UnityEngine.Object MainObject { get; private set; }

        AsyncSetAssetTask task;

        private string ABfilename;

        private SharedPtr<KAssetBundle> Res;
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

        private BundleRef()
        {

        }

        public static BundleRef  Create(SharedPtr<KAssetBundle> ab,string abname, string loadname,string bundlename)
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

            KAssetBundle kab = ab.get();
            if (kab.isStreamedSceneAssetBundle)
            {
                bundle.LoadName = System.IO.Path.GetFileNameWithoutExtension( kab.GetAllScenePaths()[0]);
            }
            else
                bundle.LoadName = BundlePathConvert.EditorName2AssetName(loadname);

            bundle.name = bundlename;
            bundle.ABfilename = abname;
            return bundle;
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
                AssetBundleRequest req = Res.get().AsyncLoad(this.LoadName);
                if (this.MainObject == null || task == null )
                {
                    task = new AsyncSetAssetTask(req);

                    WaitTaskCommand cmd = WaitTaskCommand.Create(task, YieldSetMainAsset);
                    cmd.Excute();
                }

                return req;
            }
        }

        private void YieldSetMainAsset(WaitTaskCommand cmd)
        {
            this.MainObject = (task._currentAsync as AssetBundleRequest).asset;
        }

        public bool LoadAsset(out UnityEngine.Object target )
        {
            if (!this.CheckRes())
            {
                target = null;
                return false;
            }

            target = Res.get().Load(this.LoadName);
            return true;
        }

        public bool LoadAsset(string abname, out UnityEngine.Object target)
        {
            if (!this.CheckRes())
            {
                target = null;
                return false;
            }

            target = Res.get().Load(abname);
            return true;
        }

        public bool LoadAllAssets(out UnityEngine.Object[] target)
        {
            if (!this.CheckRes())
            {
                target = null;
                return false;
            }

            target = Res.get().LoadAll<UnityEngine.Object>();
            return true;
        }

        public string[] GetAllAssetNames()
        {
            KAssetBundle ab = Res.get();
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
            target = this.InstantiateWithBundle(prefab, c);
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
            if (this.Res != null && this.Res.isLock(LockType.DontDestroy))
            {
                LogMgr.LogErrorFormat("this is an DontDestory Object with name {0}", this.LoadName);
                return;
            }

            if (this.Res != null && this.DependCount == 0)
            {
                ///
                if (this.InstanceRefCount != 0)
                {
                    all = false;
                }

                if (this.Res.isLock(LockType.DontDestroy))
                {
                    LogMgr.LogErrorFormat("{0} is an DontDestrory Object Should Be Unlock first", this.LoadName);
                }
                else
                {
                    if (FrameWorkConfig.Open_DEBUG)
                        LogMgr.LogFormat("{0} Asset Will {1} Desotry  ", this.LoadName, all);

                    this.Res.get().Unload(all);
                    ResBundleMgr.mIns.Cache.Remove(this.ABfilename);
                    this.Dispose();
                }

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

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                this.Res.RemoveRef();
                if (this.Res.Count != 0 && this.SelfRefCount != 0)
                    LogMgr.LogErrorFormat("Res RefCount Error : {0}", this.Res.Count);

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
                this.MainObject = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void RemoveToPool()
        {
            this.Dispose(false);
        }

        public void RemovedFromPool()
        {
            this.Dispose(true);
        }


    }
}


