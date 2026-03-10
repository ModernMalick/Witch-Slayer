//--------------------------------------------------------------------------//
// Copyright 2025-2026 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChocDino.Workflow.RapidAssetReload.Editor
{
	internal static class UserPreferences
	{
		internal static readonly string SettingsPath = "Preferences/Chocolate Dinosaur/Workflow/Rapid Asset Reload";

		private const string KeyPrefix = "ChocDino.Workflow.RapidAssetReload.";

		private const string LogAssetImportsKey = KeyPrefix + "LogAssetImports";
		private const string IgnoreExtensionsKey = KeyPrefix + "IgnoreExtensions";
		private const string RefreshUGUICanvasesKey = KeyPrefix + "RefreshUGUICanvases";

		private const string DefaultIgnoreExtensions = "meta;pdf;txt;cs;js;boo;unity;prefab";

		internal static bool RefreshUGUICanvases
		{
			get { return EditorPrefs.GetBool(RefreshUGUICanvasesKey, true); }
			set { EditorPrefs.SetBool(RefreshUGUICanvasesKey, value); }
		}

		internal static string IgnoreExtensions
		{
			get { return EditorPrefs.GetString(IgnoreExtensionsKey, DefaultIgnoreExtensions); }
			set { EditorPrefs.SetString(IgnoreExtensionsKey, value.Trim().ToLower()); }
		}

		internal static bool LogAssetImports
		{
			get { return EditorPrefs.GetBool(LogAssetImportsKey, false); }
			set { EditorPrefs.SetBool(LogAssetImportsKey, value); }
		}

		[SettingsProvider]
		static SettingsProvider CreateUserSettingsProvider()
		{
			return new WorkflowUserPreferences(SettingsPath, SettingsScope.User);
		}

		internal static void OpenWindow()
		{
			SettingsService.OpenUserPreferences(SettingsPath);
		}

		private class WorkflowUserPreferences : SettingsProvider
		{
			private static readonly GUIContent Content_Version = new GUIContent("version 1.0.6 - 13 February 2026");
			private static readonly GUIContent Content_RefreshUGUICanvases = new GUIContent("Refresh UGUI Canvases", "Fixes Text component being corrupt when an asset changes, and also renders Image/RawImage correctly when textures change.");
			private static readonly GUIContent Content_LogAssetImports = new GUIContent("Log Asset Imports");
			private static readonly GUIContent Content_IgnoreExtensions = new GUIContent("Ignore Extensions", "A list of extensions to ignore (not import) separated by semicolon.");

			public WorkflowUserPreferences(string path, SettingsScope scope) : base(path, scope)
			{
				this.keywords = new HashSet<string>(new[] { "workflow", "Chocolate", "Dinosaur", "ChocDino", "rapid", "asset", "reload" });
			}

			private string _defines;
			private string _oldDefines;
			private bool _unappliedChanges;
			private BuildTargetGroup _buildTarget;

			public override void OnActivate(string searchContext, VisualElement rootElement)
			{
			}

			public override void OnDeactivate()
			{
			}


			private void CacheDefines()
			{
#if UNITY_2023_1_OR_NEWER
				var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(_buildTarget);
				_oldDefines = _defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
				_oldDefines = _defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTarget);
#endif
			}

			private void ApplyDefines()
			{
#if UNITY_2023_1_OR_NEWER
				var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(_buildTarget);
				PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, _defines);
#else
				PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTarget, _defines);
#endif
				CacheDefines();
			}

			private bool HasDefine(string define)
			{
				return (_defines.IndexOf(define) >= 0);
			}

			private void AddDefine(string define)
			{
				_defines = (_defines + ";" + define + ";").Replace(";;", ";");
			}

			private void RemoveDefine(string define)
			{
				_defines = _defines.Replace(define, "").Replace(";;", ";");
			}

			private bool HasDefineChanged(string define)
			{
				bool a = HasDefine(define);
				bool b = (_oldDefines.IndexOf(define) >= 0);
				return (a != b);
			}

			public override void OnTitleBarGUI()
			{
				if (_unappliedChanges)
				{
					GUI.color = Color.green;
					if (GUILayout.Button("Apply Changes"))
					{
						ApplyDefines();
					}
					GUI.color = Color.white;
				}
				if (GUILayout.Button("Open Rapid Asset Reload Window", GUILayout.ExpandWidth(false)))
				{
					RapidAssetReload.AddWindow();
				}
			}

			public override void OnFooterBarGUI()
			{
			}

			public override void OnGUI(string searchContext)
			{
				const string WORKFLOW_UITK = "WORKFLOW_UITK";
				const string WORKFLOW_UITK_BUILDER = "WORKFLOW_UITK_BUILDER";

				EditorGUIUtility.labelWidth = 200f;

				GUILayout.Label(Content_Version);
				EditorGUILayout.Space();

				{
					EditorGUILayout.BeginHorizontal();
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PrefixLabel(Content_IgnoreExtensions, EditorStyles.textField);
					var newIgnoreExtensions = EditorGUILayout.TextField(IgnoreExtensions);
					if (EditorGUI.EndChangeCheck())
					{
						IgnoreExtensions = newIgnoreExtensions;
					}
					EditorGUILayout.EndHorizontal();
				}
				{
					EditorGUI.BeginChangeCheck();
					var newRefreshUGUICanvases = EditorGUILayout.Toggle(Content_RefreshUGUICanvases, RefreshUGUICanvases);
					if (EditorGUI.EndChangeCheck())
					{
						RefreshUGUICanvases = newRefreshUGUICanvases;
					}
				}
				{
					EditorGUI.BeginChangeCheck();
					var newLogAssetImports = EditorGUILayout.Toggle(Content_LogAssetImports, LogAssetImports);
					if (EditorGUI.EndChangeCheck())
					{
						LogAssetImports = newLogAssetImports;
					}
				}

				{
					EditorGUILayout.Space();
					
					GUILayout.Label("Active platform is: " + BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget).ToString());

					bool isActivePlatform = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget) == _buildTarget;

					bool changes = false;
					{
						if (isActivePlatform)
						{
							GUI.color = new Color(1.1f, 1.1f, 1.1f, 1f);
						}
						BuildTargetGroup group = EditorGUILayout.BeginBuildTargetSelectionGrouping();
						if (_buildTarget != group)
						{
							_buildTarget = group;
							CacheDefines();
						}
						changes |= ShowDefineToggle("UI Toolkit Support (Requires com.unity.modules.uielements package)", WORKFLOW_UITK);
						if (HasDefine(WORKFLOW_UITK))
						{
							EditorGUI.indentLevel++;
							changes |= ShowDefineToggle("UI Toolkit 'UI Builder' Window Support", WORKFLOW_UITK_BUILDER);
							EditorGUI.indentLevel--;
						}
						EditorGUILayout.EndBuildTargetSelectionGrouping();
					}

					_unappliedChanges = changes;
				}

				Links();
			}

			private void Links()
			{
				const string DiscordCommunityUrl = "https://discord.gg/wKRzKAHVUE";
				const string DocumentationUrl = "https://www.chocdino.com/products/work-flow/rapid-asset-reload/about/";
				const string AssetStoreUrl = "https://assetstore.unity.com/publishers/80225?aid=1100lSvNe";

				EditorGUILayout.Space();
				GUILayout.Label("Chocolate Dinosaur Links:", EditorStyles.largeLabel);

				if (GUILayout.Button("Documentation", EditorStyles.miniButton))
				{
					Application.OpenURL(DocumentationUrl);
				}
				if (GUILayout.Button("Discord Community", EditorStyles.miniButton))
				{
					Application.OpenURL(DiscordCommunityUrl);
				}
				if (GUILayout.Button("Our Other Assets", EditorStyles.miniButton))
				{
					Application.OpenURL(AssetStoreUrl);
				}
			}

			private bool ShowDefineToggle(string label, string define)
			{
				bool enabled = HasDefine(define);
				bool changed = HasDefineChanged(define);
				if (changed)
				{
					label += " *";
				}

				bool newState = EditorGUILayout.ToggleLeft(label, enabled);
				if (newState != enabled)
				{
					if (newState)
					{
						AddDefine(define);
					}
					else
					{
						RemoveDefine(define);
					}
				}

				return HasDefineChanged(define);
			}
		}
	}
}