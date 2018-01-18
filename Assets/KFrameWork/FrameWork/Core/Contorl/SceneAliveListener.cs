using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KFrameWork
{
    [ScriptInitOrderAttribute(-5000)]
    public class SceneAliveListener : MonoBehaviour
    {

        void OnDestroy()
        {
            ScriptCommand cmd = ScriptCommand.Create((int)FrameWorkCmdDefine.SceneLeave, 1);
            cmd.CallParams.WriteInt(GameSceneCtr.mIns.CurScene.buildIndex);
            cmd.ExcuteAndRelease();
        }
    }

}

