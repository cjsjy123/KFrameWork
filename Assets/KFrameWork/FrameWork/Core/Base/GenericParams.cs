using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using KUtils;


namespace KFrameWork
{
   

    public sealed class GenericParams:AbstractParams,IPool  {

        private sealed class ParamsList<T> 
        {
            private List<T> _list ;

            private int ReadCount =0;

            public ParamsList()
            {
                _list = new List<T>(4);
            }

            public int Count
            {
                get
                {
                    if(_list != null)
                        return _list.Count;
                    return 0;
                }

            }

            public T this[int index]
            {
                get
                {
                    return _list[index];
                }
                set
                {
                    _list[index] =value;
                }
            }

            public T Pop()
            {
                if(_list.Count > ReadCount)
                {
                    return _list[ReadCount++];
                }
                throw new System.ArgumentOutOfRangeException();
            }

            public void Reset()
            {
                ReadCount =0;
            }

            public void Clear()
            {
                this._list.Clear();
            }

            public void Insert(int index ,T item)
            {
                _list.Insert(index,item);
            }


            public void Add(T item)
            {
                _list.Add(item);
            }

            public void Remove(T item)
            {
                _list.Remove(item);
            }


        }

        private ParamsList<int> intList ;
        private ParamsList<short> shortlist;
        private ParamsList<bool> boollist;
        private ParamsList<string> strlist;
        private ParamsList<long> longList;
        private ParamsList<float> floatList;
        private ParamsList<double> doubleList;
        private ParamsList<Vector3> vector3List;
        private ParamsList<System.Object> objList;
        private ParamsList<UnityEngine.Object> UnityObjList;

        private List<KeyValuePair<int,int>> ArgSortList = new List<KeyValuePair<int,int>>();

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

        private static GenericParams PreCreate(int origion =-1)
        {
            GenericParams p = new GenericParams();
            p._OrigionArgCount = origion;
            return p;
        }

        public static GenericParams Create(int origion =-1)
        {
            GenericParams p = null;
            if(KObjectPool.mIns == null)
            {
                p = new GenericParams();
            }
            else
            {
                p=  KObjectPool.mIns.Pop<GenericParams>();
            }

            if(p == null)
            {
                p = new GenericParams();
            }

            if(origion != -1)
            {
                p._OrigionArgCount = origion;
            }

            return p;
        }

        public override void Release ()
        {
            if(KObjectPool.mIns != null)
                KObjectPool.mIns.Push(this);
        }

        public void AwakeFromPool ()
        {
            if(intList != null)
            {
                intList.Reset();
                intList.Clear();
            }


            if(shortlist != null)
            {
                shortlist.Reset();
                shortlist.Clear();
            }


            if(boollist != null)
            {
                boollist.Reset();
                boollist.Clear();
            }


            if(strlist != null)
            {
                strlist.Reset();
                strlist.Clear();
            }


            if(longList != null)
            {
                longList.Reset();
                longList.Clear();
            }


            if(objList != null)
            {
                objList.Reset();
                objList.Clear();
            }

            if(UnityObjList != null)
            {
                UnityObjList.Reset();
                UnityObjList.Clear();
            }

            if(floatList != null)
            {
                floatList.Reset();
                floatList.Clear();
            }

            if(doubleList!= null)
            {
                doubleList.Reset();
                doubleList.Clear();
            }

            ArgSortList.Clear();
            _ArgCount =0;
            _virtualArg =0;
            _OrigionArgCount =-1;
            _NextReadIndex =0;
        }


        public void RemovedFromPool ()
        {
            intList = null;
            shortlist = null;
            boollist = null;
            longList = null;
            strlist = null;
            objList = null;
            UnityObjList = null;
            floatList = null;
            doubleList = null;

            ArgSortList.Clear();
            ArgSortList = null;
            _ArgCount =0;
            _virtualArg =0;
            _OrigionArgCount =-1;
            _NextReadIndex =0;
        }

