using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public abstract class TcpMessage : NetMessage, ISerialize
    {
        public int MainCmd { get; protected set; }

        public int SubCmd { get; protected set; }

        public bool IgnoreResp { get; set; }

        public int reiceved { get; set; }

        public int total { get; set; }

        public bool HasError
        {
            get
            {
                return this.errorType != NetError.None;
            }
        }

        public NetError errorType { get; set; }

        public abstract float getSendTime();

        public abstract byte[] Serialize();

        public abstract void Release();
    }
}


