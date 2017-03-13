using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace KFrameWork
{
    /// <summary>
    /// 脱离异步限制的application
    /// </summary>
    public class GameApplication
    {
        [FrameWorkStart]
        static void Start(int lv)
        {
            _playing = true;
            absoluteURL = Application.absoluteURL;
            bundleIdentifier = Application.bundleIdentifier;
            cloudProjectId = Application.cloudProjectId;
            companyName =Application.companyName;
            genuine =Application.genuine;
            genuineCheckAvailable =Application.genuineCheckAvailable;
            installMode =Application.installMode;
            internetReachability =Application.internetReachability;
            isConsolePlatform =Application.isConsolePlatform;
            isMobilePlatform = Application.isMobilePlatform;
            isShowingSplashScreen =Application.isShowingSplashScreen;
            isWebPlayer =Application.isWebPlayer;
            platform =Application.platform;
            productName =Application.productName;
            sandboxType =Application.sandboxType;
            srcValue =Application.srcValue;
            streamedBytes = Application.streamedBytes;
            systemLanguage =Application.systemLanguage;
            temporaryCachePath = Application.temporaryCachePath;
            unityVersion =Application.unityVersion;
            version = Application.version;
            webSecurityEnabled =Application.webSecurityEnabled;
            webSecurityHostUrl=Application.webSecurityHostUrl;
            _width = Screen.width;
            _Height = Screen.height;

        }

        [FrameWorkDeviceQuit]
        static void End(int lv)
        {
            _playing = false;
        }

        private static int _width;
        public static int Width
        {
            get
            {
                if (_width == 0)
                    _width = Screen.width;
                return _width;
            }
        }

        private static int _Height;
        public static int Height
        {
            get
            {
                if (_Height == 0)
                    _Height = Screen.height;
                return _Height;
            }
        }

        private static bool _playing;
        public static bool isPlaying
        {
            get
            {
                return _playing;
            }
        }

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

        private static string _persistentDataPath;

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

        public static string absoluteURL { get; private set; }

        public static string bundleIdentifier { get; private set; }

        public static string cloudProjectId { get; private set; }

        public static string companyName { get; private set; }

        public static string dataPath { get; private set; }

        public static bool genuine { get; private set; }

        public static bool genuineCheckAvailable { get; private set; }

        public static ApplicationInstallMode installMode { get; private set; }

        public static NetworkReachability internetReachability { get; private set; }

        public static bool isConsolePlatform { get; private set; }

        public static bool isMobilePlatform { get; private set; }

        public static bool isShowingSplashScreen { get;private set;  }

        public static bool isWebPlayer { get; private set; }

        public static RuntimePlatform platform { get; private set; }

        public static string productName { get; private set; }

        public static ApplicationSandboxType sandboxType { get; private set; }

        public static string srcValue { get; private set; }

        public static int streamedBytes { get; private set; }

        public static string streamingAssetsPath { get; private set; }

        public static SystemLanguage systemLanguage { get; private set; }

        public static string temporaryCachePath { get; private set; }

        public static string unityVersion { get; private set; }

        public static string version { get; private set; }

        public static bool webSecurityEnabled { get; private set; }

        public static string webSecurityHostUrl { get; private set; }
    }
}

