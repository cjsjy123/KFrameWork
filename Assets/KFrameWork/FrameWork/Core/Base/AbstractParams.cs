using System;
using UnityEngine;

namespace KFrameWork
{
    public abstract class AbstractParams:iPush,iPop,iSet,iInsert
    {
        protected int _LimitMax =-1;

        protected int _OrigionArgCount =-1;
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
                    if(_OrigionArgCount >0)
                    {
                        return this._ArgCount <_OrigionArgCount && this._ArgCount < _LimitMax;
                    }

                    return  this._ArgCount < _LimitMax;
                }

                return this._ArgCount <_OrigionArgCount;
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

        protected void UsetSimple()
        {
            throw new ArgumentException("参数较少，请使用Simpleparams");
        }

        protected void UseGener()
        {
            throw new ArgumentException("参数过多，请使用genericparams");
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
                    this.UseGener();
                    return;
                }

                if(this._OrigionArgCount >0)
                {
                    this._OrigionArgCount += cnt;
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

        public abstract int GetArgIndexType(int index);
        public abstract void ResetReadIndex();
        public abstract void Release();

        #region interface funcs
        public abstract void SetInt(int argindex,int v);
        public abstract void SetShort(int argindex,short v);
        public abstract void SetString(int argindex,string v);
        public abstract void SetBool(int argindex,bool v);
        public abstract void SetLong(int argindex,long v);
        public abstract void SetFloat(int argindex,float v);
        public abstract void SetDouble(int argindex,double v);
        public abstract void SetVector3(int argindex, Vector3 v);
        public abstract void SetObject(int argindex,System.Object v);
        public abstract void SetUnityObject(int argindex,UnityEngine.Object v);

        public abstract void WriteInt(int v);
        public abstract void WriteShort(short v);
        public abstract void WriteString(string v);
        public abstract void WriteBool(bool v);
        public abstract void WriteFloat(float v);
        public abstract void WriteDouble(double v);
        public abstract void WriteLong(long v);
        public abstract void WriteVector3(Vector3 v);
        public abstract void WriteObject(System.Object v);
        public abstract void WriteUnityObject(UnityEngine.Object v);


        public abstract void InsertInt(int index,int v);
        public abstract void InsertShort(int index,short v);
        public abstract void InsertString(int index,string v);
        public abstract void InsertBool(int index,bool v);
        public abstract void InsertFloat(int index,float v);
        public abstract void InsertDouble(int index,double v);
        public abstract void InsertLong(int index,long v);
        public abstract void InsertVector3(int index, Vector3 v);
        public abstract void InsertObject(int index,System.Object v);
        public abstract void InsertUnityObject(int index,UnityEngine.Object v);


        public abstract int ReadInt();
        public abstract bool ReadBool();
        public abstract short ReadShort();
        public abstract string ReadString();
        public abstract long ReadLong();
        public abstract float ReadFloat();
        public abstract double ReadDouble();
        public abstract Vector3 ReadVector3();
        public abstract System.Object ReadObject();
        public abstract UnityEngine.Object ReadUnityObject();
        #endregion

    }
}

