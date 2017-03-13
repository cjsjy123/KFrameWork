using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

[AttributeUsage(AttributeTargets.Class)]
public class TypeInitAttribute : Attribute
{
    public int typeSorder;

    public TypeInitAttribute(int order)
    {
        this.typeSorder = order;
    }
}
    

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkEnableAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkDisableAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkStartAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkUpdateAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkLateUpdateAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkDestroyAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkDevicePausedAttribute:Attribute
{

}
    

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkDeviceQuitAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkFixedUpdateAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkBeforeUpdateAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWorkAfterUpdateAttribute : Attribute
{

}