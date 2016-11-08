using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using KFrameWork;
using System.Runtime.InteropServices;

public sealed class ScriptInvoke : UnityMonoBehaviour {

    protected override  void Start () {
        base.Start();
       //LogMgr.OpenLog =false;

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
