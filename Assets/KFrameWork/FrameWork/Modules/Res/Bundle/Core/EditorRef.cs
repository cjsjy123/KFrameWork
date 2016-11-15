using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
namespace KFrameWork
{
    public class EditorRef : IBundleRef
    {
        private UnityEngine.Object Mainobejct;

        private string _name;

        public string name
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

        public int RefCount
        {
            get
            {
                return 1;
            }
        }

        public EditorRef(UnityEngine.Object o,string name)
        {
            this.Mainobejct = o;
            this._name = name;
        }

        public void NeedThis(IBundleRef dep)
        {

        }

        public void LogDepends()
        {

        }

        public bool Instantiate(out UnityEngine.Object target, Component c = null)
        {
            if (Mainobejct == null)
            {
                target = null;
                return false;
            }


            if (Mainobejct is GameObject)
            {
                target = GameObject.Instantiate(Mainobejct);
            }
            else
            {
                target = Mainobejct;
            }
            return true;
        }

        public bool LoadAsset(out UnityEngine.Object target)
        {
            target = this.Mainobejct;
            return true;
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
    }
}


