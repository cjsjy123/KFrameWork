using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
using UnityEngine.UI;

public class SpriteAtlasMgr : MonoBehaviour {
    [HideInInspector]
    public static SpriteAtlasMgr mIns;

    [SerializeField]
    private List<SpriteAtlas> Sprites =null;

    void Awake()
    {
        mIns = this;
    }

    public void Clear()
    {
        if (Sprites != null)
        {
            Sprites.Clear();
        }
    }

    public void InitSprites(SpriteAtlas atlas)
    {
        if (Sprites == null)
            Sprites = new List<SpriteAtlas>();

        Sprites.TryAdd(atlas);
    }

    public SpriteAtlas TryGetAtlas(Sprite spr)
    {
        if (Sprites == null || spr == null)
            return null;

        for (int i = 0; i < Sprites.Count; ++i)
        {
            SpriteAtlas atlas = Sprites[i];
            if (atlas.ContainsSprite(spr))
            {
                return atlas;
            }
        }
        LogMgr.LogErrorFormat("{0} 不存在在任何一个图集中 ",spr);
        return null;
    }

    public SpriteAtlas TryGetAtlas(string sprname)
    {
        if (Sprites == null || string.IsNullOrEmpty(sprname))
            return null;

        for (int i = 0; i < Sprites.Count; ++i)
        {
            SpriteAtlas atlas = Sprites[i];
            if (atlas.ContainsSprite(sprname))
            {
                return atlas;
            }
        }
        LogMgr.LogErrorFormat("{0} 不存在在任何一个图集中 ", sprname);
        return null;
    }

    public SpriteAtlas ChangeSprite(Image image, string imageSpr, Action<bool, IBundleRef> resultCallBack = null)
    {
        SpriteAtlas atlas = TryGetAtlas(imageSpr);
        if (atlas != null)
        {
            atlas.ChangeSprite(image,imageSpr, resultCallBack);
        }

        return atlas;
    }

}
