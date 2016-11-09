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

        private LinkedList<BaseContent> UIStack ;

        private Canvas baseCanvas;

        private int currentDeep =-1000;

        private int _limitsize = -1;



        public int MaxSize 
        {
            get
            {
                return this._limitsize;
            }

            private set
            {
                this._limitsize= value;
            }
        }

        public UIContentMgr()
        {
            this.UIStack = new LinkedList<BaseContent>();

        }

        public void ChangeBaseCanvas(Canvas canvas)
        {
            if(this.baseCanvas != canvas)
            {
                this.baseCanvas = canvas;
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

        private bool CanPush()
        {
            if(this.MaxSize  == -1)
                return true;
            return this.UIStack.Count <this.MaxSize;
        }

        private void _LoadBase(BaseContent self)
        {
            if(this.baseCanvas == null)
            {
                this._InitCanvas(self);
            }
        }

        private void _InitCanvas(BaseContent self)
        {

            if(self != null)
            {
                Canvas canvas = self.transform.GetComponentInParent<Canvas>();
    

                if(FrameWorkDebug.Open_DEBUG && canvas == null)
                {
                    throw new FrameWorkException("Canvas 缺失");
                }
                else if(canvas != null)
                {
                    this.ChangeBaseCanvas(canvas);
                        
                }
            }
            else if(FrameWorkDebug.Open_DEBUG)
            {
                LogMgr.LogErrorFormat("{0} isn't MonoBehaviour or null ",self);
            }
        }

        private void _AutoControlFlag(BaseContent current,UIContent content,GameObject Instance)
        {
            bool hasCover =false;
            bool hasExit =false;

            if(!UIContent.isFlag(content.Flag,UIFlag.Discover) && !hasCover)
            {
                hasCover =true;
                current.OnDisCover(Instance);
            }
                
  
            if(UIContent.isFlag(content.Flag,UIFlag.VisiableNow))
            {
                if(!hasExit)
                    current.OnExit();

                current.DoInVisiable();

                hasExit =true;
            }

            if(UIContent.isFlag(content.Flag,UIFlag.RemoveNow))
            {
                if(!hasExit)
                    current.OnExit();

                hasExit =true;
                
                GameObject.Destroy(current);
            }
           
        }

        private void _AutoAdjustDeep(BaseContent Next)
        {
            
        }
           

        public void PushContent(BaseContent self,  UIContent content)
        {
            try
            {
                if(self == null)
                {
                    LogMgr.LogErrorFormat("{0} is Null",self);
                }
                else if(this.CanPush())
                {
                    this._LoadBase(self);

                    GameObject newPrefab =  ResLoader.mIns.Load<GameObject>(content.PrefabPath);
                    if(newPrefab == null)
                        throw new FrameWorkException("Load Prefab is Null");

                    GameObject Instance = GameObject.Instantiate(newPrefab);

                    BaseContent current = this.UIStack.First.Value;
                    if(current != null)
                    {

                        this._AutoControlFlag(self,content,Instance);
                    }

                    BaseContent Next = Instance.GetComponent<BaseContent>();
                    if(Next != null)
                    {
                        this.currentDeep++;
                        this._AutoAdjustDeep(Next);

                        Next.OnEnter();
                        this.UIStack.AddFirst(Next);
                    }
                    else
                    {
                        LogMgr.LogError("Prefab 中不包含basecontent脚本，可能导致深度异常");
                    }

                }
            }
            catch(FrameWorkException ex)
            {
                LogMgr.LogException(ex);

                ex.RaiseExcption();
            }
            catch(Exception ex)
            {
                LogMgr.LogException(ex);
            }

            
        }

        public void AsyncPushContent(BaseContent self, UIContent content)
        {
            
        }



    }
}


