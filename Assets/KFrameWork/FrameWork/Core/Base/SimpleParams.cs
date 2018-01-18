using System;
using UnityEngine;
using System.Runtime.InteropServices;
using KUtils;
using System.Text;
using System.Reflection;

namespace KFrameWork
{
//    [StructLayout(LayoutKind.Sequential)]
    public class SimpleParams:AbstractParams,IPool
    {
        /// <summary>
        /// 
        /// </summary>
        private long bitIndex =0;

        /// <summary>
        /// 4位一个类型，64/4=16
        /// </summary>
        private long bitDataSort =0;
     
        private long? m_l1;
        private long? m_l2;
        private long? m_l3;

        private int? m_i1;
        private int? m_i2;
        private int? m_i3;

        private short? m_st1;
        private short? m_st2;
        private short? m_st3;

        private bool? m_b1;
        private bool? m_b2;
        private bool? m_b3;

        private string m_string1;
        private string m_string2;
        private string m_string3;

        private float? m_f1;
        private float? m_f2;
        private float? m_f3;

        private double? m_d1;
        private double? m_d2;
        private double? m_d3;

        private Vector3? m_v1;
        private Vector3? m_v2;
        private Vector3? m_v3;

        private Color? m_c1;
        private Color? m_c2;
        private Color? m_c3;

        private System.Object m_o1;
        private System.Object m_o2;
        private System.Object m_o3;

        private UnityEngine.Object m_uo1;
        private UnityEngine.Object m_uo2;
        private UnityEngine.Object m_uo3;

        //private bool released = false;

        [FrameWorkStart]
        private static void Preload(int v)
        {
            for(int i=0; i < FrameWorkConfig.Preload_ParamsCount;++i)
            {
                KObjectPool.mIns.Push(PreCreate(1));
                KObjectPool.mIns.Push(PreCreate(2));
                KObjectPool.mIns.Push(PreCreate(3));
            }
        }

        private static SimpleParams PreCreate(int origion =-1)
        {
            SimpleParams p = new SimpleParams();
            p._OriginArgCount = origion;
            p._virtualArg = 0;
            p._LimitMax = 16;//64/4
            return p;
        }

        public static SimpleParams Create(int origion =-1)
        {
            SimpleParams p = null;
            if(KObjectPool.mIns != null)
            {
                p=  KObjectPool.mIns.Pop<SimpleParams>();
            }

            if(p == null)
            {
                p = new SimpleParams();
            }

            if(origion != -1)
            {
                p._OriginArgCount = origion;
                p._virtualArg = 0;
            }

            p._LimitMax = 16;//64/4

            return p;
        }

        private SimpleParams()
        {
            
        }

        public override void Release ()
        {
            if (KObjectPool.mIns != null )
            {
                KObjectPool.mIns.Push(this);
            }
                
        }

        public override void RemoveToPool ()
        {
            this.m_l1 = null;
            this.m_l2 = null;
            this.m_l3 = null;

            this.m_i1 = null;
            this.m_i2 = null;
            this.m_i2 = null;

            this.m_st1= null;
            this.m_st2= null;
            this.m_st3 = null;

            this.m_string1= null;
            this.m_string2 = null;
            this.m_st3 = null;

            this.m_b1 = null;
            this.m_b2 = null;
            this.m_b3 = null;

            this.m_f1 = null;
            this.m_f2 = null;
            this.m_f3 = null;

            this.m_c1 = null;
            this.m_c2 = null;
            this.m_c3 = null;

            this.m_d1 = null;
            this.m_d2 = null;
            this.m_d3 = null;

            this.m_v1 = null;
            this.m_v2 = null;
            this.m_v3 = null;

            this.m_o1 = null;
            this.m_o2 = null;
            this.m_o3 = null;

            this.m_uo1 = null;
            this.m_uo2 = null;
            this.m_uo3 = null;

            this._ArgCount = 0;
            this._virtualArg = 0;
            this._NextReadIndex = 0;
            this._OriginArgCount =-1;
            this.bitIndex = 0;
            this.bitDataSort =0;
        }
   
        private long _ReadIndex(int ReadIndex, out int index)
        {
            long value = this.bitDataSort & (15<<(4*ReadIndex));
            long tp = value >> (4* ReadIndex);

            long lindex = this.bitIndex &(3 <<(2 * ReadIndex));
            index =(int)( lindex >> (2 * ReadIndex));

            return tp;
        }

   
        private long _ReadNextTp(out int index)
        {
            if (_NextReadIndex >= ArgCount)
            {
                _NextReadIndex = ArgCount - 1;
                index = -1;
                return 0L;
            }

            long tp = this._ReadIndex(this._NextReadIndex,out index);

            if(ArgCount != 0)
            {
                _NextReadIndex++;
            }

            return tp;
        }

