using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KUtils;
using System;

namespace KFrameWork
{
    [ModelRegisterClass]
    public abstract class NetConnection
    {
        private static Dictionary<int, NetConnection> ReferencesConnections = new Dictionary<int, NetConnection>();
        [ModelRegister]
        private static void Register(BaseAttributeRegister register)
        {
            register.RegisterHandler(RegisterType.ClassAttr,typeof(NetAutoRegisterAttribute), RegisterToConnection);
        }

        private static void RegisterToConnection(object att,object target)
        {
            NetAutoRegisterAttribute attribute = att as NetAutoRegisterAttribute;
            if (!ReferencesConnections.ContainsKey(attribute.id))
            {
                ReferencesConnections[attribute.id] = System.Activator.CreateInstance(target as System.Type) as NetConnection;
            }
            else
            {
                LogMgr.LogErrorFormat("重复注册conncetion :{0}",target);
            }
        }

        public static void CloseAll()
        {
            var en = ReferencesConnections.GetEnumerator();
            while (en.MoveNext())
            {
                en.Current.Value.release();
            }
        }

        public static NetConnection Get(int def)
        {
            NetConnection connect;
            if (ReferencesConnections.TryGetValue(def, out connect))
            {
                return connect;
            }
            return null;
        }

        public static T GenericGet<T>(int def) where T:NetConnection
        {
            NetConnection connect;
            if (ReferencesConnections.TryGetValue(def, out connect))
            {
                return (T)connect;
            }
            return null;
        }
        private bool _isConnnected;
        public bool isConnected
        {
            get
            {
                return _isConnnected;
            }
            set
            {
                if (_isConnnected != value)
                {
                    if (value)
                        ConnectionOpened(this);
                    else
                        ConnectionClosed(this);

                    _isConnnected = value;
                }
            }
        }

        public float timeout { get; set; }

        protected abstract void CreatedConnection(NetConnection connection);

        protected abstract void ConnectionOpened(NetConnection connection);

        protected abstract void ConnectionClosed(NetConnection connection);

        public abstract void MessageArrived(NetConnection connection,NetMessage msg);

        public abstract void SendMessage(NetMessage msg,Action<NetMessage> msgCallback = null);

        public abstract void ExceptionThrowed(Exception exception);

        public abstract void release();
    }
}


