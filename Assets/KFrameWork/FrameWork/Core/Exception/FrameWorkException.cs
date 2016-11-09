using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{


    public class FrameWorkException : Exception {

        private ExceptionType tp;

        public FrameWorkException(string errorInfo,ExceptionType extp = ExceptionType.Ignore_Exception):base(errorInfo)
        {

            this.tp = extp;
        }

        public void RaiseExcption()
        {
            switch(tp)
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
}


