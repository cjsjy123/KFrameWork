//#define FLIP
using System.Collections;
using System;
using System.Collections.Generic;
using KUtils;
using System.Runtime.InteropServices;

namespace KFrameWork
{

    public class NetByteBuffer :IPool,IDisposable
    {
        private int _Position ;
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

            set
            {
                _Position = value;
                if (_Position >= DataCount)
                {
                    _Position = DataCount - 1;
                    if (FrameWorkConfig.Open_DEBUG)
                        ThrowError("Position out of Index");
                }
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

        /// <summary>
        /// 容量
        /// </summary>
        /// <value>The capacity.</value>
        public int capacity
        {
            get
            {
                if(sourceArray != null)
                {
                    return sourceArray.Length;
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
        }

        private NetByteBuffer(byte[] bys)
        {
            sourceArray = new byte[bys.Length];
            Array.Copy(bys,sourceArray,bys.Length);
            this._datacnt = bys.Length;
        }

        private void _Reset()
        {
            this._Position =0;
            this._datacnt = 0;
        }

        private void _ResetWithBytes(byte[] bys)
        {
            this._Reset();
            this.Write(bys);
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
                buffer = KObjectPool.mIns.Pop<NetByteBuffer>();
            }

            if (buffer == null)
            {
                buffer = new NetByteBuffer(size);
            }
            else
            {
                if (buffer.capacity < size)
                {
                    buffer.Resize(size);
                }

                buffer._Reset();
            }
            return buffer;
        }

        private void Resize(int newsize)
        {
            byte[] newarray = new byte[newsize];
            if (this.sourceArray != null)
            {
                if (newsize >= this.sourceArray.Length)
                {
                    Array.Copy(this.sourceArray, newarray, this.sourceArray.Length);
                }
                else
                {
                    Array.Copy(this.sourceArray, this.sourceArray.Length - newsize, newarray, 0, newsize);
                }
            }

            this.sourceArray = newarray;
        }

        private void Clear(int offset,int len)
        {
            if (this.Position > offset)
            {
                this.Position = Math.Max(0,this.Position-len);
            }

            Array.Copy(this.sourceArray,offset+len,this.sourceArray,offset, this.capacity - offset - len);
            this._datacnt -= len;
        }

        public static NetByteBuffer CreateWithBytes(byte[] bys)
        {
            NetByteBuffer buffer =  null;
            if(KObjectPool.mIns != null)
            {
                buffer = KObjectPool.mIns.Pop<NetByteBuffer>();
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


        public static void ThrowErrorFormat<A>(string format, A s1)
        {
            throw new ArgumentException(string.Format(format, s1));
        }

        public static void ThrowErrorFormat<A, B>(string format, A s1, B s2)
        {
            throw new ArgumentException(string.Format(format, s1,s2));
        }

        public static void ThrowErrorFormat<A, B, C>(string format, A s1, B s2, C s3)
        {
            throw new ArgumentException(string.Format(format, s1, s2,s3));
        }

        public static void ThrowErrorFormat<A, B, C, D>(string format, A s1, B s2, C s3, D s4)
        {
            throw new ArgumentException(string.Format(format, s1, s2,s3,s4));
        }

        public static void ThrowErrorFormat<A, B, C, D, F>(string format, A s1, B s2, C s3, D s4, F s5)
        {
            throw new ArgumentException(string.Format(format, s1, s2, s3, s4,s5));
        }

        private static void ThrowError(string info)
        {
            throw new ArgumentException(info);
            //LogMgr.LogError(info);
        }

        public NetByteBuffer Copy()
        {
            NetByteBuffer buffer = CreateWithSize(this.DataCount);
            buffer.Write(this.sourceArray,0,this.DataCount);

            return buffer;
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

        public byte Readbyte()
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

        public void Writebyte(byte b)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public byte[] PopBytes(int offset,int len)
        {
            int readlen = len;

            byte[] retbys = null;
            if (readlen > 0)
            {
                retbys = ArrayPool.TrySpawn<byte>(readlen);// new byte[readlen];

                Array.Copy(this.sourceArray, offset, retbys, 0, readlen);

                this._Position = Math.Max(0,_Position - readlen);
                this._datacnt -= len;
                //
                //Array.Clear(this.sourceArray, 0, readlen);

                Resize(this.sourceArray.Length - len);
            }

            return retbys;
        }

        public void ClearBytes(int offset, int len)
        {
            Clear(offset, len);
        }

        public virtual void Write(byte[] bys,int offset =0, int len =-1)
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

                Array.Copy(bys, offset, this.sourceArray,this.DataCount,writelen);
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

        public static void _Reverse(byte[] bys)
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
                right.sourceArray = null;
                right._Position =0;
            }

            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, byte[] right)
        {
            if (right != null )
            {
                left.Write(right);
            }
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, byte right)
        {
            left.Writebyte(right);
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, int right)
        {
            byte[] bys = BitConverter.GetBytes(right);

            _Reverse(bys);
            left += bys;
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
            byte[] bys = BitConverter.GetBytes(right);
            _Reverse(bys);
            left += bys;
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, double right)
        {
            byte[] bys = BitConverter.GetBytes(right);
            _Reverse(bys);
            left += bys;
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, short right)
        {
            byte[] bys = BitConverter.GetBytes(right);
            _Reverse(bys);
            left += bys;
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, long right)
        {
            byte[] bys = BitConverter.GetBytes(right);
            _Reverse(bys);
            left += bys;
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, uint right)
        {
            byte[] bys = BitConverter.GetBytes(right);
            _Reverse(bys);
            left += bys;
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, ulong right)
        {
            byte[] bys = BitConverter.GetBytes(right);
            _Reverse(bys);
            left += bys;
            return left;
        }

        public static NetByteBuffer operator +(NetByteBuffer left, ushort right)
        {
            byte[] bys = BitConverter.GetBytes(right);
            _Reverse(bys);
            left += bys;
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
                byte b  = left.Readbyte();

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
            ThrowError("int Read from ByteBuffer Error");

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
            ThrowError("uint Read from ByteBuffer Error");

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
            ThrowError("short Read from ByteBuffer Error");

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
            ThrowError("float Read from ByteBuffer Error");

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
            ThrowError("double Read from ByteBuffer Error");

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
            ThrowError("ushort Read from ByteBuffer Error");

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
            ThrowError("ulong Read from ByteBuffer Error");

            return default(ulong);
        }

        public static explicit operator byte(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 1)
            {
                return left.Readbyte();
            }
            ThrowError("byte Read from ByteBuffer Error");
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
            ThrowError("long Read from ByteBuffer Error");
            return default(long);
        }

        public static explicit operator string(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                short strLen = (short)left;
                if (strLen == 0)
                    return string.Empty;

                var tempBys = left.Read(left.Position,strLen);
                //_Reverse(tempBys);
                return System.Text.Encoding.UTF8.GetString(tempBys);
            }
            ThrowError("string Read from ByteBuffer Error");
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
                        ThrowErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    } 
                }
                else
                    return null;
            }
            ThrowError("List<int> Read from ByteBuffer Error");
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
                        ThrowErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            ThrowError("List<short> Read from ByteBuffer Error");

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
                        ThrowErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            ThrowError("List<long> Read from ByteBuffer Error");

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
                        ThrowErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            ThrowError("List<uint> Read from ByteBuffer Error");
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
                        ThrowErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            ThrowError("List<ushort> Read from ByteBuffer Error");
 
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
                        ThrowErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            ThrowError("List<ulong> Read from ByteBuffer Error");

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
                        ThrowErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            ThrowError("List<double> Read from ByteBuffer Error");
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
                        ThrowErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            ThrowError("List<float> Read from ByteBuffer Error");
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
                        ThrowErrorFormat("字节空间数量不足！ 只有 {0} 但是需求为:{1}", left.byteAviliable, strLen * 4);
                    }
                }
                else
                    return null;
            }
            ThrowError("List<bool> Read from ByteBuffer Error");
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
            ThrowError("List<string>  Read from ByteBuffer Error");

            return null;
        }

        #region generic

        public void WriteGeneric<T>(T data) where T : ISerialize
        {
            this.Write(data.Serialize());
        }

        public void WriteGenericList<T>(List<T> data) where T : ISerialize
        {
            if (data == null)
            {
                this.Writeshort(0);
            }
            else
            {
                this.Writeshort((short)data.Count);
                for (int i = 0; i < data.Count; ++i)
                {
                    this.Write(data[i].Serialize());
                }
            }
        }

        public List<T> ReadGenericList<T>() where T : IDeSerialize,new()
        {
            List<T> list = ListPool.TrySpawn<List<T>>();
            short cnt = (short)this;
            for (int i = 0; i < cnt; ++i)
            {
                list.Add(ReadGeneric<T>());
            }

            return list;
        }

        public T ReadGeneric<T>() where T:IDeSerialize,new()
        {
            T data = new T();
            data.DeSerialize(this);
            return data;
        }
        #endregion
        public short Readshort()
        {
            return (short)this;
        }

        
        public ushort Readushort()
        {
            return (ushort)this;
        }

        
        public uint Readuint()
        {
            return (uint)this;
        }

        
        public int Readint()
        {
            return (int)this;
        }

        
        public float Readfloat()
        {
            return (float)this;
        }

        
        public double Readdouble()
        {
            return (double)this;
        }

        
        public long Readlong()
        {
            return (long)this;
        }

        
        public ulong Readulong()
        {
            return (ulong)this;
        }

        
        public string Readstring()
        {
            return (string)this;
        }

        
        public List<int> ReadintList()
        {
            return (List<int>)this;
        }

        
        public List<uint> ReaduintList()
        {
            return (List<uint>)this;
        }

        
        public List<short> ReadshortList()
        {
            return (List<short>)this;
        }

        
        public List<ushort> ReadushortList()
        {
            return (List<ushort>)this;
        }

        
        public List<long> ReadlongList()
        {
            return (List<long>)this;
        }

        
        public List<ulong> ReadulongList()
        {
            return (List<ulong>)this;
        }

        public List<float> ReadfloatList()
        {
            return (List<float>)this;
        }

        public List<double> ReaddoubleList()
        {
            return (List<double>)this;
        }

        public List<string> ReadstringList()
        {
            return (List<string>)this;
        }


        public List<bool> ReadboolList()
        {
            return (List<bool>)this;
        }

        public void Writeshort(short value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        public void Writeushort(ushort value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void Writeint(int value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void Writeuint(uint value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void Writelong(long value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }
        
        public void Writeulong(ulong value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void Writebool(bool value)
        {
            byte[] bys = new byte[1];
            bys[0] = value ? (byte)1 : (byte)0;

            this.Write(bys);
        }
        
        public void Writestring(string value)
        {
            byte[] bys = System.Text.Encoding.UTF8.GetBytes(value);
            this.Writeshort((short)bys.Length);

            this.Write(bys);
        }
        
        public void Writefloat(float value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        
        public void Writedouble(double value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        public void WritedoubleList(List<double> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writedouble(value[i]);
            }
        }

        public void WritefloatList(List<float> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writefloat(value[i]);
            }
        }

        public void WriteshortList(List<short> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writeshort(value[i]);
            }
        }

        
        public void WriteushortList(List<ushort> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writeushort(value[i]);
            }
        }

        
        public void WriteintList(List<int> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writeint(value[i]);
            }
        }

        
        public void WriteuintList(List<uint> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writeuint(value[i]);
            }
        }

        
        public void WriteulongList(List<ulong> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writeulong(value[i]);
            }
        }

        
        public void WritelongList(List<long> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writelong(value[i]);
            }
        }
        
        public void WritestringList(List<string> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writestring(value[i]);
            }
        }

        public void WriteboolList(List<bool> value)
        {
            this.Writeshort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.Writebool(value[i]);
            }
        }

    }
}


