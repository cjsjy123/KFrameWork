using System;
using System.Collections.Generic;
using UnityEngine;
using KUtils;
using System.Reflection;

namespace KFrameWork
{
    [Serializable]
    public class TypeListWrapper
    {
        public string name;

        public string attributename;

        public List<string> seekTargetList = new List<string>();
    }

    [Serializable]
    public class CachedListStringWrapper
    {
        public List<TypeListWrapper> TypeList = new List<TypeListWrapper>();
    }

    [Serializable]
    public class TypeCachedDictionary : SimpleDictionary<int, CachedListStringWrapper>
    {

    }

    [Serializable]
    public class RegisterCachedTypes : ScriptableObject
    {
        [SerializeField]
        private TypeCachedDictionary types = new TypeCachedDictionary();

        public List<TypeListWrapper> getAlltypeWrappers()
        {
            List<TypeListWrapper> TypeList = null;
            if (types != null)
            {
                for (int i =0; i < types.ReadOnlyValues.Count;++i)
                {
                    if(TypeList == null )
                    {
                        TypeList = types.ReadOnlyValues[i].TypeList;
                    }
                    else
                    {
                        TypeList.AddRange(types.ReadOnlyValues[i].TypeList);
                    }
                }
            }
            return TypeList;
        }

        public CachedListStringWrapper getTypes(int registertype)
        {
            if (types.ContainsKey(registertype))
            {
                return types[registertype];
            }
            return null;
        }

        /// <summary>
        /// ignore gc affect
        /// </summary>
        /// <param name="registertype"></param>
        /// <param name="tp"></param>
        public void LoadTypes(RegisterType registertype,Type tp,Type atttype)
        {
            TypeListWrapper typewrapper = new TypeListWrapper();
            typewrapper.name = tp.FullName;
            typewrapper.attributename = atttype.FullName;

            int register = (int)registertype;
            if (types.ContainsKey(register))
            {
                types[register].TypeList.Add(typewrapper);
            }
            else
            {
                CachedListStringWrapper wrapper = new CachedListStringWrapper();
                wrapper.TypeList.Add(typewrapper);
                types[register] = wrapper;
            }
        }

        public void LoadTypes(RegisterType registertype, Type tp,Type atttype,List<MethodInfo> methodnames)
        {
            TypeListWrapper typewrapper = new TypeListWrapper();
            typewrapper.name = tp.FullName;
            typewrapper.attributename = atttype.FullName;

            for (int i = 0; i < methodnames.Count; ++i)
            {
                typewrapper.seekTargetList.TryAdd(methodnames[i].Name);
            }
            int register = (int)registertype;
            if (types.ContainsKey(register))
            {
                types[register].TypeList.TryAdd(typewrapper);
            }
            else
            {
                CachedListStringWrapper wrapper = new CachedListStringWrapper();
                wrapper.TypeList.Add(typewrapper);
                types[register] = wrapper;
            }
        }

        public void Analyze(BaseAttributeRegister register, int tp, TypeListWrapper wrapper, Type attname, BaseAttributeRegister.GameAttriHandler handler)
        {
            //if (!attname.FullName.Equals(wrapper.attributename))
            //    return;

            RegisterType registerType =(RegisterType)tp;
            if (registerType == RegisterType.ClassAttr)
            {
                Type clstype = Type.GetType(wrapper.name);
                if (clstype == null)
                {
                    LogMgr.LogErrorFormat("cant find class:{0}", wrapper.name);
                }
                else
                {
                    object[] atts = clstype.GetCustomAttributes(attname, true);
                    if (atts.Length > 0)
                        handler(atts[0], clstype);
                    else
                    {
                        LogMgr.LogErrorFormat("missing attribute :{0} from {1}", clstype, attname);
                    }
                }
            }
            else if (registerType == RegisterType.MethodAtt || registerType == RegisterType.Register)
            {
                if (wrapper.seekTargetList.Count == 0)
                {
                    LogMgr.LogErrorFormat("没有匹配的函数 :{0} From :{1}", attname, wrapper.name);
                }
                else
                {
                    Type clstype = Type.GetType(wrapper.name);
                    if (clstype == null)
                    {
                        LogMgr.LogErrorFormat("cant find class:{0}", wrapper.name);
                    }
                    else
                    {

                        for (int i = 0; i < wrapper.seekTargetList.Count; ++i)
                        {
                            string target = wrapper.seekTargetList[i];
                            MethodInfo targetMethod = clstype.GetMethod(target, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                            if (targetMethod == null)
                            {
                                LogMgr.LogErrorFormat("没有匹配的函数 :{0} From :{1}  named :{2}", attname, wrapper.name, target);
                                continue;
                            }

                            object[] atts = targetMethod.GetCustomAttributes(attname, true);
                            if (atts.Length == 0)
                            {
                                LogMgr.LogErrorFormat("method {0} in {1} missing attribute:{2}", targetMethod.Name, wrapper.name, attname.Name);
                            }
                            else
                            {
                                if (registerType == RegisterType.Register)
                                {
                                    handler(atts[0],new  KeyValuePair<BaseAttributeRegister, MethodInfo> (register,targetMethod));
                                }
                                else
                                {
                                    handler(atts[0], targetMethod);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                LogMgr.LogError("error");
            }
        }

        public void Clear()
        {
            if(this.types != null)
                this.types.Clear();
        }
    }
}
