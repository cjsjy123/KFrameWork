using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    public enum UIFlag
    {
        NormalUI    =1,
        WolrdUI     =2,
        Discover    =4,
        RemoveNow   =8,
        VisiableNow =16,

        BindParnt   =32,
        //待修改完UI系统后再使用
        ReUSE       =64,
    }

    public struct UIContent :IEquatable<UIContent>,IEqualityComparer<UIContent>
    {
        public string PrefabPath;

        public int UItype;

        public int Depth;

        public UIFlag Flag;

        public GameObject Parent;

        public GameObject Binder;

        public AbstractParams PackedParams;

        public bool isDefault()
        {
            return string.IsNullOrEmpty(PrefabPath) && UItype == 0 && Flag == UIFlag.Discover && Parent == null && Depth == 0 && PackedParams == null;
        }

        public UIContent(string name, int tp, UIFlag flag, GameObject p,int depth)
        {
            this.PrefabPath = name;
            this.Flag = flag;
            this.UItype = tp;
            this.Parent = p;
            this.Depth = depth;
            this.PackedParams = null;
            this.Binder = null;
        }

        public UIContent(string name, int tp) : this(name, tp, UIFlag.NormalUI, null, 0)
        {

        }

        public UIContent(string name, int tp, UIFlag flag) : this(name, tp, flag, null, 0)
        {

        }

        public UIContent(string name, int tp, int depth) : this(name, tp, UIFlag.NormalUI, null, depth)
        {

        }

        public UIContent(string name, int tp, GameObject p) : this(name, tp, UIFlag.NormalUI, p, 0)
        {

        }

        public UIContent(string name, int tp, GameObject p, int depth) : this(name, tp, UIFlag.NormalUI, p, depth)
        {

        }

        public void ResetExceptConfig()
        {
            //if(!this.isFlag(UIFlag.ReUSE))
            this.Parent = null;

            this.PackedParams = null;
        }

        public bool isFlag( UIFlag other)
        {
            int l = (int)this.Flag;
            int r = (int)other;
            return (l & r) == r;
        }

        public static bool isFlag(UIFlag self, UIFlag other)
        {
            int l = (int)self;
            int r = (int)other;
            return (l & r) == r;
        }

        public bool SimpleEquals(UIContent self, UIContent other)
        {
            if (self.PrefabPath != other.PrefabPath) return false;
            if (self.Flag != other.Flag) return false;
            if (self.UItype != other.UItype) return false;
            if (self.Parent != other.Parent) return false;
            //if (this.ScriptType != other.ScriptType) return false;
            if (self.Depth != other.Depth) return false;
            //if (this.PackedParams != other.PackedParams) return false;
            //if (!isFlag(UIFlag.ReUSE))// 忽视实例差异
            //{
                if (self.Binder != other.Binder) return false;
            //}
            return true;
        }

        public bool Equals(UIContent x, UIContent y)
        {
            return SimpleEquals(x,y);
        }

        public bool Equals(UIContent other)
        {
            return SimpleEquals(this, other);
        }

        public int GetHashCode(UIContent obj)
        {
            return obj.GetHashCode();
        }
    }

    //public class UIContent:PoolCls<UIContent>, IEquatable<UIContent>,IPool,ICloneable  {

    //    public string PrefabPath;

    //    public int UItype;

    //    public UIFlag Flag;

    //    public GameObject Parent;

    //    public Type ScriptType;

    //    public int Depth ;

    //    public bool isDefault()
    //    {
    //        return string.IsNullOrEmpty(PrefabPath)
    //            && UItype == 0 && Flag == UIFlag.Discover && Parent == null
    //            && ScriptType == null && Depth == 0;
    //    }

    //    public UIContent(string name,int tp, UIFlag flag,GameObject p,Type scriptTp,int depth)
    //    {
    //        this.PrefabPath = name;
    //        this.Flag = flag;
    //        this.UItype = tp;
    //        this.Parent = p;
    //        this.Depth = depth;
    //        this.ScriptType = scriptTp;
    //    }

    //    public UIContent(string name, int tp) :this(name, tp, UIFlag.Discover,null, null,0)
    //    {

    //    }

    //    public UIContent(string name, int tp, int depth) : this(name, tp, UIFlag.Discover, null, null, depth)
    //    {

    //    }

    //    public UIContent(string name, int tp, Type scriptTp) : this(name, tp, UIFlag.Discover, null, scriptTp, 0)
    //    {

    //    }

    //    public UIContent(string name, int tp, GameObject p) : this(name, tp, UIFlag.Discover, p,null, 0)
    //    {

    //    }

    //    public UIContent(string name, int tp, GameObject p, Type scriptTp) : this(name, tp, UIFlag.Discover, p, scriptTp, 0)
    //    {

    //    }

    //    public UIContent(string name, int tp, GameObject p, int depth) : this(name, tp, UIFlag.Discover, p, null, depth)
    //    {

    //    }

    //    public UIContent(string name, int tp, GameObject p, Type scriptTp, int depth) : this(name, tp, UIFlag.Discover, p, scriptTp, depth)
    //    {

    //    }

    //    private UIContent()
    //    {

    //    }

    //    public static UIContent Create()
    //    {
    //        UIContent c = TrySpawn<UIContent>();
    //        if (c == null)
    //            c = new UIContent();

    //        return c;
    //    }

    //    public static bool isFlag(UIFlag self,UIFlag other)
    //    {
    //        int l =(int)self;
    //        int r = (int)other;
    //        return (l & r) == r;
    //    }

    //    public bool Equals(UIContent other)
    //    {
    //        if (other == null)
    //            return false;

    //        if (this.PrefabPath != other.PrefabPath) return false;
    //        if (this.Flag != other.Flag) return false;
    //        if (this.UItype != other.UItype) return false;
    //        if (this.Parent != other.Parent) return false;
    //        return true;
    //    }

    //    public void RemoveToPool()
    //    {
    //        this.UItype = 0;
    //        this.Parent = null;
    //        this.ScriptType = null;
    //        this.Depth = 0;
    //        this.Flag = UIFlag.Discover;
    //    }

    //    public void RemovedFromPool()
    //    {
    //        this.Parent = null;
    //        this.ScriptType = null;
    //    }

    //    public object Clone()
    //    {
    //        return MemberwiseClone();
    //    }
    //}
}


