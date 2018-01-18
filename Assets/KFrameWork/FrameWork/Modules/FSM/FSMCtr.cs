using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KUtils;
using System;
using NodeEditorFramework;
using NodeEditorFramework.IO;

namespace KFrameWork
{

    [SingleTon]
    public partial class FSMCtr 
    {
        public static FSMCtr mIns;

        private bool _EnableSelfUpdate = false;
        public bool EnableSelfUpdate
        {
            get
            {
                return _EnableSelfUpdate;
            }
            set
            {
                if (_EnableSelfUpdate != value)
                {
                    _EnableSelfUpdate = value;
                    if (Root != null)
                    {
                        if (value)
                        {
                            Root.StartUpdate();
                        }
                        else
                        {
                            Root.EndUpdate();
                        }
                    }
                }
            }
        }

        private FSMRunner Root;

        public void UpdateInFrame(long frameCnt)
        {
            if (EnableSelfUpdate == false && Root != null)
            {
                Root.UpdateInFrame(frameCnt);
            }
        }

        public NodeCanvas LoadFsmFromCanvas(NodeCanvas nodecanvas)
        {
            if (nodecanvas == null)
            {
                LogMgr.LogError("nodecanvas is Null");
                return null;
            }

            if (Root == null)
            {
                GameObject target = GameObject.Find("FSM");
                if (target == null)
                {
                    target = new GameObject("FSM");
                }

                this.Root = target.AddComponent<FSMRunner>();
            }

            return this.Root.AddNode(nodecanvas);
        }

        public void RemoveFsmFromCanvas(NodeCanvas nodecanvas)
        {
            if (nodecanvas == null)
            {
                LogMgr.LogError("nodecanvas is Null");
                return;
            }

            if (Root != null)
            {
                this.Root.RemoveNode(Root.Map2Copy(nodecanvas));
            }
        }

        public bool isRunning(NodeCanvas nodecanvas)
        {
            if (this.Root != null)
            {
                return this.Root.isRunning(Root.Map2Copy(nodecanvas));
            }
            return false;
        }

        public List<NodeCanvas> GetAllFSM()
        {
            if (this.Root != null)
            {
                return this.Root.GetAllFSM();
            }
            return null;
        }

        public FSMElement CurrentFSMElement(FSMElement e)
        {
            if (this.Root == null)
                return null;

            return this.Root.CurrentFSMElement(Root.MapElement2Canvas(e));
        }

        public FSMElement CurrentFSMElement(NodeCanvas outnodecanvas)
        {
            if (this.Root == null)
                return null;

            return this.Root.CurrentFSMElement(Root.Map2Copy( outnodecanvas));
        }

        public FSMElement LastFSMElement(FSMElement e)
        {
            if (this.Root == null)
                return null;

            return this.Root.LastFSMElement(Root.MapElement2Canvas( e));
        }

        public FSMElement LastFSMElement(NodeCanvas outnodecanvas)
        {
            if (this.Root == null)
                return null;

            return this.Root.LastFSMElement(Root.Map2Copy(outnodecanvas));
        }

        public void ReStart(NodeCanvas canvas)
        {
            if (Root != null)
            {
                this.Root.Restart(Root.Map2Copy(canvas));
            }
        }

        public void ReStart(FSMElement element)
        {
            if (Root != null)
            {
                this.Root.Restart(Root.MapElement2Canvas(element));
            }
        }

        public bool TryGetCanvas(FSMElement e, out NodeCanvas outcanvas)
        {
            if (Root != null)
            {
                outcanvas = Root.MapElement2Canvas(e);
                return true;
            }
            outcanvas = null;
            return false;
        }

        public T FetchElement<T>(NodeCanvas nodecanvas) where T : FSMElement
        {
            if (nodecanvas != null && this.Root != null)
            {
                NodeCanvas realcanvas = this.Root.Map2Copy(nodecanvas);
                if (realcanvas != null)
                {
                    Type tp = typeof(T);
                    for (int i = 0; i < realcanvas.nodes.Count; ++i)
                    {
                        Node nd = realcanvas.nodes[i];
                        if (nd.GetType() == tp)
                        {
                            return nd as T;
                        }
                    }
                }
            }

            return null;
        }

        public List<T> FetchElements<T>(NodeCanvas nodecanvas) where T : FSMElement
        {
            List<T> list = ListPool.TrySpawn<List<T>>();
            if (nodecanvas != null && this.Root != null)
            {
                NodeCanvas realcanvas = this.Root.Map2Copy(nodecanvas);
                if (realcanvas != null)
                {
                    for (int i = 0; i < realcanvas.nodes.Count; ++i)
                    {
                        Node nd = realcanvas.nodes[i];
                        if (nd is T)
                        {
                            list.Add( nd as T);
                        }
                    }
                }
            }

            return list;
        }

        public void Clear()
        {
            if (this.Root != null)
            {
                this.Root.Clear();
            }
        }

        #region global
        public bool GetGlobal(NodeCanvas nodecanvas, string key, out int outvalue)
        {
            if (this.Root != null)
            {
                this.Root.GetGlobal(nodecanvas, key, out outvalue);
                return true;
            }
            outvalue = -1;
            return false;
        }

