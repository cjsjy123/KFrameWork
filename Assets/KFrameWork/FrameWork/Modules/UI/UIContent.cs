using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{

    public enum UIFlag
    {
        Discover,
        RemoveNow,
        VisiableNow,
    }

    public struct UIContent  {

        public string PrefabPath;

        public UIFlag Flag;

        public static bool isFlag(UIFlag self,UIFlag other)
        {
            int l =(int)self;
            int r = (int)other;
            return (l & r) == r;
        }

    }
}


