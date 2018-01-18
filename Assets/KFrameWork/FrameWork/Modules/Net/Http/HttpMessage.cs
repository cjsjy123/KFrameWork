using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public enum HttpConnectionType
    {
        Post,
        Get,
    }

    public class HttpMessage : PoolCls<HttpMessage>, NetMessage,IPool
    {
        public string url;

        public HttpConnectionType method = HttpConnectionType.Post;

        private SimpleClsDictionary<string, string> fields ;

        public bool IgnoreResp { get; set; }

        public int reiceved { get; set; }

        public int total { get; set; }

        public bool HasError
        {
            get
            {
                return this.errorType != NetError.None;
            }
        }

        public NetError errorType { get; set; }

        public static T Create<T>() where T: HttpMessage,new()
        {
            T msg = TrySpawn<T>();
            if (msg == null)
            {
                msg = new T();
            }

            return msg;
        }

        public void AddField(string k,string v)
        {
            if (fields == null)
                fields = new SimpleClsDictionary<string, string>();

            fields[k] = v;
        }

        public void RemoveField(string k)
        {
            if (fields == null)
                fields = new SimpleClsDictionary<string, string>();

            fields.RemoveKey(k);
        }

        public SimpleClsDictionary<string, string> getFields()
        {
            ////protect
            //if (fields == null)
            //    fields = new SimpleClsDictionary<string, string>();
            return fields;
        }

        public string ToFieldString()
        {
            if (this.fields == null)
            {
                return "";
            }

            string endstring = "";
            var keys = this.fields.ReadOnlyKeys;
            var values = this.fields.ReadOnlyValues;
            for (int i = 0; i < keys.Count; ++i)
            {
                if(i != keys.Count-1)
                    endstring += string.Format("{0}={1}&", keys[i], values[i]);
                else
                    endstring += string.Format("{0}={1}", keys[i], values[i]);
            }

            return endstring;

        }


        public virtual void RemoveToPool()
        {
            if (fields != null)
            {
                fields.Clear();
            }
        }

    }
}


