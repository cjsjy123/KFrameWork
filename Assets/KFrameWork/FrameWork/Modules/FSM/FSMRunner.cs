using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.IO;

namespace KFrameWork
{
    public partial class FSMCtr
    {

        private class FSMRunner : MonoBehaviour
        {
            private class FSMRunerState
            {
                internal FSMElement current;

                internal FSMElement laststate;

                internal bool isRunning = false;

                private List<Node> nodequeue = new List<Node>();

                private NodeCanvas cachecanvas;

                internal void Reset(NodeCanvas nodecanvas)
                {
                    this.current = null;
                    this.laststate = null;
                    this.isRunning = true;

                    if (nodecanvas != null && nodecanvas.nodes.Count > 0)
                    {
                        if (cachecanvas == null || cachecanvas != nodecanvas)
                        {
                            cachecanvas = nodecanvas;
                            for (int i = 0; i < nodecanvas.nodes.Count; ++i)
                            {
                                Node nd = nodecanvas.nodes[i];
                                this.nodequeue.Add(nd);
                            }
                            current = nodequeue[0] as FSMElement;
                        }
                        else
                        {
                            nodecanvas.nodes.Clear();
                            for (int i = 0; i < nodequeue.Count; ++i)
                            {
                                Node nd = nodequeue[i];
                                if (nd is FSMElement)
                                {
                                    ((FSMElement)nd).ResetValues();
                                }
                                nodecanvas.nodes.Add(nd);
                            }

                            current = nodequeue[0] as FSMElement;
                        }
                    }
                    else
                    {
                        LogMgr.LogErrorFormat("结点异常 :{0}", nodecanvas);
                    }
                }
            }

            private class FSMGloablValues
            {
                private Dictionary<string, short> shortdic;
                private Dictionary<string, int> intdic;
                private Dictionary<string, bool> booldic;
                private Dictionary<string, long> longdic;
                private Dictionary<string, byte> bytedic;
                private Dictionary<string, object> clsDic;

                internal bool Has(string str)
                {
                    if (shortdic != null && shortdic.ContainsKey(str))
                    {
                        return true;
                    }

                    if (intdic != null && intdic.ContainsKey(str))
                    {
                        return true;
                    }

                    if (booldic != null && booldic.ContainsKey(str))
                    {
                        return true;
                    }

                    if (longdic != null && longdic.ContainsKey(str))
                    {
                        return true;
                    }

                    if (bytedic != null && bytedic.ContainsKey(str))
                    {
                        return true;
                    }

                    if (clsDic != null && clsDic.ContainsKey(str))
                    {
                        return true;
                    }
                    return false;
                }

                internal void Push(string key, byte value)
                {
                    if (bytedic == null)
                        bytedic = new Dictionary<string, byte>();

                    bytedic.Add(key, value);
                }

                internal void Push(string key, bool value)
                {
                    if (booldic == null)
                        booldic = new Dictionary<string, bool>();

                    booldic.Add(key, value);
                }

                internal void Push(string key, int value)
                {
                    if (intdic == null)
                        intdic = new Dictionary<string, int>();

                    intdic.Add(key, value);
                }

                internal void Push(string key, short value)
                {
                    if (shortdic == null)
                        shortdic = new Dictionary<string, short>();

                    shortdic.Add(key, value);
                }

                internal void Push(string key, long value)
                {
                    if (longdic == null)
                        longdic = new Dictionary<string, long>();

                    longdic.Add(key, value);
                }

                internal void Push(string key, float value)
                {
                    LogMgr.LogError("不支持float");
                }

                internal void Push<T>(string key, T value) where T : class
                {
                    if (clsDic == null)
                        clsDic = new Dictionary<string, object>();

                    clsDic.Add(key, value);
                }

                internal byte getbyte(string key)
                {
                    byte value;
                    if (bytedic != null)
                    {
                        if (bytedic.TryGetValue(key, out value))
                            return value;
                    }

                    value = (byte)0;
                    return value;
                }

                internal bool getbool(string key)
                {
                    bool value;
                    if (booldic != null)
                    {
                        if (booldic.TryGetValue(key, out value))
                            return value;
                    }

                    value = false;
                    return value;
                }

