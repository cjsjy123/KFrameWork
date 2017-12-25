using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public abstract class PoolCls<T>  where T : class,IPool
    {
        #region fields

        #endregion

        protected static U TrySpawn<U>() where U : class, T
        {
            if (KObjectPool.mIns != null)
            {
                return KObjectPool.mIns.Pop<U>();
            }

            return default(U);
        }

        protected static object TrySpawnWithType(Type tp) 
        {
            if (KObjectPool.mIns != null)
            {
                return KObjectPool.mIns.Pop(tp);
            }

            return null;
        }

        public static void TryDespawn(object data)
        {
            if (KObjectPool.mIns != null)
            {
                KObjectPool.mIns.Push(data);
            }
        }
    }

}

