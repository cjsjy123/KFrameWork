using UnityEngine;
using System.Collections;
using KUtils;

namespace KFrameWork
{

    public enum RegisterType
    {
        None ,

        ClassAttr ,

        MethodAtt ,

        END,

    }

    public enum FrameWorkEvent
    {
        EnterScene,
        LeaveScene,
        NetSend,
        NetReceiver,

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
        LateUpdate,
        OnLevelWasLoaded,

        OnApplicationPause,
        OnApplicationQuit,
        OnApplicationFocus,
        OnDestroy,
        OnDisable,
        OnEnable,

        ///extensions
        OnlevelLeaved,
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
    }

    public enum ScriptTarget
    {
        None,
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
