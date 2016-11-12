using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using KUtils;

namespace KFrameWork
{
    public sealed class GameFrameWork :BaseAttributeRegister
    {
        public readonly float FrameWorkStartTime ;

        public const string Version ="0.0.01b";

        private bool m_binit;
        public bool Inited
        {
            get
            {
                return this.m_binit;
            }
        }


        public GameFrameWork()
        {
            FrameWorkStartTime = Time.realtimeSinceStartup;
        }

        public void Initialite()
        {
            try
            {
                AttributeRegister.Register(this);
                this.Start();
                this.End();
                this.m_binit = true;
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
                this.m_binit =false;
            }

        }
    }
}

