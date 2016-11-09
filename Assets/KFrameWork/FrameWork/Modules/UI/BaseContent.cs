using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public abstract class BaseContent :MonoBehaviour {

        public void DoVisiable()
        {
            
        }

        public void DoInVisiable()
        {
            
        }

        public abstract void OnEnter();

        public abstract void OnDisCover(GameObject discoverObject);

        public abstract void OnExit();
    }

}

