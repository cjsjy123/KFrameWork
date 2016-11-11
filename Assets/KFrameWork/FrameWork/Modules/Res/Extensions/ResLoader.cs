using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

[SingleTon]
public sealed class ResLoader  {

    public static ResLoader mIns;

    private string LastLoadName;
    private UnityEngine.Object LastLoadObject;

    public UnityEngine.Object Load(string name) 
    {
        if(name.Equals(LastLoadName) && LastLoadObject != null)
        {
            return LastLoadObject;
        }
        else
        {
            LastLoadObject=Resources.Load(name);
            LastLoadName = name;
            return LastLoadObject;
        }
        

    }

    public ResourceRequest AsyncLoad( string name)
    {
        return Resources.LoadAsync(name);
    }

}
