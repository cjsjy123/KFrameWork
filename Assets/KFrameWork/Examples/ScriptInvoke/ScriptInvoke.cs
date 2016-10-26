using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using KFrameWork;
using System.Runtime.InteropServices;

public class ScriptInvoke : UnityMonoBehaviour {

    protected override  void Start () {
        base.Start();
       //LogMgr.OpenLog =false;

//        AbstractParams simple = GenericParams.Create();
//
//        simple.InsertShort(0,(short)4);
//        simple.InsertShort(0,(short)14);
//        simple.PushShort((short)5);
////        simple.PushShort((short)6);
////        simple.PushShort((short)7);
//        simple.PushUnityObject(null);
//        simple.PushBool(true);
//
//        var temp= SimpleParams.Create();
//        temp.PushLong((long)11);
//
//        simple.Push(temp);
//
//        var v1 = simple.PopShort();
//        var v2 = simple.PopShort();
//        var v3 = simple.PopShort();
//       
//        var v5 = simple.PopUnityObject();
//        var v6 = simple.PopBool();
//        var v4 = simple.PopLong();

//        KBaseState k =new KBaseState();
//        KBaseState k1 =new KBaseState();
//
//        ScriptCommand cmd = ScriptCommand.Create((int)AttrtiDataCMD.LevlUp_CMD,1);
//        cmd.CallParms.PushInt(123);
//        cmd.Excute();


       StartCoroutine(YieldCall());
	}

    IEnumerator YieldCall()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);

            ScriptCommand cmd = ScriptCommand.Create(100,2);
            cmd.CallParms.WriteLong((long)9);
            cmd.CallParms.WriteObject(null);
            cmd.CallParms.WriteLong((long)2);
            cmd.Excute();
            cmd.Release(true);

        }

    }

    [Script_SharpLogic(100)]
    public static void Invoke_NOR(AbstractParams p)
    {
        long arg_1 = p.ReadLong();
        var arg_2 = p.ReadObject();
        var arg_3 = p.ReadLong();
        LogMgr.LogFormat("enter Invoke_NOR arg1 ={0},arg2={1} arg3={2}",arg_1,arg_2,arg_3);

        ScriptCommand cmd = ScriptCommand.Create(101,2);

        cmd.CallParms.WriteShort((short)12);
        cmd.CallParms.WriteUnityObject(MainLoop.getLoop());
        cmd.Excute();

        AbstractParams ret = cmd.ReturnParams;

        LogMgr.LogFormat("返回值 {0}",ret);

        cmd.Release(true);

    }

    [Script_SharpLogic(101)]
    public static AbstractParams Invoke_HasR(AbstractParams p)
    {
        short arg_1 = p.ReadShort();
        UnityEngine.Object arg_2 = p.ReadUnityObject();
        short arg_3= p.ReadShort();

        LogMgr.LogFormat("enter Invoke_HasR arg1 ={0} arg2 ={1} arg3={2}",arg_1,arg_2,arg_3);
        return p;
    }
}
