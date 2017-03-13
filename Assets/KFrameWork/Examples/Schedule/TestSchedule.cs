using UnityEngine;
using System.Collections;
using KFrameWork;
using KUtils;
#if EXAMPLE
public class TestSchedule : UnityMonoBehaviour {
    
    private float _delay =0.1f;
    private float delta = 0.5f;
    private int _times =1;
	// Use this for initialization
    protected override  void Start () {
        base.Start();
	}

    void ScheduleOnce(float time)
    {
        LogMgr.LogFormat("Once定时器启动时间: {0}",Time.realtimeSinceStartup);
        Schedule.mIns.ScheduleInvoke(time,this.InvokeDone);
    }

    void InvokeDone()
    {
        float now = Time.realtimeSinceStartup;
        LogMgr.LogFormat("当前时间为 {0}",now);
    }

    void ScheduleMultiTimes(float delay,float delta,int times)
    {
        LogMgr.LogFormat("Multi定时器启动时间: {0}",Time.realtimeSinceStartup);
        Schedule.mIns.ScheduleRepeatInvoke(delay,delta,times,this.InvokeDone);
        
    }

	
	// Update is called once per frame
	void OnGUI() {

        GUILayout.BeginHorizontal();
        GUILayout.Label("delay time : "+ _delay.ToString(),GUILayout.Width(100));
        _delay = GUILayout.HorizontalSlider(_delay,0f,10f,GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Delta time : "+ delta.ToString(),GUILayout.Width(100));
        delta = GUILayout.HorizontalSlider(delta,0f,10f,GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("invoke times : "+ _times.ToString(),GUILayout.Width(100));
        _times = (int)GUILayout.HorizontalSlider((int)_times,0f,100f,GUILayout.Width(100));
        GUILayout.EndHorizontal();

        if(GUILayout.Button("定时器invoke 1次"))
        {
            this.ScheduleOnce(_delay);
        }

        if(GUILayout.Button("定时器invoke 多次"))
        {
            this.ScheduleMultiTimes(_delay,delta,_times);
        }

	}
}
#endif
