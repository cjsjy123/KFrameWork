#define FLIP
using System.Collections;
using System;
using System.Collections.Generic;
using KUtils;
using System.Runtime.InteropServices;

namespace KFrameWork
{

    public class NetByteBuffer :IPool,IDisposable
    {
        private int _Position;
        /// <summary>
        /// 获得它下一次将要读取的位置
        /// </summary>
        /// <value>The position.</value>
        public int Position
        {
            get
            {
                return _Position;
            }
        }
        /// <summary>
        /// 剩余可读字节
        /// </summary>
        /// <value>The left data count.</value>
        public int byteAviliable
        {
            get
            {
                return this.DataCount - this._Position;
            }
        }

        private int _datacnt;
        /// <summary>
        /// 数据容量小于等于capacity
        /// </summary>
        /// <value>The data count.</value>
        public int DataCount
        {
            get
            {
                return _datacnt;
            }
        }

        private int? _capacity;
        /// <summary>
        /// 容量
        /// </summary>
        /// <value>The capacity.</value>
        public int capacity
        {
            get
            {
                if(_capacity.HasValue)
                {
                    return _capacity.Value;
                }
                return 0;
            }
        }

        private byte[] sourceArray;

        private NetByteBuffer():this(16)
        {
        }

        private NetByteBuffer(int size)
        {
            sourceArray = new byte[size];
            _capacity = size;
        }

        private NetByteBuffer(byte[] bys)
        {
            sourceArray = new byte[bys.Length];
            Array.Copy(bys,sourceArray,bys.Length);
            _capacity = bys.Length;
            this._datacnt = bys.Length;
        }

        private void _Reset()
        {
            this._Position =0;
            this._capacity = this.sourceArray.Length;
            this._datacnt = 0;
        }

        private void _ResetWithBytes(byte[] bys)
        {
            this._Reset();
            this.Write(bys);
        }

        private static int _SeekFunc(NetByteBuffer buffer,int target)
        {
            if(buffer == null)
                return -1000000;
            else
            {
                return buffer.capacity - target;
            }
        }

        public static NetByteBuffer Create<T>() where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            return CreateWithSize(size);
        }

        public static NetByteBuffer CreateWithSize(int size)
        {
            NetByteBuffer buffer = null;
            if (KObjectPool.mIns != null)
            {

                buffer = KObjectPool.mIns.Seek<NetByteBuffer, int>(_SeekFunc, size, 5);
            }

            if (buffer == null)
            {
                buffer = new NetByteBuffer(size);
            }
            else
            {
                buffer._Reset();
            }
            return buffer;
        }

        public static NetByteBuffer CreateWithBytes(byte[] bys)
        {
            NetByteBuffer buffer =  null;
            if(KObjectPool.mIns != null)
            {
                int byslen =0;
                if(bys  != null)
                    byslen = bys.Length;

                buffer = KObjectPool.mIns.Seek<NetByteBuffer,int>(_SeekFunc,byslen,5);
                if (buffer != null)
                {
                    buffer._ResetWithBytes(bys);
                }  
            }

            if(buffer == null)
            {
                buffer = new NetByteBuffer(bys);
            }

            return buffer;
        }

        public void RemoveToPool ()
        {
            this._Reset();
        }

        public void RemovedFromPool ()
        {
            this.sourceArray = null;
        }

        public void Dispose ()
        {
            if(KObjectPool.mIns != null)
            {
                KObjectPool.mIns.Push(this);
            }
        }