                internal short getshort(string key)
                {
                    short value;
                    if (shortdic != null)
                    {
                        if (shortdic.TryGetValue(key, out value))
                            return value;
                    }

                    value = -1;
                    return value;
                }

                internal int getint(string key)
                {
                    int value;
                    if (intdic != null)
                    {
                        if (intdic.TryGetValue(key, out value))
                            return value;
                    }

                    value = -1;
                    return value;
                }

                internal long getlong(string key)
                {
                    long value;
                    if (longdic != null)
                    {
                        if (longdic.TryGetValue(key, out value))
                            return value;
                    }

                    value = -1;
                    return value;
                }

                internal object getObject(string key)
                {
                    object value;
                    if (clsDic != null)
                    {
                        if (clsDic.TryGetValue(key, out value))
                            return value;
                    }

                    value = null;
                    return value;
                }
            }

            private List< NodeCanvas> runners = new List<NodeCanvas>();

            private Dictionary<NodeCanvas, FSMRunerState> nodestates = new Dictionary<NodeCanvas, FSMRunerState>();

            private Dictionary<NodeCanvas, FSMGloablValues> globalvals = new Dictionary<NodeCanvas, FSMGloablValues>();
            #region global
            internal bool GetGlobal(NodeCanvas nodecanvas, string key, out int outvalue)
            {
                if (nodecanvas != null && this.globalvals.ContainsKey(nodecanvas))
                {
                    FSMGloablValues fbv = this.globalvals[nodecanvas];
                    if (fbv.Has(key))
                    {
                        outvalue = fbv.getint(key);
                        return true;
                    }
                }

                outvalue = -1;
                return false;
            }

            internal bool GetGlobal(NodeCanvas nodecanvas, string key, out bool outvalue)
            {
                if (nodecanvas != null && this.globalvals.ContainsKey(nodecanvas))
                {
                    FSMGloablValues fbv = this.globalvals[nodecanvas];
                    if (fbv.Has(key))
                    {
                        outvalue = fbv.getbool(key);
                        return true;
                    }
                }

                outvalue = false;
                return false;
            }

            internal bool GetGlobal(NodeCanvas nodecanvas, string key, out byte outvalue)
            {
                if (nodecanvas != null && this.globalvals.ContainsKey(nodecanvas))
                {
                    FSMGloablValues fbv = this.globalvals[nodecanvas];
                    if (fbv.Has(key))
                    {
                        outvalue = fbv.getbyte(key);
                        return true;
                    }
                }

                outvalue =0 ;
                return false;
            }

            internal bool GetGlobal(NodeCanvas nodecanvas, string key, out short outvalue)
            {
                if (nodecanvas != null && this.globalvals.ContainsKey(nodecanvas))
                {
                    FSMGloablValues fbv = this.globalvals[nodecanvas];
                    if (fbv.Has(key))
                    {
                        outvalue = fbv.getshort(key);
                        return true;
                    }
                }

                outvalue = -1;
                return false;
            }

            internal bool GetGlobal(NodeCanvas nodecanvas, string key, out string outvalue)
            {
                if (nodecanvas != null && this.globalvals.ContainsKey(nodecanvas))
                {
                    FSMGloablValues fbv = this.globalvals[nodecanvas];
                    if (fbv.Has(key))
                    {
                        outvalue = fbv.getObject(key) as string;
                        return true;
                    }
                }

                outvalue = "";
                return false;
            }

            internal bool GetGlobal(NodeCanvas nodecanvas, string key, out long outvalue)
            {
                if (nodecanvas != null && this.globalvals.ContainsKey(nodecanvas))
                {
                    FSMGloablValues fbv = this.globalvals[nodecanvas];
                    if (fbv.Has(key))
                    {
                        outvalue = fbv.getlong(key) ;
                        return true;
                    }
                }

                outvalue = -1;
                return false;
            }

            internal bool GetGlobal(NodeCanvas nodecanvas, string key, out System.Object outvalue)
            {
                if (nodecanvas != null && this.globalvals.ContainsKey(nodecanvas))
                {
                    FSMGloablValues fbv = this.globalvals[nodecanvas];
                    if (fbv.Has(key))
                    {
                        outvalue = fbv.getObject(key);
                        return true;
                    }
                }

                outvalue = null;
                return false;
            }

