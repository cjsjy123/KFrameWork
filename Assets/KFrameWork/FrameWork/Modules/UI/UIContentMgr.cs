#define UGUI

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

namespace KFrameWork
{
    [SingleTon]
    public class UIContentMgr  {
        public static UIContentMgr mIns;

        private Stack<UIContent> UIStack ;

        private Canvas baseCanvas;

        private int currentDeep =-1000;

        public UIContentMgr()
        {
            this.UIStack = new Stack<UIContent>();

        }

        private void _InitCanvas(UIContentEvent self)
        {
            MonoBehaviour m = self as MonoBehaviour;
            if(m != null)
            {
                this.baseCanvas = m.transform.GetComponentInParent<Canvas>();
    

                if(FrameWorkDebug.Open_DEBUG && this.baseCanvas == null)
                {
                    throw new ArgumentException("Canvas 缺失");
                }
                else if(this.baseCanvas != null)
                {
                    if(this.currentDeep != this.baseCanvas.sortingOrder )
                    {
                        if(this.currentDeep != -1000)
                        {
                            LogMgr.LogFormat("默认深度改变 to {0}",this.baseCanvas.sortingOrder);
                        }

                        this.currentDeep = this.baseCanvas.sortingOrder;
                    }
                        
                }
            }
            else if(FrameWorkDebug.Open_DEBUG)
            {
                LogMgr.LogErrorFormat("{0} is Null ",self);
            }
        }


        public void PushContent(UIContentEvent self,  UIContent content)
        {
            if(self == null)
            {
                LogMgr.LogErrorFormat("{0} is Null",self);
            }
            else
            {
                
            }
            
        }

        public void AysncPushContent(UIContentEvent self, UIContent content)
        {
            
        }

//        public UIContent PopContent()
//        {
//            
//        }

    }
}


