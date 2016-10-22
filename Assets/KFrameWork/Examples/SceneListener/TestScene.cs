using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using System;
using KFrameWork;

////////////test/////
/// 
public class GameScene:KEnum
{
    public GameScene(string s,int i):base(s,i){}

    public static GameScene t1 = new GameScene("Example_TestScene_1",0);
    public static GameScene t2 = new GameScene("Example_TestScene_2",1);
    public static GameScene t3 = new GameScene("Example_TestScene_3",2);
}

public class TestScene : UnityMonoBehaviour {


    protected override  void Start () {
        base.Start();
        DontDestroyOnLoad(gameObject);
        
        SceneCtr.DefaultScene = GameScene.t1;
        StartCoroutine(LoadScene());
	}


    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(3f);

        KFrameWork.SceneCtr.LoadScene(GameScene.t2);


        yield return new WaitForSeconds(4f);

        KFrameWork.SceneCtr.LoadScene(GameScene.t3);

    }

    [SceneEnter]
    public static void Enter(int level)
    {
        LogMgr.LogFormat("场景进入 in Test => {0}",level);
    }

    [ScenLeave]
    public static void Leave(int level)
    {
        LogMgr.LogFormat("场景离开 in Test =>{0}",level);
    }
	

}
