using UnityEngine;
using System.Collections;
using KFrameWork;

public class Test : UnityMonoBehaviour
{
    // Use this for initialization
    protected override void Start () {
        base.Start();
        LogMgr.Log("test start");
        this.test();
        LogMgr.Log("test end");
    }

    private void test()
    {
        UIContent uc = new UIContent();
        UITestContent uicontent =  this.GetComponent<UITestContent>();
        UIContentMgr.mIns.PushContent(uicontent, uc);
    }
}