        protected void _IncreaseCapacity(int nextSize)
        {
            if(sourceArray != null && capacity < nextSize)
            {
                nextSize = getNearstGreatSize(nextSize);

                byte[] newbys = new byte[nextSize];
                Array.Copy(this.sourceArray,newbys,this.sourceArray.Length);

                this.sourceArray = newbys;
                this._capacity = nextSize;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="begin">Begin.</param>
        /// <param name="len">Length.</param>
        public virtual byte[] Read(int begin, int len)
        {
            int readlen =len;
            int endIndex = begin +len;
            if(endIndex > this.DataCount)
            {
                readlen -=(endIndex - this.DataCount);
            }

            byte[] retbys = null;

            if(readlen >0)
            {
                retbys = new byte[readlen];

                Array.Copy(this.sourceArray,begin,retbys,0,readlen);
                this._Position = endIndex;
            }
            return retbys;
        }
        /// <summary>
        /// 获得结果，同时对象放入对象池
        /// </summary>
        /// <returns></returns>
        public byte[] GetResult()
        {
            int len = this.DataCount;
            byte[] result = this.Read(0, len);
            this.Dispose();
            return result;
        }

        public byte ReadByte()
        {
            int endIndex = this.Position;
            if(endIndex < this.DataCount)
            {
                var b = this.sourceArray[endIndex];
                this._Position ++;
                return b;
            }
            throw new ArgumentOutOfRangeException();
        }

        public void WriteByte(byte b)
        {
            int writelen =1;
            int nextsize = this.DataCount + writelen;
            this._IncreaseCapacity(nextsize);
            this.sourceArray[this.DataCount]= b;

            this._datacnt += writelen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The next.</returns>
        /// <param name="len">Length.</param>
        public byte[] ReadNext(int len)
        {
            int readlen =len;
            int endIndex = this.Position +len -1;

            if(endIndex > this.DataCount)
            {
                readlen -=(endIndex - this.DataCount);
            }
            byte[] retbys = null;

            if(readlen >0)
            {
                retbys = new byte[readlen];
                Array.Copy(this.sourceArray,this.Position,retbys,0,readlen);
                this._Position += readlen;
            }

            return retbys;
        }

        public virtual void Write(byte[] bys, int len =-1)
        {
            int writelen = 0;
            if(bys != null && bys.Length >0)
            {
                if(len < 0 )
                {
                    writelen = bys.Length;
                }
                else
                {
                    writelen = Math.Min(bys.Length,len);
                }

                int nextsize = this.DataCount + writelen;
                this._IncreaseCapacity(nextsize);

                Array.Copy(bys,0,this.sourceArray,this.DataCount,writelen);
                this._datacnt += writelen;
            }
        }

        protected virtual int getNearstGreatSize(int size)
        {
            int val =2;
            while( val < size)
            {
                val = val<<1;
            }

            if(size > val)
            {
                val *=2;
            }
            return val;
        }

        protected static void _Reverse(byte[] bys)
        {
            #if FLIP
            if (!BitConverter.IsLittleEndian)
                _MyReverse(bys);
            #else
            if (BitConverter.IsLittleEndian)
                _MyReverse(bys);
            #endif
        }
        /// <summary>
        /// 使用array.reservse 有更高的gc
        /// </summary>
        /// <param name="bys">Bys.</param>
        protected static void _MyReverse(byte[] bys)
        {
            if(bys != null && bys.Length >1)
            {
                int midindex = bys.Length/2;

                for(int i=0; i < midindex;++i)
                {
                    byte p = bys[i];
                    byte l = bys[bys.Length-1-i];
                    bys[i] = l;
                    bys[bys.Length -1 -i] =p;
                }
            }
        }

        public static NetByteBuffer operator +(NetByteBuffer left, NetByteBuffer right)
        {
            if (right != null && left != right)
            {
                left.Write(right.sourceArray);
                right._capacity =0;
                right.sourceArray = null;
                right._Position =0;
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, byte[] right)
        {
            if (right != null )
            {
                _Reverse(right);
                left.Write(right);
            }
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, byte right)
        {
            left.WriteByte(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, int right)
        {
            left += BitConverter.GetBytes(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, bool right)
        {
            byte[] bys= new byte[1];
            bys[0]=right ?(byte)1:(byte)0;
            left += bys;
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, float right)
        {
            left += BitConverter.GetBytes(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, double right)
        {
            left += BitConverter.GetBytes(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, short right)
        {
            left += BitConverter.GetBytes(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, long right)
        {
            left += BitConverter.GetBytes(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, uint right)
        {
            left += BitConverter.GetBytes(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, ulong right)
        {
            left += BitConverter.GetBytes(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, ushort right)
        {
            left += BitConverter.GetBytes(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, string right)
        {
            if (string.IsNullOrEmpty(right))
            {
                left += (short)0;
            }
            else
            {
                byte[] bs = System.Text.Encoding.UTF8.GetBytes(right);
                left += (short)bs.Length;
                if (bs.Length > 0)
                    left += bs;
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, List<int> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, List<short> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }


        public static NetByteBuffer operator +(NetByteBuffer left, List<long> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, List<ushort> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, List<string> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, List<bool> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, List<float> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, List<double> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, List<uint> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, List<ulong> right)
        {

            left += (short)right.Count;
            for (int i = 0; i < right.Count; ++i)
            {
                left += right[i];
            }

            return left;
        }
            

        public static explicit operator bool(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 1)
            {
                byte b  = left.ReadByte();

                return b ==1 ?true:false;
            }
            return false;
        }

        public static explicit operator int(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 4)
            {
                var tempBys = left.Read(left.Position,4);
                _Reverse(tempBys);
                
                return BitConverter.ToInt32(tempBys, 0);
            }
            LogMgr.LogError("int Read from ByteBuffer Error");

            return default(int);
        }

        public static explicit operator uint(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 4)
            {
                var tempBys = left.Read(left.Position,4);
                _Reverse(tempBys);
                return BitConverter.ToUInt32(tempBys, 0);
            }
            LogMgr.LogError("uint Read from ByteBuffer Error");

            return default(UInt32);
        }

        public static explicit operator short(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                var tempBys = left.Read(left.Position,2);
                _Reverse(tempBys);
                return BitConverter.ToInt16(tempBys, 0);
            }
            LogMgr.LogError("short Read from ByteBuffer Error");

            return default(short);
        }

        public static explicit operator float(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 4)
            {
                var tempBys = left.Read(left.Position,4);
                _Reverse(tempBys);
                return BitConverter.ToSingle(tempBys, 0);
            }
            LogMgr.LogError("float Read from ByteBuffer Error");

            return default(float);
        }

        public static explicit operator double(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 8)
            {
                var tempBys = left.Read(left.Position,8);
                _Reverse(tempBys);
                return BitConverter.ToDouble(tempBys, 0);
            }
            LogMgr.LogError("double Read from ByteBuffer Error");

            return default(double);
        }

        public static explicit operator ushort(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                var tempBys = left.Read(left.Position,2);
                _Reverse(tempBys);
                return BitConverter.ToUInt16(tempBys, 0);
            }
            LogMgr.LogError("ushort Read from ByteBuffer Error");

            return default(ushort);
        }

        public static explicit operator ulong(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 8)
            {
                var tempBys = left.Read(left.Position,8);
                _Reverse(tempBys);
                return BitConverter.ToUInt64(tempBys, 0);
            }
            LogMgr.LogError("ulong Read from ByteBuffer Error");

            return default(ulong);
        }

        public static explicit operator byte(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 1)
            {
                return left.ReadByte();
            }
            LogMgr.LogError("byte Read from ByteBuffer Error");
            return default(byte);
        }

        public static explicit operator long(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 8)
            {
                var tempBys = left.Read(left.Position,8);
                _Reverse(tempBys);
                return BitConverter.ToInt64(tempBys, 0);
            }
            LogMgr.LogError("long Read from ByteBuffer Error");
            return default(long);
        }

        public static explicit operator string(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                var tempBys = left.Read(left.Position,strLen);
                _Reverse(tempBys);
                return System.Text.Encoding.UTF8.GetString(tempBys);
            }
            LogMgr.LogError("string Read from ByteBuffer Error");
            return "";
        }

        public static explicit operator List<int>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen > 0)
                {
                    List<int> list = new List<int>();
                    if (left.byteAviliable >= strLen * 4)
                    {
                        for (int i = 0; i < strLen; ++i)
                        {
                            list.Add((int)left);
                        }
                        return list;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    } 
                }
                else
                    return null;
            }
            LogMgr.LogError("List<int> Read from ByteBuffer Error");
            return null;
        }

