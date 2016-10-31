using System;
using UnityEngine;
using System.Runtime.InteropServices;
using KUtils;
using System.Text;

namespace KFrameWork
{
//    [StructLayout(LayoutKind.Sequential)]
    public class SimpleParams:AbstractParams,KUtils.IPool
    {
        private long bitIndex =0;
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

        private System.Object m_o1;
        private System.Object m_o2;
        private System.Object m_o3;

        private UnityEngine.Object m_uo1;
        private UnityEngine.Object m_uo2;
        private UnityEngine.Object m_uo3;

        [FrameWokAwakeAttribute]
        private static void Preload(int v)
        {
            for(int i=0; i < FrameWorkDebug.Preload_ParamsCount;++i)
            {
                KObjectPool.mIns.Push(PreCreate(1));
                KObjectPool.mIns.Push(PreCreate(2));
                KObjectPool.mIns.Push(PreCreate(3));
            }
        }

        private static SimpleParams PreCreate(int origion =-1)
        {
            SimpleParams p = new SimpleParams();
            p._OrigionArgCount = origion;
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
                p._OrigionArgCount = origion;
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
            if(KObjectPool.mIns != null)
                KObjectPool.mIns.Push(this);
        }

        public void AwakeFromPool ()
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
            this._OrigionArgCount =-1;
            this.bitIndex = 0;
            this.bitDataSort =0;
        }


        public void RemovedFromPool ()
        {
            this.m_l1 = null;
            this.m_l2 = null;
            this.m_l3 = null;

            this.m_i1 = null;
            this.m_i2 = null;
            this.m_i2 = null;

            this.m_st1 = null;
            this.m_st2 = null;
            this.m_st3 = null;

            this.m_string1 = null;
            this.m_string2 = null;
            this.m_st3 = null;

            this.m_b1 = null;
            this.m_b2 = null;
            this.m_b3 = null;

            this.m_f1 = null;
            this.m_f2 = null;
            this.m_f3 = null;

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
            long tp = this._ReadIndex(this._NextReadIndex,out index);

            if(ArgCount != 0)
            {
                _NextReadIndex= (++_NextReadIndex)%ArgCount;
            }

            return tp;
        }

        private void _SetNextTp(long tp,long index)
        {
            if( index >4 || index <0 )
            {
                LogMgr.LogError("参数索引错误");
                return;
            }

            if(this.ArgCount+1 > this._LimitMax)
            {
                LogMgr.LogError("参数过多");
                return ;
            }

            int old = this._ArgCount;
            this._ArgCount++;

            if(this._OrigionArgCount >-1)
            {
                if(this._OrigionArgCount < this.ArgCount)
                {
                    this._ArgCount = this._OrigionArgCount;
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

        private long _insertBit(long orig,long powcnt, int index,int tpval)
        {
            long powval =_MyPow(powcnt,index)-1;

            long right = orig & powval;
            long left = orig | powval;

            return  left *powcnt + right + tpval<<index;
        }

        /// <summary>
        /// 数据会发生遗弃
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="v">V.</param>
        public override void InsertInt (int index, int v)
        {
            if(this._OrigionArgCount >0)
                this._OrigionArgCount++;
            short count =0;
            for(int i =0; i < this._virtualArg;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.INT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,16,index,(int)ParamType.INT); 

            if(count == 0)
            {
                this.m_i1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_i2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_i3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
        }

        public override void InsertShort (int index, short v)
        {
            if(this._OrigionArgCount >0)
                this._OrigionArgCount++;
            
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.SHORT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,16,index,(int)ParamType.SHORT); 

            if(count == 0)
            {
                this.m_st1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_st2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_st3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
        }

        public override void InsertString (int index, string v)
        {
            if(this._OrigionArgCount >0)
                this._OrigionArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.STRING)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,16,index,(int)ParamType.STRING); 

            if(count == 0)
            {
                this.m_string1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_string2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_string3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
        }

        public override void InsertBool (int index, bool v)
        {
            if(this._OrigionArgCount >0)
                this._OrigionArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.BOOL)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,16,index,(int)ParamType.BOOL); 

            if(count == 0)
            {
                this.m_b1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_b2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_b3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
        }

        public override void InsertFloat (int index, float v)
        {
            if(this._OrigionArgCount >0)
                this._OrigionArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.FLOAT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,16,index,(int)ParamType.FLOAT); 

            if(count == 0)
            {
                this.m_f1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_f2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_f3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
        }

        public override void InsertDouble (int index, double v)
        {
            if(this._OrigionArgCount >0)
                this._OrigionArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.DOUBLE)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,16,index,(int)ParamType.DOUBLE); 

            if(count == 0)
            {
                this.m_d1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_d2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_d3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
        }

        public override void InsertLong (int index, long v)
        {
            if(this._OrigionArgCount >0)
                this._OrigionArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.LONG)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,16,index,(int)ParamType.LONG); 

