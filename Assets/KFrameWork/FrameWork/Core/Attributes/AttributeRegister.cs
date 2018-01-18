using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using KFrameWork;
using System;
using KUtils;
using System.Runtime.CompilerServices;

public static class AttributeRegister 
{
    public static void Register( FrameworkAttRegister frameWork)
    {
        frameWork.RegisterHandler(RegisterType.ClassAttr, typeof(SingleTonAttribute), Register_Singleton);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(SceneEnterAttribute), Register_SceneAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(SceneLeaveAttribute), Register_SceneLeaveAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(Script_SharpLogicAttribute), Register_SharpScriptLogicAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(Script_LuaLogicAttribute), Register_LuaScriptLogicAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkDestroyAttribute), Register_KFKDestroyAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkStartAttribute), Register_KFKStartcAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkDevicePausedAttribute), Register_KFKPauseAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkDeviceQuitAttribute), Register_KFKQuitAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkDisableAttribute), Register_KFKDisableAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkEnableAttribute), Register_KFKEnableAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkFixedUpdateAttribute), Register_KFKFixedupdateAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkUpdateAttribute), Register_KFKUpdateAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkLateUpdateAttribute), Register_KFKLateupdateAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkBeforeUpdateAttribute), Register_KFKBeforeUpdateAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(FrameWorkAfterUpdateAttribute), Register_KFKAfterUpdateAtt);
        frameWork.RegisterHandler(RegisterType.MethodAtt, typeof(DelegateMethodAttribute), Register_DelegateAtt);

        frameWork.RegisterHandler(RegisterType.Register, typeof(ModelRegisterAttribute), Reigister_Model);
        //GameAttrRegister.Register(frameWork);
    }

    private static void Reigister_Model(object att, object target)
    {
        ModelRegisterAttribute attribute = att as ModelRegisterAttribute;
        KeyValuePair<BaseAttributeRegister, MethodInfo> value =(KeyValuePair<BaseAttributeRegister, MethodInfo>) target  ;

        if (attribute != null && value.Value != null && value.Value.IsStatic)
        {
            MethodInfo method = value.Value;
            var param = method.GetParameters();
            if (param.Length == 1 && param[0].ParameterType == typeof(BaseAttributeRegister))
            {
                method.Invoke(null, new object[] { value.Key });
            }
            else {
                LogMgr.LogErrorFormat("参数不匹配，无法自动注册 :{0}", method.DeclaringType);
            }
        }
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
                        GameSceneCtr.Register_SceneSingleton((KEnum)attval.initTp,0,f);
                    }

                    if(attval.destroyTp != -1)
                    {
                        GameSceneCtr.Register_SceneSingleton((KEnum)attval.destroyTp,1,f);
                    }

                    exist = true;
                    break;
                }
            }

            if (!exist)
                LogMgr.Log(targetType +" 类型不包含单例的静态字段");
        }

    }
        

    private static void Register_SceneAtt(object att,object target)
    {
        SceneEnterAttribute scenter = att as SceneEnterAttribute;
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            if (MainLoop.getInstance() != null)
                MainLoop.getInstance().RegisterStaticEvent(MainLoopEvent.OnLevelWasLoaded, method,scenter.Priority);
        }
    }

    private static void Register_SceneLeaveAtt(object att,object target)
    {
        SceneLeaveAttribute scene = att as SceneLeaveAttribute;
        MethodInfo method = target as MethodInfo;
        if (method.isLoopFunction())
        {
            if (MainLoop.getInstance() != null)
                MainLoop.getInstance().RegisterStaticEvent(MainLoopEvent.OnLevelLeaved, method, scene.Priority); 
        }
    }

    private static void Register_SharpScriptLogicAtt(object att,object target)
    {
        Script_SharpLogicAttribute attval = att as Script_SharpLogicAttribute;
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            ScriptLogicCtr.mIns.RegisterLogicFunc(method, attval.CMD, method.Name, attval, ScriptTarget.Sharp);
        }
        
    }

    private static void Register_LuaScriptLogicAtt(object att,object target)
    {
        Script_LuaLogicAttribute attval = att as Script_LuaLogicAttribute;
        MethodInfo method = target as MethodInfo;
        if(method.LogStaticMethod())
        {
            string methodname = string.IsNullOrEmpty(attval.methodName) ? method.Name : attval.methodName;
            ScriptLogicCtr.mIns.RegisterLogicFunc(method,attval.CMD, methodname, attval, ScriptTarget.Lua);
        }

    }
    #region framework att

    private static void Register_KFKStartcAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.Start,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKFixedupdateAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.FixedUpdate,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKUpdateAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.Update,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKLateupdateAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.LateUpdate,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKPauseAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.OnApplicationPause,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKQuitAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.OnApplicationQuit,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKDestroyAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.OnDestroy,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKDisableAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.OnDisable,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKEnableAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.OnEnable,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKBeforeUpdateAtt(object att,object target)
    {
        MethodInfo method = target as MethodInfo;
        if(method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.BeforeUpdate,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>),method),
                false);
        }
    }

    private static void Register_KFKAfterUpdateAtt(object att, object target)
    {
        MethodInfo method = target as MethodInfo;
        if (method.isLoopFunction())
        {
            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.AfterUpdate,
                (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), method),
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
            MainLoop.getInstance().PreRegisterCachedAction(delegateAtt.Invokeopportunity,callback);


            Type tp = delegateAtt.tp;

            FieldInfo fs = tp.GetField(delegateAtt.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if(fs != null)
            {
                fs.SetValue(null, FrameWorkTools.GetHashCode(callback));
            }
        }
    }


}
