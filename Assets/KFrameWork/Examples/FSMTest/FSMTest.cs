using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;


public class FSMTestState:KEnum
{
    public FSMTestState(string key,int value):base(key,value)
    {
        
    }

    public static FSMTestState FirstState =new FSMTestState("FirstState",1);
    public static FSMTestState SecondState =new FSMTestState("SecondState",2);
    public static FSMTestState ThirdState =new FSMTestState("ThirdState",3);
    public static FSMTestState FourthState =new FSMTestState("FourthState",4);
    public static FSMTestState FifthState =new FSMTestState("FifthState",5);
}


public class FSMTest : UnityMonoBehaviour {

    private FSMElement root ;

    protected override void Awake ()
    {
        base.Awake ();
    }
	// Use this for initialization
    protected override void Start () {

        root = FSMCtr.mIns.CreateFSMMachine(this).CreateElement<FSMElement>(FSMTestState.FirstState, new FrameTestRunner());
	}

    private void ToState2()
    {
        LogMgr.Log("ToState2 Done time = "+ Time.realtimeSinceStartup);
        root.ChangeState(FSMTestState.SecondState);
    }

    private void ToState3()
    {
        LogMgr.Log("ToState3 Done time = "+ Time.realtimeSinceStartup);
        root.ChangeState(FSMTestState.ThirdState);
    }


    void OnGUI()
    {
        if(GUILayout.Button("Awake Root"))
        {
            root.AwakeState();
        }

        if(GUILayout.Button("InActive Root"))
        {
            root.InActiveState();
        }

        if(GUILayout.Button("test delay time SecondState "))
        {
            root.RegisterState<FSMElement>(FSMTestState.SecondState,new DelayTimeRunner());
            TimeCommand cmd = TimeCommand.Create(ToState2,1f);
            cmd.ExcuteAndRelease();
        }

        if(GUILayout.Button("Awake SecondState  "))
        {
            if(root[FSMTestState.SecondState] != null)
            {
                root[FSMTestState.SecondState].AwakeState();
            }
        }

        if(GUILayout.Button("Awake ThirdState "))
        {
            if(root[FSMTestState.ThirdState] != null)
            {
                root[FSMTestState.ThirdState].AwakeState();
            }
        }

        if(GUILayout.Button("test delay time ThirdState  "))
        {
            DelayTimeRunner runner = new DelayTimeRunner();
            runner.delaytime =2f;
            root.RegisterState<FSMElement>(FSMTestState.ThirdState,runner);

            TimeCommand cmd = TimeCommand.Create(ToState3,2f);
            cmd.ExcuteAndRelease();
        }

        if(GUILayout.Button("Register and Awake Fourth state"))
        {
            if(root[FSMTestState.ThirdState] != null)
            {
                root[FSMTestState.ThirdState].RegisterState<FSMElement>(FSMTestState.FourthState,new DelayTimeRunner()).AwakeState();
            }
        }

        if(GUILayout.Button("Register and Awake Fourth state"))
        {
            if(root[FSMTestState.FourthState] != null)
            {
                root[FSMTestState.FourthState].RegisterState<FSMElement>(FSMTestState.FifthState,new FrameTestRunner()).AwakeState();
            }
        }


    }


}
