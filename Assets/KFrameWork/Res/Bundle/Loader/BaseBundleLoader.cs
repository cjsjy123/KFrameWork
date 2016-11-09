//#define AB_DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public abstract class BaseBundleLoader
{
    

    #region events
    public abstract void OnStart();

    public abstract void OnError();

    public abstract void OnFinish();

    public abstract void OnPaused();

    public abstract void OnResume();

    #endregion


    protected string getBundleDownloadPath (string urlpath)
    {

        if (BundleConfig.SAFE_MODE) {
            if (string.IsNullOrEmpty (urlpath))
                throw new ArgumentException (string.Format ("ulr error: {0}", urlpath));

            //trim
            urlpath = urlpath.Trim ();

            bool gend = urlpath.EndsWith ("/");

            if (gend)
                urlpath = urlpath.Remove (urlpath.Length - 1, 1);

            if (BundleConfig.ABDownLoadPath.EndsWith ("/")) {
                #if UNITY_EDITOR && !AB_DEBUG
                return string.Format ("file://{0}{1}", Application.streamingAssetsPath, urlpath);
                #elif UNITY_IOS || UNITY_IPHONE
                return string.Format("{0}{1}",BundleConfig.ABDownLoadPath,urlpath);
                #elif UNITY_ANDROID
                return string.Format("{0}{1}",BundleConfig.ABDownLoadPath,urlpath);
                #endif
            }

        }

        #if UNITY_EDITOR && !AB_DEBUG
        return string.Format ("file://{0}/{1}", Application.streamingAssetsPath, urlpath);
        #elif UNITY_IOS || UNITY_IPHONE
        return string.Format("{0}/{1}",BundleConfig.ABDownLoadPath,urlpath);
        #elif UNITY_ANDROID
        return string.Format("{0}/{1}",BundleConfig.ABDownLoadPath,urlpath);
        #endif
    }



    protected string getBundleRootPath (string basename)
    {
        if (BundleConfig.SAFE_MODE) {
            if (string.IsNullOrEmpty (basename))
                throw new ArgumentException (string.Format ("ulr error: {0}", basename));

            //trim
            basename = basename.Trim ();

            bool gend = basename.EndsWith ("/");

            if (gend)
                basename = basename.Remove (basename.Length - 1, 1);

            if (BundleConfig.ABSavePath.EndsWith ("/")) {
                #if UNITY_EDITOR && !AB_DEBUG
                return string.Format ("file://{0}/{1}{2}", Application.streamingAssetsPath, BundleConfig.ABSavePath, basename);
                #elif UNITY_IOS || UNITY_IPHONE
                return string.Format("{0}/{1}{2}",Application.streamingAssetsPath,BundleConfig.ABSavePath,basename);
                #elif UNITY_ANDROID
                return string.Format("{0}!assets/{1}{2}", Application.dataPath, BundleConfig.ABSavePath, basename);;
                #endif
            }

        }

        #if UNITY_EDITOR && !AB_DEBUG
        return string.Format ("file://{0}/{1}/{2}", Application.streamingAssetsPath, BundleConfig.ABSavePath, basename);
        #elif UNITY_IOS || UNITY_IPHONE
                return string.Format("{0}/{1}/{2}",Application.streamingAssetsPath,BundleConfig.ABSavePath,basename);
        #elif UNITY_ANDROID
                return string.Format("{0}!assets/{1}/{2}", Application.dataPath, BundleConfig.ABSavePath, basename);;
        #endif
    }

    protected string getBundlePersistentPath (string basename)
    {

        if (BundleConfig.SAFE_MODE) {
            if (string.IsNullOrEmpty (basename))
                throw new ArgumentException (string.Format ("ulr error: {0}", basename));

            //trim
            basename = basename.Trim ();

            bool gend = basename.EndsWith ("/");

            if (gend)
                basename = basename.Remove (basename.Length - 1, 1);

        }

        #if UNITY_EDITOR && !AB_DEBUG
        return string.Format ("file://{0}/Editor/{1}", Application.persistentDataPath, basename);
        #elif UNITY_IOS || UNITY_IPHONE
                return string.Format("{0}/IOS/{1}",Application.persistentDataPath,basename);
        #elif UNITY_ANDROID
                return string.Format("{0}/ANDROID/{1}", Application.persistentDataPath, basename);;
        #endif
    }
}
