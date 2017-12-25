using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MenuToolsAsset : ScriptableObject
{
    [Serializable]
    public class clstoolasset
    {
        public string Keyname;

        public string clsname;

        public string drawfuncname;

        public string loadfuncname;
    }

    public List<clstoolasset> data = new List<clstoolasset>();

}