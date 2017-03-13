
using UnityEngine;
using System.Collections;
using KFrameWork;
using KUtils;

public class LuaScriptInvoke : UnityMonoBehaviour {
#if EXAMPLE
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

            ScriptCommand cmd = ScriptCommand.Create(50,2);
            cmd.target = ScriptTarget.Lua;
            cmd.CallParams.WriteLong((long)123);
            cmd.CallParams.WriteString("你好");
            cmd.Excute();
            cmd.Release(true);
        }

    }

    [Script_LuaLogic(50, "main.lua", "Invoke_NOR")]
    public static void Invoke_NOR(AbstractParams p)
    {
        long arg_1 = p.ReadLong();
        string arg_2 = p.ReadString();
        LogMgr.LogFormat("enter Invoke_NOR arg1 ={0},arg2={1}",arg_1,arg_2);
    }

    [Script_LuaLogic(51, "main.lua","Invoke_HasR")]
    public static AbstractParams Invoke_HasR(AbstractParams p)
    {
        short arg_1 = p.ReadShort();
        UnityEngine.Object arg_2 = p.ReadUnityObject();
        short arg_3= p.ReadShort();

        LogMgr.LogFormat("enter Invoke_HasR arg1 ={0} arg2 ={1} arg3={2}",arg_1,arg_2,arg_3);
        return p;
    }
#endif
}

