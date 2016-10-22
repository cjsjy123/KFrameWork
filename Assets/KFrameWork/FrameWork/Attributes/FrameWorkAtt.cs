using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokAwakeAttribute:Attribute
{
    
}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokEnableAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokDisableAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokStartAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokUpdateAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokLateUpdateAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokDestroyAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokDevicePausedAttribute:Attribute
{

}
    

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokDeviceQuitAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokFixedUpdateAttribute:Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class FrameWokBeforeUpdateAttribute:Attribute
{

}