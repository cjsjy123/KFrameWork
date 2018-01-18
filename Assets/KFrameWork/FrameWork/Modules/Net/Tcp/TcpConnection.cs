using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KFrameWork
{
    [NetAutoRegister(NetDefine.Tcp)]
    public class TcpConnection : NetConnection
    {
        protected Socket msocket;

        private string selfip;

        private int selfport;

        private int logicLock;

        protected object objectLock = new object();

        private int _ReceiveBufferSize ;

        public int ReceiveBufferSize
        {
            get
            {
                return _ReceiveBufferSize;
            }
            set
            {
                if (_ReceiveBufferSize != value || buffer == null)
                {
                    lock (objectLock)
                    {
                        if (buffer == null)
                        {
                            buffer = NetByteBuffer.CreateWithSize(value);
                        }
                        else
                        {
                            byte[] bys = buffer.GetResult();
                            //resize
                            buffer = NetByteBuffer.CreateWithSize(value);
                            buffer.Write(bys);
                        }
                    }

                    _ReceiveBufferSize = value;

                }
            }
        }

        protected List<KeyValuePair<float,int>> sendQueue = new List<KeyValuePair<float, int>>();

        protected Queue<TcpRespMessage> receiveQueue = new Queue<TcpRespMessage>();

        protected NetByteBuffer buffer;

        private int connectFlag;

        private Action<bool> ConnectCallback;

        public TcpConnection()
        {
            ReceiveBufferSize = 1024;
            timeout = 10;
        }

        public override void MessageArrived(NetConnection connection, NetMessage msg)
        {
            if (FrameWorkConfig.Open_DEBUG)
            {
                LogMgr.LogFormat("receive Tcp Message :{0}", msg);
            }
        }

        public override void release()
        {
            Close();

            this.selfport = 0;
            this.selfip = null;

            MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.LateUpdate, UpdateAsync2Sync);
        }

        protected virtual void TimeOutCallBack(int code)
        {

        }

        public override void ExceptionThrowed(Exception exception)
        {
            if (exception is SocketException)
            {
                SocketException ex = exception as SocketException;
                if (ex.SocketErrorCode == SocketError.SocketError || ex.SocketErrorCode == SocketError.Shutdown
|| ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.ConnectionRefused
|| ex.SocketErrorCode == SocketError.ConnectionAborted || ex.SocketErrorCode == SocketError.Disconnecting)
                {
                    isConnected = false;
                    msocket = null;
                }
                else
                {
                    BeginReceive();
                }
            }

        }

        protected virtual byte[] WriteHead(NetByteBuffer buffer, TcpMessage tcpmsg, byte[] body)
        {
            return null;
        }

        public override void SendMessage(NetMessage msg, Action<NetMessage> msgCallback = null)
        {
            if (isConnected)
            {
                TcpMessage tcpmsg = msg as TcpMessage;
                if (tcpmsg != null)
                {
                    byte[] netmsg = tcpmsg.Serialize();
                    NetByteBuffer buffer = NetByteBuffer.CreateWithSize(netmsg.Length+ 10);

                    netmsg = this.WriteHead(buffer,tcpmsg,netmsg);

                    if (netmsg != null && netmsg.Length > 0)
                    {
                        if (!msg.IgnoreResp)
                            sendQueue.Add(new KeyValuePair<float, int>(tcpmsg.getSendTime(), tcpmsg.SubCmd));

                        //LogMgr.LogFormat("发送消息 :{0}", tcpmsg);
                        msocket.BeginSend(netmsg, 0, netmsg.Length, SocketFlags.None, SendCallback, null);
                    }
                    else
                    {
                        LogMgr.LogError(" TcpMessage Serialize error");
                    }
                }
                else
                {
                    LogMgr.LogError("Not TcpMessage");
                }
            }
            else
            {
                LogMgr.LogError("tcp connected not yield");
            }
        }

        protected void SendCallback(IAsyncResult result)
        {
            try
            {
                if (!result.IsCompleted)
                {
                    LogMgr.LogError("send error ");
                }

                //暂时如此先
                if (this.msocket != null)
                {
                    msocket.EndSend(result);//int bytelen = 
                    //LogMgr.LogFormat("send {0} bytes", bytelen);
                }
            }
            catch (Exception ex)
            {
                LogMgr.LogError(ex);
            }

        }

        protected override void ConnectionClosed(NetConnection connection)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("TCP :{0} Connection Closed",connection);
        }

        protected override void ConnectionOpened(NetConnection connection)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("TCP :{0} Connection Open", connection);

        }

        protected override void CreatedConnection(NetConnection connection)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("TCP :{0} Connection Created", connection);

            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.LateUpdate, UpdateAsync2Sync);
        }


        public void RemoveTimeOutMsg(int timeoutcmd,float time)
        {
            for (int i = 0; i < sendQueue.Count; ++i)
            {
                KeyValuePair<float, int> kv = sendQueue[i];
                if (kv.Value == timeoutcmd)
                {
                    if (time == kv.Key)
                    {
                        sendQueue.RemoveAt(i);
                        break;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("时间匹配异常 :{0}", kv.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 同步事件
        /// </summary>
        /// <param name="v"></param>
        protected virtual void UpdateAsync2Sync(int v)
        {
            if (connectFlag > 0)
            {
                if (ConnectCallback != null)
                {
                    ConnectCallback(true);
                }

                ConnectCallback = null;
                connectFlag = 0;
            }

            while (receiveQueue.Count > 0)
            {
                TcpRespMessage msg = receiveQueue.Dequeue();
                this.MessageArrived(this,msg);
            }

            float currenttime = GameSyncCtr.mIns.FrameWorkTime;
            for (int i = sendQueue.Count - 1; i >= 0; i--)
            {
                KeyValuePair<float, int> kv = sendQueue[i];
                if (currenttime - kv.Key > this.timeout)
                {
                    TimeOutCallBack(kv.Value);
                    sendQueue.RemoveAt(i);
                }
            }
        }

        [FrameWorkDestroy]
        private static void Quit(int lv)
        {
            if (NetConnection.GenericGet<TcpConnection>(NetDefine.Tcp) != null)
            {
                NetConnection.GenericGet<TcpConnection>(NetDefine.Tcp).Close();
            }
        }

        void CloseSocket()
        {
            if (msocket != null)
            {
                msocket.Shutdown(SocketShutdown.Both);
                msocket.Close();
                msocket = null;
            }
        }

        public void Close()
        {
            if (isConnected && msocket != null)
            {
                LogMgr.Log("close Socket");
                CloseSocket();

                if (buffer != null)
                {
                    buffer.Dispose();
                    buffer = null;
                }

                isConnected = false;

                selfip = null;
                selfport = 0;
                logicLock = 0;
            }
        }

        public void Connect(string ipaddress,int port,Action<bool> connectCbk)
        {
            if (msocket == null)
            {
                msocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
                this.CreatedConnection(this);
            }

            ///判断是否重复链接
            if (isConnected && selfip.Equals(ipaddress) && selfport == port)
            {
                return;
            }
            ///正在执行逻辑
            if (logicLock == 1)
            {
                return;
            }

            IPAddress targetIP;
            if (!IPAddress.TryParse(ipaddress, out targetIP))
            {
                LogMgr.LogErrorFormat("ipaddress convert Error :{0}", ipaddress);
            }
            else
            {
                IPEndPoint ipe = new IPEndPoint(targetIP, port);

                this.selfport = port;
                this.selfip = ipaddress;

                logicLock++;
                //replace
                this.ConnectCallback = connectCbk;

                try
                {
                    msocket.BeginConnect(ipe, ConnectCallBack, null);
                }
                catch (Exception ex)
                {
                    logicLock--;
                    LogMgr.LogError(ex);
                    ExceptionThrowed(ex);
                }
            }
        }

        void ConnectCallBack(IAsyncResult result)
        {
            try
            {
                if (result.IsCompleted)
                {
                    if (this.msocket != null)
                    {
                        this.msocket.EndConnect(result);

                        this.isConnected = true;
                        this.connectFlag++;
                        //
                        BeginReceive();
                    }
                }
                else
                {
                    LogMgr.LogError("connect completed failed");
                }

                logicLock--;//unlock
            }
            catch (Exception ex)
            {
                LogMgr.LogError(ex);
                logicLock--;//unlock
                this.isConnected = false;
                ExceptionThrowed(ex);
            }
        }

        void BeginReceive()
        {
            if (msocket != null && isConnected)
            {
                byte[] buffer = new byte[ReceiveBufferSize/2];
                msocket.BeginReceive(buffer,0, buffer.Length, SocketFlags.None, ReceiveCallBack, buffer);
            }
        }

        void ReceiveCallBack(IAsyncResult result)
        {
            try
            {
                if (!result.IsCompleted)
                {
                    LogMgr.LogError("recive error");
                }
                if (msocket != null && isConnected )
                {
                    int datalen = msocket.EndReceive(result);
                    if (datalen > 0)
                    {
                       // LogMgr.LogFormat("接受到字节 :{0}", datalen);
                        byte[] bysarray = result.AsyncState as byte[];

                        if (buffer.byteAviliable + datalen > buffer.capacity && FrameWorkConfig.Open_DEBUG)
                        {
                            LogMgr.LogError("缓冲区即将扩容");
                        }

                        lock (objectLock)
                        {
                            buffer.Write(bysarray, 0, datalen);
                        }

                        //NetByteBuffer copy = buffer.Copy();

                        //开始pack
                        DispatcherPkg(buffer);
                    }
                    //继续接受
                    BeginReceive();
                }
                else
                {
                    LogMgr.LogError("not connect to socket");
                }
            }
            catch (SocketException ex)
            {
                LogMgr.LogError(ex);
                ExceptionThrowed(ex);

            }
            catch (Exception ex)
            {
                LogMgr.LogError(ex);
                ExceptionThrowed(ex);
            }

        }

        protected virtual void DispatcherPkg(NetByteBuffer tempBuffer)
        {
            //read head

        }
    }
}