        public static explicit operator List<short>(NetByteBuffer left)
        {
            if (left != null && left.DataCount >= 2)
            {
                short strLen = (short)left;
                if (strLen >0)
                {
                    List<short> list = new List<short>();
                    if (left.byteAviliable >= strLen * 2)
                    {
                        for (int i = 0; i < strLen; ++i)
                        {
                            list.Add((short)left);
                        }
                        return list;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            LogMgr.LogError("List<short> Read from ByteBuffer Error");

            return null;
        }

        public static explicit operator List<long>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen >0)
                {
                    List<long> list = new List<long>();
                    if (left.byteAviliable >= strLen * 8)
                    {
                        for (int i = 0; i < strLen; ++i)
                        {
                            list.Add((long)left);
                        }
                        return list;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            LogMgr.LogError("List<long> Read from ByteBuffer Error");

            return null;
        }

        public static explicit operator List<uint>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen >0)
                {
                    List<uint> list = new List<uint>();
                    if (left.byteAviliable >= strLen * 4)
                    {
                        for (int i = 0; i < strLen; ++i)
                        {
                            list.Add((uint)left);
                        }
                        return list;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            LogMgr.LogError("List<uint> Read from ByteBuffer Error");
            return null;
        }

        public static explicit operator List<ushort>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen >0)
                {
                    List<ushort> list = new List<ushort>();
                    if (left.byteAviliable >= strLen * 2)
                    {
                        for (int i = 0; i < strLen; ++i)
                        {
                            list.Add((ushort)left);
                        }
                        return list;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            LogMgr.LogError("List<ushort> Read from ByteBuffer Error");
 
            return null;
        }

        public static explicit operator List<ulong>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen >0)
                {
                    List<ulong> list = new List<ulong>();
                    if (left.byteAviliable >= strLen * 8)
                    {
                        for (int i = 0; i < strLen; ++i)
                        {
                            list.Add((ulong)left);
                        }
                        return list;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            LogMgr.LogError("List<ulong> Read from ByteBuffer Error");

            return null;
        }

        public static explicit operator List<double>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen > 0)
                {
                    List<double> list = new List<double>();
                    if (left.byteAviliable >= strLen * 8)
                    {
                        for (int i = 0; i < strLen; ++i)
                        {
                            list.Add((double)left);
                        }
                        return list;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            LogMgr.LogError("List<double> Read from ByteBuffer Error");
            return null;
        }

        public static explicit operator List<float>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen > 0)
                {
                    List<float> list = new List<float>();
                    if (left.byteAviliable >= strLen * 4)
                    {
                        for (int i = 0; i < strLen; ++i)
                        {
                            list.Add((float)left);
                        }
                        return list;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            LogMgr.LogError("List<float> Read from ByteBuffer Error");
            return null;
        }

        public static explicit operator List<bool>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen > 0)
                {
                    List<bool> list = new List<bool>();
                    if (left.byteAviliable >= strLen )
                    {
                        for (int i = 0; i < strLen; ++i)
                        {
                            list.Add((bool)left);
                        }
                        return list;
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            LogMgr.LogError("List<bool> Read from ByteBuffer Error");
            return null;
        }

        public static explicit operator List<string>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen >0)
                {
                    List<string> list = new List<string>();
                    for (int i = 0; i < strLen; ++i)
                    {
                        list.Add((string)left);
                    }
                    return list;
                }
                else
                    return null;
            }
            LogMgr.LogError("List<string>  Read from ByteBuffer Error");

            return null;
        }

