//--------------------------------------------------------------------------//
// Copyright 2025-2026 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

#if UNITY_2022_3_OR_NEWER
#define RAPIDASSETRELOAD_FOCUSCHANGED
#endif

#if UNITY_6000_0_OR_NEWER && WORKFLOW_UITK
#define RAPIDASSETRELOAD_UITK
#endif

#if UNITY_6000_0_OR_NEWER && WORKFLOW_UITK && WORKFLOW_UITK_BUILDER
#define RAPIDASSETRELOAD_UITK_BUILDER
#endif

#if UNITY_2022_1_OR_NEWER
#define RAPIDASSETRELOAD_GETDEFAULTIMPORTER
#endif

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;

namespace ChocDino.Workflow.RapidAssetReload.Editor
{
	/// <summary>
	/// </summary>
	public class RapidAssetReload : EditorWindow
	{
		internal const string ProductTitle = "Rapid Asset Reload";
		private const string KeyPrefix = "ChocDino.Workflow.RapidAssetReload.";
		private const string IsWatchingEnabledKey = KeyPrefix + "IsWatchingEnabled";

		private const string ExtensionFilter = "*.*";

		private ConcurrentQueue<string> _watcherModifiedPaths = new ConcurrentQueue<string>();
		private FileSystemWatcher _watcher;
		private List<string> _changedPaths = new List<string>(4);
		private string _assetsRootFullPath;
		private bool _unityFocused;

#if RAPIDASSETRELOAD_UITK
		private static System.Type _baseVisualElementPanelType;
		private static System.Reflection.MethodInfo _baseVisualElementPanelRepaint;
#endif
#if RAPIDASSETRELOAD_UITK_BUILDER
		private static System.Type _builderPanelWindowType;
		private static System.Reflection.MethodInfo _builderPanelWindowOnEnable;
		private static System.Reflection.MethodInfo _builderPanelWindowOnDisable;
#endif

		private bool UnityHasFocus
		{
#if RAPIDASSETRELOAD_FOCUSCHANGED
			get => EditorApplication.isFocused;
#else
			get => UnityEditorInternal.InternalEditorUtility.isApplicationActive;
#endif
		}

		internal static bool IsWatchingEnabled
		{
			get { return EditorPrefs.GetBool(IsWatchingEnabledKey, true); }
			set { EditorPrefs.SetBool(IsWatchingEnabledKey, value); }
		}

