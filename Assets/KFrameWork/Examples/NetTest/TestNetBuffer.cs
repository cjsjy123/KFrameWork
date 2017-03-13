using UnityEngine;
using System.Collections;
using KFrameWork;
using System;
using KUtils;
#if EXAMPLE
public class TestNetBuffer : UnityMonoBehaviour {

    NetByteBuffer buffer = NetByteBuffer.CreateWithSize(128);

    int intv = 12;
    short shortv = 13;
    long longv = 14;
    ushort ushorv = 15;
    ulong ulongv = 16;
    bool boolv = true;
    byte bytev = 3;
    float floatv = 34.123f;
    double doublev = 124.123d;
    string stringv = "你好 Kframework";

    // Use this for initialization
    protected override  void Start () {
        base.Start();
        TestNet();

        this.TestPool();
	}

    void TestNet()
    {
        float current = Time.realtimeSinceStartup;
        for(int i =0; i <1000;++i)
        {
            this.TestWrite();
        }

        float end = Time.realtimeSinceStartup;

        LogMgr.LogFormat("Write 1000 times : {0}" ,(end-current));

        LogMgr.LogFormat("Buffer size = {0} ", buffer.DataCount);

        current = Time.realtimeSinceStartup;
        for(int i =0; i <1000;++i)
        {
            this.TestRead();
        }

        end = Time.realtimeSinceStartup;

        LogMgr.LogFormat("read 1000 times : {0}",(end-current));
    }


    void TestPool()
    {
        buffer.Dispose();

//        var b1 = NetByteBuffer.Create(32);
//        var b2 = NetByteBuffer.Create(64);
//        b1.Dispose();
//        b2.Dispose();
//
//        var b3 = NetByteBuffer.Create(40);
//
//        int b1code = b1.GetHashCode();
//        int b2code = b2.GetHashCode();
//        int b3code = b3.GetHashCode();
//
//        buffer = b3;

        TestNet();

    }

    private void TestWrite()
    {
        buffer+= intv;
        buffer += shortv;
        buffer += longv;
        buffer += ushorv;
        buffer += boolv;
        buffer+= ulongv;
        buffer+= bytev;
        buffer += floatv;
        buffer+= doublev;
        buffer+=  stringv;

    }

    private void TestRead()
    {
        int ret_int =(int)buffer;
        short ret_short =(short)buffer;
        long ret_long =(long)buffer;
        ushort ret_ushort =(ushort)buffer;
        bool ret_bool = (bool)buffer;
        ulong ret_ulong =(ulong)buffer;
        byte ret_byte = (byte)buffer;
        float ret_float =(float)buffer;
        double ret_double =(double)buffer;
        string ret_string =(string)buffer;

        if(ret_int == this.intv
            && ret_short == this.shortv
            && ret_long == this.longv
            && ret_ushort == this.ushorv
            && ret_bool == this.boolv
            && ret_ulong == this.ulongv
            && ret_byte == this.bytev
            && ret_float == this.floatv
            && ret_double == this.doublev
            && ret_string == this.stringv)
        {
            LogMgr.Log("结果正确");
        }
        else
        {
            LogMgr.LogError("结果错误");
        }

    }
	

}
#endif