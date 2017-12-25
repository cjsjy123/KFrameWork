//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;
//using KUtils;
//using KFrameWork;
//using UnityEditor;

//[CustomEditor(typeof(ButtonExpand),true)]
//public class ButtonExpandEditor :Editor {

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        ButtonExpand btn = target as ButtonExpand;
//        if (btn != null)
//        {
//            btn.btnlabel = (TextExpand)EditorGUILayout.ObjectField("内置label",btn.btnlabel, typeof(TextExpand), true);

//        }
//    }

//}