		public static bool TestCanOpenFile(string fullPath)
		{
			// Some files may take a long time to write to, so we have to make sure the writing it finished.
			bool result = false;
			try
			{
				using (Stream stream = new System.IO.FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite))
				{
					result = true;
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		[MenuItem("Window/General/Chocolate Dinosaur/Rapid Asset Reload")]
		internal static void AddWindow()
		{
			EditorWindow.GetWindow<RapidAssetReload>();
		}

		private void OnEnable()
		{
			this.titleContent = new GUIContent(ProductTitle);
			this.wantsMouseMove = false;
			this.minSize = new Vector2(128f, 32);

			_assetsRootFullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
			_unityFocused = UnityHasFocus;

			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;

#if RAPIDASSETRELOAD_FOCUSCHANGED
			EditorApplication.focusChanged -= OnFocusChanged;
			EditorApplication.focusChanged += OnFocusChanged;
#endif

#if RAPIDASSETRELOAD_UITK
			if (_baseVisualElementPanelType == null)
			{
				var assembly = typeof(UnityEngine.UIElements.IRuntimePanel).Assembly;
				if (assembly != null)
				{
					_baseVisualElementPanelType = assembly.GetType("UnityEngine.UIElements.BaseVisualElementPanel");
					if (_baseVisualElementPanelType != null)
					{
						_baseVisualElementPanelRepaint = _baseVisualElementPanelType.GetMethod("Repaint", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
					}
				}
			}
#endif
#if RAPIDASSETRELOAD_UITK_BUILDER
			if (_builderPanelWindowType == null)
			{
				var assembly = System.Reflection.Assembly.Load("UnityEditor.UIBuilderModule");
				if (assembly != null)
				{
					_builderPanelWindowType = assembly.GetType("Unity.UI.Builder.BuilderPaneWindow");
					if (_builderPanelWindowType != null)
					{
						_builderPanelWindowOnEnable = _builderPanelWindowType.GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
						_builderPanelWindowOnDisable = _builderPanelWindowType.GetMethod("OnDisable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					}
				}
			}
#endif

			if (ShouldUpdateWatch())
			{
				StartWatcher();
			}
		}

		private void OnDisable()
		{
			if (_watcher != null)
			{
				_watcher.Dispose();
				_watcher = null;
			}

#if RAPIDASSETRELOAD_FOCUSCHANGED
			EditorApplication.focusChanged -= OnFocusChanged;
#endif

			EditorApplication.update -= OnUpdate;
		}

		private void OnDestroy()
		{
			//Debug.Log("destroy");
		}

		void OnGUI()
		{
			bool isWatchingEnabled = IsWatchingEnabled;

			string text = isWatchingEnabled ? "Enabled" : "Disabled";
			Color color = isWatchingEnabled ? Color.green : Color.red;
			if (_watcher != null)
			{
				color = (Color.red + Color.yellow * 0.85f);
				text = "Enabled - WATCHING";
			}

			GUI.color = color;
			if (GUILayout.Button(text))
			{
				IsWatchingEnabled = !IsWatchingEnabled;
				if (ShouldUpdateWatch())
				{
					StartWatcher();
				}
				else
				{
					StopWatcher();
					RestoreAsyncShaderCompile();
				}
			}
			GUI.color = Color.white;
		}

		// Callback function called from EditorApplication.focusChanged
		private void OnFocusChanged(bool focus)
		{
			CheckUnityFocus(false);
		}

		void CheckUnityFocus(bool forced)
		{
			bool unityFocused = UnityHasFocus;
			if (unityFocused != _unityFocused || forced)
			{
				_unityFocused = unityFocused;
				//Debug.Log("Focus changed: " + _unityFocused);

				if (_unityFocused)
				{
					StopWatcher();
					RestoreAsyncShaderCompile();
					Repaint();
				}
				else if (IsWatchingEnabled)
				{
					StartWatcher();
					Repaint();
				}
			}
		}

		// Callback function called from EditorApplication.update
		private void OnUpdate()
		{
			//Debug.Log("onupdate");
			if (ShouldUpdateWatch())
			{
				CheckItems();
			}
		}

		private bool ShouldUpdateWatch()
		{
			return (IsWatchingEnabled && !UnityHasFocus);
		}

		private void StopWatcher()
		{
			if (_watcher != null)
			{
				_watcher.Dispose();
				_watcher = null;
			}
			_watcherModifiedPaths = null;
			_changedPaths.Clear();
		}

		public void StartWatcher()
		{
			_asyncShaderCompileFrameCount = 0;

			Debug.Assert(!UnityHasFocus);
			Debug.Assert(_watcher == null);

			_watcherModifiedPaths = new ConcurrentQueue<string>();

			_watcher = new FileSystemWatcher();
			_watcher.Path = Application.dataPath;
			_watcher.IncludeSubdirectories = true;
			_watcher.NotifyFilter = NotifyFilters.LastWrite;
			// TODO: allow users to specify filter
			_watcher.Filter = ExtensionFilter;
			_watcher.Changed += (_, e) => { _watcherModifiedPaths.Enqueue(e.FullPath); };
			_watcher.EnableRaisingEvents = true;
			// TODO: Toggle EnableRaisingEvents when focus changes
		}

		/*
				private static readonly string[] FbxImporterExtensions = { ".blend", ".c4d", ".dae", ".dxf", ".fbx", ".jas", ".lxo", ".ma", ".mb", ".max", ".obj" };
				private static readonly string[] Mesh3DsImporterExtensions = new[] { ".3ds" };
				private static readonly string[] SketchupImporterExtensions = new[] { ".skp" };
				private static readonly string[] SpeedTreeImporterExtensions = new[] { ".spm", ".st" };
				private static readonly string[] SubstanceImporterExtensions = new[] { ".sbsar" };
				private const string[] SupportedAssetExtensions = new[] { ".shader", ".cginc", "", }


				private static bool IsSupportedAssetExtension(string ext)
				{
					ext = ext.ToLower();
					switch
				}*/

		private bool GetAssetRelativePath(string fullPath, ref string relativePath)
		{
			// Convert to full path to get the same directory seperators as _assetsRootFullPath
			fullPath = Path.GetFullPath(fullPath);

			if (fullPath.StartsWith(_assetsRootFullPath))
			{
				relativePath = fullPath.Substring(_assetsRootFullPath.Length);
				return true;
			}

			return false;
		}

		private bool CollectChanges()
		{
			if (_watcherModifiedPaths != null)
			{
				string[] ignoreExtensions = UserPreferences.IgnoreExtensions.Split(';');

				int count = _watcherModifiedPaths.Count;
				if (count > 0)
				{
					for (int i = 0; i < count; i++)
					{
						string path;
						if (_watcherModifiedPaths.TryDequeue(out path))
						{
							// Check if the file exists (and is not a directory)
							if (File.Exists(path))
							{
								// Check if the extension is allowed for importing.
								{
									string ext = Path.GetExtension(path);
									if (ext.Length > 0)
									{
										ext = ext.Substring(1); // skip over the '.'
										if (ext.Length > 0)
										{
											ext = ext.ToLower();
										}
									}

									if (System.Array.IndexOf(ignoreExtensions, ext) >= 0)
									{
										continue;
									}
								}

								string assetPath = string.Empty;
								if (GetAssetRelativePath(path, ref assetPath))
								{
									if (!_changedPaths.Contains(assetPath))
									{

#if RAPIDASSETRELOAD_GETDEFAULTIMPORTER
										//bool isAsset = AssetDatabase.AssetPathExists(assetPath);
										var importer = AssetDatabase.GetDefaultImporter(assetPath);
										if (importer != null)
										{
											if (TestCanOpenFile(path))
											{
												_changedPaths.Add(assetPath);
											}
										}
#else
										if (TestCanOpenFile(path))
										{
											_changedPaths.Add(assetPath);
										}
										else
										{
										}
#endif
									}
									else
									{
									}
								}
							}
						}
						else
						{
							break;
						}
					}
				}
			}
			return _changedPaths.Count > 0;
		}

		private int _asyncShaderCompileFrameCount = 0;

		private bool ImportChanges()
		{
			if (_changedPaths.Count > 0)
			{
				DisableAsyncShaderCompile();

				//ImportAssetOptions importOptions = ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUncompressedImport | ImportAssetOptions.ForceUpdate;
				ImportAssetOptions importOptions = ImportAssetOptions.ForceUncompressedImport | ImportAssetOptions.ForceUpdate;
				//ImportAssetOptions importOptions = ImportAssetOptions.Default;

				int secondsToday = (int)System.Math.Round((System.DateTime.Now - System.DateTime.Today).TotalSeconds);

				if (_changedPaths.Count == 1)
				{
					if (UserPreferences.LogAssetImports)
					{
						Debug.Log("[RapidAssetReload] " + secondsToday + " Import " + System.IO.Path.GetFileName(_changedPaths[0]) + " (" + _changedPaths[0] + ")");
					}
					AssetDatabase.ImportAsset(_changedPaths[0], importOptions);
				}
				else
				{
					AssetDatabase.StartAssetEditing();
					for (int i = 0; i < _changedPaths.Count; i++)
					{
						if (UserPreferences.LogAssetImports)
						{
							Debug.Log("[RapidAssetReload] " + secondsToday + " Import " + System.IO.Path.GetFileName(_changedPaths[i]) + " (" + _changedPaths[i] + ")");
						}
						AssetDatabase.ImportAsset(_changedPaths[i], importOptions);
					}
					AssetDatabase.StopAssetEditing();
				}
				_changedPaths.Clear();

				return true;
			}
			return false;
		}

		private void RefreshUnity()
		{
			// When the editor isn't in play mode, we have to force repaint
			if (!EditorApplication.isPlaying)
			{
				//EditorApplication.delayCall -= OnDelayCall;
				//EditorApplication.delayCall += c;
				//EditorApplication.Step();
				//EditorApplication.QueuePlayerLoopUpdate();

				UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

#if RAPIDASSETRELOAD_UITK
				// For UITK this forces the UI to render in the GameView and UI Builder.
				var documents = Resources.FindObjectsOfTypeAll<UnityEngine.UIElements.UIDocument>();
				foreach (var doc in documents)
				{
					if (doc.isActiveAndEnabled)
					{
						// Toggling enabled forces it to redraw
						doc.enabled = false;
						doc.enabled = true;

						// NOTE: Before Unity 6 the runtimePanel didn't exist and the GameView with UIDocument refreshed just fine.
						if (doc.runtimePanel != null)
						{
							if (_baseVisualElementPanelRepaint != null)
							{
								Event repaintEvent = new Event();
								repaintEvent.type = EventType.Repaint;
								_baseVisualElementPanelRepaint.Invoke(doc.runtimePanel, new object[] { repaintEvent });
							}
						}
					}
				}
#endif

#if RAPIDASSETRELOAD_UITK_BUILDER
				// This forces UI Builder window to refresh.
				if (_builderPanelWindowType != null)
				{
					var editorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
					foreach (var editorWindow in editorWindows)
					{
						if (editorWindow.GetType().FullName == "Unity.UI.Builder.Builder")
						{
							// Disable then Enable to force refresh.
							if (_builderPanelWindowOnDisable != null)
							{
								_builderPanelWindowOnDisable.Invoke(editorWindow, null);
							}
							if (_builderPanelWindowOnEnable != null)
							{
								_builderPanelWindowOnEnable.Invoke(editorWindow, null);
							}
						}
					}
				}
#endif

				/*var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
				foreach (var window in windows)
				{
					if (window.GetType().FullName == "UnityEditor.GameView")
					{
						window.Repaint();
					}
				}*/

				// We need to call this UGUI to update every time, especially 
				// when changing shaders or textures.
				if (UserPreferences.RefreshUGUICanvases)
				{
					Canvas.ForceUpdateCanvases();
				}

				SceneView.RepaintAll();
				/*if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
				{
					UnityEditorInternal.InternalEditorUtility.OnGameViewFocus(true);
				}*/
			}

			Repaint();
		}

		private void CheckItems()
		{
			if (ShouldUpdateWatch())
			{
				if (CollectChanges())
				{
					if (ImportChanges())
					{
						RefreshUnity();
					}
				}
			}
		}

		// This is called 10 times a second
		private void OnInspectorUpdate()
		{
#if !RAPIDASSETRELOAD_FOCUSCHANGED
			CheckUnityFocus(false);
			//Repaint(); 
#endif
		}

		private const int DisableSyncShaderCompileFrameCount = 3;

		private void DisableAsyncShaderCompile()
		{
			if (EditorSettings.asyncShaderCompilation)
			{
				Debug.Assert(_asyncShaderCompileFrameCount == 0);
				_asyncShaderCompileFrameCount = DisableSyncShaderCompileFrameCount;
				Debug.Assert(_asyncShaderCompileFrameCount > 1);
				EditorSettings.asyncShaderCompilation = false;
			}
		}

		private void RestoreAsyncShaderCompile()
		{
			if (DidDisableAsyncShaderCompile())
			{
				_asyncShaderCompileFrameCount = 0;
				EditorSettings.asyncShaderCompilation = true;
			}
		}

		private bool DidDisableAsyncShaderCompile()
		{
			return _asyncShaderCompileFrameCount > 0;
		}

		private void Update()
		{
			ShaderUtil.allowAsyncCompilation = false;

#if RAPIDASSETRELOAD_FOCUSCHANGED
#else
			CheckUnityFocus(false);
#endif

			// When the editor isn't playing then EditorApplication.update isn't called all of the time, so we also update here in Update()
			if (!EditorApplication.isPlaying)
			{
				CheckItems();
			}

			if (DidDisableAsyncShaderCompile())
			{
				_asyncShaderCompileFrameCount--;
				if (_asyncShaderCompileFrameCount <= 1)
				{
					RestoreAsyncShaderCompile();
				}
			}
		}
	}
}