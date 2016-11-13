using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{

    public class BundleRef:IDisposable
    {
        private string LoadName;

        private SharedPtr<KAssetBundle> Res;
        /// <summary>
        /// 这里完全依赖mono的gc来做引用检查
        /// </summary>
        private List<WeakReference> refs ;

        private List<BundleRef> depends;

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

        public BundleRef(AssetBundle ab,string loadname )
        {
            this.Res = new SharedPtr<KAssetBundle>(new KAssetBundle(ab));
            this.depends = new List<BundleRef>();
            this.refs = new List<WeakReference>();
            this.LoadName = loadname;
        }

        public void AddDepend(BundleRef dep)
        {
            if(!this.depends.Contains(dep))
            {
                this.depends.Add(dep);
            }
        }

        public void Lock(LockType tp = LockType.DontDestroy | LockType.OnlyReadNoWrite)
        {
            if(this.Res != null)
            {
                this.Res.Lock(tp);
            }
        }

        public void UnLock(LockType tp = LockType.DontDestroy | LockType.OnlyReadNoWrite)
        {
            if(this.Res != null)
            {
                this.Res.UnLock(tp);
            }
        }

        public void UnLoad(bool all)
        {
            if(this.Res != null && this.DepndCount ==0 )
            {
                ///
                if(this.RefCount != 0)
                {
                    all =false;
                }

                if(this.Res.isLock(LockType.DontDestroy))
                {
                    LogMgr.LogErrorFormat("{0} is an DontDestrory Object Should Be Unlock first",this.LoadName);
                }
                else
                {
                    if(FrameWorkDebug.Open_DEBUG)
                        LogMgr.LogFormat("{0} Asset Will {1} Desotry  ",this.LoadName,all);

                    this.Res.get().Unload(all);
                }

            }
        }

        public UnityEngine.Object Instantiate(Component c = null )
        {
            if(this.Res == null)
            {
                if(BundleConfig.SAFE_MODE)
                {
                    throw new FrameWorkResMissingException(string.Format( "Asset Bundle {0} Missing",this.LoadName));
                }
                else
                {
                    LogMgr.LogErrorFormat("Asset Bundle {0} Missing",this.LoadName);
                    return null;
                }
            }

            UnityEngine.Object target = Res.get().Load(this.LoadName);

            Res.AddRef();

            if(target is GameObject)
            {

                UnityEngine.Object ins =GameObject.Instantiate(target);
                this.refs.Add(new WeakReference(ins));

                return ins;
            }
            else
            {
                if(c ==  null)
                    throw new FrameWorkException("If Target isnt Gameobejct ,you should Pass with a component ");

                this.refs.Add(new WeakReference(c));

            }

            return target;
        }

        void UpdateRefs()
        {
            int count = this.RefCount;

            for(int i = this.refs.Count-1; i >=0; i--)
            {
                WeakReference r = this.refs[i];
                if(r == null || !r.IsAlive || r.Target == null)
                {
                    this.refs.RemoveAt(i);
                }
            }

            if(FrameWorkDebug.Open_DEBUG)
                LogMgr.LogFormat("Changed {0} ref  ",count- this.RefCount);
        }


        public void Dispose ()
        {
            this.LoadName =null;
            this.refs.Clear();
            this.depends.Clear();

        }
    }

    public class BundleCache  {

        private Dictionary<string,SharedPtr<KAssetBundle>> caches ;

        public BundleCache()
        {
            this.caches = new Dictionary<string, SharedPtr<KAssetBundle>>(16);
        }

        public SharedPtr<KAssetBundle> this[string key]
        {
            get
            {
                return this.caches[key];
            }
            set
            {
                this.Push(key,value);
            }
        }

        public bool Contains(string abname)
        {
            return this.caches.ContainsKey(abname) ;
        }

        public void Push(string abname, SharedPtr<KAssetBundle> ab)
        {
            if(!this.Contains(abname))
            {
                this.caches.Add(abname,ab);
            }
        }


        public SharedPtr<KAssetBundle> Push(string abname, AssetBundle ab)
        {
            SharedPtr<KAssetBundle> ptr = null;
            if(!this.Contains(abname))
            {
                ptr = new SharedPtr<KAssetBundle>( new KAssetBundle(ab));
                this.caches.Add(abname,ptr);
            }
            else
            {
                ptr = this[abname];
            }
            return ptr;
        }
    }
}


