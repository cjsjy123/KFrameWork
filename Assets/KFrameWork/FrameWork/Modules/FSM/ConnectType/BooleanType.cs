using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NodeEditorFramework;

namespace KFrameWork
{
    public class BooleanType : ValueConnectionType
    {
        public override string Identifier { get { return "Boolean"; } }
        public override Color Color { get { return Color.blue; } }
        public override Type Type { get { return typeof(bool); } }
    }
}