        #region generic

        public void WriteGeneric<T>(T data) where T : ISeriable
        {
            this.Write(data.Seriable());
        }

        public void WriteGenericList<T>(List<T> data) where T : ISeriable
        {
            if (data == null)
            {
                this.WriteShort(0);
            }
            else
            {
                this.WriteShort((short)data.Count);
                for (int i = 0; i < data.Count; ++i)
                {
                    this.Write(data[i].Seriable());
                }
            }
        }

        public void WriteGenericDictionary<K, V>(Dictionary<K, V> dict) where K : ISeriable where V : ISeriable
        {
            List<K> keys = new List<K>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteGenericList(keys);
            this.WriteGenericList(values);
        }


        public void WriteGenericDictionary<V>(Dictionary<float, V> dict) where V : ISeriable
        {
            List<float> keys = new List<float>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteFloatList(keys);
            this.WriteGenericList(values);
        }

        public void WriteGenericDictionary<V>(Dictionary<double, V> dict) where V : ISeriable
        {
            List<double> keys = new List<double>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteDoubleList(keys);
            this.WriteGenericList(values);
        }

        public void WriteGenericDictionary<V>(Dictionary<int, V> dict) where V : ISeriable
        {
            List<int> keys = new List<int>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteIntList(keys);
            this.WriteGenericList(values);
        }

