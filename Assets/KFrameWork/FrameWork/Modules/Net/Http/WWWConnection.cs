using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    [NetAutoRegister(NetDefine.WWW)]
    public class WWWConnection : NetConnection
    {
        private SimpleClsDictionary<string, int> wwwreferences = new SimpleClsDictionary<string, int>(true);

        public const int ErrorRetryTime = 3;

        private List<WWW> tiemoutList = new List<WWW>();

        private Dictionary<WWW, Action<NetMessage>> binders = new Dictionary<WWW, Action<NetMessage>>();

        /// <summary>
        /// 当前的www
        /// </summary>
        WWW www = null;

        public WWWConnection()
        {
            this.timeout = 10;
        }

        protected override void ConnectionClosed(NetConnection connection)
        {
            if(FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("WWW Http Closed :{0}", connection);

        }

        protected override void ConnectionOpened(NetConnection connection)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("WWW Http Opened :{0}", connection);

            if(timeout > 0)
                Schedule.mIns.ScheduleInvoke(Mathf.Max(0.1f, this.timeout), www, TimeOutEvent);
        }

        protected override void CreatedConnection(NetConnection connection)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("WWW Http Connected :{0}", connection);
        }

        public override void MessageArrived(NetConnection connection, NetMessage msg)
        {
            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("WWW Http MessageArrived :{0} Msg:{1}", connection,msg);
        }


        public override void ExceptionThrowed(Exception exception)
        {
           
        }

        public override void SendMessage(NetMessage msg, Action<NetMessage> msgCallback = null)
        {
            if (GameApplication.isPlaying == false)
                return;

            HttpMessage httpmsg =msg as HttpMessage;
            TaskManager.CreateTask(SendWWW(httpmsg, msgCallback)).Start();
        }

        void TimeOutEvent(object o, int left)
        {
            WWW passwww = o as WWW;
            if (passwww != null)
            {
                tiemoutList.Add(passwww);

                if (binders.ContainsKey(passwww))
                {
                    var callback = binders[passwww];
                    if (callback != null)
                    {
                        HttpRespMessage resp = new HttpRespMessage();
                        resp.url = passwww.url;
                        resp.errorType = NetError.TimeOut;

                        callback(resp);
                    }

                    binders.Remove(passwww);
                }

                passwww.Dispose();

                LogMgr.LogErrorFormat("WWW Http timeout :{0}", this);
            }
        }

        IEnumerator SendWWW(HttpMessage httpmsg, Action<NetMessage> msgCallback = null)
        {
            WWW localWWW = null;
            if (httpmsg.method == HttpConnectionType.Post)
            {
                WWWForm form = new WWWForm();
                SimpleClsDictionary<string, string> dicts = httpmsg.getFields();

                if (dicts != null)
                {
                    List<string> keys = dicts.Keys;
                    for (int i = 0; i < keys.Count; ++i)
                    {
                        form.AddField(keys[i], dicts[keys[i]]);
                    }
                }

                if (FrameWorkConfig.Open_DEBUG)
                    LogMgr.LogFormat("发送http 请求 :{0}",httpmsg.url);

                localWWW = www = new WWW(httpmsg.url, form);
                this.isConnected = true;

                binders[localWWW] = msgCallback;
                yield return localWWW;
            }
            else
            {
                if (FrameWorkConfig.Open_DEBUG)
                    LogMgr.LogFormat("发送http 请求 :{0}", httpmsg.url);
                localWWW = www = new WWW(httpmsg.url);
                this.isConnected = true;
                binders[localWWW] = msgCallback;
                yield return localWWW;
            }

            if (localWWW != null && !tiemoutList.Remove(localWWW))
            {
                binders.Remove(localWWW);
                if (string.IsNullOrEmpty(localWWW.error))
                {
                    Schedule.mIns.UnScheduleInvoke(TimeOutEvent);

                    HttpRespMessage resp = new HttpRespMessage();
                    resp.url = httpmsg.url;
                    resp.content = localWWW.text;
                    resp.texture = localWWW.texture;
                    resp.bys = localWWW.bytes;
                    resp.total = resp.bys == null ? 0 : resp.bys.Length;
                    resp.reiceved = resp.total;

                    MessageArrived(this, resp);

                    if (msgCallback != null)
                    {
                        msgCallback(resp);
                    }
                }
                else
                {
                    LogMgr.LogErrorFormat("Www Error :{0} to {1}", localWWW.error, localWWW.url);
                    Schedule.mIns.UnScheduleInvoke(TimeOutEvent);
                    if (wwwreferences.ContainsKey(httpmsg.url))
                    {
                        int value = wwwreferences[httpmsg.url];
                        if (value < ErrorRetryTime)
                        {
                            wwwreferences[httpmsg.url] = value  + 1;
                            //retry
                            SendMessage(httpmsg, msgCallback);
                        }
                        else
                        {
                            wwwreferences.RemoveKey(httpmsg.url);

                            HttpRespMessage resp = new HttpRespMessage();
                            resp.url = httpmsg.url;
                            resp.errorType = NetError.NetException;

                            if (msgCallback != null)
                            {
                                msgCallback(resp);
                            }
                        }
                    }
                    else
                    {
                        wwwreferences.Add(httpmsg.url, 0);
                    }
                }

                this.isConnected = false;

                localWWW.Dispose();
                localWWW = null;
            }
        }

        public override void release()
        {
            this.wwwreferences.Clear();
            this.tiemoutList.Clear();
        }

    }
}