        private void _SetNextTp(long tp,long index)
        {
            if( index >4 || index <0 )
            {
                ThrowError("参数索引错误");
                return;
            }

            if(this.ArgCount+1 > this._LimitMax)
            {
                ThrowError("参数过多");
                return ;
            }

            int old = this._ArgCount;
            this._ArgCount++;

            if(this._OriginArgCount >-1)
            {
                if(this._OriginArgCount < this.ArgCount)
                {
                    this._ArgCount = this._OriginArgCount;
                }
                else
                {
                    this.bitDataSort |=tp <<(4*old); 
                    this.bitIndex |= index <<(2*old);
                }
            }
            else
            {
                this.bitDataSort |=tp <<(4*old); 
                this.bitIndex |= index <<(2*old);
            }
        }

        private long _insertBit(long orig,int databit, int index,int tpval)
        {
            if (index == 0)
            {
                return orig * _MyPow(2, databit) + tpval;
            }

            long value = _MyPow(2, databit * index)-1;
            long right = orig & value;
            long left = orig & (~value);

            return  (left+ tpval) * (value+1) + right ;
        }

        /// <summary>
        /// 数据会发生遗弃
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="v">V.</param>
        public override AbstractParams InsertInt (int index, int v)
        {
            if(this._OriginArgCount >0)
                this._OriginArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.INT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,4,index,(int)ParamType.INT); 

            if(count == 0)
            {
                this.m_i1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_i2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_i3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }
            return this;
        }

        public override AbstractParams InsertShort (int index, short v)
        {
            if(this._OriginArgCount >0)
                this._OriginArgCount++;
            
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.SHORT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,4,index,(int)ParamType.SHORT); 

