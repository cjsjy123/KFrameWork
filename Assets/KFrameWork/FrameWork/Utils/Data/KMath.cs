//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace KFrameWork
//{

//    public class KMath
//    {

//        public static int isqrt(long x)
//        {

//            long remainder = x;

//            long place = 1 << 30;//4 * 8 - 2

//            while (place > remainder) { place /= 4; }

//            long root = 0;
//            while (place != 0)
//            {
//                if (remainder >= root + place)
//                {
//                    remainder -= root + place;
//                    root += place * 2;
//                }
//                root /= 2;
//                place /= 4;
//            }

//            if(x < 0)
//            {
//                root = -root;
//            }

//            return (int)root;

//            //if (x < 0) return -1;
//            //int x1 = x;
//            //int left = 0;
//            //int right = x1;
//            //int mid = 0;
//            //while (left <= right)
//            //{
//            //    mid = left + (right - left) / 2;
//            //    if (mid * mid == x1 || (mid * mid < x1 && (mid + 1) * (mid + 1) > x1)) return mid;
//            //    else if (mid * mid < x1) left = mid + 1;
//            //    else right = mid - 1;
//            //}



//            //int rem = 0;
//            //int root = 0;
//            //int i;

//            //for (i = 0; i < 16; i++)
//            //{
//            //    root <<= 1;
//            //    rem <<= 2;
//            //    rem += a >> 30;
//            //    a <<= 2;

//            //    if (root < rem)
//            //    {
//            //        root++;
//            //        rem -= root;
//            //        root++;
//            //    }
//            //}

//            //return (root >> 1);
//        }

//    }
//}

