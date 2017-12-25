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
            return this._keep;
        }
    }

    public TestTask()
    {
        this._keep =true;
        TimeCommand cmd = TimeCommand.Create(5f,Done);
        cmd.Excute();
    }

    private void Done()
    {
        this._keep =false;
    }
}


public class OtherScriptInvoke : UnityMonoBehaviour {

    private CacheCommand CurrentCmd;

    private int waitcnt;
	// Use this for initialization
    protected override  void Start () {
        base.Start();
	}


    private  void Done(FrameCommand cmd)
    {
        LogMgr.LogFormat("帧命令已经完成 :{0} :{1}", GameSyncCtr.mIns.RenderFrameCount,Time.renderedFrameCount);

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

    private void WaitDone(WaitTaskCommand cmd)
    {
        LogMgr.LogFormat("Wait Finished :{0}" , GameSyncCtr.mIns.RenderFrameCount);
    }

    void OnGUI()
    {
        waitcnt = Mathf.RoundToInt( GUILayout.HorizontalSlider(waitcnt,0,500));
        if(GUILayout.Button("Test Frame CMD :"+waitcnt))
        {
            LogMgr.LogFormat("Now Frame {0} :{1}",GameSyncCtr.mIns.RenderFrameCount,Time.renderedFrameCount);

            CurrentCmd = FrameCommand.Create(waitcnt,Done );
            CurrentCmd.Excute();

        }

        if (GUILayout.Button("Cancel All"))
        {
            CacheCommand.CanCelAll();
        }


        if (GUILayout.Button("Cancel framecmd"))
        {
            CurrentCmd.Cancel();
        }

        if (GUILayout.Button("Cancel framecmd By"))
        {
            CacheCommand.CanCelAllBy(this);
        }

        if (GUILayout.Button("Test Frame Restart"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);
            GameSyncCtr.mIns.NeedReCalculateFrameCnt =true;

        }
        if(GUILayout.Button("Test Multi Frame CMD"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            FrameCommand cmd = FrameCommand.Create(50, Done);
            cmd+=FrameCommand.Create(50, Done);
            cmd+=FrameCommand.Create(50, Done);
            cmd+=FrameCommand.Create(50, Done);
            cmd.Excute();

        }

        if(GUILayout.Button("Test Multi Script CMD"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            ScriptCommand cmd = ScriptCommand.Create(10,2);
            cmd.CallParams.WriteUnityObject(this);
            cmd.CallParams.WriteFloat(1f);
            for(int i=0; i <10;++i)
            {
                ScriptCommand temp = ScriptCommand.Create(10,2);
                temp.CallParams.WriteUnityObject(this);
                temp.CallParams.WriteFloat(1f);
                cmd+=temp;
            }

            cmd.Excute();
        }


        if(GUILayout.Button("Test Batch CMD"))
        {
            LogMgr.LogFormat("Batch Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            ScriptCommand cmd1 = ScriptCommand.Create(10,2);
            cmd1.CallParams.WriteUnityObject(this);
            cmd1.CallParams.WriteFloat(1f);

            ScriptCommand cmd2 = ScriptCommand.Create(10,2);
            cmd2.CallParams.WriteUnityObject(this);
            cmd2.CallParams.WriteFloat(23f);

            BatchCommand batch1 = BatchCommand.Create(FrameCommand.Create(50,Done),cmd1);
            BatchCommand batch2 = BatchCommand.Create(FrameCommand.Create(50,Done),cmd2);
            batch1+= batch2;
            batch1.Excute();

        }


        if(GUILayout.Button("Test Batch Frame CMD"))
        {
            LogMgr.LogFormat("BatchFrame Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            CurrentCmd = BatchCommand.Create(FrameCommand.Create(50,Done),FrameCommand.Create(50, Done),FrameCommand.Create(50, Done),FrameCommand.Create(50, Done));
            CurrentCmd.Excute();

        }

        if(GUILayout.Button("Test time Cmd"))
        {
            LogMgr.LogFormat("Now Frame : {0} Now Time:{1}",GameSyncCtr.mIns.RenderFrameCount,Time.realtimeSinceStartup);

            CurrentCmd= TimeCommand.Create(2.4f,TimeDone);
            CurrentCmd.Excute();

        }

        if(GUILayout.Button("Test Both Frame Op"))
        {
            LogMgr.LogFormat(" Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            StartCoroutine(YieldCall());

            CurrentCmd = FrameCommand.Create(50,Done);
            CurrentCmd.Excute();

        }

        if(GUILayout.Button("Test Wait Cmd"))
        {
            LogMgr.LogFormat("Wait Cmd Start Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            CurrentCmd= WaitTaskCommand.Create(new TestTask(),WaitDone);
            CurrentCmd.Excute();

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
