using System;

namespace KFrameWork
{
    public static class FrameWorkDebug
    {
        #if UNITY_EDITOR
        public static bool Open_DEBUG =false;
        public static int Preload_ParamsCount =10;
        public static bool ShowUnityInfoReport =true;
        #else
        public const bool Open_DEBUG =false;
        public const int Preload_ParamsCount =10;
        public const bool ShowUnityInfoReport =true;
        #endif
    }
}

