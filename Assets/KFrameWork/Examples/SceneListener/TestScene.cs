//#define KDEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using System;
using KFrameWork;
#if EXAMPLE

////////////test/////
/// 
public class TestGameScene:KEnum
{
    public TestGameScene(string s,int i):base(s,i){}

    public static TestGameScene t1 = new TestGameScene("example_testscene_1.unity",0);
    public static TestGameScene t2 = new TestGameScene("example_testscene_2.unity",1);
    public static TestGameScene t3 = new TestGameScene("example_testscene_3.unity", 2);
}

public class TestScene : UnityMonoBehaviour {


    protected override  void Start () {
        base.Start();
        DontDestroyOnLoad(gameObject);
        

        StartCoroutine(LoadScene());
	}


    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(3f);

        // GameSceneCtr.LoadScene((string)TestGameScene.t2);
        ResBundleMgr.mIns.LoadSceneAsync((string)TestGameScene.t2,(loader, current,end)=>
        {
            if (loader.LoadState == BundleLoadState.Finished)
            {
                loader.GetABResult().SceneAsyncResult.allowSceneActivation = true;
            }
        });

        yield return new WaitForSeconds(10f);

        ResBundleMgr.mIns.LoadSceneAsync((string)TestGameScene.t3, (loader, current, end) =>
        {
            if (loader.LoadState == BundleLoadState.Finished)
            {
                loader.GetABResult().SceneAsyncResult.allowSceneActivation = true;
            }
        });
        // GameSceneCtr.LoadScene((string)TestGameScene.t3);

    }

    [SceneEnter]
    public static void Enter(int level)
    {
        LogMgr.LogFormat("场景进入 in Test => {0}",GameSceneCtr.mIns[level].name);
    }

    [SceneLeave]
    public static void Leave(int level)
    {
        LogMgr.LogFormat("场景离开 in Test =>{0}", GameSceneCtr.mIns[level].name);
    }
	

}
#endif
