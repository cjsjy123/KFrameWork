using UnityEngine;
using System.Collections;
using System;

namespace KFrameWork
{
    public interface ISharedPtr
    {
        void AddRef ();

        void RemoveRef ();
    }


    public interface ILife
    {
        void Created ();

        void Destoryed ();
    }

    public interface iPush
    {
        void WriteInt(int v);
        void WriteShort(short v);
        void WriteString(string v);
        void WriteBool(bool v);
        void WriteFloat(float v);
        void WriteDouble(double v);
        void WriteLong(long v);
        void WriteVector3(Vector3 v);
        void WriteObject(System.Object v);
        void WriteUnityObject(UnityEngine.Object v);
    }

    public interface iSet
    {
        void SetInt(int argindex,int v);
        void SetShort(int argindex,short v);
        void SetString(int argindex,string v);
        void SetBool(int argindex,bool v);
        void SetLong(int argindex,long v);
        void SetFloat(int argindex,float v);
        void SetDouble(int argindex,double v);
        void SetVector3(int argindex, Vector3 v);
        void SetObject(int argindex,System.Object v);
        void SetUnityObject(int argindex,UnityEngine.Object v);
    }

    public interface iInsert
    {
        void InsertInt(int index,int v);
        void InsertShort(int index,short v);
        void InsertString(int index,string v);
        void InsertBool(int index,bool v);
        void InsertFloat(int index,float v);
        void InsertDouble(int index,double v);
        void InsertLong(int index,long v);
        void InsertVector3(int index, Vector3 v);
        void InsertObject(int index,System.Object v);
        void InsertUnityObject(int index,UnityEngine.Object v);
    }

    public interface iPop
    {
        int ReadInt();
        bool ReadBool();
        short ReadShort();
        string ReadString();
        long ReadLong();
        float ReadFloat();
        double ReadDouble();
        Vector3 ReadVector3();
        System.Object ReadObject();
        UnityEngine.Object ReadUnityObject();
    }

    public interface IScriptLoader
    {
        bool CanDispose{get;}
        void Init(AbstractParams InitParams);
        AbstractParams Invoke(AbstractParams ScriptParms);
        void PushAttachObject(System.Object o);
        void RemovettachObject(System.Object o);

        void Reset();
    }

    public interface ICommand
    {
        int? CMD{ get; }

        bool isDone{ get; }

        ICommand Next{ get; set; }

        AbstractParams CallParms{ get; }

        bool HasCallParams{ get; }

        AbstractParams ReturnParams { get; set; }

        void Release (bool force);

        void Excute ();

        void Stop();

        void Pause();

        void Resume();

    }

    public interface IService
    {
        /// <summary>
        /// 如果ireceiver为空，则所有对象都会受到消息
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="p">P.</param>
        /// <param name="receiver">Receiver.</param>
        void DisPatcher(string name,AbstractParams p,IReveiver receiver = null);
    }

    public interface IReveiver
    {
        void Receive(IService sender, string name,AbstractParams p);
    }

    public interface FSMEvent
    {
        bool DetermineRequest();

        void EnterFS();

        void LeaveFS();
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



