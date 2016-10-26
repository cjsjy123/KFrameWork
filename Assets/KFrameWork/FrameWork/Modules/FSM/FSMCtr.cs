using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    [SingleTon]
    public class FSMCtr  {

        public static FSMCtr mIns;

        public FSMRuningComponet CreateFSMMachine<T>(T t) where T:Component
        {
            FSMRuningComponet c = t.GetComponent<FSMRuningComponet>();
            if(c == null)
                c = t.gameObject.AddComponent<FSMRuningComponet>();

            return c;
        }

    }
}

