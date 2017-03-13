using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;

namespace KFrameWork
{
    public abstract class EditorAssetImportAttribute:Attribute
    {

        public System.Delegate callback;

        public Type actionTp;

        public virtual bool CheckEnable(MethodInfo method)
        {
            if(actionTp == null)
                return false;

            return EditorTools.MatchActionOrFunc(method, this.actionTp);
        }

        public void SetDelegate(MethodInfo method)
        {
            if(this.actionTp == null)
                return ;

            callback = Delegate.CreateDelegate(this.actionTp,method);
        }

        public bool LogInfo(MethodInfo method)
        {
            bool ret = this.CheckEnable(method);
            if(!ret )
            {
                LogMgr.LogError("类型不匹配 "+actionTp);
            }
            return ret;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostImportAssetNameAttribute:EditorAssetImportAttribute
    {
        public PostImportAssetNameAttribute()
        {
            this.actionTp = typeof(Action<string[]>);
        }
            
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostDelAssetNameAttribute:EditorAssetImportAttribute
    {
        public PostDelAssetNameAttribute()
        {
            this.actionTp = typeof(Action<string[]>);
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class PostMoveAssetNameAttribute:EditorAssetImportAttribute
    {
        public PostMoveAssetNameAttribute()
        {
            this.actionTp = typeof(Action<string[]>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostMoveFromAssetNameAttribute:EditorAssetImportAttribute
    {
        public PostMoveFromAssetNameAttribute()
        {
            this.actionTp = typeof(Action<string[]>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostAllAssetNameAttribute:EditorAssetImportAttribute
    {
        public PostAllAssetNameAttribute()
        {
            this.actionTp = typeof(Action<string[],string[],string[],string[]>);
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class PostModelAttribute:EditorAssetImportAttribute
    {
        public PostModelAttribute()
        {
            this.actionTp = typeof(Action<GameObject>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostABNameChangeAttribute:EditorAssetImportAttribute
    {
        public PostABNameChangeAttribute()
        {
            this.actionTp = typeof(Action<string,string,string>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostAudioAttribute:EditorAssetImportAttribute
    {
        public PostAudioAttribute()
        {
            this.actionTp = typeof(Action<AudioClip>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostSpeedTreeAttribute:EditorAssetImportAttribute
    {
        public PostSpeedTreeAttribute()
        {
            this.actionTp = typeof(Action<GameObject>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostTextureAttribute:EditorAssetImportAttribute
    {
        public PostTextureAttribute()
        {
            this.actionTp = typeof(Action<Texture2D>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostGameobjecPropertyAttribute:EditorAssetImportAttribute
    {
        public PostGameobjecPropertyAttribute()
        {
            this.actionTp = typeof(Action<GameObject ,string[] , System.Object[]>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostSpritesAttribute:EditorAssetImportAttribute
    {
        public PostSpritesAttribute()
        {
            this.actionTp = typeof(Action<Texture2D , Sprite[] >);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostAssignModelsAttribute:EditorAssetImportAttribute
    {
        public PostAssignModelsAttribute()
        {
            this.actionTp = typeof(Action<Material,Renderer>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PrepareAudioAttribute:EditorAssetImportAttribute
    {
        public PrepareAudioAttribute()
        {
            this.actionTp = typeof(Action<AssetImporter,string>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PrepareModelAttribute:EditorAssetImportAttribute
    {
        public PrepareModelAttribute()
        {
            this.actionTp = typeof(Action<AssetImporter,string>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PrepareSpeedTreeAttribute:EditorAssetImportAttribute
    {
        public PrepareSpeedTreeAttribute()
        {
            this.actionTp = typeof(Action<AssetImporter,string>);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PrepareTextureAttribute:EditorAssetImportAttribute
    {
        public PrepareTextureAttribute()
        {
            this.actionTp = typeof(Action<AssetImporter,string>);
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class PrepareAnimationAttribute:EditorAssetImportAttribute
    {
        public PrepareAnimationAttribute()
        {
            this.actionTp = typeof(Action<AssetImporter,string>);
        }
    }
}

#endif
