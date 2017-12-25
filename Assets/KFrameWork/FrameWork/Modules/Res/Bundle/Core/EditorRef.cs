#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using UnityEditor;

namespace KFrameWork
{
    public class EditorRef : IBundleRef
    {
        public UnityEngine.Object MainObject { get; private set; }

        private string _name;

        public string name
        {
            get
            {
                return this._name;
            }
        }

        public string LoadName
        {
            get
            {
                return this._name;
            }
        }

        public int DependCount
        {
            get
            {
                return 1;
            }
        }

        public int InstanceRefCount
        {
            get
            {
                return 1;
            }
        }

        public bool SupportAsync
        {
            get
            {
                return false;
            }
        }

        public int SelfRefCount
        {
            get
            {
                return 0;
            }
        }

        public string filename { get; private set; }

        private string _editorpath;

        public EditorRef(UnityEngine.Object o,string bundlename,string name,string editorpath)
        {
            this.MainObject = o;
            this._name = name;
            this.filename = name;
            this._editorpath = editorpath;
        }

        public void NeedThis(IBundleRef dep)
        {

        }

        public void LogDepends()
        {

        }

        public bool Instantiate(out UnityEngine.Object target, Component c = null)
        {
            if (MainObject == null)
            {
                target = null;
                return false;
            }


            if (MainObject is GameObject)
            {
                target = GameObject.Instantiate(MainObject);
            }
            else
            {
                target = MainObject;
            }
            return true;
        }

        public bool LoadAsset(out UnityEngine.Object target)
        {
            target = this.MainObject;
            return true;
        }

        public bool LoadAsset(string abname, out UnityEngine.Object target)
        {
            target = this.MainObject;
            return true;
        }

        public AssetBundleRequest LoadAssetAsync()
        {
            return null;
        }

        public void Lock(LockType tp = LockType.END)
        {

        }

        public void UnLoad(bool all)
        {

        }

        public void UnLock(LockType tp = LockType.END)
        {

        }

        public UnityEngine.Object InstantiateWithBundle(UnityEngine.Object prefab, Component c = null)
        {
            if (prefab is GameObject)
            {
                return GameObject.Instantiate(prefab);
            }
            else
                return prefab;
        }

        public UnityEngine.Object SimpleInstantiate()
        {
            UnityEngine.Object o;
            this.Instantiate(out o);
            return o;
        }

        public void Retain()
        {
            
        }

        public void Retain(UnityEngine.Object o)
        {
           
        }

        public void Release()
        {
            
        }

        public string[] GetAllAssetNames()
        {
            return null;
        }

        public void Release(UnityEngine.Object o)
        {
            
        }

        public bool LoadAllAssets(out UnityEngine.Object[] target)
        {
            target = AssetDatabase.LoadAllAssetsAtPath(this._editorpath);
            return true;
        }
    }
}
#endif

