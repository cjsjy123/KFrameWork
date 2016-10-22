#define FLIP
#define KEEP_OBS
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
        private int byteAviliable
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

        private static int _SeekFunc(NetByteBuffer buffer,int target)
        {
            if(buffer == null)
                return -1000000;
            else
            {
                return buffer.capacity - target;
            }
        }

        public static NetByteBuffer Create(int size)
        {
            NetByteBuffer buffer =  null;
            if(KObjectPool.mIns != null)
            {
                buffer = KObjectPool.mIns.Seek<NetByteBuffer,int>(_SeekFunc,size,5);
            }

            if(buffer == null)
            {
                buffer = new NetByteBuffer(size);
            }
            else
            {
                buffer._Reset();
            }
            return buffer;
        }

        public static NetByteBuffer Create<T>() where T:struct
        {
            int size = Marshal.SizeOf(typeof(T));
            return Create(size);
        }

        public static NetByteBuffer Create(byte[] bys)
        {
            NetByteBuffer buffer =  null;
            if(KObjectPool.mIns != null)
            {
                int byslen =0;
                if(bys  != null)
                    byslen = bys.Length;

                buffer = KObjectPool.mIns.Seek<NetByteBuffer,int>(_SeekFunc,byslen,5);
            }

            if(buffer == null)
            {
                buffer = new NetByteBuffer(bys);
            }
            else
            {
                buffer._Reset();
            }
            return buffer;
        }

        public void AwakeFromPool ()
        {
            this._Reset();
        }

        public void ReleaseToPool ()
        {

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
            #if FILP
            if (!BitConverter.IsLittleEndian)
                _MyReverse(right);
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
                int strLen = (short)left;
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
                int strLen = (short)left;
                if (strLen >0)
                {
                    List<int> list = new List<int>();
                    for (int i = 0; i < strLen; ++i)
                    {
                        list.Add((int)left);
                    }
                    return list;
                }
            }
            LogMgr.LogError("List<int> Read from ByteBuffer Error");
            return null;
        }

        public static explicit operator List<short>(NetByteBuffer left)
        {
            if (left != null && left.DataCount >= 2)
            {
                int strLen = (short)left;
                if (strLen >0)
                {
                    List<short> list = new List<short>();
                    for (int i = 0; i < strLen; ++i)
                    {
                        list.Add((short)left);
                    }
                    return list;
                }
            }
            LogMgr.LogError("List<short> Read from ByteBuffer Error");

            return null;
        }

        public static explicit operator List<long>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                int strLen = (short)left;
                if (strLen >0)
                {
                    List<long> list = new List<long>();
                    for (int i = 0; i < strLen; ++i)
                    {
                        list.Add((long)left);
                    }
                    return list;
                }
            }
            LogMgr.LogError("List<long> Read from ByteBuffer Error");

            return null;
        }

        public static explicit operator List<uint>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                int strLen = (short)left;
                if (strLen >0)
                {
                    List<uint> list = new List<uint>();
                    for (int i = 0; i < strLen; ++i)
                    {
                        list.Add((uint)left);
                    }
                    return list;
                }
            }
            LogMgr.LogError("List<uint> Read from ByteBuffer Error");
            return null;
        }

        public static explicit operator List<ushort>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                int strLen = (short)left;
                if (strLen >0)
                {
                    List<ushort> list = new List<ushort>();
                    for (int i = 0; i < strLen; ++i)
                    {
                        list.Add((ushort)left);
                    }
                    return list;
                }
            }
            LogMgr.LogError("List<ushort> Read from ByteBuffer Error");
 
            return null;
        }

        public static explicit operator List<ulong>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                int strLen = (short)left;
                if (strLen >0)
                {
                    List<ulong> list = new List<ulong>();
                    for (int i = 0; i < strLen; ++i)
                    {
                        list.Add((ulong)left);
                    }
                    return list;
                }            
            }
            LogMgr.LogError("List<ulong> Read from ByteBuffer Error");

            return null;
        }

        public static explicit operator List<string>(NetByteBuffer left)
        {
            if (left != null && left.byteAviliable >= 2)
            {
                int strLen = (short)left;
                if (strLen >0)
                {
                    List<string> list = new List<string>();
                    for (int i = 0; i < strLen; ++i)
                    {
                        list.Add((string)left);
                    }
                    return list;
                }     
            }
            LogMgr.LogError("List<string>  Read from ByteBuffer Error");

            return null;
        }
        #if KEEP_OBS
        [Obsolete]
        public short ReadShort()
        {
            return (short)this;
        }

        [Obsolete]
        public ushort ReadUShort()
        {
            return (ushort)this;
        }

        [Obsolete]
        public uint ReadUInt()
        {
            return (uint)this;
        }

        [Obsolete]
        public int ReadInt()
        {
            return (int)this;
        }

        [Obsolete]
        public float ReadFloat()
        {
            return (float)this;
        }

        [Obsolete]
        public double ReadDouble()
        {
            return (double)this;
        }

        [Obsolete]
        public long ReadLong()
        {
            return (long)this;
        }

        [Obsolete]
        public ulong ReadULong()
        {
            return (ulong)this;
        }

        [Obsolete]
        public string ReadString()
        {
            return (string)this;
        }

        [Obsolete]
        public List<int> ReadIntList()
        {
            return (List<int>)this;
        }

        [Obsolete]
        public List<uint> ReadUIntList()
        {
            return (List<uint>)this;
        }

        [Obsolete]
        public List<short> ReadShortList()
        {
            return (List<short>)this;
        }

        [Obsolete]
        public List<ushort> ReadUShortList()
        {
            return (List<ushort>)this;
        }

        [Obsolete]
        public List<long> ReadLongList()
        {
            return (List<long>)this;
        }

        [Obsolete]
        public List<ulong> ReadULongList()
        {
            return (List<ulong>)this;
        }

        [Obsolete]
        public List<string> ReadStringList()
        {
            return (List<string>)this;
        }


        [Obsolete]
        public void WriteShort(short value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        [Obsolete]
        public void WriteUShort(ushort value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        [Obsolete]
        public void WriteInt(int value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        [Obsolete]
        public void WriteUint(uint value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        [Obsolete]
        public void WriteLong(long value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        [Obsolete]
        public void WriteUlong(ulong value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        [Obsolete]
        public void WriteBool(bool value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }
        [Obsolete]
        public void WriteString(string value)
        {
            byte[] bys = System.Text.Encoding.UTF8.GetBytes(value);
            this.WriteShort((short)bys.Length);

            _Reverse(bys);
            this.Write(bys);
        }
        [Obsolete]
        public void WriteFloat(float value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        [Obsolete]
        public void WriteDouble(double value)
        {
            byte[] bys = BitConverter.GetBytes(value);
            _Reverse(bys);
            this.Write(bys);
        }

        [Obsolete]
        public void WriteShortList(List<short> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteShort(value[i]);
            }
        }

        [Obsolete]
        public void WriteUShortList(List<ushort> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteUShort(value[i]);
            }
        }

        [Obsolete]
        public void WriteIntList(List<int> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteInt(value[i]);
            }
        }

        [Obsolete]
        public void WriteUintList(List<uint> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteUint(value[i]);
            }
        }

        [Obsolete]
        public void WriteUlongList(List<ulong> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteUlong(value[i]);
            }
        }

        [Obsolete]
        public void WriteLongList(List<long> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteLong(value[i]);
            }
        }
        [Obsolete]
        public void WriteStringList(List<string> value)
        {
            this.WriteShort((short)value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                this.WriteString(value[i]);
            }
        }

        #endif

    }
}


