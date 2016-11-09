using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public enum BundleLoadState
    {
        Prepared,
        Running,
        Paused,
        Stopped,
        Error,
        Finished,

    }
}