            if(count == 0)
            {
                this.m_st1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_st2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_st3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }
            return this;
        }

        public override AbstractParams InsertString (int index, string v)
        {
            if(this._OriginArgCount >0)
                this._OriginArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.STRING)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,4,index,(int)ParamType.STRING); 

            if(count == 0)
            {
                this.m_string1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_string2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_string3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }

            return this;
        }

        public override AbstractParams InsertBool (int index, bool v)
        {
            if(this._OriginArgCount >0)
                this._OriginArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.BOOL)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,4,index,(int)ParamType.BOOL); 

            if(count == 0)
            {
                this.m_b1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_b2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_b3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }

            return this;
        }

        public override AbstractParams InsertFloat (int index, float v)
        {
            if(this._OriginArgCount >0)
                this._OriginArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.FLOAT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,4,index,(int)ParamType.FLOAT); 

            if(count == 0)
            {
                this.m_f1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_f2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_f3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }
            return this;
        }

        public override AbstractParams InsertDouble (int index, double v)
        {
            if(this._OriginArgCount >0)
                this._OriginArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.DOUBLE)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,4,index,(int)ParamType.DOUBLE); 

            if(count == 0)
            {
                this.m_d1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_d2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_d3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }
            return this;
        }

        public override AbstractParams InsertLong (int index, long v)
        {
            if(this._OriginArgCount >0)
                this._OriginArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.LONG)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,4,index,(int)ParamType.LONG); 

            if(count == 0)
            {
                this.m_l1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_l2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_l3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }
            return this;
        }

        public override AbstractParams InsertVector3(int index, Vector3 v)
        {
            if (this._OriginArgCount > 0)
                this._OriginArgCount++;
            short count = 0;
            for (int i = 0; i < this.ArgCount; ++i)
            {
                int tp = this.GetArgIndexType(i);
                if (tp == (int)ParamType.VETOR3)
                {
                    count++;
                }
            }

            this.bitDataSort = this._insertBit(this.bitDataSort, 4, index, (int)ParamType.VETOR3);

            if (count == 0)
            {
                this.m_v1 = v;
                this.bitIndex = this._insertBit(this.bitIndex, 2, index, 0);
                this._ArgCount++;
            }
            else if (count == 1)
            {
                this.m_v2 = v;
                this.bitIndex = this._insertBit(this.bitIndex, 2, index, 1);
                this._ArgCount++;
            }
            else if (count == 2)
            {
                this.m_v3 = v;
                this.bitIndex = this._insertBit(this.bitIndex, 2, index, 2);
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }
            return this;
        }

        public override AbstractParams InsertColor(int index, Color v)
        {
            if (this._OriginArgCount > 0)
                this._OriginArgCount++;
            short count = 0;
            for (int i = 0; i < this.ArgCount; ++i)
            {
                int tp = this.GetArgIndexType(i);
                if (tp == (int)ParamType.Color)
                {
                    count++;
                }
            }

            this.bitDataSort = this._insertBit(this.bitDataSort, 4, index, (int)ParamType.Color);

            if (count == 0)
            {
                this.m_c1 = v;
                this.bitIndex = this._insertBit(this.bitIndex, 2, index, 0);
                this._ArgCount++;
            }
            else if (count == 1)
            {
                this.m_c2 = v;
                this.bitIndex = this._insertBit(this.bitIndex, 2, index, 1);
                this._ArgCount++;
            }
            else if (count == 2)
            {
                this.m_c3 = v;
                this.bitIndex = this._insertBit(this.bitIndex, 2, index, 2);
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }
            return this;
        }

        public override AbstractParams InsertObject (int index, object v)
        {
            if(this._OriginArgCount >0)
                this._OriginArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.OBJECT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,4,index,(int)ParamType.OBJECT); 

            if(count == 0)
            {
                this.m_o1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_o2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_o3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }
            return this;
        }

        public override AbstractParams InsertUnityObject (int index, UnityEngine.Object v)
        {
            if(this._OriginArgCount >0)
                this._OriginArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.UNITYOBJECT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,4,index,(int)ParamType.UNITYOBJECT); 

            if(count == 0)
            {
                this.m_uo1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_uo2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_uo3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,2,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.throwUseGener();
            }
            return this;
        }

        public override int NextValue ()
        {
            int index;
            int old = _NextReadIndex;
            long tp = this._ReadNextTp(out index);
            _NextReadIndex = old;
            return (int)tp;
        }

        private void ThrowError(string info)
        {
           // LogMgr.LogError(info);
            throw new ArgumentException(info);
        }

        public override int ReadInt ()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if(tp == (int)ParamType.INT)
            {
                if(index == 0)
                {
                    if(this.m_i1.HasValue)
                    {
                        return this.m_i1.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index == 1)
                {
                    if(this.m_i2.HasValue)
                    {
                        return this.m_i2.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index== 2)
                {
                    if(this.m_i3.HasValue)
                    {
                        return this.m_i3.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp,old));
            }
            return 0;
        }

        public override bool ReadBool ()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if(tp == (int)ParamType.BOOL)
            {
                if(index == 0)
                {
                    if(this.m_b1.HasValue)
                    {
                        return this.m_b1.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index == 1)
                {
                    if(this.m_b2.HasValue)
                    {
                        return this.m_b2.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index== 2)
                {
                    if(this.m_b3.HasValue)
                    {
                        return this.m_b3.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return false;
        }

        public override short ReadShort ()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if(tp == (int)ParamType.SHORT)
            {
                if(index == 0)
                {
                    if(this.m_st1.HasValue)
                    {
                        return this.m_st1.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index == 1)
                {
                    if(this.m_st2.HasValue)
                    {
                        return this.m_st2.Value;
                    }
                    else
                    {
                        
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index== 2)
                {
                    if(this.m_st3.HasValue)
                    {
                        return this.m_st3.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return 0;
        }

        public override string ReadString ()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if(tp == (int)ParamType.STRING)
            {
                if(index == 0)
                {
                    if(this.m_string1 != null)
                    {
                        return this.m_string1;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index == 1)
                {
                    if(this.m_string2 != null)
                    {
                        return this.m_string2;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index== 2)
                {
                    if(this.m_string3 != null)
                    {
                        return this.m_string3;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return "";
        }

        public override long ReadLong ()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if(tp == (int)ParamType.LONG)
            {
                if(index == 0)
                {
                    if(this.m_l1.HasValue)
                    {
                        return this.m_l1.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index == 1)
                {
                    if(this.m_l2.HasValue)
                    {
                        return this.m_l2.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index== 2)
                {
                    if(this.m_l3.HasValue)
                    {
                        return this.m_l3.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return 0;
        }

        public override float ReadFloat ()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if(tp == (int)ParamType.FLOAT)
            {
                if(index == 0)
                {
                    if(this.m_f1.HasValue)
                    {
                        return this.m_f1.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index == 1)
                {
                    if(this.m_f2.HasValue)
                    {
                        return this.m_f2.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index== 2)
                {
                    if(this.m_f3.HasValue)
                    {
                        return this.m_f3.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return 0;
        }

        public override double ReadDouble ()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if(tp == (int)ParamType.DOUBLE)
            {
                if(index == 0)
                {
                    if(this.m_d1.HasValue)
                    {
                        return this.m_d1.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index == 1)
                {
                    if(this.m_d2.HasValue)
                    {
                        return this.m_d2.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if(index== 2)
                {
                    if(this.m_d3.HasValue)
                    {
                        return this.m_d3.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return 0;
        }

        public override Vector3 ReadVector3()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if (tp == (int)ParamType.VETOR3)
            {
                if (index == 0)
                {
                    if (this.m_v1.HasValue)
                    {
                        return this.m_v1.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if (index == 1)
                {
                    if (this.m_v2.HasValue)
                    {
                        return this.m_v2.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if (index == 2)
                {
                    if (this.m_v3.HasValue)
                    {
                        return this.m_v3.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return Vector3.zero;
        }

        public override Color ReadColor()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if (tp == (int)ParamType.Color)
            {
                if (index == 0)
                {
                    if (this.m_c1.HasValue)
                    {
                        return this.m_c1.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if (index == 1)
                {
                    if (this.m_c2.HasValue)
                    {
                        return this.m_c2.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else if (index == 2)
                {
                    if (this.m_c3.HasValue)
                    {
                        return this.m_c3.Value;
                    }
                    else
                    {
                        _NextReadIndex = old;
                        ThrowError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return Color.white;
        }

        public override object ReadObject ()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if(tp == (int)ParamType.OBJECT)
            {
                if(index == 0)
                {
                    return this.m_o1;
                }
                else if(index == 1)
                {
                    return this.m_o2;
                }
                else if(index== 2)
                {
                    return this.m_o3;
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return null;
        }

        public override UnityEngine.Object ReadUnityObject ()
        {
            int old = _NextReadIndex;
            int index;
            long tp = this._ReadNextTp(out index);
            if(tp == (int)ParamType.UNITYOBJECT)
            {
                if(index == 0)
                {
                    return this.m_uo1;
                }
                else if(index == 1)
                {
                    return this.m_uo2;
                }
                else if(index== 2)
                {
                    return this.m_uo3;
                }
                else
                {
                    _NextReadIndex = old;
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                ThrowError(string.Format("下一个类型为:{0} 参数异常 当前索引为 :{1}", (ParamType)tp, old));
            }
            return null;
        }

        public override AbstractParams WriteInt(int v)
        {
            if(this._OriginArgCount >0)
            {
                short count =0;
                for(int i =0; i < this._virtualArg;++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if(tp == (int)ParamType.INT)
                    {
                        count++;
                    }
                }

                if(count == 0)
                {
                    this.m_i1 = v;
                    this._SetNextTp((int)ParamType.INT,0);
                }
                else if(count == 1)
                {
                    this.m_i2 = v;
                    this._SetNextTp((int)ParamType.INT,1);
                }
                else if(count == 2)
                {
                    this.m_i3 = v;
                    this._SetNextTp((int)ParamType.INT,2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if(!this.m_i1.HasValue)
                {
                    this.m_i1 = v;
                    this._SetNextTp((int)ParamType.INT,0);
                }
                else if(!this.m_i2.HasValue)
                {
                    this.m_i2 =v;
                    this._SetNextTp((int)ParamType.INT,1);
                }
                else if(!this.m_i3.HasValue)
                {
                    this.m_i3 =v;
                    this._SetNextTp((int)ParamType.INT,2);
                }
                else
                {
                    this.throwUseGener();
                }
            }
            return this;
        }

        public override AbstractParams WriteShort (short v)
        {
            if(this._OriginArgCount >0)
            {
                short count =0;
                for(int i =0; i < this._virtualArg;++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if(tp == (int)ParamType.SHORT)
                    {
                        count++;
                    }
                }

                if(count == 0)
                {
                    this.m_st1 = v;
                    this._SetNextTp((int)ParamType.SHORT,0);
                }
                else if(count == 1)
                {
                    this.m_st2 = v;
                    this._SetNextTp((int)ParamType.SHORT,1);
                }
                else if(count == 2)
                {
                    this.m_st3 = v;
                    this._SetNextTp((int)ParamType.SHORT,2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if(!this.m_st1.HasValue)
                {
                    this.m_st1 = v;
                    this._SetNextTp((int)ParamType.SHORT,0);
                }
                else if(!this.m_st2.HasValue)
                {
                    this.m_st2 =v;
                    this._SetNextTp((int)ParamType.SHORT,1);
                }
                else if(!this.m_st3.HasValue)
                {
                    this.m_st3 =v;
                    this._SetNextTp((int)ParamType.SHORT,2);
                }
                else
                {
                    this.throwUseGener();
                }
            }
            return this;
        }

        public override AbstractParams WriteString (string v)
        {
            if(this._OriginArgCount >0)
            {
                short count =0;
                for(int i =0; i < this._virtualArg;++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if(tp == (int)ParamType.STRING)
                    {
                        count++;
                    }
                }

                if(count == 0)
                {
                    this.m_string1 = v;
                    this._SetNextTp((int)ParamType.STRING,0);
                }
                else if(count == 1)
                {
                    this.m_string2 = v;
                    this._SetNextTp((int)ParamType.STRING,1);
                }
                else if(count == 2)
                {
                    this.m_string3 = v;
                    this._SetNextTp((int)ParamType.STRING,2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if(this.m_string1 == null)
                {
                    this.m_string1 = v;
                    this._SetNextTp((int)ParamType.STRING,0);
                }
                else if(this.m_string2 == null)
                {
                    this.m_string2 =v;
                    this._SetNextTp((int)ParamType.STRING,1);
                }
                else if(this.m_string3 == null)
                {
                    this.m_string3 =v;
                    this._SetNextTp((int)ParamType.STRING,2);
                }
                else
                {
                    this.throwUseGener();
                }
            }
            return this;
        }

        public override AbstractParams WriteFloat (float v)
        {
            if(this._OriginArgCount >0)
            {
                short count =0;
                for(int i =0; i < this._virtualArg;++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if(tp == (int)ParamType.FLOAT)
                    {
                        count++;
                    }
                }

                if(count == 0)
                {
                    this.m_f1 = v;
                    this._SetNextTp((int)ParamType.FLOAT,0);
                }
                else if(count == 1)
                {
                    this.m_f2 = v;
                    this._SetNextTp((int)ParamType.FLOAT,1);
                }
                else if(count == 2)
                {
                    this.m_f3 = v;
                    this._SetNextTp((int)ParamType.FLOAT,2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if(!this.m_f1.HasValue)
                {
                    this.m_f1 = v;
                    this._SetNextTp((int)ParamType.FLOAT,0);
                }
                else if(!this.m_f2.HasValue)
                {
                    this.m_f2 =v;
                    this._SetNextTp((int)ParamType.FLOAT,1);
                }
                else if(!this.m_f3.HasValue)
                {
                    this.m_f3 =v;
                    this._SetNextTp((int)ParamType.FLOAT,2);
                }
                else
                {
                    this.throwUseGener();
                }
            }
            return this;
        }

        public override AbstractParams WriteDouble (double v)
        {
            if(this._OriginArgCount >0)
            {
                short count =0;
                for(int i =0; i < this._virtualArg;++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if(tp == (int)ParamType.DOUBLE)
                    {
                        count++;
                    }
                }

                if(count == 0)
                {
                    this.m_d1 = v;
                    this._SetNextTp((int)ParamType.DOUBLE,0);
                }
                else if(count == 1)
                {
                    this.m_d2 = v;
                    this._SetNextTp((int)ParamType.DOUBLE,1);
                }
                else if(count == 2)
                {
                    this.m_d3 = v;
                    this._SetNextTp((int)ParamType.DOUBLE,2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if(!this.m_d1.HasValue)
                {
                    this.m_d1 = v;
                    this._SetNextTp((int)ParamType.DOUBLE,0);
                }
                else if(!this.m_d2.HasValue)
                {
                    this.m_d2 =v;
                    this._SetNextTp((int)ParamType.DOUBLE,1);
                }
                else if(!this.m_d3.HasValue)
                {
                    this.m_d3 =v;
                    this._SetNextTp((int)ParamType.DOUBLE,2);
                } 
                else
                {
                    this.throwUseGener();
                }
            }
            return this;
        }

        public override AbstractParams WriteBool (bool v)
        {
            if(this._OriginArgCount >0)
            {
                short count =0;
                for(int i =0; i < this._virtualArg;++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if(tp == (int)ParamType.BOOL)
                    {
                        count++;
                    }
                }

                if(count == 0)
                {
                    this.m_b1 = v;
                    this._SetNextTp((int)ParamType.BOOL,0);
                }
                else if(count == 1)
                {
                    this.m_b2 = v;
                    this._SetNextTp((int)ParamType.BOOL,1);
                }
                else if(count == 2)
                {
                    this.m_b3 = v;
                    this._SetNextTp((int)ParamType.BOOL,2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if(!this.m_b1.HasValue)
                {
                    this.m_b1 = v;
                    this._SetNextTp((int)ParamType.BOOL,0);
                }
                else if(!this.m_b2.HasValue)
                {
                    this.m_b2 =v;
                    this._SetNextTp((int)ParamType.BOOL,1);
                }
                else if(!this.m_b3.HasValue)
                {
                    this.m_b3 =v;
                    this._SetNextTp((int)ParamType.BOOL,2);
                }
                else
                {
                    this.throwUseGener();
                }
            }
            return this;
        }

        public override AbstractParams WriteLong (long v)
        {
            if(this._OriginArgCount >0)
            {
                short count =0;
                for(int i =0; i < this._virtualArg;++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if(tp == (int)ParamType.LONG)
                    {
                        count++;
                    }
                }

                if(count == 0)
                {
                    this.m_l1 = v;
                    this._SetNextTp((int)ParamType.LONG,0);
                }
                else if(count == 1)
                {
                    this.m_l2 = v;
                    this._SetNextTp((int)ParamType.LONG,1);
                }
                else if(count == 2)
                {
                    this.m_l3 = v;
                    this._SetNextTp((int)ParamType.LONG,2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if(!this.m_l1.HasValue)
                {
                    this.m_l1 = v;
                    this._SetNextTp((int)ParamType.LONG,0);
                }
                else if(!this.m_l2.HasValue)
                {
                    this.m_l2 =v;
                    this._SetNextTp((int)ParamType.LONG,1);
                }
                else if(!this.m_l3.HasValue)
                {
                    this.m_l3 =v;
                    this._SetNextTp((int)ParamType.LONG,2);
                }
                else
                {
                    this.throwUseGener();
                }
            }
            return this;
        }

        public override AbstractParams WriteVector3(Vector3 v)
        {
            if (this._OriginArgCount > 0)
            {
                short count = 0;
                for (int i = 0; i < this._virtualArg; ++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if (tp == (int)ParamType.VETOR3)
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    this.m_v1 = v;
                    this._SetNextTp((int)ParamType.VETOR3, 0);
                }
                else if (count == 1)
                {
                    this.m_v2 = v;
                    this._SetNextTp((int)ParamType.VETOR3, 1);
                }
                else if (count == 2)
                {
                    this.m_v3 = v;
                    this._SetNextTp((int)ParamType.VETOR3, 2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if (this.m_v1 == null)
                {
                    this.m_v1 = v;
                    this._SetNextTp((int)ParamType.VETOR3, 0);
                }
                else if (this.m_v2 == null)
                {
                    this.m_v2 = v;
                    this._SetNextTp((int)ParamType.VETOR3, 1);
                }
                else if (this.m_v3 == null)
                {
                    this.m_v3 = v;
                    this._SetNextTp((int)ParamType.VETOR3, 2);
                }
                else
                {
                    this.throwUseGener();
                }
            }
            return this;
        }

        public override AbstractParams WriteColor(Color v)
        {
            if (this._OriginArgCount > 0)
            {
                short count = 0;
                for (int i = 0; i < this._virtualArg; ++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if (tp == (int)ParamType.Color)
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    this.m_c1 = v;
                    this._SetNextTp((int)ParamType.Color, 0);
                }
                else if (count == 1)
                {
                    this.m_c2 = v;
                    this._SetNextTp((int)ParamType.Color, 1);
                }
                else if (count == 2)
                {
                    this.m_c3 = v;
                    this._SetNextTp((int)ParamType.Color, 2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if (this.m_c1 == null)
                {
                    this.m_c1 = v;
                    this._SetNextTp((int)ParamType.Color, 0);
                }
                else if (this.m_c2 == null)
                {
                    this.m_c2 = v;
                    this._SetNextTp((int)ParamType.Color, 1);
                }
                else if (this.m_c3 == null)
                {
                    this.m_c3 = v;
                    this._SetNextTp((int)ParamType.Color, 2);
                }
                else
                {
                    this.throwUseGener();
                }
            }
            return this;
        }

        public override AbstractParams WriteObject (object v)
        {
            if(this._OriginArgCount >0)
            {
                short count =0;
                for(int i =0; i < this._virtualArg;++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if(tp == (int)ParamType.OBJECT)
                    {
                        count++;
                    }
                }

                if(count == 0)
                {
                    this.m_o1 = v;
                    this._SetNextTp((int)ParamType.OBJECT,0);
                }
                else if(count == 1)
                {
                    this.m_o2 = v;
                    this._SetNextTp((int)ParamType.OBJECT,1);
                }
                else if(count == 2)
                {
                    this.m_o3 = v;
                    this._SetNextTp((int)ParamType.OBJECT,2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if(this.m_o1 == null)
                {
                    this.m_o1 = v;
                    this._SetNextTp((int)ParamType.OBJECT,0);
                }
                else if(this.m_o2 == null)
                {
                    this.m_o2 =v;
                    this._SetNextTp((int)ParamType.OBJECT,1);
                }
                else if(this.m_o3 == null)
                {
                    this.m_o3 =v;
                    this._SetNextTp((int)ParamType.OBJECT,2);
                }
                else
                {
                    this.throwUseGener();
                }
            }

            return this;
        }

        public override AbstractParams WriteUnityObject (UnityEngine.Object v)
        {
            if(this._OriginArgCount >0)
            {
                short count =0;
                for(int i =0; i < this._virtualArg;++i)
                {
                    int tp = this.GetArgIndexType(i);
                    if(tp == (int)ParamType.UNITYOBJECT)
                    {
                        count++;
                    }
                }

                if(count == 0)
                {
                    this.m_uo1 = v;
                    this._SetNextTp((int)ParamType.UNITYOBJECT,0);
                }
                else if(count == 1)
                {
                    this.m_uo2 = v;
                    this._SetNextTp((int)ParamType.UNITYOBJECT,1);
                }
                else if(count == 2)
                {
                    this.m_uo3 = v;
                    this._SetNextTp((int)ParamType.UNITYOBJECT,2);
                }
                else
                {
                    this.throwUseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OriginArgCount;
            }
            else
            {
                if(this.m_uo1 == null)
                {
                    this.m_uo1 = v;
                    this._SetNextTp((int)ParamType.UNITYOBJECT,0);
                }
                else if(this.m_uo2 == null)
                {
                    this.m_uo2 =v;
                    this._SetNextTp((int)ParamType.UNITYOBJECT,1);
                }
                else if(this.m_uo3 == null)
                {
                    this.m_uo3 =v;
                    this._SetNextTp((int)ParamType.UNITYOBJECT,2);
                }
                else
                {
                    this.throwUseGener();
                }
            }
            return this;

        }

        public override AbstractParams SetInt(int argindex,int v)
        {
            int index;
            long tp = this._ReadIndex(argindex,out index);
            if(tp == (int)ParamType.INT)
            {
                if(index == 0)
                {
                    this.m_i1 = v;
                }
                else if(index == 1)
                {
                    this.m_i2 = v;
                }
                else if(index== 2)
                {
                    this.m_i3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }

            return this;
        }

        public override AbstractParams SetShort(int argindex,short v)
        {
            int index;
            long tp = this._ReadIndex(argindex,out index);
            if(tp == (int)ParamType.SHORT)
            {
                if(index == 0)
                {
                    this.m_st1 = v;
                }
                else if(index == 1)
                {
                    this.m_st2 = v;
                }
                else if(index== 2)
                {
                    this.m_st3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }
            return this;
        }
        
        public override AbstractParams SetString(int argindex,string v)
        {
            int index;
            long tp = this._ReadIndex(argindex,out index);
            if(tp == (int)ParamType.STRING)
            {
                if(index == 0)
                {
                    this.m_string1 = v;
                }
                else if(index == 1)
                {
                    this.m_string2 = v;
                }
                else if(index== 2)
                {
                    this.m_string3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }
            return this;
        }

        public override AbstractParams SetBool(int argindex,bool v)
        {
            int index;
            long tp = this._ReadIndex(argindex,out index);
            if(tp == (int)ParamType.BOOL)
            {
                if(index == 0)
                {
                    this.m_b1 = v;
                }
                else if(index == 1)
                {
                    this.m_b2 = v;
                }
                else if(index== 2)
                {
                    this.m_b3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }

            return this;
        }

        public override AbstractParams SetLong(int argindex,long v)
        {
            int index;
            long tp = this._ReadIndex(argindex,out index);
            if(tp == (int)ParamType.LONG)
            {
                if(index == 0)
                {
                    this.m_l1 = v;
                }
                else if(index == 1)
                {
                    this.m_l2 = v;
                }
                else if(index== 2)
                {
                    this.m_l3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }

            return this;
        }

        public override AbstractParams SetVector3(int argindex, Vector3 v)
        {
            int index;
            long tp = this._ReadIndex(argindex, out index);
            if (tp == (int)ParamType.VETOR3)
            {
                if (index == 0)
                {
                    this.m_v1 = v;
                }
                else if (index == 1)
                {
                    this.m_v2 = v;
                }
                else if (index == 2)
                {
                    this.m_v3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }

            return this;
        }

        public override AbstractParams SetColor(int argindex, Color v)
        {
            int index;
            long tp = this._ReadIndex(argindex, out index);
            if (tp == (int)ParamType.Color)
            {
                if (index == 0)
                {
                    this.m_c1 = v;
                }
                else if (index == 1)
                {
                    this.m_c2 = v;
                }
                else if (index == 2)
                {
                    this.m_c3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }

            return this;
        }

        public override AbstractParams SetObject (int argindex,object v)
        {
            int index;
            long tp = this._ReadIndex(argindex,out index);
            if(tp == (int)ParamType.OBJECT)
            {
                if(index == 0)
                {
                    this.m_o1 = v;
                }
                else if(index == 1)
                {
                    this.m_o2 = v;
                }
                else if(index== 2)
                {
                    this.m_o3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }

            return this;
        }

        public override AbstractParams SetUnityObject(int argindex,UnityEngine.Object v)
        {
            int index;
            long tp = this._ReadIndex(argindex,out index);
            if(tp == (int)ParamType.UNITYOBJECT)
            {
                if(index == 0)
                {
                    this.m_uo1 = v;
                }
                else if(index == 1)
                {
                    this.m_uo2 = v;
                }
                else if(index== 2)
                {
                    this.m_uo3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }

            return this;
        }

        public override AbstractParams SetFloat(int argindex,float v)
        {
            int index;
            long tp = this._ReadIndex(argindex,out index);
            if(tp == (int)ParamType.FLOAT)
            {
                if(index == 0)
                {
                    this.m_f1 = v;
                }
                else if(index == 1)
                {
                    this.m_f2 = v;
                }
                else if(index== 2)
                {
                    this.m_f3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }

            return this;
        }

        public override AbstractParams SetDouble(int argindex,double v)
        {
            int index;
            long tp = this._ReadIndex(argindex,out index);
            if(tp == (int)ParamType.DOUBLE)
            {
                if(index == 0)
                {
                    this.m_d1 = v;
                }
                else if(index == 1)
                {
                    this.m_d2 = v;
                }
                else if(index== 2)
                {
                    this.m_d3 = v;
                }
                else
                {
                    ThrowError("参数索引异常");
                }
            }
            else
            {
                ThrowError(string.Format( "下一个类型为:{0} 参数异常 当前索引为 :{1}",(ParamType)tp, argindex));
            }
            return this;
        }

        public override void ResetReadIndex()
        {
            this._NextReadIndex =0;
        }

        public override int GetArgIndexType(int index)
        {
            int dataindex;
            long tp = this._ReadIndex(index,out dataindex);
            return (int)tp;
        }

        public override string ToString ()
        {
            StringBuilder sb = new  StringBuilder();
            sb.AppendFormat("{0} 参数个数: {1} ",base.ToString(),this.ArgCount.ToString());

            int old = _NextReadIndex;

            this.ResetReadIndex();

            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if (tp == (int)ParamType.INT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadInt());
                }
                else if (tp == (int)ParamType.SHORT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadShort());
                }
                else if (tp == (int)ParamType.BOOL)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadBool());
                }
                else if (tp == (int)ParamType.STRING)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadString());
                }
                else if (tp == (int)ParamType.DOUBLE)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadDouble());
                }
                else if (tp == (int)ParamType.FLOAT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadFloat());
                }
                else if (tp == (int)ParamType.NULL)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, "空");
                }
                else if (tp == (int)ParamType.LONG)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadLong());
                }
                else if (tp == (int)ParamType.VETOR3)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadVector3());
                }
                else if (tp == (int)ParamType.OBJECT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadObject());
                }
                else if (tp == (int)ParamType.UNITYOBJECT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadUnityObject());
                }
                else if (tp == (int)ParamType.Color)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.ReadColor());
                }
                else
                {
                    LogMgr.LogError("未增加的类型");
                }
            }

            this._NextReadIndex=old;
            return sb.ToString();

        }
    }
}

