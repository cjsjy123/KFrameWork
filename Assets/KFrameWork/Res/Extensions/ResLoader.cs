using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

[SingleTon]
public class ResLoader  {

    public static ResLoader mIns;

    private string LastLoadName;
    private UnityEngine.Object LastLoadObject;

    public T Load<T>(string name) where T:UnityEngine.Object
    {
        if(name.Equals(LastLoadName) && LastLoadObject != null)
        {
            return (T)LastLoadObject;
        }
        else
        {
            LastLoadObject=Resources.Load<T>(name);
            LastLoadName = name;
            return (T)LastLoadObject;
        }
        

    }

    public ResourceRequest AsyncLoad( string name)
    {
        return Resources.LoadAsync(name);
    }

}