        public void WriteGenericDictionary<V>(Dictionary<short, V> dict) where V : ISeriable
        {
            List<short> keys = new List<short>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteShortList(keys);
            this.WriteGenericList(values);
        }

        public void WriteGenericDictionary<V>(Dictionary<long, V> dict) where V : ISeriable
        {
            List<long> keys = new List<long>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteLongList(keys);
            this.WriteGenericList(values);
        }

        public void WriteGenericDictionary<V>(Dictionary<uint, V> dict) where V : ISeriable
        {
            List<uint> keys = new List<uint>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteUintList(keys);
            this.WriteGenericList(values);
        }

        public void WriteGenericDictionary<V>(Dictionary<ushort, V> dict) where V : ISeriable
        {
            List<ushort> keys = new List<ushort>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteUShortList(keys);
            this.WriteGenericList(values);
        }

        public void WriteGenericDictionary<V>(Dictionary<ulong, V> dict) where V : ISeriable
        {
            List<ulong> keys = new List<ulong>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteUlongList(keys);
            this.WriteGenericList(values);
        }

        public void WriteGenericDictionary<V>(Dictionary<bool, V> dict) where V : ISeriable
        {
            List<bool> keys = new List<bool>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteBoolList(keys);
            this.WriteGenericList(values);
        }

        public void WriteGenericDictionary<V>(Dictionary<string, V> dict) where V : ISeriable
        {
            List<string> keys = new List<string>(dict.Keys);
            List<V> values = new List<V>(dict.Values);
            this.WriteStringList(keys);
            this.WriteGenericList(values);
        }