            internal bool GetGlobal<T>(NodeCanvas nodecanvas, string key, out T outvalue) where T:class
            {
                if (nodecanvas != null && this.globalvals.ContainsKey(nodecanvas))
                {
                    FSMGloablValues fbv = this.globalvals[nodecanvas];
                    if (fbv.Has(key))
                    {
                        outvalue = (T)fbv.getObject(key);
                        return true;
                    }
                }

                outvalue = null;
                return false;
            }

            internal void SetGlobal(NodeCanvas nodecanvas, string key, int value)
            {
                if (nodecanvas == null)
                    return;
                if (this.globalvals.ContainsKey(nodecanvas))
                {
                    this.globalvals[nodecanvas].Push(key, value);
                }
                else
                {
                    FSMGloablValues fbv = new FSMGloablValues();
                    this.globalvals[nodecanvas] = fbv;
                    fbv.Push(key,value);
                }
            }

            internal void SetGlobal(NodeCanvas nodecanvas, string key, short value)
            {
                if (nodecanvas == null)
                    return;
                if (this.globalvals.ContainsKey(nodecanvas))
                {
                    this.globalvals[nodecanvas].Push(key, value);
                }
                else
                {
                    FSMGloablValues fbv = new FSMGloablValues();
                    this.globalvals[nodecanvas] = fbv;
                    fbv.Push(key, value);
                }
            }

            internal void SetGlobal(NodeCanvas nodecanvas, string key, byte value)
            {
                if (nodecanvas == null)
                    return;
                if (this.globalvals.ContainsKey(nodecanvas))
                {
                    this.globalvals[nodecanvas].Push(key, value);
                }
                else
                {
                    FSMGloablValues fbv = new FSMGloablValues();
                    this.globalvals[nodecanvas] = fbv;
                    fbv.Push(key, value);
                }
            }

            internal void SetGlobal(NodeCanvas nodecanvas, string key, bool value)
            {
                if (nodecanvas == null)
                    return;
                if (this.globalvals.ContainsKey(nodecanvas))
                {
                    this.globalvals[nodecanvas].Push(key, value);
                }
                else
                {
                    FSMGloablValues fbv = new FSMGloablValues();
                    this.globalvals[nodecanvas] = fbv;
                    fbv.Push(key, value);
                }
            }

            internal void SetGlobal(NodeCanvas nodecanvas, string key, long value)
            {
                if (nodecanvas == null)
                    return;
                if (this.globalvals.ContainsKey(nodecanvas))
                {
                    this.globalvals[nodecanvas].Push(key, value);
                }
                else
                {
                    FSMGloablValues fbv = new FSMGloablValues();
                    this.globalvals[nodecanvas] = fbv;
                    fbv.Push(key, value);
                }
            }

            internal void SetGlobal(NodeCanvas nodecanvas, string key, string value)
            {
                if (nodecanvas == null)
                    return;
                if (this.globalvals.ContainsKey(nodecanvas))
                {
                    this.globalvals[nodecanvas].Push(key, value);
                }
                else
                {
                    FSMGloablValues fbv = new FSMGloablValues();
                    this.globalvals[nodecanvas] = fbv;
                    fbv.Push(key, value);
                }
            }

            internal void SetGlobal(NodeCanvas nodecanvas, string key, System.Object value)
            {
                if (nodecanvas == null)
                    return;

                if (this.globalvals.ContainsKey(nodecanvas))
                {
                    this.globalvals[nodecanvas].Push(key, value);
                }
                else
                {
                    FSMGloablValues fbv = new FSMGloablValues();
                    this.globalvals[nodecanvas] = fbv;
                    fbv.Push(key, value);
                }
            }

            internal void ClearValues(NodeCanvas nodecanvas)
            {
                if(nodecanvas != null)
                    globalvals.Remove(nodecanvas);
            }

            #endregion

