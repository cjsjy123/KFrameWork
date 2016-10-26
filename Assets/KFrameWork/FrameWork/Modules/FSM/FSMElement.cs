using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using KUtils;

namespace KFrameWork
{

    /// <summary>
    /// 自上而下的层级状态管理元素,状态包裹层
    /// </summary>
    public class FSMElement:FSMEvent,IDisposable  {

        protected KEnum _state;

        public KEnum CurrentState
        {
            get
            {
                return this._state;
            }
        }

        protected int _priority;

        public int Priority
        {
            get
            {
                return this._priority;
            }
        }

        private bool _InitBefore;

        protected bool _Active;

        public bool Active
        {
            get
            {
                return this._Active;
            }
            private set
            {
                this._Active =value;
                if(value)
                {

                    this.EnterFS();
                    if(this.Runner.runningType== FSMRunningType.WhenEnableInvoke)
                    {
                        this.Runner.InvokeOnceWhenEnable();
                    }
                }
                else
                {
                    if(this.MachineComponet != null)
                        this.MachineComponet.RemoveRunner(this.Runner);
                }
                    
            }
        }

        private FSMRuningComponet _Machine;

        public FSMRuningComponet MachineComponet
        {
            get
            {
                return this._Machine;
            }
        }

        private FSMRunningEvent _runner;

        public FSMRunningEvent Runner
        {
            get
            {
                return this._runner;
            }
        }

        private List<FSMElement> _Children;

        public List<FSMElement> Children
        {
            get
            {
                if(_Children == null)
                    this._Children = new List<FSMElement>(this.Elements.Count);

                if(_Children.Count == 0 && this.Elements.Count>0)
                {
                    for(var first = this.Elements.First;first != null;first = first.Next)
                    {
                        this._Children.Add(first.Value);
                    }
                }
                return _Children;
            }
        }

        private LinkedList<FSMElement> Elements;

        private List<KEnum> cachedManagedTypes;
        /// <summary>
        /// Gets the managed types directly by this
        /// </summary>
        /// <value>The managed types.</value>
        public List<KEnum> ManagedTypes
        {
            get
            {
                if(cachedManagedTypes == null)
                {
                    InitCacheTypes();
                }

                return this.cachedManagedTypes;
            }
        }



        public FSMElement()
        {
            this.Elements = new LinkedList<FSMElement>();
        }

        public FSMElement(KEnum Pstate, FSMRunningEvent runner,FSMRuningComponet com):this()
        {
            this.Initiate(Pstate,runner,com);
        }

        public virtual void Initiate(KEnum Pstate, FSMRunningEvent runner,FSMRuningComponet com)
        {
            this._state = Pstate;
            this._Machine = com;
            this._runner = runner;
            this._runner.name = Pstate.ToString();
            if(runner.runningType == FSMRunningType.WhenInitInvoke)
            {
                runner.InvokeOnceWhenInit();
            }
        }

        public virtual void UdateLogic (){}
       
        public virtual void EnterFS (){}

        public virtual void LeaveFS (){}

        public virtual bool DetermineRequest(){return true;}

        public T RegisterState<T>(KEnum state,FSMRunningEvent runner,int priority =0) where T:FSMElement,new()
        {
            KeyValuePair<int,FSMElement> old = this._FindElementKV(state);
            if(old.Value == null )
            {
                T element = new T();
                element.Initiate(state,runner,this.MachineComponet);
                this.ChangePriorityOrder(element,false);
                return element;
            }
            else if(old.Value._priority  != priority)
            { 
                old.Value._priority=priority;
                this.ChangePriorityOrder(old.Value,true);
            }

            return old.Value as T;
        }

        public FSMElement this[KEnum state]
        {
            get
            {
                return this.FindElement(state);
            }
        }

        public List<FSMElement> FindRuningElements(bool includeself =false)
        {
            List<FSMElement> list = new List<FSMElement>();
            if(includeself && this._Active)
            {
                list.Add(this);
            }

            for(var first = this.Elements.First;first != null;first= first.Next)
            {
                if(first.Value._Active)
                {
                    list.Add(first.Value);
                }
            }

            return list;
        }

        public FSMElement FindElement(KEnum state)
        {
            
            for(var first = this.Elements.First;first != null;first= first.Next)
            {
                if(first.Value._state == state)
                {
                    return first.Value;
                }
            }
            return null;
        }

        public List<FSMElement> FindElements(KEnum state)
        {
            List<FSMElement> list = new List<FSMElement>();
            for(var first = this.Elements.First;first != null;first= first.Next)
            {
                if(first.Value._state == state)
                {
                    list.Add(first.Value);
                }
            }
            return list;
        }

