using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using KUtils;

namespace KFrameWork
{
    public class BundleBinaryInfo : BundleInfoFilter
    {
        private Dictionary<string, BundlePkgInfo> caches = new Dictionary<string, BundlePkgInfo>();

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
                int typeData = sr.ReadInt32();
                int depsCount = sr.ReadInt32();
                string[] deps = new string[depsCount];

                for (int i = 0; i < depsCount; i++)
                {
                    deps[i] = names[sr.ReadInt32()];
                }

                BundlePkgInfo pkg = new BundlePkgInfo(hash, name, shortFileName, assetpath, null, deps);

                if (!this.caches.ContainsKey(shortFileName))
                {
                    this.caches.Add(shortFileName, pkg);
                    this.caches.Add(name, pkg);
                }
                else
                {
                    LogMgr.LogErrorFormat("short name dupliate {0}", shortFileName);
                }
            }


            sr.Close();
        }


        public BundlePkgInfo SeekInfo(string name)
        {
            if (BundleConfig.SAFE_MODE)
                name = name.ToLower();

            if (this.caches.ContainsKey(name))
            {
                return this.caches[name];
            }
            else if (FrameWorkDebug.Open_DEBUG)
            {
                LogMgr.LogErrorFormat("Not Found {0} ",name);
            }

            return null;
        }

    }
}


