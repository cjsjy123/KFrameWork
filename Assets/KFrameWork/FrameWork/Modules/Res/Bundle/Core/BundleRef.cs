using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{

    public class BundleRef : IBundleRef, IDisposable
    {
        private string LoadName;

        private SharedPtr<KAssetBundle> Res;
        /// <summary>
        /// 这里完全依赖mono的gc来做引用检查
        /// </summary>
        private List<WeakReference> refs;

        private List<WeakReference> depends;

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
        public int DepndCount
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

        public BundleRef(SharedPtr<KAssetBundle> ab, string loadname)
        {
            this.Res = ab;
            this.Res.AddRef();
            this.depends = new List<WeakReference>();
            this.refs = new List<WeakReference>();
            this.LoadName = BundlePathConvert.EditorName2AssetName(loadname);
        }

        public void NeedThis(IBundleRef dep)
        {
            for (int i = 0; i < this.depends.Count; ++i)
            {
                WeakReference weakptr = this.depends[i];
                if (weakptr != null && weakptr.IsAlive && weakptr.Target.Equals(dep))
                {
                    return;
                }
            }

            this.depends.Add(new WeakReference(dep));
        }

        public void LogDepends()
        {
            //this.UpdateRefs();

            for (int i = 0; i < this.depends.Count; ++i)
            {
                WeakReference weakpter = this.depends[i];
                IBundleRef bundle = weakpter.Target as IBundleRef;
                LogMgr.LogFormat("<color=#ff0ff0ff>   Need  Bundle is {0} ,RefCount is {1} ,DependedCnt is {2}</color>", bundle.name, bundle.RefCount, bundle.DepndCount);
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

        public void UnLoad(bool all)
        {
            if (this.Res != null && this.Res.isLock(LockType.DontDestroy))
            {
                LogMgr.LogErrorFormat("this is an DontDestory Object with name {0}",this.LoadName);
                return;
            }

            if (this.Res != null && this.DepndCount == 0)
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

                    if (all)
                    {
                        this.Dispose();
                    }
                }

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
            int count = this.refs.Count;

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
                WeakReference r = this.depends[i];
                if (r == null || !r.IsAlive || r.Target == null)
                {
                    this.depends.RemoveAt(i);
                }
            }

            if (FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Changed {0} ref  ", count - this.refs.Count);
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
    }
}


