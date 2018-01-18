using UnityEngine;
using System.Collections;
using System;

namespace KFrameWork
{

    public interface ITask
    {
        bool KeepWaiting{get;}
    }

    public interface IScriptLoader
    {
        string methodname { get; }

        void Init(AbstractParams InitParams);

        AbstractParams Invoke(AbstractParams ScriptParms);

        void Reset();
    }


    public interface FSMEvent
    {
        bool DetermineRequest();

        void EnterFS();

        void LeaveFS();
    }

    public interface ISerialize
    {
        byte[] Serialize();
    }

    public interface IDeSerialize
    {
        void DeSerialize(byte[] bys);

        void DeSerialize(NetByteBuffer buffer);
    }

    public interface FSMRunningEvent
    {
        string name{get;set;}
        bool changed {get;set;}
        long lastFrame{get;set;}
        float lasttime{get;set;}
        float? delaytime{get;}
        int? delayFrame{get;}

        FSMRunningType runningType{get;}

        void FrameUpdateForLogic();

        void DelayTimeUpdateForLogic();

        void DelayFrameUpdateForLogic();

        void InvokeOnceWhenInit();

        void InvokeOnceWhenEnable();
    }
}



