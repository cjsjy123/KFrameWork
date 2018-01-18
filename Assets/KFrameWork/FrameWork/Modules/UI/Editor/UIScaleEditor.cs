using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIScale))]
[CanEditMultipleObjects]
public class UIScaleEditor : Editor {



    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("UIScale");
        EditorGUILayout.ObjectField(target,typeof(Component),true);

        EditorGUILayout.EndHorizontal();

        for(int i =0; i < targets.Length;++i)
        {
            UIScale scalescript = targets[i] as UIScale;
            Vector2 value = scalescript.uiscale;

            GUILayout.BeginHorizontal();
            GUILayout.Label("x scale", GUILayout.Width(60));
            float x = GUILayout.HorizontalSlider(value.x, -1000, 1000);

            x = EditorGUILayout.FloatField(x,GUILayout.Width(50));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("y scale", GUILayout.Width(60));
            float y= GUILayout.HorizontalSlider(value.y, -1000, 1000);

            y= EditorGUILayout.FloatField(y, GUILayout.Width(50));

            GUILayout.EndHorizontal();

            value = new Vector2(x, y);
            scalescript.uiscale = value;
            if(GUILayout.Button("confirm"))
            {
                scalescript.SetScale(value);
            }
            
        }
    }

}
