using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KFrameWork
{
    [Serializable]
    public abstract class FSMElement : Node
    {
        private static Texture2D _greenTex;

        public static Texture2D GreenTexture
        {
            get
            {
                if(_greenTex == null)
                {
                    _greenTex = new Texture2D(1,1);
                    _greenTex.SetPixel(0,0,Color.green);
                    _greenTex.Apply();
                }
                return _greenTex;
            }
        }

        public int Priority = 0;

        public bool isRunning
        {
            get
            {
                if (FSMCtr.mIns != null)
                {
                    return this == FSMCtr.mIns.CurrentFSMElement(this);
                }
                return false;
            }
        }

        public override Vector2 DefaultSize
        {
            get
            {
                return new Vector2(250, 300);
            }
        }

        [FSMOutPutConnectionKnob("Output", "Float", ConnectionCount.Multi)]
        public FSMOutputNode OutputValue;

        [FSMInputConnectionKnob("Input", "Float", ConnectionCount.Multi)]
        public FSMInputNode InputValue;
#if UNITY_EDITOR
        private bool TestForNext = false;

#endif
        protected NodeCanvas nodecanvas;

        public virtual bool OpenBaseLog { get { return false; } }
        #region abstract
        public abstract void ResetValues();

        protected abstract bool UpdateFrame(long frameCnt);

        protected abstract bool Select(FSMElement element);

        protected abstract void OnEnter();

        protected abstract void OnExit();

        protected abstract void DrawFSMGUI();

        protected abstract void From(FSMElement element);
        #endregion

        [Script_SharpLogic((int)FrameWorkCmdDefine.FSMCallEnter)]
        private static void CallEnter(AbstractParams p)
        {
            if (p.ArgCount > 0)
            {
                FSMElement e = p.ReadObject() as FSMElement;
                if (e != null)
                {
                    e.OnEnter();
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        [Script_SharpLogic((int)FrameWorkCmdDefine.FSMCallExit)]
        private static void CallExit(AbstractParams p)
        {
            if (p.ArgCount > 0)
            {
                FSMElement e = p.ReadObject() as FSMElement;
                if (e != null)
                {
                    e.OnExit();
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public virtual void OnCanvasFinished()
        {

        }

        public FSMElement SelectForNext()
        {
            for (int i = 0; i < this.OutputValue.connections.Count; ++i)
            {
                var element = OutputValue.connections[i].body as FSMElement;
                if (element != null && this.Select(element))
                {
                    element.From(this);
                    return element;
                }
            }

            return null;
        }

        public bool UpdateFrameInFSM(long cnt)
        {
            if (nodecanvas == null && FSMCtr.mIns != null)
            {
                FSMCtr.mIns.TryGetCanvas(this,out nodecanvas);
            }

#if UNITY_EDITOR
            if (TestForNext)
            {
                TestForNext = false;
                return false;
            }
#endif
            return this.UpdateFrame(cnt);

        }

        public sealed override void NodeGUI()
        {
#if UNITY_EDITOR
            bool running = this.isRunning;
            if (running)
            {
                this.backgroundColor = Color.green;
            }
            else
            {
                this.backgroundColor = Color.white;
            }

            GUILayout.BeginHorizontal();
            Priority = RTEditorGUI.IntField(new GUIContent("Priority:", "state excute order "),Priority);
            GUILayout.Toggle(running, new GUIContent("Running state:","if it is the fsm current state"));
            GUILayout.EndHorizontal();
#if UNITY_EDITOR
            TestForNext = GUILayout.Toggle(TestForNext, new GUIContent("To Next", "Just for Test"));
#endif

            DrawFSMGUI();
            if (GUI.changed)
            {
                NodeEditor.curNodeCanvas.OnNodeChange(this);
            }
#endif
        }

        protected T DrawField<T>(T value,string content,string tips = null)
        {
#if UNITY_EDITOR
            if (typeof(T) == typeof(int))
            {
                return DynamicConvert<int, T>.Convert(RTEditorGUI.IntField(new GUIContent(content, tips), DynamicConvert<T, int>.Convert(value)));
            }
            else if (typeof(T) == typeof(float))
            {
                return DynamicConvert<float, T>.Convert(RTEditorGUI.FloatField(new GUIContent(content, tips), DynamicConvert<T, float>.Convert(value)));
            }
            else if (typeof(T) == typeof(string))
            {
                return DynamicConvert<string, T>.Convert(RTEditorGUI.TextField(new GUIContent(content, tips), DynamicConvert<T, string>.Convert(value)));
            }
            else if(value is UnityEngine.Object)
            {
                return DynamicConvert<UnityEngine.Object, T>.Convert(UnityEditor.EditorGUILayout.ObjectField(new GUIContent(content, tips),DynamicConvert<T, UnityEngine.Object>.Convert(value),typeof(T), true ));
            }
#endif
            return value;
        }

    }
}