using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public interface UIContentEvent
    {
        void OnEnter();

        void OnExit();
    }

    public struct UIContent  {

        public string PrefabPath;



    }
}