            if(count == 0)
            {
                this.m_l1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_l2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_l3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
        }

        public override void InsertVector3(int index, Vector3 v)
        {
            if (this._OrigionArgCount > 0)
                this._OrigionArgCount++;
            short count = 0;
            for (int i = 0; i < this.ArgCount; ++i)
            {
                int tp = this.GetArgIndexType(i);
                if (tp == (int)ParamType.VETOR3)
                {
                    count++;
                }
            }

            this.bitDataSort = this._insertBit(this.bitDataSort, 16, index, (int)ParamType.VETOR3);

            if (count == 0)
            {
                this.m_v1 = v;
                this.bitIndex = this._insertBit(this.bitIndex, 4, index, 0);
                this._ArgCount++;
            }
            else if (count == 1)
            {
                this.m_v2 = v;
                this.bitIndex = this._insertBit(this.bitIndex, 4, index, 1);
                this._ArgCount++;
            }
            else if (count == 2)
            {
                this.m_v3 = v;
                this.bitIndex = this._insertBit(this.bitIndex, 4, index, 2);
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
        }

        public override void InsertObject (int index, object v)
        {
            if(this._OrigionArgCount >0)
                this._OrigionArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.OBJECT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,16,index,(int)ParamType.OBJECT); 

            if(count == 0)
            {
                this.m_o1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_o2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_o3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
        }

        public override void InsertUnityObject (int index, UnityEngine.Object v)
        {
            if(this._OrigionArgCount >0)
                this._OrigionArgCount++;
            short count =0;
            for(int i =0; i < this.ArgCount;++i)
            {
                int tp = this.GetArgIndexType(i);
                if(tp == (int)ParamType.UNITYOBJECT)
                {
                    count++;
                }
            }

            this.bitDataSort= this._insertBit(this.bitDataSort,16,index,(int)ParamType.UNITYOBJECT); 

            if(count == 0)
            {
                this.m_uo1 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,0); 
                this._ArgCount++;
            }
            else if(count == 1)
            {
                this.m_uo2 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,1); 
                this._ArgCount++;
            }
            else if(count == 2)
            {
                this.m_uo3 = v;
                this.bitIndex= this._insertBit(this.bitIndex,4,index,2); 
                this._ArgCount++;
            }
            else
            {
                this.UseGener();
            }
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
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
                        LogMgr.LogError("参数异常，未赋值");
                    }
                }
                else
                {
                    _NextReadIndex = old;
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
            }
            return Vector3.zero;
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                _NextReadIndex = old;
                LogMgr.LogError("参数异常");
            }
            return null;
        }

        public override void WriteInt (int v)
        {
            if(this._OrigionArgCount >0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }

   
        }

        public override void WriteShort (short v)
        {
            if(this._OrigionArgCount >0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }

        }

        public override void WriteString (string v)
        {
            if(this._OrigionArgCount >0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }

        }

        public override void WriteFloat (float v)
        {
            if(this._OrigionArgCount >0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }

        }

        public override void WriteDouble (double v)
        {
            if(this._OrigionArgCount >0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }

        }

        public override void WriteBool (bool v)
        {
            if(this._OrigionArgCount >0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }

        }

        public override void WriteLong (long v)
        {
            if(this._OrigionArgCount >0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }
                

        }

        public override void WriteVector3(Vector3 v)
        {
            if (this._OrigionArgCount > 0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }
        }

        public override void WriteObject (object v)
        {
            if(this._OrigionArgCount >0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }

        }

        public override void WriteUnityObject (UnityEngine.Object v)
        {
            if(this._OrigionArgCount >0)
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
                    this.UseGener();
                }

                this._virtualArg++;
                this._virtualArg = this._virtualArg % this._OrigionArgCount;
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
                    this.UseGener();
                }
            }

        }

        public override void SetInt(int argindex,int v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public override void SetShort(int argindex,short v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public override void SetString(int argindex,string v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public override void SetBool(int argindex,bool v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public override void SetLong(int argindex,long v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public override void SetVector3(int argindex, Vector3 v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public override void SetObject (int argindex,object v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public override void SetUnityObject(int argindex,UnityEngine.Object v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public override void SetFloat(int argindex,float v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
        }

        public override void SetDouble(int argindex,double v)
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
                    LogMgr.LogError("参数索引异常");
                }
            }
            else
            {
                LogMgr.LogError("参数异常");
            }
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
            sb.AppendFormat("参数个数: {0} ",this.ArgCount.ToString());

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
            }

            this._NextReadIndex=old;
//
//            if(sb.Length == 0)
//            {
//                return "空";
//            }

            return sb.ToString();

        }
    }
}