            internal NodeCanvas AddNode(NodeCanvas nodecanvas)
            {
                if (this.runners.Count == 0)
                {
                    //init node runtime
                    ConnectionPortStyles.FetchConnectionPortStyles();
                    NodeTypes.FetchNodeTypes();
                    NodeCanvasManager.FetchCanvasTypes();
                    ConnectionPortManager.FetchNodeConnectionDeclarations();
                    ImportExportManager.FetchIOFormats();

                    // Setup Callback system
                    NodeEditorCallbacks.SetupReceivers();
                    NodeEditorCallbacks.IssueOnEditorStartUp();
                }

                if (nodecanvas == null)
                {
                    return null;
                }

                NodeCanvas copycanvas = NodeEditorSaveManager.CreateWorkingCopy(nodecanvas, true);
                //push copy to runners
                runners.Add( copycanvas);

                Restart(copycanvas);
                return copycanvas;
            }

            internal bool isRunning(NodeCanvas nodecanvas)
            {
                if (nodecanvas == null)
                {
                    return false;
                }

                if (this.nodestates.ContainsKey(nodecanvas))
                {
                    return this.nodestates[nodecanvas].isRunning;
                }
                return false;
            }

            internal void RemoveNode(NodeCanvas nodecanvas)
            {
                if (nodecanvas == null)
                {
                    return;
                }

                if( runners.Remove(nodecanvas) == false)
                {
                    LogMgr.LogErrorFormat("Not Equal for this canvas :{0}", nodecanvas);
                }

            }

            internal void Clear()
            {
                this.runners.Clear();
                this.nodestates.Clear();
                this.globalvals.Clear();
            }

            internal List<NodeCanvas> GetAllFSM()
            {
                List<NodeCanvas> copy= ListPool.TrySpawn<List<NodeCanvas>>();
                copy.AddRange(this.runners);
                return copy;
            }

            internal void CallFinishCanvas(NodeCanvas nodecanvas)
            {
                if (nodecanvas != null)
                {
                    for (int i = 0; i < nodecanvas.nodes.Count; ++i)
                    {
                        var nd = nodecanvas.nodes[i];
                        if (nd is FSMElement)
                        {
                            ((FSMElement)nd).OnCanvasFinished();
                        }
                    }
                }
            }

            internal void FinishFSM(NodeCanvas canvas)
            {
                if (canvas != null && this.nodestates.ContainsKey(canvas))
                {
                    this.nodestates[canvas].Reset(canvas);
                    this.nodestates[canvas].isRunning = false;
                    this.CallFinishCanvas(canvas);
                }
            }

            internal void ResumeAll()
            {
                List<NodeCanvas> values = GetAllFSM();
                for (int i = 0; i < values.Count; ++i)
                {
                    if (nodestates.ContainsKey(values[i]))
                    {
                        nodestates[values[i]].isRunning = true;
                    }
                }

                ListPool.TryDespawn(values);
            }

            internal void PauseAll()
            {
                List<NodeCanvas> values = GetAllFSM();
                for (int i = 0; i < values.Count; ++i)
                {
                    if (nodestates.ContainsKey(values[i]))
                    {
                        nodestates[values[i]].isRunning = false;
                    }
                }

                ListPool.TryDespawn(values);
            }

            internal void PauseFSM(NodeCanvas canvas)
            {
                if (canvas != null && this.nodestates.ContainsKey(canvas))
                {
                    this.nodestates[canvas].isRunning = false;
                }
            }

            internal void ResumeFSM(NodeCanvas canvas)
            {
                if (canvas != null && this.nodestates.ContainsKey(canvas))
                {
                    this.nodestates[canvas].isRunning = true;
                }
            }

            internal FSMElement CurrentFSMElement(NodeCanvas outnodecanvas)
            {
                if (outnodecanvas == null)
                    return null;

                if (nodestates.ContainsKey(outnodecanvas))
                {
                    return nodestates[outnodecanvas].current;
                }
                else
                    return null;
            }

            internal FSMElement LastFSMElement(NodeCanvas outnodecanvas)
            {
                if (outnodecanvas == null)
                    return  null;
                if (nodestates.ContainsKey(outnodecanvas))
                {
                    return nodestates[outnodecanvas].laststate;
                }
                else
                    return null;
            }

