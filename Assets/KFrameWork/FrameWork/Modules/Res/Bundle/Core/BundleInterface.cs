using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.IO;

namespace KFrameWork
{
    public interface BundleInfoFilter
    {
        void LoadFromMemory(Stream stream);

        BundlePkgInfo TrygetInfo(string name);
    }

}