        public Dictionary<K, V> ReadGenericDictionary<K, V>() where K:IDeseriable,new()  where V:IDeseriable,new()
        {
            List<K> keys = ReadGenericList<K>();
            List<V> values= ReadGenericList<V>();

            Dictionary<K, V> dict = new Dictionary<K, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i],values[i]);
            }
            return dict;
        }

        public Dictionary<int, V> ReadIntDictionary< V>() where V : IDeseriable, new()
        {
            List<int> keys = ReadIntList();
            List<V> values = ReadGenericList<V>();

            Dictionary<int, V> dict = new Dictionary<int, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public Dictionary<short, V> ReadShortDictionary<V>() where V : IDeseriable, new()
        {
            List<short> keys = ReadShortList();
            List<V> values = ReadGenericList<V>();

            Dictionary<short, V> dict = new Dictionary<short, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public Dictionary<long, V> ReadLongDictionary<V>() where V : IDeseriable, new()
        {
            List<long> keys = ReadLongList();
            List<V> values = ReadGenericList<V>();

            Dictionary<long, V> dict = new Dictionary<long, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public Dictionary<uint, V> ReadUintDictionary<V>() where V : IDeseriable, new()
        {
            List<uint> keys = ReadUIntList();
            List<V> values = ReadGenericList<V>();

            Dictionary<uint, V> dict = new Dictionary<uint, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public Dictionary<ushort, V> ReadUshortDictionary<V>() where V : IDeseriable, new()
        {
            List<ushort> keys = ReadUShortList();
            List<V> values = ReadGenericList<V>();

            Dictionary<ushort, V> dict = new Dictionary<ushort, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public Dictionary<ulong, V> ReadULongictionary<V>() where V : IDeseriable, new()
        {
            List<ulong> keys = ReadULongList();
            List<V> values = ReadGenericList<V>();

            Dictionary<ulong, V> dict = new Dictionary<ulong, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public Dictionary<bool, V> ReadBoolDictionary<V>() where V : IDeseriable, new()
        {
            List<bool> keys = ReadBoolList();
            List<V> values = ReadGenericList<V>();

            Dictionary<bool, V> dict = new Dictionary<bool, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public Dictionary<float, V> ReadFloatDictionary<V>() where V : IDeseriable, new()
        {
            List<float> keys = ReadFloatList();
            List<V> values = ReadGenericList<V>();

            Dictionary<float, V> dict = new Dictionary<float, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public Dictionary<double, V> ReadDoubleDictionary<V>() where V : IDeseriable, new()
        {
            List<double> keys = ReadDoubleList();
            List<V> values = ReadGenericList<V>();

            Dictionary<double, V> dict = new Dictionary<double, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public Dictionary<string, V> ReadStringDictionary<V>() where V : IDeseriable, new()
        {
            List<string> keys = ReadStringList();
            List<V> values = ReadGenericList<V>();

            Dictionary<string, V> dict = new Dictionary<string, V>(keys.Count);

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public List<T> ReadGenericList<T>() where T : IDeseriable,new()
        {
            List<T> list = ListPool.TrySpawn<List<T>>();
            short cnt = (short)this;
            for (int i = 0; i < cnt; ++i)
            {
                list.Add(ReadGeneric<T>());
            }

            return list;
        }

        public T ReadGeneric<T>() where T:IDeseriable,new()
        {
            T data = new T();
            data.Deseriable(this);
            return data;
        }
        #endregion
        public short ReadShort()
        {
            return (short)this;
        }

        
        public ushort ReadUShort()
        {
            return (ushort)this;
        }

        
        public uint ReadUInt()
        {
            return (uint)this;
        }

        
        public int ReadInt()
        {
            return (int)this;
        }

        
        public float ReadFloat()
        {
            return (float)this;
        }

        
        public double ReadDouble()
        {
            return (double)this;
        }

        
        public long ReadLong()
        {
            return (long)this;
        }

        
        public ulong ReadULong()
        {
            return (ulong)this;
        }

        
        public string ReadString()
        {
            return (string)this;
        }

        
        public List<int> ReadIntList()
        {
            return (List<int>)this;
        }

        
        public List<uint> ReadUIntList()
        {
            return (List<uint>)this;
        }

        
        public List<short> ReadShortList()
        {
            return (List<short>)this;
        }

        
        public List<ushort> ReadUShortList()
        {
            return (List<ushort>)this;
        }

        
        public List<long> ReadLongList()
        {
            return (List<long>)this;
        }

        
        public List<ulong> ReadULongList()
        {
            return (List<ulong>)this;
        }

        public List<float> ReadFloatList()
        {
            return (List<float>)this;
        }

        public List<double> ReadDoubleList()
        {
            return (List<double>)this;
        }

        public List<string> ReadStringList()
        {
            return (List<string>)this;
        }


        public List<bool> ReadBoolList()
        {
            return (List<bool>)this;
        }

        public void WriteShort(short value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void WriteUShort(ushort value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void WriteInt(int value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void WriteUint(uint value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void WriteLong(long value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void WriteUlong(ulong value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void WriteBool(bool value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }
        
        public void WriteString(string value)
        {
            byte[] bys = System.Text.Encoding.UTF8.GetBytes(value);
            this.WriteShort((short)bys.Length);

            _Reverse(bys);
            this.Write(bys);
        }
        
        public void WriteFloat(float value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void WriteDouble(double value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        public void WriteDoubleList(List<double> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteDouble(value[i]);
            }
        }

        public void WriteFloatList(List<float> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteFloat(value[i]);
            }
        }

        public void WriteShortList(List<short> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteShort(value[i]);
            }
        }

        
        public void WriteUShortList(List<ushort> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteUShort(value[i]);
            }
        }

        
        public void WriteIntList(List<int> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteInt(value[i]);
            }
        }

        
        public void WriteUintList(List<uint> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteUint(value[i]);
            }
        }

        
        public void WriteUlongList(List<ulong> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteUlong(value[i]);
            }
        }

        
        public void WriteLongList(List<long> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteLong(value[i]);
            }
        }
        
        public void WriteStringList(List<string> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteString(value[i]);
            }
        }

        public void WriteBoolList(List<bool> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteBool(value[i]);
            }
        }

    }
}


