using System;
using System.Collections.Generic;
namespace KUtils
{
    public class KEnum:IDisposable
    {
        protected string _strkey;
        protected int _intkey;

        /// <summary>
        /// always the max value for unique value
        /// </summary>
        private static int _counter =-1;

        private static List<KEnum> enumlist = new List<KEnum> ();

        public KEnum(int val):this(null,val){}
        public KEnum(string val):this(val,++_counter){}


        public KEnum(string strval,int val)
        {
            _intkey = val;
            _strkey = strval;

            bool exist =false;

            for(int i =0; i < enumlist.Count;++i)
            {
                KEnum e = enumlist[i];
                if(e._intkey == val || (e._strkey == strval) && e._strkey != null )
                {
                    e._intkey = val;
                    e._strkey = strval;
                    exist = true;
                    break;
                }
            }

            if(!exist)
                enumlist.Add(this);
        }

        public static explicit operator KEnum(int val)
        {
            for(int i =0; i < enumlist.Count;++i)
            {
                KEnum e = enumlist[i];
                if(e._intkey == val)
                {
                    return e;
                }
            }

            KEnum ev = new KEnum(val);

            _counter = Math.Max(val,_counter);
            _counter++;
            return ev;

        }

        public static explicit operator KEnum(string val)
        {
            for(int i =0; i < enumlist.Count;++i)
            {
                KEnum e = enumlist[i];
                if(e._strkey == val)
                {
                    return e;
                }
            }

            KEnum ev = new KEnum(val);

            return ev;

        }

        public static implicit operator int(KEnum ke)
        {
            if(ke != null)
            {
                return ke._intkey;
            }
            throw new NullReferenceException("enum is Null");
        }

        public static implicit operator string(KEnum ke)
        {
            if(ke != null)
            {
                return ke.ToString();
            }
            throw new NullReferenceException("enum is Null");
        }

        public static bool operator  ==(KEnum lft,KEnum rht)
        {
            bool lft_null = object.ReferenceEquals(lft,null);
            bool rht_null = object.ReferenceEquals(rht,null);
            if(lft_null == rht_null && lft_null)
            {
                return true;
            }
            else if(lft_null || rht_null)
                return false;

            return lft.Equals(rht);
        }

        public static bool operator  !=(KEnum lft,KEnum rht)
        {
            bool lft_null = object.ReferenceEquals(lft,null);
            bool rht_null = object.ReferenceEquals(rht,null);
            if(lft_null == rht_null && lft_null)
            {
                return false;
            }
            else if(lft_null || rht_null)
                return true;

            return !lft.Equals(rht);
        }

        public override bool Equals(object em)
        {
            if(em == null) return false;

            if( em is KEnum)
            {
                KEnum k = em as KEnum;
                if(k._intkey != this._intkey) return false;
                if(k._strkey != this._strkey ) return false;

                return true;
            }

            return false;

        }

        public override string ToString ()
        {
            if(this._strkey != null)
                return this._strkey;
            else
                return this._intkey.ToString();
        }

        public override int GetHashCode ()
        {
            return this._intkey;
        }

        public void Dispose()
        {
            enumlist.Remove(this);
        }

    }  
}



