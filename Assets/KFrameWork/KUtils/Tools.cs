using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace KUtils
{
    public class Tools 
    {

        private const float kb = 1024;
        private const float mb = 1048576;
        private const float gb = 1073741824;

        private Stopwatch _w;
        private Stopwatch watch
        {
            get
            {
                if (_w == null)
                    _w = Stopwatch.StartNew();
                return _w;
            }
        }

        /// <summary>
        /// tick = 100 毫微秒  10^-7
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static TimeSpan MillsecdstoTime(long time)
        {
            return TimeSpan.FromMilliseconds(time);
        }
        /// <summary>
        /// 秒数
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static TimeSpan SecondsToTime(long time)
        {
            return TimeSpan.FromSeconds(time);
        }
            

        public static long RunnedTimetoLong()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)( DateTime.Now -startTime).TotalMilliseconds;
        }

        public static void Swap<T>(ref T t ,ref T r) where T:struct
        {
            T temp = t;
            t= r;
            r = temp;
        }

        public static float ByteToMB(long bdata)
        {
            return bdata / mb;
        }

        public static float ByteToGB(long bdata)
        {
            return bdata / gb;
        }
        /// <summary>
        /// 浅拷贝,反射版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origion"></param>
        /// <returns></returns>
        public static T Copy<T>(T origion) where T:class
        {
            Type tp = origion.GetType();

            MethodInfo method = tp.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                return (T)method.Invoke(origion, null);
            }
            return null;
        }
        /// <summary>
        /// 深拷贝
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origion"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T origion) where T : class
        {
            object retval = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();

                bf.Serialize(ms, origion);
                ms.Seek(0, SeekOrigin.Begin);

                retval = bf.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }

        /// <summary>
        /// 反射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static long CalculateCostTime<T>(T t, string name, params object[] objs)
        {
            Type tp = typeof(T);
            var methodinfo = tp.GetMethod(name);
            var watch = Stopwatch.StartNew();
            methodinfo.Invoke(t, objs);
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }


        public void StartTimer()
        {
            watch.Reset();
            watch.Start();
        }

        public long EndTimer()
        {
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        public TimeSpan EndTimerWithSpan()
        {
            watch.Stop();
            return watch.Elapsed;
        }

    }
}


