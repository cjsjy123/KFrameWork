using UnityEngine;
using System.Collections;
using KFrameWork;
using KUtils;

public class LuaScriptInvoke : UnityMonoBehaviour {
    // Use this for initialization
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

            ScriptCommond cmd = ScriptCommond.Create(50,2);
            cmd.CallParms.WriteLong((long)123);
            cmd.CallParms.WriteString("你好");
            cmd.Excute();
            cmd.Release(true);

        }

    }

    [Script_SharpLogic(50)]
    public static void Invoke_NOR(AbstractParams p)
    {
        long arg_1 = p.ReadLong();
        string arg_2 = p.ReadString();
        LogMgr.LogFormat("enter Invoke_NOR arg1 ={0},arg2={1}",arg_1,arg_2);

        ScriptCommond cmd = ScriptCommond.Create(51,2);

        cmd.CallParms.WriteShort((short)88);
        cmd.CallParms.WriteUnityObject(MainLoop.getLoop());
        cmd.Excute();

        AbstractParams ret = cmd.ReturnParams;
        if(ret == null)
        {
            LogMgr.Log("返回值 为空");
        }
        else
        {
            LogMgr.LogFormat("返回值 {0}",ret);
        }

        cmd.Release(true);

    }

    [Script_LuaLogic(51,"Invoke_HasR")]
    public static AbstractParams Invoke_HasR(AbstractParams p)
    {
        short arg_1 = p.ReadShort();
        UnityEngine.Object arg_2 = p.ReadUnityObject();
        short arg_3= p.ReadShort();

        LogMgr.LogFormat("enter Invoke_HasR arg1 ={0} arg2 ={1} arg3={2}",arg_1,arg_2,arg_3);
        return p;
    }
}
