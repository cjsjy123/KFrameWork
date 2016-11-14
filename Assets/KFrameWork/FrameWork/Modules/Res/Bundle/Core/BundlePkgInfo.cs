using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public class BundlePkgInfo
    {
        /// <summary>
        /// ab 文件名
        /// </summary>
        public readonly string AbFileName;
        /// <summary>
        /// ab真实名字
        /// </summary>
        public readonly string BundleName;

        public readonly string EditorPath;

        public readonly string Hash;

        public readonly Type ResTp;

        public readonly string[] Depends;

        public BundlePkgInfo(string hash, string abname, string bundlename, string editpath, Type tp, string[] dep)
        {
            this.Hash = hash;
            this.AbFileName = abname;
            this.BundleName = bundlename;
            this.EditorPath = editpath;
            this.ResTp = tp;
            this.Depends = dep;
        }
    }
}

