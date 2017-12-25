using UnityEngine;
using System.Collections;
using KUtils;

namespace KFrameWork
{
    public struct AssetBundleResult
    {
        public IBundleRef MainObject;

        public readonly string LoadPath;

        public readonly GameObject ParentPath;
        /// <summary>
        /// just for scene
        /// </summary>
        public SceneOperation SceneAsyncResult;

        private UnityEngine.Object m_LoadedObject;

        public UnityEngine.Object LoadedObject
        {
            get
            {
                if(m_LoadedObject == null)
                {
                    this.MainObject.LoadAsset(out m_LoadedObject);
                }

                return this.m_LoadedObject;
            }
        }

        private GameObject _autoObject;

        /// <summary>
        /// 自动实例化的部分
        /// </summary>
        /// <value>The instanced object.</value>
        public GameObject InstancedObject
        {
            get
            {
                return _autoObject;
            }

        }

        public AssetBundleResult(IBundleRef bundleref,UnityEngine.Object bundle, GameObject parentPath,GameObject hadload, string loadpath)
        {
            this.MainObject = bundleref;
            this.LoadPath = loadpath;
            this.ParentPath = parentPath;
            this.SceneAsyncResult = null;

            this.m_LoadedObject = bundle;
            this._autoObject =hadload;
        }

        public GameObject SimpleInstance()
        {
            Object o;
            MainObject.Instantiate(out o);
            if (o is GameObject)
            {
                GameObject gameobject = o as GameObject;
                if (ParentPath != null)
                {
                    return ParentPath.AddInstance(gameobject);
                }
                return gameobject;
            }
            else
            {
                LogMgr.LogErrorFormat("{0} 类型错误非gameobject ", o);
            }
            return null;
        }
    }
}
