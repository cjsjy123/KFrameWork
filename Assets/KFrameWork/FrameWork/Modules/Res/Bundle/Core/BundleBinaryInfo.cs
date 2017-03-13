using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using KUtils;
#if USE_TANGAB
using Tangzx.ABSystem;

namespace KFrameWork
{
    public class BundleBinaryInfo : BundleInfoFilter
    {
        /// <summary>
        /// 放弃一点查询的开销，减少大小写转换的gc开销(统一大小写之后好比较)
        /// </summary>
        private SimpleDictionary<string, BundlePkgInfo> caches = new SimpleDictionary<string, BundlePkgInfo>(true);

        public void LoadFromMemory(Stream depStream)
        {
            if (depStream.Length > 4)
            {
                BinaryReader br = new BinaryReader(depStream);
                if (br.ReadChar() == 'A' && br.ReadChar() == 'B' && br.ReadChar() == 'D')
                {
                    char c = br.ReadChar();
                    if (c != 'B')
                    {
                        throw new FrameWorkException(string.Format("核心资源异常"), ExceptionType.HighDanger_Exception);
                    }
                    this.ReadBinary(br, depStream);
                }
            }

            depStream.Close();
        }

        private void ReadBinary(BinaryReader sr,Stream fs)
        {
            int namesCount = sr.ReadInt32();
            string[] names = new string[namesCount];
            for (int i = 0; i < namesCount; i++)
            {
                names[i] = sr.ReadString();
            }

            while (true)
            {
                if (fs.Position == fs.Length)
                    break;

                string name = names[sr.ReadInt32()];
                string shortFileName = sr.ReadString();
                string hash = sr.ReadString();
                string assetpath = sr.ReadString();
                sr.ReadInt32();//int typeData =
                int depsCount = sr.ReadInt32();
                string[] deps = new string[depsCount];
                //LogMgr.LogError(shortFileName +" > "+assetpath);

                for (int i = 0; i < depsCount; i++)
                {
                    deps[i] = names[sr.ReadInt32()];
                }
                BundlePkgInfo pkg = new BundlePkgInfo(hash, name, shortFileName, assetpath, null, deps);

                this.caches[shortFileName] = pkg;
                this.caches[name] = pkg;
                this.caches[assetpath] = pkg;

                if (BundleConfig.SAFE_MODE)
                {
#if UNITY_EDITOR
                    this.caches[assetpath.Replace("\\", "/")] = pkg;
#else
                    this.caches.Add(assetpath, pkg);
#endif
                }
            }

            if (FrameWorkConfig.Open_DEBUG)
                LogMgr.LogFormat("Bundle Count Is {0}", this.caches.Count);

            sr.Close();
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

