using System;
using System.Collections.Generic;
using System.Text;
//using System.Threading.Tasks;
namespace KUtils
{
    public static class ToolsExtension
    {


        public static bool TryAdd<T>(this List<T> list,T data)
        {
            if (list.Contains(data) )
            {
                return false;
            }
            list.Add(data);
            return true;
        }


        public static bool FloatEqual(this float tf, float other)
        {
            if (Math.Abs(tf - other) < 0.0001f)
            {
                return true;
            }
            return false;
        }

        public static bool FloatLessEqual(this float tf, float other)
        {
            return tf.FloatEqual(other) || tf < other;
        }

        public static bool FloatLargetEqual(this float tf, float other)
        {
            return tf.FloatEqual(other) || tf > other;
        }

        public static bool FloatEqual(this float? tf, float other)
        {
            if(!tf.HasValue)
                return false;

            if (Math.Abs(tf.Value - other) < 0.0001f)
            {
                return true;
            }
            return false;
        }

        public static bool FloatEqual(this float? tf, float? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;
            
            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;

            if (Math.Abs(tf.Value - other.Value) < 0.0001f)
            {
                return true;
            }
            return false;
        }

        public static bool FloatLessEqual(this float? tf, float other)
        {
            if(!tf.HasValue)
                return false;
            return tf.FloatEqual(other) || tf.Value < other;
        }

        public static bool FloatLargetEqual(this float? tf, float other)
        {
            if(!tf.HasValue)
                return false;
            return tf.FloatEqual(other) || tf.Value > other;
        }

        public static bool FloatLessEqual(this float? tf, float? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;
            return tf.FloatEqual(other) || tf.Value < other;
        }

        public static bool FloatLargetEqual(this float? tf, float? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;
            return tf.FloatEqual(other) || tf.Value > other;
        }


        public static bool DoubleLessEqual(this double tf, double other)
        {
            return tf.DoubleEqual(other) || tf < other;
        }

        public static bool DoubleLargetEqual(this double tf, double other)
        {
            return tf.DoubleEqual(other) || tf > other;
        }

        public static bool DoubleEqual(this double tf, double other)
        {
            if (Math.Abs(tf - other) < 0.0001d)
            {
                return true;
            }
            return false;
        }

        public static bool DoubleLessEqual(this double? tf, double other)
        {
            if(!tf.HasValue )
                return false;

            return tf.DoubleEqual(other) || tf < other;
        }

        public static bool DoubleLargetEqual(this double? tf, double other)
        {
            if(!tf.HasValue )
                return false;

            return tf.DoubleEqual(other) || tf > other;
        }

        public static bool DoubleEqual(this double? tf, double other)
        {
            if(!tf.HasValue )
                return false;

            if (Math.Abs(tf.Value - other) < 0.0001d)
            {
                return true;
            }
            return false;
        }

        public static bool DoubleLessEqual(this double? tf, double? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;

            return tf.DoubleEqual(other) || tf < other;
        }

        public static bool DoubleLargetEqual(this double? tf, double? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;

            return tf.DoubleEqual(other) || tf > other;
        }

        public static bool DoubleEqual(this double? tf, double? other)
        {
            if(!tf.HasValue && other.HasValue)
                return false;

            if(tf.HasValue && !other.HasValue)
                return false;

            if(!tf.HasValue && !other.HasValue)
                return true;

            if (Math.Abs(tf.Value - other.Value) < 0.0001d)
            {
                return true;
            }
            return false;
        }

    }
}

