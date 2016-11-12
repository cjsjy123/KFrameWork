using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KFrameWork;
using KUtils;

public class TestTask:ITask
{
    private bool _keep;
    public bool KeepWaiting {
        get {
            return _keep;
        }
    }

    public TestTask()
    {
        this._keep =true;
        TimeCommand cmd = TimeCommand.Create(Done,5f);
        cmd.ExcuteAndRelease();
    }

    private void Done()
    {
        this._keep =false;
    }
}


public class OtherScriptInvoke : UnityMonoBehaviour {

    private CacheCommand CurrentCmd;

	// Use this for initialization
    protected override  void Start () {
        base.Start();
	}


    private  void Done()
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

    private void WaitDone()
    {
        LogMgr.Log("Wait Finished");
    }

    void OnGUI()
    {
        if(GUILayout.Button("Test Frame CMD"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            CurrentCmd = FrameCommand.Create(Done,30);
            CurrentCmd.ExcuteAndRelease();

        }


        if(GUILayout.Button("Test Multi Frame CMD"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            FrameCommand cmd = FrameCommand.Create(Done,50);
            cmd+=FrameCommand.Create(Done,50);
            cmd+=FrameCommand.Create(Done,50);
            cmd+=FrameCommand.Create(Done,50);
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

            BatchCommand batch1 = BatchCommand.Create(FrameCommand.Create(Done,50),cmd1);
            BatchCommand batch2 = BatchCommand.Create(FrameCommand.Create(Done,50),cmd2);
            batch1+= batch2;
            batch1.ExcuteAndRelease();

        }


        if(GUILayout.Button("Test Batch Frame CMD"))
        {
            LogMgr.LogFormat("BatchFrame Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            CurrentCmd = BatchCommand.Create(FrameCommand.Create(Done,50),FrameCommand.Create(Done,50),FrameCommand.Create(Done,50),FrameCommand.Create(Done,50));
            CurrentCmd.ExcuteAndRelease();

        }

        if(GUILayout.Button("Test time Cmd"))
        {
            LogMgr.LogFormat("Now Frame : {0} Now Time:{1}",GameSyncCtr.mIns.RenderFrameCount,Time.realtimeSinceStartup);

            CurrentCmd= TimeCommand.Create(TimeDone,2.4f);
            CurrentCmd.ExcuteAndRelease();

        }

        if(GUILayout.Button("Test Both Frame Op"))
        {
            LogMgr.LogFormat(" Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            StartCoroutine(YieldCall());

            CurrentCmd = FrameCommand.Create(Done,50);
            CurrentCmd.ExcuteAndRelease();

        }

        if(GUILayout.Button("Test Wait Cmd"))
        {
            LogMgr.LogFormat("Wait Cmd Start Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            CurrentCmd= WaitTaskCommand.Create(new TestTask(),WaitDone);
            CurrentCmd.ExcuteAndRelease();

        }

        if(GUILayout.Button("pause"))
        {
            if(this.CurrentCmd != null)
            {
                this.CurrentCmd.Pause();
            }
        }

        if(GUILayout.Button("resume"))
        {
            if(this.CurrentCmd != null)
            {
                this.CurrentCmd.Resume();
            }
        }
    }
}
