using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{


    public class FrameWorkException : Exception
    {

        private ExceptionType tp;

        public FrameWorkException(string errorInfo, ExceptionType extp = ExceptionType.Ignore_Exception) : base(errorInfo)
        {

            this.tp = extp;
        }

        public virtual void RaiseExcption()
        {
            switch (tp)
            {
                case ExceptionType.Ignore_Exception:
                    {
                        break;
                    }
                case ExceptionType.Lower_Exception:
                    {
                        break;
                    }
                case ExceptionType.Higher_Excetpion:
                    {
                        break;
                    }
                case ExceptionType.HighDanger_Exception:
                    {
                        break;
                    }
            }

        }
    }

    public class FrameWorkResNotMatchException : FrameWorkException
    {
        public FrameWorkResNotMatchException(string info) : base(info, ExceptionType.Lower_Exception)
        {

        }

        public override void RaiseExcption()
        {

        }
    }

    public class FrameWorkArgumentException : FrameWorkException
    {
        public FrameWorkArgumentException(string info) : base(info, ExceptionType.Higher_Excetpion)
        {

        }

        public override void RaiseExcption()
        {

        }
    }

    public class FrameWorkResMissingException : FrameWorkException
    {
        public FrameWorkResMissingException(string info) : base(info, ExceptionType.Lower_Exception)
        {

        }

        public override void RaiseExcption()
        {
            
        }

    }
}


