using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;

public class FieldCode
{
    public string name;

    public string TypeName;

    public bool isgeneric;
}


[Serializable]
public class SubSharpCode
{
    public ReplaceFlag replaceflag;

    public SeekFlag seekFlag;

    public string seekContent;
    /// <summary>
    /// T 代表泛型参数，f代表字段名字，t代表字段类型 b为基本类型
    /// </summary>
    public string replaceContent;

}


[Serializable]
public class SharpCodeModel  {
    /// <summary>
    /// file search flag
    /// </summary>
    public FileSearchFlag Flag;
    /// <summary>
    ///target path
    /// </summary>
    public string TargetPath;

    public string OutPutPath;

    public string FlagString;

    public List<SubSharpCode> SubCodes = new List<SubSharpCode>();

}
