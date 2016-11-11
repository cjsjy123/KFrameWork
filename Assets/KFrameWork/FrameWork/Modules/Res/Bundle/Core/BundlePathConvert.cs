using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using KUtils;

namespace KFrameWork
{
    public static class BundlePathConvert
    {
        public static string getBundleDownloadPath(string urlpath)
        {

            if (BundleConfig.SAFE_MODE)
            {
                if (string.IsNullOrEmpty(urlpath))
                    throw new FrameWorkException(string.Format("ulr error: {0}", urlpath));

                //trim
                urlpath = urlpath.Trim();

                bool gend = urlpath.EndsWith("/");

                if (gend)
                    urlpath = urlpath.Remove(urlpath.Length - 1, 1);

                if (BundleConfig.ABDownLoadPath.EndsWith("/"))
                {

                    return string.Format("{0}/{1}{2}", Application.persistentDataPath, BundleConfig.ABDownLoadPath, urlpath);

                }

            }

            return string.Format("{0}/{1}/{2}", Application.persistentDataPath,BundleConfig.ABDownLoadPath, urlpath);

        }



        public static string getBundleStreamPath(string basename)
        {
            if (BundleConfig.SAFE_MODE)
            {
                if (string.IsNullOrEmpty(basename))
                    throw new FrameWorkException(string.Format("ulr error: {0}", basename));

                //trim
                basename = basename.Trim();

                bool gend = basename.EndsWith("/");

                if (gend)
                    basename = basename.Remove(basename.Length - 1, 1);

                if (BundleConfig.ABSavePath.EndsWith("/"))
                {
#if UNITY_EDITOR && !AB_DEBUG
                    return string.Format("{0}/{1}/{2}", Application.streamingAssetsPath,BundleConfig.ABSavePath, basename);
#elif UNITY_IOS || UNITY_IPHONE
                return string.Format("{0}/{1}/{2}",Application.streamingAssetsPath,BundleConfig.ABSavePath,basename);
#elif UNITY_ANDROID
                return string.Format("{0}!assets/{1}/{2}", Application.dataPath, BundleConfig.ABSavePath, basename);;
#endif
                }

            }

#if UNITY_EDITOR && !AB_DEBUG
            return string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, BundleConfig.ABSavePath, basename);
#elif UNITY_IOS || UNITY_IPHONE
                return string.Format("{0}/{1}/{2}",Application.streamingAssetsPath,BundleConfig.ABSavePath,basename);
#elif UNITY_ANDROID
                return string.Format("{0}!assets/{1}/{2}", Application.dataPath, BundleConfig.ABSavePath, basename);;
#endif
        }

        public static string getBundlePersistentPath(string basename)
        {

            if (BundleConfig.SAFE_MODE)
            {
                if (string.IsNullOrEmpty(basename))
                    throw new FrameWorkException(string.Format("ulr error: {0}", basename));

                //trim
                basename = basename.Trim();

                bool gend = basename.EndsWith("/");

                if (gend)
                    basename = basename.Remove(basename.Length - 1, 1);

            }

#if UNITY_EDITOR && !AB_DEBUG
            return string.Format("{0}/Editor/{1}", Application.persistentDataPath, basename);
#elif UNITY_IOS || UNITY_IPHONE
                return string.Format("{0}/IOS/{1}",Application.persistentDataPath,basename);
#elif UNITY_ANDROID
                return string.Format("{0}/ANDROID/{1}", Application.persistentDataPath, basename);;
#endif
        }

        public static string EditorName2AssetName(string name)
        {
            name = name.Replace('\\', '/');
            return name;
        }

        public static string GetRunningPath(string filepath)
        {
            string perpath = getBundlePersistentPath(filepath);
            if (File.Exists(perpath))
            {
                return perpath;
            }
            return getBundleStreamPath(filepath);
        }

        public static string GetWWWPath(string path)
        {
#if UNITY_EDITOR && !AB_DEBUG
            return "file://" + path;
#else
            return path;
#endif

        }

    }
}


