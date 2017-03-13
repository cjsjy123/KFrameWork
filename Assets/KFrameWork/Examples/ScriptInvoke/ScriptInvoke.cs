
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using KFrameWork;
using System.Runtime.InteropServices;

public sealed class ScriptInvoke : UnityMonoBehaviour {

#if !EXAMPLE
    protected override  void Start () {
        base.Start();
       //LogMgr.OpenLog =false;

       StartCoroutine(YieldCall());
	}

    IEnumerator YieldCall()
    {
        while(true)
        {
            yield return new WaitForSeconds(5f);

            int flag = Mathf.RoundToInt(Time.realtimeSinceStartup) % 3;
            if (flag == 0)
            {
                ScriptCommand cmd = ScriptCommand.Create(100, 2);
                cmd.CallParams.WriteLong((long)9);
                cmd.CallParams.WriteObject(null);
                //because arg == 2  this will replace 9
                cmd.CallParams.WriteLong((long)2);
                cmd.Excute();
                cmd.Release(true);
            }
            else if (flag == 1)
            {
                ScriptCommand cmd = ScriptCommand.Create(88, 2);
                cmd.InitParams.WriteObject(this);
                cmd.InitParams.WriteString(this.GetType().Name);
                cmd.InitParams.WriteString("Instance_Call");
                cmd.CallParams.WriteLong((long)29);
                cmd.CallParams.WriteLong((long)172);
                cmd.Excute();
                cmd.Release(true);
            }
            else if (flag == 2)
            {
                ScriptCommand cmd = ScriptCommand.Create(50, 2);
                cmd.target = ScriptTarget.Lua;
                //如果没有设置initparams 并且没有注册属性，再并且之前没注册过，会找不到匹配的函数
                cmd.InitParams.WriteString("main.lua");
                cmd.InitParams.WriteString("Invoke_NOR");
                cmd.CallParams.WriteLong(uint.MaxValue);
                cmd.CallParams.WriteString("你好");
                cmd.Excute();
                cmd.Release(true);
            }
        }

    }

    public void Instance_Call(AbstractParams p)
    {
        long arg_1 = p.ReadLong();
        var arg_2 = p.ReadLong();
        LogMgr.LogFormat("******enter Instance_Call arg1 ={0},arg2={1} ", arg_1, arg_2);
    }

    [Script_SharpLogic(100)]
    public static void Invoke_NOR(AbstractParams p)
    {
        long arg_1 = p.ReadLong();
        var arg_2 = p.ReadObject();
        var arg_3 = p.ReadLong();
        LogMgr.LogFormat(">>>>>>enter Invoke_NOR arg1 ={0},arg2={1} arg3={2}",arg_1,arg_2,arg_3);

        ScriptCommand cmd = ScriptCommand.Create(101,2);

        cmd.CallParams.WriteShort((short)12);
        cmd.CallParams.WriteUnityObject(MainLoop.getLoop());
        cmd.Excute();
        cmd.Release(true);

    }

    [Script_SharpLogic(101)]
    public static AbstractParams Invoke_HasR(AbstractParams p)
    {
        short arg_1 = p.ReadShort();
        UnityEngine.Object arg_2 = p.ReadUnityObject();
        short arg_3= p.ReadShort();

        LogMgr.LogFormat("$$$$$$$enter Invoke_HasR arg1 ={0} arg2 ={1} arg3={2}",arg_1,arg_2,arg_3);
        return p;
    }
#endif
}

