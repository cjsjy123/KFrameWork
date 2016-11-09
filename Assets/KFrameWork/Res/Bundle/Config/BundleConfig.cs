using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public static class BundleConfig  {
    #if UNITY_EDITOR
    public static string ABDownLoadPath ="" ;

    public static string ABSavePath ="";

    public static bool SAFE_MODE = false;
    #else
    public const string ABDownLoadPath ="" ;

    public const string ABSavePath ="";

    public const bool SAFE_MODE = false;
    #endif
}
