using UnityEngine;
using System.Collections;
using System.Reflection;
using KFrameWork;
using System;
using KUtils;
using System.Runtime.CompilerServices;

public class AttributeRegister 
{
    public static void Register( GameFrameWork frameWork)
    {
        frameWork.RegisterHandler(RegisterType.ClassAttr, typeof(SingleTonAttribute), Register_Singleton);
        frameWork.RegisterHandler(RegisterType.ClassAttr, typeof(GameServiceAttribute), Register_JustCallContrustion);
        frameWork.RegisterHandler(RegisterType.ClassAttr, typeof(GSReceiverAttribute), Register_JustCallContrustion);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(LoopEventAttribute), Register_LoopEvent);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(SceneEnterAttribute), Register_SceneAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(ScenLeaveAttribute), Register_SceneLeaveAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(Script_SharpLogicAttribute), Register_SharpScriptLogicAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(Script_LuaLogicAttribute), Register_LuaScriptLogicAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokAwakeAttribute), Register_KFKAwakecAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokDestroyAttribute), Register_KFKDestroyAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokStartAttribute), Register_KFKStartcAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokDevicePausedAttribute), Register_KFKPauseAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokDeviceQuitAttribute), Register_KFKQuitAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokDisableAttribute), Register_KFKDisableAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokEnableAttribute), Register_KFKEnableAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokFixedUpdateAttribute), Register_KFKFixedupdateAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokUpdateAttribute), Register_KFKUpdateAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokLateUpdateAttribute), Register_KFKLateupdateAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWokBeforeUpdateAttribute), Register_KFKBeforeUpdateAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(DelegateMethodAttribute), Register_DelegateAtt);

    }

    private static void Register_JustCallContrustion(object att,object target)
    {
        
    }


    private static void Register_Singleton(object att,object target)
    {
        Type targetType = target as System.Type;
        SingleTonAttribute attval = att as SingleTonAttribute;
        FieldInfo[] statField = targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

        if (statField != null )
        {
            bool exist = false;
            for (int i = 0; i < statField.Length; ++i)
            {
                FieldInfo f = statField[i];
                if (f.FieldType == targetType)
                {
                    if(attval.initTp == -1)
                    {
                        f.SetValue(null, Activator.CreateInstance(targetType,true));
                    }
                    else
                    {
                        SceneCtr.Register_SceneSingleton((KEnum)attval.initTp,0,f);
                    }

                    if(attval.destroyTp != -1)
                    {
                        SceneCtr.Register_SceneSingleton((KEnum)attval.destroyTp,1,f);
                    }

                    exist = true;
                    break;
                }
            }

            if (!exist)
                LogMgr.Log(targetType +" 类型不包含单例的静态字段");
        }

    }


    private static void Register_LoopEvent(object att,object target)
    {
        LoopEventAttribute attval = att as LoopEventAttribute;
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(attval.ev,method);
        }


    }

    private static void Register_SceneAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            if(method.ReflectedType == typeof(SceneCtr))
            {
                MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.OnLevelWasLoaded,method,true);
            }
            else
            {
                MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.OnLevelWasLoaded,method);
            }

        }
        
    }

    private static void Register_SceneLeaveAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            if(method.ReflectedType == typeof(SceneCtr))
            {
                MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.OnlevelLeaved,method,true);
            }
            else
            {
                MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.OnlevelLeaved,method);
            }
        }

    }

    private static void Register_SharpScriptLogicAtt(object att,object target)
    {
        Script_SharpLogicAttribute attval = att as Script_SharpLogicAttribute;
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            ScriptLogicCtr.mIns.RegisterLogicFunc(method,attval.CMD,null,ScriptTarget.Sharp);
        }
        
    }

    private static void Register_LuaScriptLogicAtt(object att,object target)
    {
        Script_LuaLogicAttribute attval = att as Script_LuaLogicAttribute;
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            ScriptLogicCtr.mIns.RegisterLogicFunc(method,attval.CMD,attval.methodName,ScriptTarget.Lua);
        }

    }
    #region framework att
    private static void Register_KFKAwakecAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.Awake,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKStartcAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.Start,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKFixedupdateAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.FixedUpdate,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKUpdateAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.Update,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKLateupdateAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.LateUpdate,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKPauseAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.OnApplicationPause,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKQuitAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.OnApplicationQuit,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKDestroyAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.OnDestroy,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKDisableAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.OnDisable,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKEnableAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.OnEnable,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKBeforeUpdateAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            MainLoop.getLoop().RegisterLoopEvent(MainLoopEvent.BeforeUpdate,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    #endregion


    private static void Register_DelegateAtt(object att,object target)
    {
        DelegateMethodAttribute delegateAtt = att as DelegateMethodAttribute;
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            Action<System.Object, int> callback =(Action<System.Object, int>)Delegate.CreateDelegate(typeof(Action<System.Object,int>),method);
            MainLoop.getLoop().PreRegisterCachedAction(delegateAtt.e,callback);


            Type tp = delegateAtt.tp;

            FieldInfo fs = tp.GetField(delegateAtt.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if(fs != null)
            {
                fs.SetValue(null,RuntimeHelpers.GetHashCode(callback));
            }
        }
    }


}
