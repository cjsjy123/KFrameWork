using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public class FPS : MonoBehaviour {


    void Start () {
        if(!MainLoop.getLoop().OpenFps){
            enabled = false;
            return;
        }
    }
        
    void OnGUI()
    {
        try
        {
            GUI.Label(new Rect(Screen.width-80,0,80,20),string.Format("{0:F2} FPS",GameSyncCtr.mIns.Fps));
        }
        catch (Exception ex)
        {
            LogMgr.LogException(ex);
            this.enabled=false;
        }
        

    }
}
