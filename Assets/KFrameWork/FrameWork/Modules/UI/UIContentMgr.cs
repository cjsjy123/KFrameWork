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

        /// <summary>
        /// 按UIDepth降序排列
        /// </summary>
        private LinkedList<BaseContent> UIStack;

        private Canvas baseCanvas;

        private int _depth =-1000;

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
                if(this._depth != this.baseCanvas.sortingOrder )
                {
                    if(this._depth != -1000)
                    {
                        LogMgr.LogFormat("默认深度改变 to {0}",this.baseCanvas.sortingOrder);
                    }

                    this._depth = this.baseCanvas.sortingOrder;
                }
            }
        }

        private bool CanPush()
        {
            if(this.MaxSize == -1)
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

        private void _AutoControlFlag(BaseContent current, UIContent content, GameObject Instance)
        {
            bool hasCover =false;
            bool hasExit =false;

            if(!UIContent.isFlag(content.Flag, UIFlag.Discover) && !hasCover)
            {
                hasCover =true;
                current.OnDisCover(Instance);
            }
                
  
            if(UIContent.isFlag(content.Flag, UIFlag.VisiableNow))
            {
                if(!hasExit)
                    current.OnExit();

                current.DoInVisiable();

                hasExit =true;
            }

            if(UIContent.isFlag(content.Flag, UIFlag.RemoveNow))
            {
                if(!hasExit)
                    current.OnExit();

                hasExit =true;
                
                GameObject.Destroy(current);
            }
           
        }

        private LinkedListNode<BaseContent> SeekNearst(BaseContent Next)
        {
            if (Next == null)
                return null;
            LinkedListNode<BaseContent> first = this.UIStack.First;
            for (; first != null;)
            {
                LinkedListNode<BaseContent> next = first.Next;
                if (next.Value.UIDepth < Next.UIDepth)
                {
                    break;
                }
                first = next.Next;
            }

            return first;
        }

        private GameObject _CanvasAdd(BaseContent Next, GameObject ins)
        {
            if (this.baseCanvas == null)
                throw new FrameWorkException("Missing canvas");

            Next.UIDepth = (++this._depth);
            return this.baseCanvas.AddInstance(ins);
        }


        private GameObject _AutoAdjustDeep(BaseContent old, BaseContent Next, GameObject ins, LinkedListNode<BaseContent> target = null)
        {
            GameObject instance =  null;
            if (Next != null)
            {
                if (Next.UIDepth != -100)
                {
                    if (FrameWorkDebug.Open_DEBUG)
                        LogMgr.LogFormat("手动修改的深度 :{0}", Next.UIDepth);

                    //BaseContent target = this.SeekNearst(Next.UIDepth);

                    if (target != null)
                    {
                        instance = target.Value.AddInstance(ins);
                    }
                    else
                    {
                        instance = this._CanvasAdd(Next, ins);
                    }

                }
                else
                {
                    if (old != null)
                    {
                        Next.UIDepth = old.UIDepth + 1;

                        if (old.UIDepth >= this._depth)
                        {
                            this._depth = old.UIDepth + 1;
                        }

                        instance = Next.AddInstance(ins);
                    }
                    else 
                    {
                        instance = this._CanvasAdd(Next, ins);
                    }

                }
                
            }
            return instance;
        }

        private GameObject GetObjectByUIContent(UIContent content)
        {
            GameObject newPrefab = ResLoader.mIns.Load(content.PrefabPath) as GameObject;
            if (newPrefab == null)
                throw new FrameWorkException("Load Prefab is Null");

            GameObject Instance = GameObject.Instantiate(newPrefab);

            return Instance;
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
                    BaseContent current = null;
                    if(this.UIStack.First != null)
                        current = this.UIStack.First.Value;

                    GameObject Instance = this.GetObjectByUIContent(content);
                    BaseContent Next = Instance.GetComponent<BaseContent>();
                    if (Next != null)
                    {
                        ++this._depth;
                        LinkedListNode<BaseContent> target = this.SeekNearst(Next);
                        Instance = this._AutoAdjustDeep(current, Next, Instance, target);

                        if (this._limitsize == -1 || current == null || target == null)
                        {
                            this.UIStack.AddFirst(Next);
                        }
                        else
                        {
                            this.UIStack.AddAfter(target, Next);
                        }
                        Next.OnEnter();
                    }
                    else
                    {
                        LogMgr.LogError("Prefab 中不包含basecontent脚本，可能导致深度异常");
                    }

                    if (current != null)
                    {
                        this._AutoControlFlag(self, content, Instance);
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
            try
            {

            }
            catch (FrameWorkException ex)
            {
                LogMgr.LogException(ex);

                ex.RaiseExcption();
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
            }
        }

        public void PopContent(BaseContent self)
        {
            try
            {
                if (self == null)
                {
                    LogMgr.LogErrorFormat("{0} is Null", self);
                    return;
                }

                this._LoadBase(self);

                BaseContent current = null;
                if (this.UIStack.First != null)
                    current = this.UIStack.First.Value;

                --this._depth;
                this.UIStack.Remove(current);
                current.OnExit();

                if (current != null)
                {
                    GameObject.Destroy(current);
                }
            }
            catch (FrameWorkException ex)
            {
                LogMgr.LogException(ex);

                ex.RaiseExcption();
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
            }
        }

        public void RemoveContent(BaseContent self, UIContent content)
        {
            try
            {
                if (self == null)
                {
                    LogMgr.LogErrorFormat("{0} is Null", self);
                    return;
                }

                GameObject Instance = this.GetObjectByUIContent(content);
                BaseContent bContent = Instance.GetComponent<BaseContent>();
                if(bContent == null)
                {
                    LogMgr.LogError("Prefab 中不包含basecontent脚本，可能导致深度异常");
                }
                else if(this.UIStack.Contains(bContent))
                {
                    this._LoadBase(self);

                    BaseContent current = null;
                    if (this.UIStack.First != null)
                        current = this.UIStack.First.Value;

                    if (current.Equals(bContent))
                    {
                        --this._depth;
                    }

                    this.UIStack.Remove(bContent);
                    bContent.OnExit();

                    if (current != null)
                    {
                        this._AutoControlFlag(self, content, Instance);
                    }

                }
            }
            catch (FrameWorkException ex)
            {
                LogMgr.LogException(ex);

                ex.RaiseExcption();
            }
            catch (Exception ex)
            {
                LogMgr.LogException(ex);
            }


        }

    }
}


