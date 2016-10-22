using UnityEngine;
using System.Collections;
using System.IO;
using System;
using KFrameWork;

public class TestProtobuf : UnityMonoBehaviour
{

  // Use this for initialization
  protected override void Start ()
  {
      base.Start();
//    Test(Test1);
//    Test(Test2);

  }

  private void Test(Action callback)
  {
    float start = Time.realtimeSinceStartup;
    callback();

    float end= Time.realtimeSinceStartup;
    LogMgr.Log("cost "+ (end-start));
  }

//  void Test1()
//  {
//    float f = 1.23456f;
//    var bs = BitConverter.GetBytes(f);
//
//  }
//
//  void Test2()
//  {
//    float myFloat = 1.23456f;
//
//    var bytes = new byte[4];
//
//    bytes [0] = (byte)myFloat;
//    bytes [1] = (byte)(myFloat >> 8);
//    bytes [2] = (byte)(myFloat >> 16);
//    bytes [3] = (byte)(myFloat >> 24);
//
//  }


}
