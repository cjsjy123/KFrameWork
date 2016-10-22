using UnityEngine;
using System.Collections;
using KFrameWork;
using KUtils;

public class OtherScriptInvoke : UnityMonoBehaviour {

	// Use this for initialization
    protected override  void Start () {
        base.Start();
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
            Profiler.BeginSample("Test Frame CMD");
            FrameCommond  cmd = FrameCommond.Create(Done,2);
            cmd.Excute();
            Profiler.EndSample();
        }


        if(GUILayout.Button("Test Multi Frame CMD"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);
            Profiler.BeginSample("Test Multi Frame CMD");
            FrameCommond  cmd = FrameCommond.Create(Done,2);
            cmd+=FrameCommond.Create(Done,1);
            cmd+=FrameCommond.Create(Done,2);
            cmd+=FrameCommond.Create(Done,3);
            cmd.Excute();
            Profiler.EndSample();
        }

        if(GUILayout.Button("Test Multi Script CMD"))
        {
            LogMgr.LogFormat("Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);

            Profiler.BeginSample("Test Multi Script CMD");
            ScriptCommond cmd = ScriptCommond.Create(10,2);
            cmd.CallParms.WriteUnityObject(this);
            cmd.CallParms.WriteFloat(1f);
            for(int i=0; i <10;++i)
            {
                ScriptCommond temp = ScriptCommond.Create(10,2);
                temp.CallParms.WriteUnityObject(this);
                temp.CallParms.WriteFloat(1f);
                cmd+=temp;
            }

            cmd.Excute();
            Profiler.EndSample();
        }


        if(GUILayout.Button("Test Batch CMD"))
        {
            LogMgr.LogFormat("Batch Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);
            Profiler.BeginSample("Test Batch CMD");
            ScriptCommond cmd1 = ScriptCommond.Create(10,2);
            cmd1.CallParms.WriteUnityObject(this);
            cmd1.CallParms.WriteFloat(1f);

            ScriptCommond cmd2 = ScriptCommond.Create(10,2);
            cmd2.CallParms.WriteUnityObject(this);
            cmd2.CallParms.WriteFloat(23f);

            BatchCommond batch1 = BatchCommond.Create(FrameCommond.Create(Done,1),cmd1);
            BatchCommond batch2 = BatchCommond.Create(FrameCommond.Create(Done,2),cmd2);
            batch1+= batch2;
            batch1.Excute();
            Profiler.EndSample();
        }


        if(GUILayout.Button("Test Batch Frame CMD"))
        {
            LogMgr.LogFormat("BatchFrame Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);
            Profiler.BeginSample("Test Batch Frame CMD");
            BatchCommond batch1 = BatchCommond.Create(FrameCommond.Create(Done,1),FrameCommond.Create(Done,1),FrameCommond.Create(Done,1),FrameCommond.Create(Done,1));
            batch1.Excute();
            Profiler.EndSample();
        }

        if(GUILayout.Button("Test time Cmd"))
        {
            LogMgr.LogFormat("Now Frame : {0} Now Time:{1}",GameSyncCtr.mIns.RenderFrameCount,Time.realtimeSinceStartup);

            Profiler.BeginSample("Test time Cmd ");

            TimeCommond cmd = TimeCommond.Create(TimeDone,0.4f);
            cmd.Excute();

            Profiler.EndSample();
        }

        if(GUILayout.Button("Test Both Frame Op"))
        {
            LogMgr.LogFormat(" Now Frame {0}",GameSyncCtr.mIns.RenderFrameCount);
            Profiler.BeginSample("Test Both Frame Op -Co");
            StartCoroutine(YieldCall());
            Profiler.EndSample();
            Profiler.BeginSample("Test Both Frame Op -frame cmd");
            FrameCommond  cmd = FrameCommond.Create(Done,1);
            cmd.Excute();
            Profiler.EndSample();
        }
    }
}
