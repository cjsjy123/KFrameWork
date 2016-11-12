using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using KUtils;

namespace KFrameWork
{

    public struct BundlePkgInfo
    {
        public string realpath;

        public string bundlename;

        public string editorPath;

        public Type ResTp;

        public string[] Depends;
    }


    public class BundleInfo : BundleInfoFilter
    {
        private Dictionary<string, BundlePkg> caches = new Dictionary<string, BundlePkg>();

        public BundleInfo()
        {

        }

        private class BundlePkg
        {
            public string Filename;
            public string hash;
            public string ShortName;
            public string EditorPath;
            public int ResTp;
            public string[] depends;
        }

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

                BundlePkg pkg = new BundlePkg();

                if (!this.caches.ContainsKey(shortFileName))
                {
                    this.caches.Add(shortFileName, pkg);
                }
                else
                {
                    LogMgr.LogErrorFormat("short name dupliate {0}",shortFileName);
                }
                    
                for (int i = 0; i < depsCount; i++)
                {
                    deps[i] = names[sr.ReadInt32()];
                }

                pkg.hash = hash;
                pkg.Filename = name;
                pkg.EditorPath = assetpath;
                pkg.ShortName = shortFileName;
                pkg.depends = deps;
                pkg.ResTp = typeData;
            }
            sr.Close();
        }

        public BundlePkgInfo SeekInfo(string name)
        {
            BundlePkgInfo info = new BundlePkgInfo();
            string lowername = name.ToLower();
            if (this.caches.ContainsKey(lowername))
            {
                BundlePkg pkg = this.caches[lowername];

                info.bundlename = lowername;
                info.Depends = pkg.depends;
                info.realpath = pkg.Filename;
                info.editorPath = pkg.EditorPath;

            }
            else if (FrameWorkDebug.Open_DEBUG)
            {
                LogMgr.LogErrorFormat("Not Found {0} ",name);
            }


            return info;
           
        }

    }
}


