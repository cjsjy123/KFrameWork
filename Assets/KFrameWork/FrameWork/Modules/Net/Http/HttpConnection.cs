using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Net;
using System.Threading;
using System.IO;

namespace KFrameWork
{
    [NetAutoRegister(NetDefine.Http)]
    public class HttpConnection : NetConnection
    {
        protected object lockObject = new object();
        //64kb
        public const int readconst = 1024;//64

        private bool needclose = false;

        private Queue<KeyValuePair<Action<NetMessage>, NetMessage>> receivedqueue = new Queue<KeyValuePair<Action<NetMessage>, NetMessage>>();

        private Queue<KeyValuePair<NetMessage, Action<NetMessage>>> httpqueue = new Queue<KeyValuePair<NetMessage, Action<NetMessage>>>();

        public HttpConnection()
        {
            this.timeout = 10000;
        }

        public override void ExceptionThrowed(Exception exception)
        {

        }

        protected override void ConnectionClosed(NetConnection connection)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("Http Closed :{0}", connection);
            isConnected = false;

            //MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.LateUpdate, this.UpdateQueue);
        }

        protected override void ConnectionOpened(NetConnection connection)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("Http Opened :{0}", connection);
            isConnected = true;
        }

        protected override void CreatedConnection(NetConnection connection)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("Http Connected :{0}",connection);

            MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.LateUpdate, this.UpdateQueue);
        }

        public override void MessageArrived(NetConnection connection, NetMessage msg)
        {

        }



        public override void SendMessage(NetMessage msg, Action<NetMessage> msgCallback = null)
        {
            if (!GameApplication.isPlaying)
                return;

            this.CreatedConnection(this);

            httpqueue.Enqueue(new KeyValuePair<NetMessage, Action<NetMessage>>(msg,msgCallback));
            ThreadPool.QueueUserWorkItem(ThreadTask);
        }

        private void UpdateQueue(int lv)
        {
            while (this.receivedqueue.Count > 0)
            {
                var tuple = receivedqueue.Dequeue();
                this.MessageArrived(this,tuple.Value);

                if(tuple.Key != null)
                    tuple.Key(tuple.Value);
            }
        }

        void ThreadTask(object obj)
        {
            KeyValuePair<NetMessage, Action<NetMessage>> tuple = default(KeyValuePair<NetMessage, Action<NetMessage>>) ;
            lock (lockObject)
            {
                tuple = httpqueue.Dequeue();
            }

            HttpMessage httpmsg = (HttpMessage)tuple.Key;
            Action<NetMessage> callback = tuple.Value;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream responseStream = null;

            try
            {
                request = new HttpWebRequest(new Uri(httpmsg.url));
                if (httpmsg.method == HttpConnectionType.Post)
                {
                    request.Accept = "text/html, application/xhtml+xml, */*";
                    request.Method = "POST";
                    SimpleClsDictionary<string, string> fields = httpmsg.getFields();
                    if (fields != null && fields.Count > 0)
                    {
                        byte[] btBodys = System.Text.Encoding.UTF8.GetBytes(httpmsg.ToFieldString());
                        using (Stream respSb = request.GetRequestStream())
                        {
                            respSb.Write(btBodys, 0, btBodys.Length);
                        }
                    }

                }
                else
                {
                    request.Method = "GET";
                    request.Accept = "*/*";
                    request.ContentType = "application/x-www-form-urlencoded,application/json";
                }

                request.Timeout = (int)this.timeout;
                byte[] allbys = null;

                using (response = (HttpWebResponse)request.GetResponse())
                {
                    allbys = new byte[response.ContentLength];
                    int readpos = 0;
                    int readlen = 1;

                    using (responseStream = response.GetResponseStream())
                    {
                        while (readlen > 0 && !needclose && readpos < allbys.Length)
                        {
                            readlen = responseStream.Read(allbys, readpos,Mathf.Min(allbys.Length - readpos, readconst));
                            readpos += readlen;
//#if UNITY_EDITOR
//                            LogMgr.LogFormat("下载：{0} /{1}", readpos, allbys.Length);
//#endif
                            if (callback != null)
                            {
                                HttpRespMessage respmsg = new HttpRespMessage();
                                respmsg.url = httpmsg.url;
                                respmsg.total = allbys.Length;
                                respmsg.reiceved = readpos;
                                respmsg.bys = allbys;
                                lock (lockObject)
                                {
                                    this.receivedqueue.Enqueue(new KeyValuePair<Action<NetMessage>, NetMessage>(callback, respmsg));
                                }
                                    
                            }
                        }
                        // if(FrameWorkConfig.Open_DEBUG)
#if UNITY_EDITOR
                        LogMgr.Log("下载结束");
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                LogMgr.LogError(ex);
                HttpRespMessage respmsg = new HttpRespMessage();
                respmsg.url = httpmsg.url;
                respmsg.errorType = NetError.NetException;
                lock (lockObject)
                {
                    this.receivedqueue.Enqueue(new KeyValuePair<Action<NetMessage>, NetMessage>(callback, respmsg));
                }
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }

                if (response != null)
                {
                    response.Close();
                }

                if (responseStream != null)
                {
                    responseStream.Close();
                }

                this.ConnectionClosed(this);
            }
        }

        public override void release()
        {
            needclose = true;
        }
    }
}


