//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;
//using KFrameWork;
//using KUtils;
//using System.Reflection;


//[AttributeUsage(AttributeTargets.Class )]
//public class GameServiceAttribute : Attribute {


//    private static Dictionary<Type,Dictionary<Type,IReveiver>> ServiceDic = new Dictionary<Type, Dictionary<Type, IReveiver>>() ;

//    private Type targetTp ;

//    public static Dictionary<Type,Dictionary<Type,IReveiver>> GetDic()
//    {
//        return ServiceDic;
//    }

//    public GameServiceAttribute(Type selfTp){

//        try
//        {

//            targetTp = selfTp;

//            if(!ServiceDic.ContainsKey(targetTp))
//            {
//                Dictionary<Type,IReveiver> dic =new Dictionary<Type, IReveiver>();
//                ServiceDic.Add(selfTp,dic);
//            }

//        }
//        catch (Exception ex)
//        {
//            LogMgr.LogError(ex);
//        }

//    }
////
////    private void CheckSendAtt(MethodInfo method)
////    {
////        bool defined = method.IsDefined(typeof(GSSendAttribute),true);
////        if(defined)
////        {
////            GSSendAttribute att = method.GetCustomAttributes(typeof(GSSendAttribute),true)[0] as GSSendAttribute;
////            att.Sender = (Action<IService,string,IReveiver> ) Delegate.CreateDelegate(typeof(Action<IService,string,IReveiver>),method);
////            att.Sender += Send;
////        }
////    }
////
//    public void Bind(Type receiverTp)
//    {
//        if(ServiceDic.ContainsKey(targetTp))
//        {
//            Dictionary<Type,IReveiver> dic = ServiceDic[targetTp];
//            if(!dic.ContainsKey(receiverTp))
//            {
//                dic.Add(receiverTp,null);
//            }
//        }
//        else
//        {
//            Dictionary<Type,IReveiver> dic =new Dictionary<Type, IReveiver>();
//            dic.Add(receiverTp,null);
//            ServiceDic.Add(targetTp,dic);
//        }
//    }

//    public static void Send(IService self, string name,IReveiver target)
//    {
//        LogMgr.Log(self);
//    }

//}

////[AttributeUsage(AttributeTargets.Method )]
////public class GSSendAttribute:Attribute
////{
////    public Action<IService,string,IReveiver> Sender;
////
////    public GSSendAttribute(){}
////
////}


//[AttributeUsage(AttributeTargets.Class )]
//public class GSReceiverAttribute : Attribute {

//    private GameServiceAttribute service;

//    public GSReceiverAttribute(Type serviceTp,Type selfTp){
//        bool defined = serviceTp.IsDefined(typeof(GameServiceAttribute),true);

//        if(defined)
//        {
//            bool assignenable = typeof(IReveiver).IsAssignableFrom(selfTp);
            
//            this.service = serviceTp.GetCustomAttributes(typeof(GameServiceAttribute),true)[0] as GameServiceAttribute;
//            if(assignenable )
//            {
//                this.service.Bind(selfTp);
//            }
//            else
//            {
//                LogMgr.LogErrorFormat("{0} 为实现ireceiver 接口 ",selfTp);
//            }
//        }
//        else
//        {
//            throw new FrameWorkException(serviceTp +" 不包含 ServiceAttribute");
//        }

//    }

//}
