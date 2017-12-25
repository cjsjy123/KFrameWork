using UnityEngine;
using System;
using KUtils;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace KFrameWork
{

    public interface LoadrefInterface
    {
        Component c { get; }

        string p { get; }

        void SetReferces(Action DoneEvent);

        void ReleaseReferencs();
    }

    //[Serializable]
    //public class UguiLoadRefKv: LoadrefInterface
    //{
    //    public const string BaseCachePath = "Assets/ReferencesCaches";

    //    [SerializeField]
    //    private Component _c;
    //    public Component c { get { return _c; } }

    //    [SerializeField]
    //    private string _p;
    //    public string p { get { return _p; } }

    //    private Action Done;

    //    public UguiLoadRefKv(Component comp, string path)
    //    {
    //        this._c = comp;
    //        this._p = path;
    //    }

    //    void AsyncDone(bool ret, IBundleRef result)
    //    {
    //        if (ret)
    //        {
    //            TryCall();
    //        }
    //    }

    //    void TryCall()
    //    {
    //        if (Done != null)
    //            Done();
    //        else
    //        {
    //            LogMgr.LogError("missing event");
    //        }
    //    }


    //    public void ReleaseReferencs()
    //    {
    //        if (c is Image)
    //        {
    //            BundlePkgInfo pkg = ResBundleMgr.mIns.BundleInformation.SeekInfo(p);
    //            //if (pkg != null)
    //            //{
    //            //    if (pkg.ResTp == PkgResType.Image)//不对的
    //            //    {

    //            //    }
    //            //    else if (pkg.ResTp == PkgResType.Material)
    //            //    {
    //            //        ResBundleMgr.mIns.Cache.TryGetValue(p).Release();
    //            //    }

    //            //}
    //        }
    //        else if (c is RawImage)
    //        {
    //            BundlePkgInfo pkg = ResBundleMgr.mIns.BundleInformation.SeekInfo(p);
    //            //if (pkg != null)
    //            //{
    //            //    if (pkg.ResTp == PkgResType.Image)
    //            //    {
    //            //        ResBundleMgr.mIns.Cache.TryGetValue(p).Release();
    //            //    }
    //            //    else if (pkg.ResTp == PkgResType.Material)
    //            //    {
    //            //        ResBundleMgr.mIns.Cache.TryGetValue(p).Release();
    //            //    }

    //            //}
    //        }
    //        else if (c is Text)
    //        {
    //            BundlePkgInfo pkg = ResBundleMgr.mIns.BundleInformation.SeekInfo(p);
    //            //if (pkg != null)
    //            //{
    //            //    if (pkg.ResTp == PkgResType.Material)
    //            //    {
    //            //        ResBundleMgr.mIns.Cache.TryGetValue(p).Release();
    //            //    }
    //            //    else if (pkg.ResTp == PkgResType.Font)
    //            //    {

    //            //        ResBundleMgr.mIns.Cache.TryGetValue(p).Release();
    //            //    }
    //            //}
    //        }
    //    }

    //    //public void SetReferces(Action DoneEvent )
    //    //{
    //    //    if (c == null)
    //    //    {
    //    //        throw new FrameWorkResMissingException("ui ref missing");
    //    //    }

    //    //    Done = DoneEvent;

    //    //    if (c is Image)
    //    //    {
    //    //        Image img = c as Image;
    //    //        BundlePkgInfo pkg = ResBundleMgr.mIns.BundleInformation.SeekInfo(p);
    //    //        if (pkg != null)
    //    //        {
    //    //            if (pkg.ResTp == PkgResType.Image)
    //    //            {
    //    //                SpriteAtlasMgr.mIns.ChangeSprite(img, p, AsyncDone);
    //    //            }
    //    //            else if (pkg.ResTp == PkgResType.Material)
    //    //            {
    //    //                img.material = ResBundleMgr.mIns.LoadAsset(p) as Material;
    //    //                ResBundleMgr.mIns.Cache.TryGetValue(p).Retain();
    //    //                TryCall();
    //    //            }

    //    //        }
    //    //    }
    //    //    else if (c is RawImage)
    //    //    {
    //    //        RawImage img = c as RawImage;
    //    //        BundlePkgInfo pkg = ResBundleMgr.mIns.BundleInformation.SeekInfo(p);
    //    //        if (pkg != null)
    //    //        {
    //    //            if (pkg.ResTp == PkgResType.Image)
    //    //            {
    //    //                img.texture = ResBundleMgr.mIns.LoadAsset(p) as Texture;
    //    //                ResBundleMgr.mIns.Cache.TryGetValue(p).Retain();
    //    //                TryCall();
    //    //            }
    //    //            else if (pkg.ResTp == PkgResType.Material)
    //    //            {
    //    //                img.material = ResBundleMgr.mIns.LoadAsset(p) as Material;
    //    //                ResBundleMgr.mIns.Cache.TryGetValue(p).Retain();
    //    //                TryCall();
    //    //            }

    //    //        }
    //    //    }
    //    //    else if (c is Text)
    //    //    {
    //    //        Text text = c as Text;
    //    //        BundlePkgInfo pkg = ResBundleMgr.mIns.BundleInformation.SeekInfo(p);
    //    //        if (pkg != null)
    //    //        {
    //    //            if (pkg.ResTp == PkgResType.Material)
    //    //            {
    //    //                text.material = ResBundleMgr.mIns.LoadAsset(p) as Material;
    //    //                ResBundleMgr.mIns.Cache.TryGetValue(p).Retain();
    //    //                TryCall();
    //    //            }
    //    //            else if (pkg.ResTp == PkgResType.Font)
    //    //            {
    //    //                text.font = ResBundleMgr.mIns.LoadAsset(p) as Font;
    //    //                ResBundleMgr.mIns.Cache.TryGetValue(p).Retain();
    //    //                TryCall();
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //}
}