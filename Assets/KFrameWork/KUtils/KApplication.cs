using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class KApplication
{

    public static bool is32
    {
        get
        {
            return IntPtr.Size == 4;
        }
    }

    public static bool is64
    {
        get
        {
            return IntPtr.Size == 8;
        }
    }

    public static bool isIOS
    {
        get
        {
            #if UNITY_IPHONE || UNITY_IOS
            return true;
            #else
            return false;

            #endif

        }

    }

    public static bool isAndroid
    {
        get
        {
            #if UNITY_ANDROID
            return true;
            #else
            return false;
            #endif

        }

    }

    private static string _persistentDataPath ;

    public static string persistentDataPath
    {
        get
        {
            if (_persistentDataPath == null)
            {
                _persistentDataPath = Application.persistentDataPath;
            }
            return _persistentDataPath;
        }
    }


    private static string _DataPath;

    public static string DataPath
    {
        get
        {
            if (_DataPath == null)
            {
                _DataPath = Application.dataPath;
            }
            return _DataPath;
        }
    }
}