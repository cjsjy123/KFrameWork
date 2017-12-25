using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public interface NetMessage
    {
        bool IgnoreResp { get; }

        int reiceved { get; set; }

        int total { get; set; }

        bool HasError { get; }

        NetError errorType { get; set; }
    }

    public enum NetError
    {
        None,
        TimeOut,
        NetException,
    }
}