        private void ChangePriorityOrder(FSMElement element,bool exist )
        {
            if(element == null)
                return;

            if(exist)
            {
                this.Elements.Remove(element);
            }

            bool hasAdd= false;
            for(var first = this.Elements.First;first != null;first = first.Next)
            {
                if(first.Value._priority > element._priority)
                {
                    this.Elements.AddBefore(first,element);
                    hasAdd =true;
                    break;
                }
            }

            if(!hasAdd)
            {
                this.Elements.AddLast(element);
            }

            this.InitCacheTypes();

        }

        private void InitCacheTypes()
        {
            if(this.cachedManagedTypes == null)
                this.cachedManagedTypes = new List<KEnum>(this.Elements.Count+1);
            else
                this.cachedManagedTypes.Clear();

            this.cachedManagedTypes.Add(this._state);

            for(var first = this.Elements.First; first != null; first = first.Next)
            {
                this.cachedManagedTypes.Add(first.Value._state);
            }

            if(this._Children != null)
                this._Children.Clear();
        }

        private void ChangePriorityOrder(FSMElement element)
        {
            if(element == null)
                return;
            
            KeyValuePair<int,FSMElement> old = this._FindElementKV(element._state);
            this.ChangePriorityOrder(old.Value,old.Value != null);
        }

        private KeyValuePair<int,FSMElement> _FindElementKV(KEnum state)
        {
            int i =0;
            for(var first=this.Elements.First; first != null;first =first.Next)
            {

                if(first.Value._state == state)
                {
                    return new KeyValuePair<int,FSMElement>(i,first.Value);
                }
                i++;
            }
            return new KeyValuePair<int, FSMElement>(-1,null);

        }


        public bool AwakeState()
        {
            if(!this._Active && this.DetermineRequest())
            {
                this.Active =true;
                if(!_InitBefore)
                {
                    _InitBefore =true;
                    if(this.Runner != null && this.Runner.runningType == FSMRunningType.WhenInitInvoke)
                        this.Runner.InvokeOnceWhenInit();
                }

                return true;
            }
            return false;
        }

        public bool InActiveState()
        {
            if(this.Active)
            {
                this.Active =false;
                return true;
            }

            return false;
        }

        public bool ChangeState(KEnum newState) 
        {
            bool ret  =false;
            List<KEnum> abelStates = this.ManagedTypes;
            if(abelStates.Contains(newState))
            {
                if(this._Active )
                {
                    KeyValuePair<int,FSMElement> target= this._FindElementKV(newState);
                    if(target.Value != null)
                    {
                        this.LeaveFS();

                        target.Value.Active =true;

                        ret =true;
                    }

                    this._Active =false;
                }
                else
                {
                    LogMgr.LogErrorFormat("{0} is inactive ",this);
                }

            }
            else
            {
                LogMgr.LogErrorFormat("未注册的状态事件 {0}",newState);
            }

            return ret;
        }


        public void Dispose()
        {
            this.cachedManagedTypes = null;
            this._state = null;
            this.Elements = null;
        }

//        public static bool operator ==(FSMElement left,FSMElement right)
//        {
//            if(!object.ReferenceEquals(left,null) && !object.ReferenceEquals(right,null))
//            {
//                return object.ReferenceEquals(left,right);
//            }
//            return false;
//        }
//
//        public static bool operator !=(FSMElement left,FSMElement right)
//        {
//            bool leftNull =object.ReferenceEquals(left,null);
//            bool rightNull =object.ReferenceEquals(right,null);
//            if( !leftNull && !rightNull)
//            {
//                return !object.ReferenceEquals(left,right);
//            }
//            else if(leftNull && rightNull)
//                return false;
//
//            return true;
//        }

        public static bool operator ==(FSMElement left,KEnum state)
        {
            bool leftNull =object.ReferenceEquals(left,null);
            bool rightNull =object.ReferenceEquals(state,null);
            if(!leftNull && !rightNull)
            {
                return left.CurrentState == state;
            }
            else if(leftNull && rightNull)
                return true;
            
            return false;
        }

        public static bool operator !=(FSMElement left,KEnum state)
        {
            bool leftNull =object.ReferenceEquals(left,null);
            bool rightNull =object.ReferenceEquals(state,null);
            if( !leftNull && !rightNull)
            {
                return left.CurrentState != state;
            }
            else if(leftNull && rightNull)
                return false;
            
            return true;
        }

        public override bool Equals (object obj)
        {
            if(obj is FSMElement)
            {
                return base.Equals(obj);
            }
            else if(obj is KEnum)
            {
                if(!object.ReferenceEquals(obj,null))
                {
                    return this.CurrentState == (obj as KEnum);
                }
            }

            return false;
        }

        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }

    }


}


