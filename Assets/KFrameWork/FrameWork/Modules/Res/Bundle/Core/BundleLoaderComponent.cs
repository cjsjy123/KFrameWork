using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public abstract class BundleLoaderComponent : MonoBehaviour {

        protected  virtual void LoadDone(string name,UnityEngine.Object target)
        {
            if(FrameWorkDebug.Open_DEBUG)
            {
                throw new FrameWorkException("并没有应有的虚函数调用");
            }
        }

    }
}


