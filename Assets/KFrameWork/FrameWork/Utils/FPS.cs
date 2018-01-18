using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public class FPS : MonoBehaviour {


    void Start () {
        if(!MainLoop.getInstance().OpenFps){
           
            enabled = false;
        }
    }
        
    void OnGUI()
    {
        try
        {
            GUI.skin.label.normal.textColor = Color.black;

            int old = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 30;
            GUI.Label(new Rect(Screen.width-150,0,150,60),string.Format("{0:F2} FPS",GameSyncCtr.mIns.Fps));

            GUI.skin.label.fontSize = old;
        }
        catch (Exception ex)
        {
            LogMgr.LogException(ex);
            this.enabled=false;
        }
        

    }
}
