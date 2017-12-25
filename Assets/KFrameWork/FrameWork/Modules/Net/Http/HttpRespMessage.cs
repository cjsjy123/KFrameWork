using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Net;

namespace KFrameWork
{
    public struct HttpRespMessage:NetMessage
    {
        public string url;

        private string _content ;
        public string content
        {
            get
            {
                if (!finished)
                {
                    LogMgr.LogError("仍未完成");
                }
                if (_content == null && bys != null)
                {
                    _content = System.Text.Encoding.UTF8.GetString(bys);
                }

                return _content;
            }
            set
            {
                _content = value;
            }
        }

        public byte[] bys;

        private Texture _texture;
        public Texture texture
        {
            get
            {
                if (!finished)
                {
                    LogMgr.LogError("仍未完成");
                }

                if (_texture == null )
                {
                    if (url.EndsWith("png") && bys != null)
                    {
                        int wid = ConvertIntToByteArray(bys, 16, false);
                        int height = ConvertIntToByteArray(bys, 20, false);

                        Texture2D tex = new Texture2D(wid, height);
                        tex.LoadImage(bys);
                        _texture = tex;
                        LogMgr.LogFormat("png 贴图加载完成 :{0}", tex);
                    }
                    else if (url.EndsWith("jpg") && bys != null)
                    {
                        int wid = ConvertIntToByteArray(bys, 16, false);
                        int height = ConvertIntToByteArray(bys, 20, false);

                        Texture2D tex = new Texture2D(wid, height);
                        tex.LoadImage(bys);
                        _texture = tex;
                        LogMgr.LogFormat("jpg 贴图加载完成 :{0}",tex);
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("{0} 贴图解析失败 字节信息:{1}",this.url, bys == null? 0: bys.Length);
                    }

                }

                return _texture;
            }
            set
            {
                _texture = value;
            }
        }

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

        public bool IgnoreResp { get; set; }

        public bool finished
        {
            get
            {
                return this.reiceved >= this.total && this.total >0 ;
            }
        }

        static int ConvertIntToByteArray( byte[] numData, int startidx,bool islittleendian)
        {
            if (islittleendian)
            {
                int value = 0;
                for (int i = 0; i < 4; i++)
                {
                    int temp = (numData[startidx + i] & 0xff) << (i * 8);
                    value |= temp;
                }
                return value;
            }
            else
            {

                int value = 0;
                for (int i =3; i >= 0; i--)
                {
                    int temp = (numData[startidx + i] & 0xff) << ((3-i) * 8);
                    value |= temp;
                }
                return value;
            }
        }
    }
}