            internal void Restart(NodeCanvas outnodecanvas)
            {
                if (outnodecanvas == null)
                    return;
                FSMRunerState state = null;
                if (nodestates.ContainsKey(outnodecanvas))
                {
                    state = nodestates[outnodecanvas];
                }
                else
                {
                    state = new FSMRunerState();
                    nodestates[outnodecanvas] = state;
                }

                state.Reset(outnodecanvas);
                this.ClearValues(outnodecanvas);
            }

            internal NodeCanvas MapElement2Canvas(FSMElement e)
            {
                List<NodeCanvas> values = GetAllFSM();

                for (int i =0; i < values.Count;++i)
                {
                    NodeCanvas canvas = values[i];
                    for (int j = 0; j < canvas.nodes.Count; ++j)
                    {
                        Node node = canvas.nodes[j];
                        if (node == e)
                        {
                            ListPool.TryDespawn(values);
                            return canvas;
                        }
                    }
                }
                ListPool.TryDespawn(values);
                return null;
            }

            internal bool isCopyCanvas(NodeCanvas nodecanvas)
            {
                if (runners.Contains(nodecanvas))
                {
                    return true;
                }
                return false;
            }

            internal NodeCanvas Map2Copy(NodeCanvas nodecanvas)
            {
                if (isCopyCanvas(nodecanvas))
                {
                    return nodecanvas;
                }
                else 
                {
                    LogMgr.LogError("Please Use Copy nodecanvas ");
                    return nodecanvas;
                }
            }

            internal void StartUpdate()
            {
                if (mIns.EnableSelfUpdate)
                {
                    MainLoop.getInstance().RegisterLoopEvent(MainLoopEvent.Update, UpdateEvent);
                }
            }

            internal void EndUpdate()
            {
                MainLoop.getInstance().UnRegisterLoopEvent(MainLoopEvent.Update, UpdateEvent);
            }

            void UpdateEvent(int cnt)
            {
                UpdateInFrame(GameSyncCtr.mIns.RenderFrameCount);
            }

            internal void UpdateInFrame(long cnt)
            {
                if (runners.Count == 0)
                    return;

                List<NodeCanvas> values = GetAllFSM();
                for (int i = 0; i < values.Count; ++i)
                {
                    NodeCanvas nodecanvas = values[i];
                    if (nodestates.ContainsKey(nodecanvas))
                    {
                        FSMRunerState runningstate = nodestates[nodecanvas];
                        if (runningstate.isRunning && runningstate.current != null)
                        {
                            FSMElement current = runningstate.current;
                            FSMElement laststate = runningstate.laststate;

                            if (laststate != null)
                            {
                                if (laststate != current)
                                {
                                    // LogMgr.LogError("this is  " + nodecanvas);
                                    ScriptCommand exitcmd = ScriptCommand.Create((int)FrameWorkCmdDefine.FSMCallExit);
                                    exitcmd.CallParams.WriteObject(laststate);
                                    exitcmd.ExcuteAndRelease();

                                    ScriptCommand entercmd = ScriptCommand.Create((int)FrameWorkCmdDefine.FSMCallEnter);
                                    entercmd.CallParams.WriteObject(current);
                                    entercmd.ExcuteAndRelease();
                                }
                            }
                            else
                            {
                                // LogMgr.LogError("this is  " + nodecanvas);
                                ScriptCommand cmd = ScriptCommand.Create((int)FrameWorkCmdDefine.FSMCallEnter);
                                cmd.CallParams.WriteObject(current);
                                cmd.ExcuteAndRelease();
                            }

                            runningstate.laststate = current;

                            bool ret = current.UpdateFrameInFSM(cnt);
                            if (!ret && runningstate.isRunning)
                            {
                                FSMElement next = current.SelectForNext();
                                if (next != null)
                                {
                                    runningstate.current = next;
                                }
                                else
                                {
                                    ScriptCommand exitcmd = ScriptCommand.Create((int)FrameWorkCmdDefine.FSMCallExit);
                                    exitcmd.CallParams.WriteObject(current);
                                    exitcmd.ExcuteAndRelease();

                                    runningstate.isRunning = false;
                                    this.CallFinishCanvas(nodecanvas);
                                }
                            }
                        }
                    }
                }

                ListPool.TryDespawn(values);
            }
        }
    }
}


