using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NodeEditorFramework;
using NodeEditorFramework.Standard;
using NodeEditorFramework.Utilities;
using KFrameWork;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using System.Reflection;


[TypeInit(-10)]
/// <summary>
/// cache for windows
/// </summary>
public class ActionMenuTools {

    private static ActionMenuTools mIns;

    private string toolasset = Application.dataPath + "/MenuTools.asset";

    private string baseassetpath ="Assets/MenuTools.asset";

    private Dictionary<string, Action<float, float>> menuactions = new Dictionary<string, Action<float, float>>();

    private Dictionary<string, UnityEditor.GenericMenu.MenuFunction> folderdict = new Dictionary<string, UnityEditor.GenericMenu.MenuFunction>();

    private List<NodeCanvas> Loadedcanvases = new List<NodeCanvas>();

    private MenuToolsAsset asset;

    private string CurrentCustomMenu;

    private Vector2 clistPos;

    public static ActionMenuTools GetInstance()
    {
        if (mIns == null)
        {
            mIns = new ActionMenuTools();
            mIns.TryLoadAsset();
        }
        return mIns;
    }

    public ActionMenuTools()
    {
        EditorLoadingControl.justEnteredPlayMode -= EnterPlay;
        EditorLoadingControl.justEnteredPlayMode += EnterPlay;
        EditorLoadingControl.justLeftPlayMode -= ExitPlay;
        EditorLoadingControl.justLeftPlayMode += ExitPlay;
    }

    void TryLoadAsset()
    {
        if (File.Exists(toolasset))
        {
            asset = AssetDatabase.LoadAssetAtPath<MenuToolsAsset>(baseassetpath);
            if (asset == null)
            {
                LogMgr.LogError("asset is Null");
            }
        }
        else
        {
            asset = ScriptableObject.CreateInstance<MenuToolsAsset>();
            AssetDatabase.CreateAsset(asset, baseassetpath);
        }

        RefreshAsset();

    }

    void RefreshAsset()
    {
        if (asset != null)
        {
            foreach (var data in asset.data)
            {
                Type type = Type.GetType(data.clsname);
                MethodInfo method = type.GetMethod(data.drawfuncname, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                Action<float,float> drawfunc = (Action<float, float>)Delegate.CreateDelegate(typeof(Action<float, float>), method);

                Action foldercbk = null;
                if (!string.IsNullOrEmpty(data.loadfuncname))
                {
                    MethodInfo funcmethod = type.GetMethod(data.loadfuncname, BindingFlags.Static| BindingFlags.Public | BindingFlags.NonPublic);
                    if (funcmethod != null)
                    {
                        foldercbk = (Action)Delegate.CreateDelegate(typeof(Action), funcmethod);
                    }
                }

                this.menuactions[data.Keyname] = drawfunc;
                this.folderdict[data.Keyname] = () =>
                {
                    if (CurrentCustomMenu == null || !CurrentCustomMenu.Equals(data.Keyname))
                    {
                        CurrentCustomMenu = data.Keyname;
                        if (foldercbk != null)
                        {
                            foldercbk();
                        }
                    }
                };
            }
            EditorUtility.SetDirty(asset);
        }
    }

    void EnterPlay()
    {
        if(NodeEditorInterface.GetInstance().canvasCache != null)
            NodeEditorInterface.GetInstance().canvasCache.NewNodeCanvas(typeof(EmptyCanvas));
        this.Loadedcanvases.Clear();
        CurrentCustomMenu = null;
    }

    void ExitPlay()
    {
        if(NodeEditorInterface.GetInstance().canvasCache != null)
            NodeEditorInterface.GetInstance().canvasCache.NewNodeCanvas(typeof(EmptyCanvas));
        this.Loadedcanvases.Clear();
        CurrentCustomMenu = null;
    }

    public Dictionary<string, Action<float, float>> Getmenuactions()
    {
        return menuactions;
    }

    public Dictionary<string, UnityEditor.GenericMenu.MenuFunction> Getfolderdict()
    {
        return folderdict;
    }


    public void AddMenuaction(string key, NoteEditorMenuAttribute att,MethodInfo method)
    {
        if (asset != null)
        {
            var gamedata = this.asset.data.Find(p => p.Keyname.Equals(key));
            if (gamedata != null)
            {
                gamedata.Keyname = key;
                gamedata.clsname = method.DeclaringType.FullName;
                gamedata.drawfuncname = method.Name;
                gamedata.loadfuncname = att.func;
            }
            else
            {
                gamedata = new MenuToolsAsset.clstoolasset();
                gamedata.Keyname = key;
                gamedata.clsname = method.DeclaringType.FullName;
                gamedata.drawfuncname = method.Name;
                gamedata.loadfuncname = att.func;
                asset.data.Add(gamedata);
            }

            RefreshAsset();
        }
    }


    public void DrawGamesMenu(float width, float height)
    {
        foreach (var kv in menuactions)
        {
            if (CurrentCustomMenu != null && this.CurrentCustomMenu.Equals(kv.Key))
            {
                GUILayout.BeginArea(new Rect(0, 17, width, height), GUI.skin.box);
                GetInstance().clistPos = GUILayout.BeginScrollView(GetInstance().clistPos);
                kv.Value(width, height);

                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }
    }

    static void LoadFSM()
    {
        if (FSMCtr.mIns != null)
        {
            GetInstance().ReloadCanvasList(FSMCtr.mIns.GetAllFSM());
        }
    }

    void ReloadCanvasList(List<NodeCanvas> list)
    {
        this.Loadedcanvases.Clear();

        if (list != null && list.Count > 0)
        {
            this.Loadedcanvases.AddRange(list);
            NodeEditorInterface.GetInstance().canvasCache.LoadNodeCanvas(list[0], false);
        }

        ListPool.TryDespawn(list);
    }

    [NoteEditorMenuAttribute("Load FSM", "LoadFSM")]
    static void DrawFSMlist(float width, float height)
    {
        ActionMenuTools tools = ActionMenuTools.GetInstance();
        HashSet<NodeCanvas> clist = new HashSet<NodeCanvas>(tools.Loadedcanvases);

        NodeCanvas currentcanvas = NodeEditorInterface.GetInstance(). canvasCache.nodeCanvas;

        GUILayout.Label(new GUIContent("Current load Canvas Cnt " + clist.Count));
        if (GUILayout.Button("Reload FSM"))
        {
            LoadFSM();
        }

        int i = 0;
        foreach (NodeCanvas loaded in clist)
        {
            if (loaded != null)
            {
                RTEditorGUI.Toggle(currentcanvas != null && currentcanvas == loaded, new GUIContent("[" + i + "]"));
                UnityEditor.EditorGUILayout.ObjectField(loaded, typeof(NodeCanvas), false);
                if (GUILayout.Button("load"))
                {
                    NodeEditorInterface.GetInstance().canvasCache.LoadNodeCanvas(loaded, false);
                }

                i++;
            }
        }

    }
}

#endif
