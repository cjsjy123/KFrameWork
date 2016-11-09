using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KFrameWork;
using KUtils;


public class OtherScriptInvoke : UnityMonoBehaviour {

	// Use this for initialization
    protected override  void Start () {
        base.Start();

        LogMgr.LogFormat("初始帧 :{0}", GameSyncCtr.mIns.RenderFrameCount);
        StartCoroutine(YieldCall());

        FrameCommand  cmd = FrameCommand.Create(Done,1);
        cmd.ExcuteAndRelease();
	}


    private void Done()
    {
        LogMgr.LogFormat("帧命令已经完成 :{0}", GameSyncCtr.mIns.RenderFrameCount);

    }

    private void TimeDone()
    {
        LogMgr.LogFormat("时间命令已经完成 :{0} {1}", GameSyncCtr.mIns.RenderFrameCount,Time.realtimeSinceStartup);
    }
	
    [Script_SharpLogic(10)]
    private static void OtherInvoke(AbstractParams args)
    {
        UnityEngine.Object o = args.ReadUnityObject();
        float f = args.ReadFloat();

        LogMgr.LogFormat("Other invoke {0} {1} ",o,f);
    }


    IEnumerator YieldCall()
    {
        yield return new WaitForEndOfFrame();

        LogMgr.LogFormat("协程finish:{0}", GameSyncCtr.mIns.RenderFrameCount);
    }

    void OnGUI()
    {
        if(GUILayout.Button("Test Frame CMD"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            FrameCommand  cmd = FrameCommand.Create(Done,2);
            cmd.ExcuteAndRelease();

        }


        if(GUILayout.Button("Test Multi Frame CMD"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            FrameCommand  cmd = FrameCommand.Create(Done,2);
            cmd+=FrameCommand.Create(Done,1);
            cmd+=FrameCommand.Create(Done,2);
            cmd+=FrameCommand.Create(Done,3);
            cmd.ExcuteAndRelease();

        }

        if(GUILayout.Button("Test Multi Script CMD"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            ScriptCommand cmd = ScriptCommand.Create(10,2);
            cmd.CallParms.WriteUnityObject(this);
            cmd.CallParms.WriteFloat(1f);
            for(int i=0; i <10;++i)
            {
                ScriptCommand temp = ScriptCommand.Create(10,2);
                temp.CallParms.WriteUnityObject(this);
                temp.CallParms.WriteFloat(1f);
                cmd+=temp;
            }

            cmd.Excute();

        }


        if(GUILayout.Button("Test Batch CMD"))
        {
            LogMgr.LogFormat("Batch Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            ScriptCommand cmd1 = ScriptCommand.Create(10,2);
            cmd1.CallParms.WriteUnityObject(this);
            cmd1.CallParms.WriteFloat(1f);

            ScriptCommand cmd2 = ScriptCommand.Create(10,2);
            cmd2.CallParms.WriteUnityObject(this);
            cmd2.CallParms.WriteFloat(23f);

            BatchCommand batch1 = BatchCommand.Create(FrameCommand.Create(Done,1),cmd1);
            BatchCommand batch2 = BatchCommand.Create(FrameCommand.Create(Done,2),cmd2);
            batch1+= batch2;
            batch1.ExcuteAndRelease();

        }


        if(GUILayout.Button("Test Batch Frame CMD"))
        {
            LogMgr.LogFormat("BatchFrame Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            BatchCommand batch1 = BatchCommand.Create(FrameCommand.Create(Done,1),FrameCommand.Create(Done,1),FrameCommand.Create(Done,1),FrameCommand.Create(Done,1));
            batch1.ExcuteAndRelease();

        }

        if(GUILayout.Button("Test time Cmd"))
        {
            LogMgr.LogFormat("Now Frame : {0} Now Time:{1}",GameSyncCtr.mIns.RenderFrameCount,Time.realtimeSinceStartup);

            TimeCommand cmd = TimeCommand.Create(TimeDone,0.4f);
            cmd.ExcuteAndRelease();

        }

        if(GUILayout.Button("Test Both Frame Op"))
        {
            LogMgr.LogFormat(" Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            StartCoroutine(YieldCall());

            FrameCommand  cmd = FrameCommand.Create(Done,1);
            cmd.ExcuteAndRelease();

        }
    }
}