        public override void InsertInt (int index, int v)
        {
            if(intList == null)
                intList=  new ParamsList<int>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                intList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.INT,intList.Count-1));
            }
            else
            {
                _ArgCount++;
                intList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.INT,intList.Count-1));
            }
        }

        public override void InsertShort (int index, short v)
        {
            if(this.shortlist == null)
                this.shortlist=  new ParamsList<short>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                shortlist.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.SHORT,shortlist.Count-1));
            }
            else
            {
                _ArgCount++;
                shortlist.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.SHORT,shortlist.Count-1));
            }
        }

        public override void InsertString (int index, string v)
        {
            if(this.strlist == null)
                strlist=  new ParamsList<string>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                strlist.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.STRING,strlist.Count-1));
            }
            else
            {  
                _ArgCount++;
                strlist.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.STRING,strlist.Count-1));
            }
        }

        public override void InsertBool (int index, bool v)
        {
            if(this.boollist == null)
                boollist=  new ParamsList<bool>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                boollist.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.BOOL,boollist.Count-1));
            }
            else
            {
                _ArgCount++;
                boollist.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.BOOL,boollist.Count-1));
            }
        }

        public override void InsertFloat (int index, float v)
        {
            if(floatList == null)
                floatList=  new ParamsList<float>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                floatList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.FLOAT,floatList.Count-1));
            }
            else
            {   
                _ArgCount++;
                floatList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.FLOAT,floatList.Count-1));
            }
        }

        public override void InsertDouble (int index, double v)
        {
            if(this.doubleList == null)
                this.doubleList=  new ParamsList<double>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                doubleList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.DOUBLE,doubleList.Count-1));
            }
            else
            {
                _ArgCount++;
                doubleList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.DOUBLE,doubleList.Count-1));
            }
        }

        public override void InsertLong (int index, long v)
        {
            if(this.longList == null)
                longList=  new ParamsList<long>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                longList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.LONG,longList.Count-1));
            }
            else
            {
                _ArgCount++;
                longList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.LONG,longList.Count-1));
            }
        }

        public override void InsertVector3(int index, UnityEngine.Vector3 v)
        {
            if (this.vector3List == null)
                vector3List = new ParamsList<Vector3>();
            if (this._OrigionArgCount >= 0)
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                vector3List.Insert(index, v);
                ArgSortList.Insert(index, new KeyValuePair<int, int>((int)ParamType.VETOR3, vector3List.Count - 1));
            }
            else
            {
                _ArgCount++;
                vector3List.Insert(index, v);
                ArgSortList.Insert(index, new KeyValuePair<int, int>((int)ParamType.VETOR3, vector3List.Count - 1));
            }
        }

        public override void InsertObject (int index, object v)
        {
            if(this.objList == null)
                objList=  new ParamsList<object>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                objList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.OBJECT,objList.Count-1));
            }
            else
            {
                _ArgCount++;
                objList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.OBJECT,objList.Count-1));
            }
        }

        public override void InsertUnityObject (int index, UnityEngine.Object v)
        {
            if(this.UnityObjList == null)
                UnityObjList=  new ParamsList<UnityEngine.Object>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                _OrigionArgCount++;
                UnityObjList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.UNITYOBJECT,UnityObjList.Count-1));
            }
            else
            {
                _ArgCount++;
                UnityObjList.Insert(index,v);
                ArgSortList.Insert(index,new KeyValuePair<int, int>((int)ParamType.UNITYOBJECT,UnityObjList.Count-1));
            }
        }
     
        public override void WriteInt(int v)
        {
            if(intList == null)
                intList=  new ParamsList<int>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                if(this._OrigionArgCount <_ArgCount)
                {
                    _ArgCount =this._OrigionArgCount;
                    this._VirtualsetInt(v);

                }
                else
                {
                    intList.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.INT,intList.Count-1));
                }
            }
            else
            {
                intList.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.INT,intList.Count-1));
            }
        }

        public override void WriteShort(short v)
        {
            if(shortlist == null)
                shortlist=  new ParamsList<short>();
            
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                if(this._OrigionArgCount <_ArgCount)
                {
                    _ArgCount =this._OrigionArgCount;
                    this._VirtualsetShort(v);
                }
                else
                {
                    shortlist.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.SHORT,shortlist.Count-1));
                }
            }
            else
            {
                shortlist.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.SHORT,shortlist.Count-1));
            }
        }

        public override void WriteBool(bool v)
        {
            if(boollist == null)
                boollist=  new ParamsList<bool>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                if(this._OrigionArgCount <_ArgCount)
                {
                    _ArgCount =this._OrigionArgCount;
                    this._VirtualsetBool(v);
                }
                else
                {
                    boollist.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.BOOL,boollist.Count-1));
                }
            }
            else
            {
                boollist.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.BOOL,boollist.Count-1));
            }
        }

        public override void WriteString(string v)
        {
            if(strlist == null)
                strlist=  new ParamsList<string>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                if(this._OrigionArgCount <_ArgCount)
                {
                    _ArgCount =this._OrigionArgCount;
                    this._VirtualsetString(v);
                }
                else
                {
                    strlist.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.STRING,strlist.Count-1));
                }
            }
            else
            {
                strlist.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.STRING,strlist.Count-1));
            }
        }

        public override void WriteLong(long v)
        {
            if(longList == null)
                longList=  new ParamsList<long>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                if(this._OrigionArgCount <_ArgCount)
                {
                    _ArgCount =this._OrigionArgCount;
                    this._VirtualsetLong(v);
                }
                else
                {
                    longList.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.LONG,longList.Count-1));
                }
            }
            else
            {
                longList.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.LONG,longList.Count-1));
            }
        }

        public override void WriteVector3(Vector3 v)
        {
            if (vector3List == null)
                vector3List = new ParamsList<Vector3>();
            if (this._OrigionArgCount >= 0)
            {
                _ArgCount++;
                _virtualArg++;
                if (this._OrigionArgCount < _ArgCount)
                {
                    _ArgCount = this._OrigionArgCount;
                    this._VirtualsetVector3(v);
                }
                else
                {
                    vector3List.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.VETOR3, vector3List.Count - 1));
                }
            }
            else
            {
                vector3List.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.VETOR3, vector3List.Count - 1));
            }
        }

        public override void WriteObject(System.Object v)
        {
            if(objList == null)
                objList=  new ParamsList<System.Object>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                if(this._OrigionArgCount <_ArgCount)
                {
                    _ArgCount =this._OrigionArgCount;
                    this._VirtualsetObject(v);
                }
                else
                {
                    objList.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.OBJECT,objList.Count-1));
                }
            }
            else
            {
                objList.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.OBJECT,objList.Count-1));
            }
        }

        public override void WriteUnityObject(UnityEngine.Object v)
        {
            if(UnityObjList == null)
                UnityObjList=  new ParamsList<UnityEngine.Object>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                if(this._OrigionArgCount <_ArgCount)
                {
                    _ArgCount =this._OrigionArgCount;
                    this._VirtualsetUnityObj(v);
                }
                else
                {
                    UnityObjList.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.UNITYOBJECT,UnityObjList.Count-1));
                }
            }
            else
            {
                UnityObjList.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.UNITYOBJECT,UnityObjList.Count-1));
            }
        }

        public override void WriteDouble(double v)
        {
            if(this.doubleList == null)
                this.doubleList =  new ParamsList<double>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                if(this._OrigionArgCount <_ArgCount)
                {
                    _ArgCount =this._OrigionArgCount;
                    this._VirtualsetUnityObj(v);
                }
                else
                {
                    doubleList.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.DOUBLE,doubleList.Count-1));
                }
            }
            else
            {
                doubleList.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.DOUBLE,doubleList.Count-1));
            }
        }

        public override void WriteFloat(float v)
        {
            if(this.floatList == null)
                this.floatList=  new ParamsList<float>();
            if(this._OrigionArgCount >= 0 )
            {
                _ArgCount++;
                _virtualArg++;
                if(this._OrigionArgCount <_ArgCount)
                {
                    _ArgCount =this._OrigionArgCount;
                    this._VirtualsetUnityObj(v);
                }
                else
                {
                    floatList.Add(v);
                    ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.FLOAT,floatList.Count-1));
                }
            }
            else
            {
                floatList.Add(v);
                _ArgCount++;
                ArgSortList.Add(new KeyValuePair<int, int>((int)ParamType.FLOAT,floatList.Count-1));
            }
        }

        private void _VirtualsetShort(short v)
        {
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.SHORT)
            {
                shortlist[argTp.Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非Short类型 ");
            }
        }

        private void _VirtualsetBool(bool v)
        {
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.BOOL)
            {
                boollist[argTp.Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非bool类型 ");
            }
        }

        private void _VirtualsetInt(int v)
        {
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.INT)
            {
                intList[ArgSortList[argIndex].Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非Int类型 ");
            }
        }

        private void _VirtualsetString(string v)
        {
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.STRING)
            {
                strlist[argTp.Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非string类型 ");
            }
        }

        private void _VirtualsetLong(long v)
        {
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.LONG)
            {
                longList[argTp.Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非Long类型 ");
            }
        }

        private void _VirtualsetVector3(Vector3 v){
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.VETOR3)
            {
                vector3List[argTp.Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非Vector3类型 ");
            }
        }

        private void _VirtualsetObject(System.Object v)
        {
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.OBJECT)
            {
                objList[argTp.Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非Object类型 ");
            }
        }

        private void _VirtualsetUnityObj(UnityEngine.Object v)
        {
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.UNITYOBJECT)
            {
                UnityObjList[argTp.Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非UnityObject类型 ");
            }
        }

        private void _VirtualsetUnityObj(float v)
        {
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.FLOAT)
            {
                this.floatList[argTp.Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非float类型 ");
            }
        }

        private void _VirtualsetUnityObj(double v)
        {
            int argIndex = (this._virtualArg-1) % this._OrigionArgCount;
            KeyValuePair<int,int> argTp = this.ArgSortList[argIndex];

            if(argTp.Key == (int)ParamType.DOUBLE)
            {
                this.doubleList[argTp.Value] = v;
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非double类型 ");
            }
        }

        public override void SetInt(int argIndex,int v)
        {
            if(argIndex < ArgSortList.Count)
            {
                if(ArgSortList[argIndex].Key == (int)ParamType.INT)
                {
                    if(intList == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else if(intList != null && ArgSortList[argIndex].Value >= intList.Count)
                    {
                        LogMgr.LogError("参数索引错误");
                    }
                    else
                    {
                        intList[ArgSortList[argIndex].Value] = v;
                    }
                }
                else
                {
                    LogMgr.LogError("参数类型错误 此为非Int类型 ");
                }
            }
            else
            {
                LogMgr.LogError("参数索引错误");
            }
        }

        public override void SetShort(int argIndex,short v)
        {
            if(argIndex < ArgSortList.Count)
            {
                if(ArgSortList[argIndex].Key == (int)ParamType.SHORT)
                {
                    if(shortlist == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else if(shortlist != null && ArgSortList[argIndex].Value >= shortlist.Count)
                    {
                        LogMgr.LogError("参数索引错误");
                    }
                    else
                    {
                        shortlist[ArgSortList[argIndex].Value] = v;
                    }
                }
                else
                {
                    LogMgr.LogError("参数类型错误 此为非short类型 ");
                }
            }
            else
            {
                LogMgr.LogError("参数索引错误");
            }
        }

        public override void SetBool(int argIndex,bool v)
        {
            if(this.GetArgIndexType(argIndex)== (int)ParamType.BOOL)
            {
                if(boollist == null)
                {
                    LogMgr.LogError("参数列表为空");
                }
                else if(boollist != null && ArgSortList[argIndex].Value >= boollist.Count)
                {
                    LogMgr.LogError("参数索引错误");
                }
                else
                {
                    boollist[ArgSortList[argIndex].Value] = v;
                }
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非bool类型 ");
            }
        }

        public override void SetString(int argIndex,string v)
        {
            if(this.GetArgIndexType(argIndex) ==(int)ParamType.STRING)
            {
                if(strlist == null)
                {
                    LogMgr.LogError("参数列表为空");
                }
                else if(strlist != null && ArgSortList[argIndex].Value >= strlist.Count)
                {
                    LogMgr.LogError("参数索引错误");
                }
                else
                {
                    strlist[ArgSortList[argIndex].Value] = v;
                }
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非string类型 ");
            }
        }

        public override void SetLong(int argIndex,long v)
        {
            if(this.GetArgIndexType(argIndex)== (int)ParamType.LONG)
            {
                if(longList == null)
                {
                    LogMgr.LogError("参数列表为空");
                }
                else if(longList != null && ArgSortList[argIndex].Value >= longList.Count)
                {
                    LogMgr.LogError("参数索引错误");
                }
                else
                {
                    longList[ArgSortList[argIndex].Value] = v;
                }
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非long类型 ");
            }
        }

        public override void SetVector3(int argIndex, Vector3 v)
        {
            if (this.GetArgIndexType(argIndex) == (int)ParamType.VETOR3)
            {
                if (objList == null)
                {
                    LogMgr.LogError("参数列表为空");
                }
                else if (vector3List != null && ArgSortList[argIndex].Value >= vector3List.Count)
                {
                    LogMgr.LogError("参数索引错误");
                }
                else
                {
                    vector3List[ArgSortList[argIndex].Value] = v;
                }
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非Vector3类型 ");
            }
        }

        public override void SetObject(int argIndex,object v)
        {
            if(this.GetArgIndexType(argIndex)== (int)ParamType.OBJECT)
            {
                if(objList == null)
                {
                    LogMgr.LogError("参数列表为空");
                }
                else if(objList != null && ArgSortList[argIndex].Value >= objList.Count)
                {
                    LogMgr.LogError("参数索引错误");
                }
                else
                {
                    objList[ArgSortList[argIndex].Value] = v;
                }
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非object类型 ");
            }
        }

        public override void SetUnityObject(int argIndex,UnityEngine.Object v)
        {
            if(this.GetArgIndexType(argIndex) == (int)ParamType.UNITYOBJECT)
            {
                if(UnityObjList == null)
                {
                    LogMgr.LogError("参数列表为空");
                }
                else if(UnityObjList != null && ArgSortList[argIndex].Value >= UnityObjList.Count)
                {
                    LogMgr.LogError("参数索引错误");
                }
                else
                {
                    UnityObjList[ArgSortList[argIndex].Value] = v;
                }
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非UnityObject类型 ");
            }
        }

        public override void SetFloat(int argIndex,float v)
        {
            if(this.GetArgIndexType(argIndex) == (int)ParamType.FLOAT)
            {
                if(this.floatList == null)
                {
                    LogMgr.LogError("参数列表为空");
                }
                else if(floatList != null && ArgSortList[argIndex].Value >= floatList.Count)
                {
                    LogMgr.LogError("参数索引错误");
                }
                else
                {
                    floatList[ArgSortList[argIndex].Value] = v;
                }
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非float类型 ");
            }
        }

        public override void SetDouble(int argIndex,double v)
        {
            if(this.GetArgIndexType(argIndex) == (int)ParamType.DOUBLE)
            {
                if(this.doubleList == null)
                {
                    LogMgr.LogError("参数列表为空");
                }
                else if(this.doubleList != null && ArgSortList[argIndex].Value >= this.doubleList.Count)
                {
                    LogMgr.LogError("参数索引错误");
                }
                else
                {
                    this.doubleList[ArgSortList[argIndex].Value] = v;
                }
            }
            else
            {
                LogMgr.LogError("参数类型错误 此为非UnityObject类型 ");
            }
        }

        private void _ResetAll()
        {
            if(intList != null)
                intList.Reset();

            if(shortlist != null)
                shortlist.Reset();

            if(boollist != null)
                boollist.Reset();

            if(strlist != null)
                strlist.Reset();

            if(longList != null)
                longList.Reset();

            if(objList != null)
                objList.Reset();

            if(UnityObjList != null)
                UnityObjList.Reset();

            _NextReadIndex =0;

        }

        private void _IncreateIndex()
        {
            _NextReadIndex++;
            _NextReadIndex = _NextReadIndex % _ArgCount;
            if(_NextReadIndex == 0) this._ResetAll();
        }

        public override void ResetReadIndex()
        {
            this._ResetAll();
        }


        public override int ReadInt()
        {
            if(_NextReadIndex < ArgSortList.Count)
            {
                if(ArgSortList[_NextReadIndex].Key == (int)ParamType.INT)
                {
                    if(intList == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }

                    return intList.Pop();
                }
                else
                {
                    LogMgr.LogError("参数类型不匹配");
                }
            }

            LogMgr.LogError("参数数量不足");
            return 0;
        }



        public override short ReadShort()
        {
            if(_NextReadIndex < ArgSortList.Count)
            {
                if(ArgSortList[_NextReadIndex].Key == (int)ParamType.SHORT)
                {
                    if(shortlist == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }
                    return shortlist.Pop();
                }

                LogMgr.LogError("参数类型不匹配");
            }
            else
            {
                LogMgr.LogError("参数数量不足");
            }
            return 0;
        }

        public override float ReadFloat()
        {
            if(_NextReadIndex < ArgSortList.Count)
            {
                if(ArgSortList[_NextReadIndex].Key == (int)ParamType.FLOAT)
                {
                    if(floatList == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }
                    return floatList.Pop();
                }

                LogMgr.LogError("参数类型不匹配");
            }
            else
            {
                LogMgr.LogError("参数数量不足");
            }
            return 0;
        }

        public override double ReadDouble()
        {
            if(_NextReadIndex < ArgSortList.Count)
            {
                if(ArgSortList[_NextReadIndex].Key == (int)ParamType.DOUBLE)
                {
                    if(doubleList == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }
                    return doubleList.Pop();
                }

                LogMgr.LogError("参数类型不匹配");
            }
            else
            {
                LogMgr.LogError("参数数量不足");
            }
            return 0;
        }

        public override bool ReadBool()
        {
            if(_NextReadIndex < ArgSortList.Count)
            {
                if(ArgSortList[_NextReadIndex].Key == (int)ParamType.BOOL)
                {
                    if(boollist == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }
                    return boollist.Pop();
                }

                LogMgr.LogError("参数类型不匹配");
            }
            else
            {
                LogMgr.LogError("参数数量不足");
            }
            return false;
        }

        public override string ReadString()
        {
            if(_NextReadIndex < ArgSortList.Count)
            {
                if(ArgSortList[_NextReadIndex].Key == (int)ParamType.STRING)
                {
                    if(strlist == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }
                    return strlist.Pop();
                }
                LogMgr.LogError("参数类型不匹配");
            }
            else
            {
                LogMgr.LogError("参数数量不足");
            }
            return "";
        }

        public override long ReadLong()
        {
            if(_NextReadIndex < ArgSortList.Count)
            {
                if(ArgSortList[_NextReadIndex].Key == (int)ParamType.LONG)
                {
                    if(longList == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }
                    return longList.Pop();
                }
                LogMgr.LogError("参数类型不匹配");
            }
            else
            {
                LogMgr.LogError("参数数量不足");
            }
            return 0;
        }

        public override Vector3 ReadVector3()
        {
            if (_NextReadIndex < ArgSortList.Count)
            {
                if (ArgSortList[_NextReadIndex].Key == (int)ParamType.VETOR3)
                {
                    if (vector3List == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }
                    return vector3List.Pop();
                }
                LogMgr.LogError("参数类型不匹配");
            }
            else
            {
                LogMgr.LogError("参数数量不足");
            }
            return Vector3.zero;
        }

        public override System.Object ReadObject()
        {
            if(_NextReadIndex < ArgSortList.Count)
            {
                if(ArgSortList[_NextReadIndex].Key == (int)ParamType.OBJECT)
                {
                    if(objList == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }
                    return objList.Pop();
                }
                LogMgr.LogError("参数类型不匹配");
            }
            else
            {
                LogMgr.LogError("参数数量不足");
            }
            return null;
        }

        public override UnityEngine.Object ReadUnityObject()
        {
            if(_NextReadIndex < ArgSortList.Count)
            {
                if(ArgSortList[_NextReadIndex].Key == (int)ParamType.UNITYOBJECT)
                {
                    if(UnityObjList == null)
                    {
                        LogMgr.LogError("参数列表为空");
                    }
                    else
                    {
                        this._IncreateIndex();
                    }
                    return UnityObjList.Pop();
                }
                LogMgr.LogError("参数类型不匹配");
            }
            else
            {
                LogMgr.LogError("参数数量不足");
            }
            return null;
        }

        public override int GetArgIndexType(int index)
        {
            if(index < ArgSortList.Count)
            {
                return ArgSortList[index].Key;
            }
            else
            {
                return -1;
            }
        }

        public override string ToString ()
        {
            StringBuilder sb = new  StringBuilder();
            sb.AppendFormat("参数个数 {0} ",this._ArgCount.ToString());

            for(int i =0; i < ArgSortList.Count;++i)
            {
                var kv  = ArgSortList[i];
                if (kv.Key == (int)ParamType.INT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.intList[kv.Value]);
                }
                else if (kv.Key == (int)ParamType.SHORT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.shortlist[kv.Value]);
                }
                else if (kv.Key == (int)ParamType.BOOL)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.boollist[kv.Value]);
                }
                else if (kv.Key == (int)ParamType.DOUBLE)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.doubleList[kv.Value]);
                }
                else if (kv.Key == (int)ParamType.FLOAT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.floatList[kv.Value]);
                }
                else if (kv.Key == (int)ParamType.NULL)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, "空");
                }
                else if (kv.Key == (int)ParamType.STRING)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.strlist[kv.Value]);
                }
                else if (kv.Key == (int)ParamType.LONG)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.longList[kv.Value]);
                }
                else if (kv.Key == (int)ParamType.VETOR3)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.vector3List[kv.Value]);
                }
                else if (kv.Key == (int)ParamType.OBJECT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1 ,this.objList[kv.Value]);
                }
                else if (kv.Key == (int)ParamType.UNITYOBJECT)
                {
                    sb.AppendFormat("第{0}个参数为: {1} ", i + 1, this.UnityObjList[kv.Value]);
                }
            }

//            if(sb.Length == 0)
//            {
//                return "空";
//            }

            return sb.ToString();
          
        }


    }
}


