using System;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;

using NodeEditorFramework.Utilities;
using NodeEditorFramework.IO;

using GenericMenu = NodeEditorFramework.Utilities.GenericMenu;
#if UNITY_EDITOR
namespace NodeEditorFramework.Standard
{
	public class NodeEditorWindow : EditorWindow 
	{
		// Information about current instance
		private static NodeEditorWindow _editor;
		public static NodeEditorWindow editor { get { AssureEditor(); return _editor; } }
		public static void AssureEditor() { if (_editor == null) OpenNodeEditor(); }

		// Canvas cache
		public NodeEditorUserCache canvasCache;

        // GUI
        private Rect canvasWindowRect { get { return new Rect(0, NodeEditorInterface.GetInstance().toolbarHeight, position.width, position.height - NodeEditorInterface.GetInstance().toolbarHeight); } }

		#region General 

		/// <summary>
		/// Opens the Node Editor window and loads the last session
		/// </summary>
		[MenuItem("Window/Node Editor")]
		public static NodeEditorWindow OpenNodeEditor () 
		{
			_editor = GetWindow<NodeEditorWindow>();
			_editor.minSize = new Vector2(400, 200);

			NodeEditor.ReInit (false);
			Texture iconTexture = NodeResourceManager.LoadTexture (EditorGUIUtility.isProSkin? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
			_editor.titleContent = new GUIContent ("Node Editor", iconTexture);

			return _editor;
		}

		/*
		/// <summary>
		/// Assures that the canvas is opened when double-clicking a canvas asset
		/// </summary>
		[UnityEditor.Callbacks.OnOpenAsset(1)]
		private static bool AutoOpenCanvas(int instanceID, int line)
		{
			if (Selection.activeObject != null && Selection.activeObject is NodeCanvas)
			{
				string NodeCanvasPath = AssetDatabase.GetAssetPath(instanceID);
				OpenNodeEditor().canvasCache.LoadNodeCanvas(NodeCanvasPath);
				return true;
			}
			return false;
		}
		*/
			
		private void OnEnable()
		{
			_editor = this;
			NormalReInit();

			// Subscribe to events
			NodeEditor.ClientRepaints -= Repaint;
			NodeEditor.ClientRepaints += Repaint;
			EditorLoadingControl.justLeftPlayMode -= NormalReInit;
			EditorLoadingControl.justLeftPlayMode += NormalReInit;
			EditorLoadingControl.justOpenedNewScene -= NormalReInit;
			EditorLoadingControl.justOpenedNewScene += NormalReInit;
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			SceneView.onSceneGUIDelegate += OnSceneGUI;

            NodeEditorInterface.GetInstance().isOpenedWindows = true;

        }
		
		private void OnDestroy()
		{
			// Unsubscribe from events
			NodeEditor.ClientRepaints -= Repaint;
			EditorLoadingControl.justLeftPlayMode -= NormalReInit;
			EditorLoadingControl.justOpenedNewScene -= NormalReInit;
			SceneView.onSceneGUIDelegate -= OnSceneGUI;

            NodeEditorInterface.GetInstance().isOpenedWindows = false;

            // Clear Cache
            canvasCache.ClearCacheEvents();
		}

		private void OnLostFocus () 
		{ // Save any changes made while focussing this window
			// Will also save before possible assembly reload, scene switch, etc. because these require focussing of a different window
			canvasCache.SaveCache();
		}

		private void OnFocus () 
		{ // Make sure the canvas hasn't been corrupted externally
			NormalReInit();
		}

		private void NormalReInit()
		{
			NodeEditor.ReInit(false);
			AssureSetup();
			if (canvasCache.nodeCanvas)
				canvasCache.nodeCanvas.Validate();
		}

		private void AssureSetup()
		{
			if (canvasCache == null)
			{ // Create cache
				canvasCache = new NodeEditorUserCache(Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))));
			}
			canvasCache.AssureCanvas();

            NodeEditorInterface.GetInstance().canvasCache = canvasCache;
            NodeEditorInterface.GetInstance().ShowNotificationAction = ShowNotification;
		}

		#endregion

		#region GUI

		private void OnGUI()
		{
			// Initiation
			NodeEditor.checkInit(true);
			if (NodeEditor.InitiationError)
			{
				GUILayout.Label("Node Editor Initiation failed! Check console for more information!");
				return;
			}
			AssureEditor ();
			AssureSetup();

			// ROOT: Start Overlay GUI for popups
			OverlayGUI.StartOverlayGUI("NodeEditorWindow");

			// Begin Node Editor GUI and set canvas rect
			NodeEditorGUI.StartNodeGUI(true);
			canvasCache.editorState.canvasRect = canvasWindowRect;

			try
			{ // Perform drawing with error-handling
				NodeEditor.DrawCanvas(canvasCache.nodeCanvas, canvasCache.editorState);
			}
			catch (UnityException e)
			{ // On exceptions in drawing flush the canvas to avoid locking the UI
				canvasCache.NewNodeCanvas();
				NodeEditor.ReInit(true);
				LogMgr.LogError("Unloaded Canvas due to an exception during the drawing phase!");
				LogMgr.LogException(e);
			}


            // Draw Interface
            NodeEditorInterface.GetInstance().DrawToolbarGUI(new Rect(0, 0, Screen.width, 0));
            ActionMenuTools.GetInstance().DrawGamesMenu( Math.Min(400, Math.Max(200, (int)(position.width / 5))),position.height);

            NodeEditorInterface.GetInstance().DrawModalPanel();

			// End Node Editor GUI
			NodeEditorGUI.EndNodeGUI();

			// END ROOT: End Overlay GUI and draw popups
			OverlayGUI.EndOverlayGUI();
		}

		private void OnSceneGUI(SceneView sceneview)
		{
			AssureSetup();
			if (canvasCache.editorState != null && canvasCache.editorState.selectedNode != null)
				canvasCache.editorState.selectedNode.OnSceneGUI();
			SceneView.lastActiveSceneView.Repaint();
		}

		#endregion
	}
}
#endif