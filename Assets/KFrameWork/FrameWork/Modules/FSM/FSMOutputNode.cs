using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using System;

namespace KFrameWork
{
    public class FSMOutputNode : ValueConnectionKnob
    {
        public override bool CanApplyConnection(ConnectionPort port)
        {
            if (port is FSMInputNode)
            {
                FSMInputNode valueKnob = port as FSMInputNode;
                if (valueKnob == null || !valueType.IsAssignableFrom(valueKnob.valueType))
                    return false;

                if (valueKnob == null || body == valueKnob.body || connections.Contains(valueKnob))
                    return false;

                if (direction == Direction.None && valueKnob.direction == Direction.None)
                    return true; // None-Directive connections can always connect

                if (direction == Direction.In && valueKnob.direction != Direction.Out)
                    return false; // Cannot connect inputs with anything other than outputs
                if (direction == Direction.Out && valueKnob.direction != Direction.In)
                    return false; // Cannot connect outputs with anything other than inputs

                if (direction == Direction.Out) // Let inputs handle checks for recursion
                    return base.CanApplyConnection(this);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FSMOutPutConnectionKnobAttribute : ValueConnectionKnobAttribute
    {
        public override Type ConnectionType { get { return typeof(FSMOutputNode); } }

        public FSMOutPutConnectionKnobAttribute(string name, string type)
            : base(name, Direction.Out, type) { }
        public FSMOutPutConnectionKnobAttribute(string name, string type, ConnectionCount maxCount)
            : base(name, Direction.Out, type, maxCount) { }
        public FSMOutPutConnectionKnobAttribute(string name, string type, NodeSide nodeSide, float nodeSidePos = 0)
            : base(name, Direction.Out, type, nodeSide, nodeSidePos) { }
        public FSMOutPutConnectionKnobAttribute(string name, string type, ConnectionCount maxCount, NodeSide nodeSide, float nodeSidePos = 0)
            : base(name, Direction.Out, type, maxCount, nodeSide, nodeSidePos) { }

        // Directly typed
        public FSMOutPutConnectionKnobAttribute(string name, Type type)
            : base(name, Direction.Out, type) { }
        public FSMOutPutConnectionKnobAttribute(string name, Type type, ConnectionCount maxCount)
            : base(name, Direction.Out, type, maxCount) { }
        public FSMOutPutConnectionKnobAttribute(string name, Type type, NodeSide nodeSide, float nodeSidePos = 0)
            : base(name, Direction.Out, type, nodeSide, nodeSidePos) { }
        public FSMOutPutConnectionKnobAttribute(string name, Type type, ConnectionCount maxCount, NodeSide nodeSide, float nodeSidePos = 0)
            : base(name, Direction.Out, type, maxCount, nodeSide, nodeSidePos) { }

        public override ConnectionPort CreateNew(Node body)
        {
            FSMOutputNode knob = ScriptableObject.CreateInstance<FSMOutputNode>();
            knob.Init(body, Name, Direction, StyleID, NodeSide, NodeSidePos);
            knob.maxConnectionCount = MaxConnectionCount;
            return knob;
        }
    }

}

