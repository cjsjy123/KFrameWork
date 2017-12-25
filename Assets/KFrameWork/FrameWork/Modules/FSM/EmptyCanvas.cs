using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;

namespace KFrameWork
{
    [NodeCanvasType("Empty")]
    public class EmptyCanvas : NodeCanvas
    {
        public override string canvasName { get { return "Empty Canvas"; } }

        protected override void OnCreate()
        {
            
        }

        public void OnEnable()
        {
            // Register to other callbacks, f.E.:
            //NodeEditorCallbacks.OnDeleteNode += OnDeleteNode;
        }

        protected override void ValidateSelf()
        {

        }
    }
}

