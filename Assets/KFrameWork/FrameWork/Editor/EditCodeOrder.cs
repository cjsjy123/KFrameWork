using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;
#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public class EditCodeOrder {

    const int FrameWorkExecOrder = -10000;


    static EditCodeOrder() {
        string scriptName = typeof(MainLoop).Name;

        // Iterate through all scripts (Might be a better way to do this?)
        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
        {
            // If found our script
            if (monoScript.name == scriptName)
            {
                // And it's not at the execution time we want already
                // (Without this we will get stuck in an infinite loop)
                if (MonoImporter.GetExecutionOrder(monoScript) != FrameWorkExecOrder)
                {
                    MonoImporter.SetExecutionOrder(monoScript, FrameWorkExecOrder);
                }
                break;
            }
        }
    }

}
#endif