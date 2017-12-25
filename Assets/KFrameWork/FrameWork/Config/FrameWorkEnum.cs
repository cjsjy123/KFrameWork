using UnityEngine;
using System.Collections;
using KUtils;

namespace KFrameWork
{

    public enum UIAlign
    {
        LEFT_TOP,
        LEFT_DOWN,
        RIGHT_TOP,
        RIGHT_DOWN,
        CENTER,
        CENTER_TOP,
        CENTER_DOWN,
    }


    public enum RegisterType
    {
        None ,

        Register,

        ClassAttr ,

        MethodAtt ,
        /// <summary>
        /// just runtime
        /// </summary>
        InstacenMethodAttr,

        END,

    }

    public enum ExceptionType
    {
        Ignore_Exception,
        Lower_Exception,
        Higher_Excetpion,
        HighDanger_Exception,

    }

    /// <summary>
    /// MainLoop 的生命周期事件
    /// </summary>
    public enum MainLoopEvent
    {
        Awake,
        Start,
        FixedUpdate,
        BeforeUpdate,
        Update,
        AfterUpdate,
        LateUpdate,
        OnLevelWasLoaded,

        OnApplicationPause,
        OnApplicationQuit,
        OnApplicationFocus,
        OnDestroy,
        OnDisable,
        OnEnable,

        ///extensions
        OnLevelLeaved,
        END
    }

    public enum ParamType
    {
        NULL    = 0,
        INT     = 1,
        SHORT   = 2,
        BOOL    = 3,
        STRING  = 4,
        LONG    = 5,
        FLOAT   = 6,
        DOUBLE  = 7,
        VETOR3  = 8,
        OBJECT  = 9,
        UNITYOBJECT = 10,
        Color       = 11,
    }

    public enum ScriptTarget
    {
        /// <summary>
        /// unknown, 将会从
        /// </summary>
        Unknown,
        Sharp,
        Lua,
    }


    public enum LockType
    {
        None = 0,
        /// <summary>
        /// 只读锁,返回一个浅拷贝
        /// </summary>
        OnlyReadNoWrite =1,

        /// <summary>
        /// 资源锁
        /// </summary>
        DontDestroy=2,

        END,
    }

    public enum FSMRunningType
    {
        Frame,
        DelayTime,
        DelayFrame,
        WhenInitInvoke,
        WhenEnableInvoke,
    }
        

}
