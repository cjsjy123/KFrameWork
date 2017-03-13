using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

public enum SeekStatus
{
    usingref,
    ClassDefine,
    syntaxStart,
    syntaxEnd,
    ClassDefineEnd,
}

public enum FileSearchFlag
{
    StartWith = 1,
    EndWith = 2,
    Contains = 4,

    IncludeSub = 128,
    Absolute = 256,
    IgnoreUpLower = 512,
}

public enum SeekFlag
{
    Class = 1,

    Field = 2,

    Method = 4,

    ToField = 8,

    ToMethod = 16,

    ContentByFieldType = 32,

    ContentByContainsFieldType = 64,
}

public enum ReplaceFlag
{
    None,
    InsertHead              =1,
    InsertEndWithMethod     =2,
    ReplaceImmediately      =4,

    InsertField             =8,
    InsertMethodHead        =16,
}