        public bool GetGlobal(NodeCanvas nodecanvas, string key, out bool outvalue)
        {
            if (this.Root != null)
            {
                this.Root.GetGlobal(nodecanvas, key, out outvalue);
                return true;
            }
            outvalue = false;
            return false;
        }

        public bool GetGlobal(NodeCanvas nodecanvas, string key, out byte outvalue)
        {
            if (this.Root != null)
            {
                this.Root.GetGlobal(nodecanvas, key, out outvalue);
                return true;
            }
            outvalue = 0;
            return false;
        }

        public bool GetGlobal(NodeCanvas nodecanvas, string key, out short outvalue)
        {
            if (this.Root != null)
            {
                this.Root.GetGlobal(nodecanvas, key, out outvalue);
                return true;
            }
            outvalue = -1;
            return false;
        }

        public bool GetGlobal(NodeCanvas nodecanvas, string key, out string outvalue)
        {
            if (this.Root != null)
            {
                this.Root.GetGlobal(nodecanvas, key, out outvalue);
                return true;
            }
            outvalue = "";
            return false;
        }

        public bool GetGlobal(NodeCanvas nodecanvas, string key, out long outvalue)
        {
            if (this.Root != null)
            {
                this.Root.GetGlobal(nodecanvas, key, out outvalue);
                return true;
            }
            outvalue = -1;
            return false;
        }

        public bool GetGlobal(NodeCanvas nodecanvas, string key, out System.Object outvalue)
        {
            if (this.Root != null)
            {
                this.Root.GetGlobal(nodecanvas, key, out outvalue);
                return true;
            }
            outvalue = null;
            return false;
        }

        public bool GetGlobal<T>(NodeCanvas nodecanvas, string key, out T outvalue) where T:class
        {
            if (this.Root != null)
            {
                this.Root.GetGlobal<T>(nodecanvas, key, out outvalue);
                return true;
            }
            outvalue = null;
            return false;
        }

        public void SetGlobal(NodeCanvas nodecanvas, string key, int value)
        {
            if (this.Root != null)
            {
                this.Root.SetGlobal(nodecanvas, key, value);
            }
        }

        public void SetGlobal(NodeCanvas nodecanvas, string key, short value)
        {
            if (this.Root != null)
            {
                this.Root.SetGlobal(nodecanvas, key, value);
            }
        }

        public void SetGlobal(NodeCanvas nodecanvas, string key, byte value)
        {
            if (this.Root != null)
            {
                this.Root.SetGlobal(nodecanvas, key, value);
            }
        }

        public void SetGlobal(NodeCanvas nodecanvas, string key, bool value)
        {
            if (this.Root != null)
            {
                this.Root.SetGlobal(nodecanvas, key, value);
            }
        }

        public void SetGlobal(NodeCanvas nodecanvas, string key, long value)
        {
            if (this.Root != null)
            {
                this.Root.SetGlobal(nodecanvas, key, value);
            }
        }

        public void SetGlobal(NodeCanvas nodecanvas, string key, string value)
        {
            if (this.Root != null)
            {
                this.Root.SetGlobal(nodecanvas, key, value);
            }
        }

        public void SetGlobal(NodeCanvas nodecanvas, string key, object value)
        {
            if (this.Root != null)
            {
                this.Root.SetGlobal(nodecanvas, key, value);
            }
        }

        public void SetGlobal<T>(NodeCanvas nodecanvas, string key, T value) where T:class
        {
            if (this.Root != null)
            {
                this.Root.SetGlobal(nodecanvas, key, value);
            }
        }

        public void ClearValues(NodeCanvas nodecanvas)
        {
            if (this.Root != null)
            {
                this.Root.ClearValues(nodecanvas);
            }
        }


        #endregion

        public bool isCopyCanvas(NodeCanvas nodecanvas)
        {
            if (this.Root != null)
            {
                return Root.isCopyCanvas(nodecanvas);
            }
            return false;
        }

        public void FinishFSM(FSMElement e)
        {
            if (this.Root != null)
            {
                this.Root.FinishFSM(Root.MapElement2Canvas(e));
            }
        }

        public void FinishFSM(NodeCanvas canvas)
        {
            if (this.Root != null)
            {
                this.Root.FinishFSM(Root.Map2Copy( canvas));
            }
        }

        public void ResumeAll()
        {
            if (this.Root != null)
            {
                this.Root.ResumeAll();
            }
        }

        public void PauseAll()
        {
            if (this.Root != null)
            {
                this.Root.PauseAll();
            }
        }

        public void PauseFSM(NodeCanvas canvas)
        {
            if (this.Root != null)
            {
                this.Root.PauseFSM(Root.Map2Copy(canvas));
            }
        }

        public void PauseFSM(FSMElement element)
        {
            if (this.Root != null)
            {
                this.Root.PauseFSM(Root.MapElement2Canvas(element));
            }
        }

        public void ResumeFSM(NodeCanvas canvas)
        {
            if (this.Root != null)
            {
                this.Root.ResumeFSM(Root.Map2Copy(canvas));
            }
        }


        public void ResumeFSM(FSMElement element)
        {
            if (this.Root != null)
            {
                this.Root.ResumeFSM(Root.MapElement2Canvas(element));
            }
        }

    }

}


