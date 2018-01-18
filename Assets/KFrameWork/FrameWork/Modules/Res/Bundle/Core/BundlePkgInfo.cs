using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.IO;

namespace KFrameWork
{
    public enum PkgResType
    {
        None,
        Font,
        Image,
        Sound,
        Scene,
        Animation,
        CustomAsset,
        Bytes,
        Material,
        GameObject,
        RenderTexture,
        TXT,
        PSD,
        SHADER,
        FBX,
        EXR,
    }

    public class BundlePkgInfo
    {
        /// <summary>
        /// ab 文件名71d8b47e86d77d85f7c36317393fcb81cb793104.ab
        /// </summary>
        public readonly string AbFileName;
        /// <summary>
        /// ab真实名字 txxx.asset
        /// </summary>
        public readonly string BundleName;

        public readonly string EditorPath;

        public readonly string Hash;

        public readonly PkgResType ResTp;

        public readonly string[] Depends;

        public BundlePkgInfo(string hash, string abname, string bundlename, string editpath, PkgResType tp, string[] dep)
        {
            this.Hash = hash;
            this.AbFileName = abname;
            this.BundleName = bundlename;
            this.EditorPath = editpath;
            this.ResTp = tp;
            this.Depends = dep;
        }

        public static PkgResType ChooseType(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            int index = filename.LastIndexOf('.');
            string extensionName = filename.Substring(index+1);
            if (extensionName.Equals("mp3"))
            {
                return PkgResType.Sound;
            }
            else if (extensionName.Equals("prefab"))
            {
                return PkgResType.GameObject;
            }
            else if (extensionName.Equals("unity"))
            {
                return PkgResType.Scene;
            }
            else if (extensionName.Equals("anim"))
            {
                return PkgResType.Animation;
            }
            else if (extensionName.Equals("mat"))
            {
                return PkgResType.Material;
            }
            else if (extensionName.Equals("png") || extensionName.Equals("tga") || extensionName.Equals("jpg"))
            {
                return PkgResType.Image;
            }
            else if (extensionName.Equals("asset"))
            {
                return PkgResType.CustomAsset;
            }
            else if (extensionName.Equals("ttf") || extensionName.Equals("fnt"))
            {
                return PkgResType.Font;
            }
            else if (extensionName.Equals("bytes"))
            {
                return PkgResType.Bytes;
            }
            else if (extensionName.Equals("renderTexture"))
            {
                return PkgResType.RenderTexture;
            }
            else if (extensionName.Equals("txt"))
            {
                return PkgResType.TXT;
            }
            else if (extensionName.Equals("psd"))
            {
                return PkgResType.PSD;
            }
            else if (extensionName.Equals("shader"))
            {
                return PkgResType.SHADER;
            }
            else if (extensionName.Equals("cginc"))
            {
                return PkgResType.SHADER;
            }
            else if (extensionName.Equals("FBX"))
            {
                return PkgResType.FBX;
            }
            else if (extensionName.Equals("exr"))
            {
                return PkgResType.EXR;
            }
            LogMgr.LogErrorFormat("Missing Type from {0}", filepath);
            return PkgResType.None;
        }
    }
}

