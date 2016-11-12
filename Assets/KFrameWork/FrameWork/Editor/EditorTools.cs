using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using KUtils;
using System.Reflection;
using System.Linq;

namespace KFrameWork
{
    public class EditorTools :BaseAttributeRegister {

        private static EditorTools mIns;

        private Dictionary<int,EditorAssetImportAttribute> importCaches =new Dictionary<int, EditorAssetImportAttribute>();

        public EditorTools()
        {
            mIns =this;

            this.Init();
        }

        public static EditorTools getInstance()
        {
            return mIns;
        }

        private void Init()
        {
            try
            {
                EditorAttRegister.Register(this);
                this.Start();
                this.End();
            }
            catch(Exception ex)
            {
                LogMgr.LogError(ex);
            }

        }

        public static bool MatchActionOrFunc(MethodInfo method,Type delegateType)
        {
            MethodInfo delegateSignature = delegateType.GetMethod("Invoke");

            bool parametersEqual = delegateSignature
                .GetParameters()
                .Select(x => x.ParameterType)
                .SequenceEqual(method.GetParameters()
                    .Select(x => x.ParameterType));

            return delegateSignature.ReturnType == method.ReturnType &&
                parametersEqual;
        }

        public void PushImportAtt(AssetImportDefine df,EditorAssetImportAttribute att)
        {
            int value = (int)df;
            if(!this.importCaches.ContainsKey(value))
            {
                this.importCaches[value] =att;
            }
            else
            {
                LogMgr.LogError("Duplicate Insert");
            }

        }

        public void DynamicInvokeAtt(AssetImportDefine df,params object[] objs)
        {
            try
            {
                int value = (int)df;
                if(this.importCaches.ContainsKey(value))
                {
                    EditorAssetImportAttribute att = this.importCaches[value];

                    att.callback.DynamicInvoke(objs);
                }
            }catch(Exception ex)
            {
                LogMgr.LogError(ex);
            }

        }


    }
}


