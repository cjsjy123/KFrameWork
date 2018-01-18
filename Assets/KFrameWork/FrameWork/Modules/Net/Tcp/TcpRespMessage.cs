using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public abstract class TcpRespMessage : NetMessage
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


        public abstract void DeSerialize(NetByteBuffer buffer);

        public abstract void DeSerialize(byte[] bytearray);

        public abstract void Release();
    }

}


