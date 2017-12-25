using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using KUtils;
#if USE_TANGAB

namespace KFrameWork
{
    public class BundleTextInfo : BundleInfoFilter
    {
        /// <summary>
        /// 放弃一点查询的开销，减少大小写转换的gc开销(统一大小写之后好比较)
        /// </summary>
        private SimpleDictionary<string, BundlePkgInfo> caches = new SimpleDictionary<string, BundlePkgInfo>(true);

        public void LoadFromMemory(Stream depStream)
        {
            caches.Clear();
            StreamReader sr = new StreamReader(depStream);
            char[] fileHeadChars = new char[6];
            sr.Read(fileHeadChars, 0, fileHeadChars.Length);
            //读取文件头判断文件类型，ABDT 意思即 Asset-Bundle-Data-Text
            if (fileHeadChars[0] != 'A' || fileHeadChars[1] != 'B' || fileHeadChars[2] != 'D' || fileHeadChars[3] != 'T')
                return;

            while (true)
            {
                string name = sr.ReadLine();
                if (string.IsNullOrEmpty(name))
                {
                    break;
                }

                string shortFileName = sr.ReadLine();
                string hash = sr.ReadLine();
                string assetpath = sr.ReadLine();
                Convert.ToInt32(sr.ReadLine());
                int depsCount = Convert.ToInt32(sr.ReadLine());
                string[] deps = new string[depsCount];

                if (FrameWorkConfig.Open_DEBUG)
                    LogMgr.Log("asset :" + assetpath);

                for (int i = 0; i < depsCount; i++)
                {
                    deps[i] = sr.ReadLine();
                }

                BundlePkgInfo pkg = new BundlePkgInfo(hash, name, shortFileName, assetpath, BundlePkgInfo.ChooseType(assetpath), deps);
                this.caches[shortFileName] = pkg;
                this.caches[name] = pkg;
                this.caches[assetpath] = pkg;

                if (BundleConfig.SAFE_MODE)
                {
#if UNITY_EDITOR
                    this.caches.Add(assetpath.Replace("\\", "/"), pkg);
#else
                        this.caches.Add(assetpath, pkg);
#endif
                }
            }

            if(FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("Bundle Count Is {0}",this.caches.Count);

            depStream.Close();
        }

        public BundlePkgInfo SeekInfo(string name)
        {
            if (this.caches.ContainsKey(name))
            {
                return this.caches[name];
            }
            else if (FrameWorkConfig.Open_DEBUG)
            {
                LogMgr.LogErrorFormat("Not Found {0} In Cache cnt:{1}",name, caches.Count);
            }

            return null;
        }

    }
}


#endif