using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace KFrameWork
{
	/// <summary>
	/// or struct?
	/// </summary>
    public sealed class KAssetBundle :IEquatable<KAssetBundle>, IDisposable
	{
		private AssetBundle _assetbundle;
		private AssetBundle assetbundle
		{
			get
			{
				return _assetbundle;

			}
		}

		public Object mainAsset
		{
			get
			{
                if(this.assetbundle == null)
                    return null;

				return _assetbundle.mainAsset;
			}
		}

		public string name
		{
			get
			{
                if(this.assetbundle == null)
                    return "empty";

				return _assetbundle.name;
			}
		}

        public bool isStreamedSceneAssetBundle
        {
            get
            {
                if(this.assetbundle == null)
                    return false;

                return this.assetbundle.isStreamedSceneAssetBundle;
            }
        }

        public KAssetBundle()
        {
            this._assetbundle = null;
        }


		public KAssetBundle(AssetBundle bundle)
		{

			_assetbundle = bundle;
		}


        public UnityEngine.Object Load(string name) 
        {
            return  _assetbundle.LoadAsset(name);
        }

		public T Load<T>(string name)  where T :Object
		{
			return  _assetbundle.LoadAsset<T>(name);
		}

		public T[] LoadAll<T>()  where T :Object
		{
			return  _assetbundle.LoadAllAssets<T>();
		}

		public T[] LoadAllSub<T>(string name)  where T :Object
		{
			return  _assetbundle.LoadAssetWithSubAssets<T>(name);
		}

		public void Unload(bool includeAll)
		{
			if(_assetbundle != null)
				_assetbundle.Unload(includeAll);
		}

		public static KAssetBundle LoadFromFile(string name) 
		{
#if UNITY_5_3
            return  new KAssetBundle( AssetBundle.LoadFromFile(name));
#else
            return  new KAssetBundle(AssetBundle.CreateFromFile(name) );
#endif
		
		}

		public string[] GetAllScenePaths()
		{
			return _assetbundle.GetAllScenePaths();
		}

		public string[] GetAllAssetNames()
		{
			return _assetbundle.GetAllAssetNames();
		}

		public void Dispose ()
        {
            // TO DO
            if(this.assetbundle != null)
                this._assetbundle.Unload(true);

            this._assetbundle = null;
        }

        public bool Equals (KAssetBundle other)
        {
            if(other == null)
                return false;

            if(this.assetbundle == null && other.assetbundle == null)
                return base.Equals(other);

            return this.assetbundle == other.assetbundle;
        }

		public static implicit operator AssetBundle( KAssetBundle bundle)
		{
			if(bundle == null)
			{
				return null;
			}
			return bundle.assetbundle;
		}


	}
}
