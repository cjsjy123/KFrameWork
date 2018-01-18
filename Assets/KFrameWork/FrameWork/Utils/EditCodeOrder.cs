using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 脚本执行顺序定义属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ScriptInitOrderAttribute : Attribute
{
    public int Order = 0;

    public ScriptInitOrderAttribute(int order)
    {
        this.Order = order;
    }
}

#if UNITY_EDITOR
[InitializeOnLoad]
public class EditCodeOrder
{

    static EditCodeOrder()
    {

        Dictionary<string, int> targetList = new Dictionary<string, int>();

        var baseTp = typeof(MainLoop);
        var asm = baseTp.Assembly;
        var types = asm.GetTypes();

        foreach (var tp in types)
        {
            bool define = Attribute.IsDefined(tp, typeof(ScriptInitOrderAttribute));

            if (define)
            {
                ScriptInitOrderAttribute att = tp.GetCustomAttributes(typeof(ScriptInitOrderAttribute), true)[0] as ScriptInitOrderAttribute;
                targetList.Add(tp.Name, att.Order);
            }
        }


        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
        {
            if (targetList.ContainsKey(monoScript.name))
            {
                int order = targetList[monoScript.name];
                if (MonoImporter.GetExecutionOrder(monoScript) != order)
                {
                    MonoImporter.SetExecutionOrder(monoScript, order);
                }
            }
        }
    }

}
#endif