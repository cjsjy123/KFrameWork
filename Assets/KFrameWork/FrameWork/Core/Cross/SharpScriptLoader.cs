using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using System;
using System.Reflection;

namespace KFrameWork
{
    public sealed class SharpScriptLoader : IScriptLoader {
        
        private Delegate ScriptFunc;

        private Type RetTp ;

        public string methodname { get; private set; }

        private WeakReference weakref;

        private MethodInfo method;

        public void Init (AbstractParams InitParams)
        {
            object passObject =InitParams.ReadObject() ;

            if (passObject != null && passObject is Delegate)
            {
                ScriptFunc = passObject as Delegate;
                this.RetTp = InitParams.ReadObject() as Type;
                this.methodname = InitParams.ReadString();
            }
            else if (passObject != null)
            {
                weakref = new WeakReference(passObject);
                method = InitParams.ReadObject() as MethodInfo;
            }
        }

        public AbstractParams Invoke (AbstractParams ScriptParms)
        {
            if (ScriptFunc != null)
            {
                if (this.RetTp == typeof(void))
                {
                    Action<AbstractParams> f = (Action<AbstractParams>)ScriptFunc;
                    f(ScriptParms);
                    return null;
                }
                else
                {
                    Func<AbstractParams, AbstractParams> f = (Func<AbstractParams, AbstractParams>)ScriptFunc;
                    return f(ScriptParms);
                }
            }
            else
            {
                if (weakref != null && weakref.IsAlive && method != null)
                {
                    object ret = method.Invoke(weakref.Target,new object[] { ScriptParms });
                    if (ret != null && ret is AbstractParams)
                    {
                        return ret as AbstractParams;
                    }
                    return null;
                }
                else
                {
                    LogMgr.LogWarningFormat("对象方法已经失效 :{0}", methodname);
                    return null;
                }
            }
        }

        public void Reset ()
        {
            this.RetTp = null;
            this.ScriptFunc = null;
            this.weakref = null;
            this.methodname = null;
        }
    }
}


