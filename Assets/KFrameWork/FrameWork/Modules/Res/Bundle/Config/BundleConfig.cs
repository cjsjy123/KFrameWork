using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public static class BundleConfig
    {
        public const string ABSavePath = "AssetBundles";

        public const string ABVersionPath = "dep.all";

        public const string depAssetBundlePath = "jjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj.ab";

        /// <summary>
        /// 表示通过底层的异常机制阻断逻辑执行
        /// </summary>
        public static bool SAFE_MODE = true;

        public static bool Bundle_Log = false;

    }
}

