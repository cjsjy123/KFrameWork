using System;
using UnityEngine;
using KUtils;

namespace KFrameWork
{
    public abstract class AbstractParams:IPool
    {
        protected int _LimitMax =-1;

        protected int _OriginArgCount =-1;
        /// <summary>
        /// 无视参数限制的arg计数
        /// </summary>
        protected int _virtualArg =0;
        /// <summary>
        /// 最大不得超过最大数的参数计数
        /// </summary>
        protected int _ArgCount =0;
        protected int _NextReadIndex= 0;

        public bool CanPush
        {
            get
            {
                if(_LimitMax  > 0)
                {
                    if(_OriginArgCount >0)
                    {
                        return this._ArgCount <_OriginArgCount && this._ArgCount < _LimitMax;
                    }

                    return  this._ArgCount < _LimitMax;
                }

                return this._ArgCount <_OriginArgCount;
            }
        }

        public int ArgCount
        {
            get
            {
                return this._ArgCount;
            }
        }

        protected long _MyPow(long v,int nums)
        {
            long ret =1;
            while(nums >0)
            {
                ret *= v;
                nums--;
            }
            return ret;
        }

        protected void throwUsetSimple()
        {
            throw new FrameWorkException("参数较少，请使用Simpleparams");
        }

        protected void throwUseGener()
        {
            throw new FrameWorkException("参数过多，请使用genericparams");
        }

        public void Push(AbstractParams args)
        {
            if(args != null)
            {
                if(args == this)
                {
                    LogMgr.Log("参数对象相同 无法添加");
                    return;
                }

                int cnt = args.ArgCount;
                if(this._LimitMax != -1 && cnt + ArgCount > this._LimitMax)
                {
                    this.throwUseGener();
                    return;
                }

                if(this._OriginArgCount >0)
                {
                    this._OriginArgCount += cnt;
                }

                for(int i =0; i < cnt ;++i)
                {
                    int tp = args.GetArgIndexType(i);
                    if(tp == (int)ParamType.INT)
                    {
                        this.WriteInt(args.ReadInt());
                    }
                    else if(tp ==(int)ParamType.SHORT)
                    {
                        this.WriteShort(args.ReadShort());
                    }
                    else if(tp ==(int)ParamType.BOOL)
                    {
                        this.WriteBool(args.ReadBool());
                    }
                    else if(tp ==(int)ParamType.FLOAT)
                    {
                        this.WriteFloat(args.ReadFloat());
                    }
                    else if(tp ==(int)ParamType.DOUBLE)
                    {
                        this.WriteDouble(args.ReadDouble());
                    }
                    else if(tp ==(int)ParamType.LONG)
                    {
                        this.WriteLong(args.ReadLong());
                    }
                    else if(tp ==(int)ParamType.STRING)
                    {
                        this.WriteString(args.ReadString());
                    }
                    else if (tp == (int)ParamType.VETOR3)
                    {
                        this.WriteVector3(args.ReadVector3());
                    }
                    else if (tp == (int)ParamType.OBJECT)
                    {
                        this.WriteObject(args.ReadObject());
                    }
                    else if(tp ==(int)ParamType.UNITYOBJECT)
                    {
                        this.WriteUnityObject(args.ReadUnityObject());
                    }
                }
            }
        }

        public abstract int NextValue();
        public abstract int GetArgIndexType(int index);
        public abstract void ResetReadIndex();
        public abstract void Release();

        #region interface funcs
        public abstract AbstractParams SetInt(int argindex,int v);
        public abstract AbstractParams SetShort(int argindex,short v);
        public abstract AbstractParams SetString(int argindex,string v);
        public abstract AbstractParams SetBool(int argindex,bool v);
        public abstract AbstractParams SetLong(int argindex,long v);
        public abstract AbstractParams SetFloat(int argindex,float v);
        public abstract AbstractParams SetDouble(int argindex,double v);
        public abstract AbstractParams SetVector3(int argindex, Vector3 v);
        public abstract AbstractParams SetColor(int argindex, Color v);
        public abstract AbstractParams SetObject(int argindex,System.Object v);
        public abstract AbstractParams SetUnityObject(int argindex,UnityEngine.Object v);

        public abstract AbstractParams WriteInt(int v);
        public abstract AbstractParams WriteShort(short v);
        public abstract AbstractParams WriteString(string v);
        public abstract AbstractParams WriteBool(bool v);
        public abstract AbstractParams WriteFloat(float v);
        public abstract AbstractParams WriteDouble(double v);
        public abstract AbstractParams WriteLong(long v);
        public abstract AbstractParams WriteVector3(Vector3 v);
        public abstract AbstractParams WriteColor(Color v);
        public abstract AbstractParams WriteObject(System.Object v);
        public abstract AbstractParams WriteUnityObject(UnityEngine.Object v);


        public abstract AbstractParams InsertInt(int index,int v);
        public abstract AbstractParams InsertShort(int index,short v);
        public abstract AbstractParams InsertString(int index,string v);
        public abstract AbstractParams InsertBool(int index,bool v);
        public abstract AbstractParams InsertFloat(int index,float v);
        public abstract AbstractParams InsertDouble(int index,double v);
        public abstract AbstractParams InsertLong(int index,long v);
        public abstract AbstractParams InsertVector3(int index, Vector3 v);
        public abstract AbstractParams InsertColor(int index, Color v);
        public abstract AbstractParams InsertObject(int index,System.Object v);
        public abstract AbstractParams InsertUnityObject(int index,UnityEngine.Object v);


        public abstract int ReadInt();
        public abstract bool ReadBool();
        public abstract short ReadShort();
        public abstract string ReadString();
        public abstract long ReadLong();
        public abstract float ReadFloat();
        public abstract double ReadDouble();
        public abstract Vector3 ReadVector3();
        public abstract Color ReadColor();
        public abstract System.Object ReadObject();
        public abstract UnityEngine.Object ReadUnityObject();

        public abstract void RemoveToPool();
        #endregion

    }
}

