using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using UnityEngine.UI;

namespace KFrameWork
{
    [Serializable]
    public class SpriteAtlas : IEquatable<SpriteAtlas>
    {
        public string AtlasName;

        private IBundleRef bundle;

        private Dictionary<string, Sprite> spritelist = new Dictionary<string, Sprite>();

        [SerializeField]
        private List<string> SprKeys;
        /// <summary>
        /// 当异步加载期间的对同一个image 的change操作是有可能表现出逻辑异常，概率极低，后面再议
        /// </summary>
        private Action<bool, IBundleRef> cacheCbk;

        private Queue<KeyValuePair<string, Image>> imageLoaders;

        public void LoadSprites(UnityEngine.Object[] allsprs)
        {
            SprKeys = new List<string>();
            foreach (var subspr in allsprs)
            {
                if (subspr is Sprite)
                {
                    SprKeys.Add(subspr.name);
                }
                else
                {
                    this.AtlasName = subspr.name.ToLower()+".png";
                }
            }
        }

        public void ChangeSprite(Image image, string imageSpr, Action<bool, IBundleRef> resultCallBack = null)
        {
            if (image == null)
                return;

            if (bundle == null)
            {
                if (cacheCbk == null)
                    cacheCbk = resultCallBack;
                else
                    cacheCbk += resultCallBack;

                if (imageLoaders == null)
                    imageLoaders = new Queue<KeyValuePair<string, Image>>();

                imageLoaders.Enqueue(new KeyValuePair<string, Image>(imageSpr, image));

                //just One
                ResBundleMgr.mIns.LoadAsync(this.AtlasName, LoadDone);
            }
            else
            {
                AfterDone(image, imageSpr);
                if (resultCallBack != null)
                    resultCallBack(true, this.bundle);
            }

        }

        public bool ContainsSprite(Sprite spr)
        {
            if (this.SprKeys == null)
            {
                LogMgr.LogWarning("atlas 尚未初始化");
                return false;
            }

            if (spr == null)
            {
                LogMgr.LogWarning("spr is Null");
                return false;
            }

            for (int i = 0; i < this.SprKeys.Count; ++i)
            {
                if (this.SprKeys[i].Equals(spr.name))
                {
                    return true;
                }
            }
            return false;
        }


        public bool ContainsSprite(string sprname)
        {
            if (this.SprKeys == null)
            {
                LogMgr.LogWarning("atlas 尚未初始化");
                return false;
            }

            for (int i = 0; i < this.SprKeys.Count; ++i)
            {
                if (this.SprKeys[i].Equals(sprname))
                {
                    return true;
                }
            }
            return false;
        }

        private void AfterDone(Image image, string imageSpr)
        {
            Sprite old = image.sprite;
            SpriteAtlas oldAtlas = SpriteAtlasMgr.mIns.TryGetAtlas(old);

            if (!spritelist.ContainsKey(imageSpr))
            {
                UnityEngine.Object[] objs ;
                if (this.bundle.LoadAllAssets( out objs))
                {
                    for (int i = 0; i < objs.Length; i++)
                    {
                        Sprite spr = objs[i] as Sprite;
                        if(spr != null)
                            spritelist[spr.name] = spr;
                    }
                }
            }

            Sprite newSpr = spritelist[imageSpr];

            if (oldAtlas != null && oldAtlas.bundle != null)
            {
                if (oldAtlas != this)
                    oldAtlas.bundle.Release();
                else
                    this.bundle.Release();
            }

            this.bundle.Retain();

            image.sprite = newSpr;
        }

        private void LoadDone(bool ret, AssetBundleResult result)
        {
            if (ret)
            {
                bundle = result.MainObject;
                if (bundle == null)
                    throw new FrameWorkResMissingException(string.Format("Missing {0}", result.LoadPath));

                if (SprKeys == null)
                    SprKeys = new List<string>(bundle.GetAllAssetNames());
                //else
                //{
                //    SprKeys.Clear();
                //    SprKeys.AddRange(bundle.GetAllAssetNames());
                //}

                while (this.imageLoaders.Count > 0)
                {
                    KeyValuePair<string, Image> kv = this.imageLoaders.Dequeue();
                    AfterDone(kv.Value, kv.Key);
                }
            }

            if (cacheCbk != null)
            {
                cacheCbk(ret, this.bundle);
                cacheCbk = null;
            }
        }

        public bool Equals(SpriteAtlas other)
        {
            if (other == null)
                return false;
            return this.AtlasName == other.AtlasName;
        }
    }

}


