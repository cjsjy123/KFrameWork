
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
        private static string _streamingAssetsPath;

        public static string streamingAssetsPath
        {
            get
            {
                if (_streamingAssetsPath == null)
                    _streamingAssetsPath = Application.streamingAssetsPath;
                return _streamingAssetsPath;
            }
        }

        private static string _persistentDataPath;

        public static string persistentDataPath
        {
            get
            {
                if (_persistentDataPath == null)
                    _persistentDataPath = Application.persistentDataPath;
                return _persistentDataPath;
            }
        }

        private static string _dataPath;

        public static string dataPath
        {
            get
            {
                if (_dataPath == null)
                    _dataPath = Application.dataPath;
                return _dataPath;
            }
        }

        [FrameWorkStartAttribute]
        private static void PreLoad(int v)
        {

            gstring.Intern(streamingAssetsPath);
            gstring.Intern(persistentDataPath);
            gstring.Intern(dataPath);

        }

        public static string getBundleDownloadPath(string basename)
        {
#if UNITY_EDITOR 
            return gstring.Format("{0}/Editor/{1}", persistentDataPath, basename);
#elif UNITY_IOS || UNITY_IPHONE
                return gstring.Format("{0}/IOS/{1}",persistentDataPath,basename);
#elif UNITY_ANDROID
                return gstring.Format("{0}/ANDROID/{1}", persistentDataPath, basename);;
#endif
        }

        public static string getBundleStreamPath(string basename)
        {

            if (BundleConfig.SAFE_MODE)
            {
                if (string.IsNullOrEmpty(basename))
                    throw new FrameWorkException(gstring.Format("ulr error: {0}", basename));
                //trim
                basename = basename.Trim();

                bool gend = basename.EndsWith("/");

                if (gend)
                    basename = basename.Remove(basename.Length - 1, 1);

                if (BundleConfig.ABSavePath.EndsWith("/"))
                {
#if UNITY_EDITOR 
                    return gstring.Format("{0}/{1}/{2}", streamingAssetsPath, BundleConfig.ABSavePath, basename);
#elif UNITY_IOS || UNITY_IPHONE
                return gstring.Format("{0}/{1}/{2}",streamingAssetsPath,BundleConfig.ABSavePath,basename);
#elif UNITY_ANDROID
                return gstring.Format("jar:file://{0}!/assets/{1}/{2}", dataPath, BundleConfig.ABSavePath, basename);;
#endif
                }

            }

#if UNITY_EDITOR 
            return gstring.Format("{0}/{1}/{2}", streamingAssetsPath, BundleConfig.ABSavePath, basename);
#elif UNITY_IOS || UNITY_IPHONE
                return gstring.Format("{0}/{1}/{2}",streamingAssetsPath,BundleConfig.ABSavePath,basename);
#elif UNITY_ANDROID
                return gstring.Format("jar:file://{0}!/assets/{1}/{2}", dataPath, BundleConfig.ABSavePath, basename);;
#endif

        }

        public static string getBundlePersistentPath(string basename)
        {
            if (BundleConfig.SAFE_MODE)
            {
                if (string.IsNullOrEmpty(basename))
                    throw new FrameWorkException(gstring.Format("ulr error: {0}", basename));

                //trim
                basename = basename.Trim();

                bool gend = basename.EndsWith("/");

                if (gend)
                    basename = basename.Remove(basename.Length - 1, 1);

            }

#if UNITY_EDITOR 
            return gstring.Format("{0}/Editor/{1}", persistentDataPath, basename);
#elif UNITY_IOS || UNITY_IPHONE
                return gstring.Format("{0}/IOS/{1}",persistentDataPath,basename);
#elif UNITY_ANDROID
                return gstring.Format("{0}/ANDROID/{1}", persistentDataPath, basename);;
#endif

        }

        public static string EditorName2AssetName(string name)
        {
            //gstring result = name;
            name = name.Replace('\\', '/');
            return name;
        }
        /// <summary>
        /// 获取运行时路径，优先判断可读写目录下的文件
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
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
#if UNITY_EDITOR
            return "file://" + path;
#else
            return path;
#endif

        }

    }
}


