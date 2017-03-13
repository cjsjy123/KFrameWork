using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using System.Reflection;
using System;

namespace KFrameWork
{
    public class GameDebugger : MonoBehaviour
    {
        private static List<FieldInfo> references = new List<FieldInfo>(); 

        void Awake()
        {
            Type[] types =  typeof(MainLoop).Assembly.GetTypes();

            for (int i = 0; i < types.Length; ++i)
            {
                Type type = types[i];
                if (type == typeof(GameDebugger) || type.IsEnum)
                    continue;
                FieldInfo[] fs = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public| BindingFlags.DeclaredOnly);
                for (int j = 0; j < fs.Length; ++j)
                {
                    FieldInfo f = fs[j];
                    if(f.FieldType.IsClass && !f.IsLiteral && !f.IsInitOnly)
                    {
                        references.Add(f);
                    }
                }

            }
        }

        void Start()
        {

        }

        void OnDestroy()
        {

            try
            {
                LogMgr.Log("---------static references --------- checker");

                int counter = 0;

                for (int i = 0; i < references.Count; ++i)
                {
                    FieldInfo field = references[i];
                    if (field != null && !field.DeclaringType.ContainsGenericParameters)
                    {
                        System.Object o = field.GetValue(null);
                        if (o != null)
                        {
                            LogMgr.LogFormat("static refence not clear :{0}>>>>> From Type :{1} >>>>>>fieldType:{2}>>>>name:{3}", o, field.DeclaringType, field.FieldType,field.Name);
                            counter++;
                        }
                    }
                }

                LogMgr.LogFormat("--------------Not Clear Count is :{0} -------------", counter);
            }
            catch (Exception ex)
            {
                LogMgr.LogError(ex);
            }

        }

    }
}


