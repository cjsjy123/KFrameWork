using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KFrameWork
{
    public abstract class BaseContent : UIBehaviour
    {

        protected bool m_bvisible;

        public int UIDepth = -100;

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

