using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using System;

namespace KFrameWork
{
    public class SharpScriptLoader : IScriptLoader {
        
        private List<System.Object> AttachedObject;
        private Delegate ScriptFunc;
        private Type RetTp ;

        public bool CanDispose
        {
            get
            {
                if(this.AttachedObject != null && this.AttachedObject.Count >0)
                {
                    return false;
                }
                return true;
            }
        }

        public void Init (AbstractParams InitParams)
        {
            this.ScriptFunc =InitParams.ReadObject() as Delegate;
            this.RetTp = InitParams.ReadObject() as Type;
        }

        public AbstractParams Invoke (AbstractParams ScriptParms)
        {
            if(this.RetTp == typeof(void))
            {
                Action<AbstractParams> f =(Action<AbstractParams>) ScriptFunc;
                if(AttachedObject != null && AttachedObject.Count >0)
                {
                    if(ScriptParms == null)
                    {
                        ScriptParms =SimpleParams.Create(1);
                    }

                    bool first =true;

                    for(int i=AttachedObject.Count -1; i >=0;--i )
                    {
                        if(first)
                        {
                            ScriptParms.InsertObject(0,AttachedObject[i]);
                            first =false;
                        }
                        else
                        {
                            ScriptParms.SetObject(0,AttachedObject[i]);
                        }

                        f(ScriptParms);
                    }

                    ScriptParms.Release();
                }
                else
                {
                    f(ScriptParms);
                }
                return null;

            }
            else
            {
                Func<AbstractParams,AbstractParams> f =(Func<AbstractParams,AbstractParams>) ScriptFunc;
                if(AttachedObject != null && AttachedObject.Count >0)
                {
                    AbstractParams lastRet = null;
                    if(ScriptParms == null)
                    {
                        ScriptParms =SimpleParams.Create(1);
                    }
                    bool first =true;
                    for(int i=AttachedObject.Count -1; i >=0;--i )
                    {
                        if(first)
                        {
                            ScriptParms.InsertObject(0,AttachedObject[i]);
                            first =false;
                        }
                        else
                        {
                            ScriptParms.SetObject(0,AttachedObject[i]);
                        }

                        lastRet = f(ScriptParms);
                    }

                    ScriptParms.Release();

                    return lastRet;
                }
                else
                {
                    return f(ScriptParms);
                }

            }
        }

        public void PushAttachObject(System.Object o)
        {
            if(this.AttachedObject == null)
                this.AttachedObject = new List<object>();

            this.AttachedObject.Add(o);
        }

        public void RemovettachObject(System.Object o)
        {
            if(this.AttachedObject != null)
                this.AttachedObject.Remove(o);
        }

        public void Reset ()
        {
            this.RetTp = null;
            this.ScriptFunc = null;
            if(this.AttachedObject != null)
                this.AttachedObject.Clear() ;
        }
    }
}


