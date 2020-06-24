#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3_0)
	#define AVPRO_UNITY_PLATFORM_TVOS
#endif
#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2_0)
	#define AVPRO_UNITY_IOS_ALLOWHTTPDOWNLOAD
#endif
#if !UNITY_5 && !UNITY_5_4_OR_NEWER
	#define AVPRO_UNITY_METRO
#endif
#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3_0)
	#define AVPRO_UNITY_WP8_DEPRECATED
#endif
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the MediaPlayer component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MediaPlayer))]
	public class MediaPlayerEditor : UnityEditor.Editor
	{
		private SerializedProperty _propLocation;
		private SerializedProperty _propPath;
		private SerializedProperty _propAutoOpen;
		private SerializedProperty _propAutoStart;
		private SerializedProperty _propLoop;
		private SerializedProperty _propRate;
		private SerializedProperty _propVolume;
		private SerializedProperty _propBalance;
		private SerializedProperty _propMuted;
		private SerializedProperty _propPersistent;
		private SerializedProperty _propEvents;
		private SerializedProperty _propEventMask;
		private SerializedProperty _propPauseMediaOnAppPause;
		private SerializedProperty _propPlayMediaOnAppUnpause;
		private SerializedProperty _propFilter;
		private SerializedProperty _propWrap;
		private SerializedProperty _propAniso;
		private SerializedProperty _propStereoPacking;
		private SerializedProperty _propAlphaPacking;
		private SerializedProperty _propDisplayStereoTint;
		private SerializedProperty _propSubtitles;
		private SerializedProperty _propSubtitleLocation;
		private SerializedProperty _propSubtitlePath;
		private SerializedProperty _propResample;
		private SerializedProperty _propResampleMode;
		private SerializedProperty _propResampleBufferSize;
		private SerializedProperty _propAudioHeadTransform;
		private SerializedProperty _propAudioEnableFocus;
		private SerializedProperty _propAudioFocusOffLevelDB;
		private SerializedProperty _propAudioFocusWidthDegrees;
		private SerializedProperty _propAudioFocusTransform;
		private SerializedProperty _propSourceAudioSampleRate;
		private SerializedProperty _propSourceAudioChannels;
		private SerializedProperty _propManualSetAudioProps;
		private SerializedProperty _propVideoMapping;
		private SerializedProperty _propForceFileFormat;

		private static bool _isTrialVersion = false;
		private static Texture2D _icon;
		private static int _platformIndex = -1;
		private static bool _expandPlatformOverrides = false;
		private static bool _expandMediaProperties = false;
		private static bool _expandGlobalSettings = false;
		private static bool _expandMain = true;
		private static bool _expandAudio = false;
		private static bool _expandEvents = false;
		private static bool _expandPreview = false;
		private static bool _expandAbout = false;
		private static bool _expandSubtitles = false;
		private static List<string> _recentFiles = new List<string>(16);

		private static GUIStyle _mediaNameStyle = null;
		private static GUIStyle _sectionBoxStyle = null;

		private static bool _showMessage_UpdatStereoMaterial = false;

		private const string SettingsPrefix = "AVProVideo-MediaPlayerEditor-";
		private const int MaxRecentFiles = 16;

#if UNITY_EDITOR_OSX
		private const string MediaExtensions = "mp4,m4v,mov,avi,mp3,m4a,aac,ac3,au,aiff,wav";
		private const string SubtitleExtensions = "srt";
#else
		private const string MediaExtensions = "Media Files;*.mp4;*.mov;*.m4v;*.avi;*.mkv;*.ts;*.webm;*.flv;*.vob;*.ogg;*.ogv;*.mpg;*.wmv;*.3gp;Audio Files;*wav;*.mp3;*.mp2;*.m4a;*.wma;*.aac;*.au;*.flac";
		private const string SubtitleExtensions = "Subtitle Files;*.srt";
#endif

		public const string LinkPluginWebsite = "http://renderheads.com/products/avpro-video/";
		public const string LinkForumPage = "http://forum.unity3d.com/threads/released-avpro-video-complete-video-playback-solution.385611/";
		public const string LinkForumLastPage = "http://forum.unity3d.com/threads/released-avpro-video-complete-video-playback-solution.385611/page-60";
		public const string LinkGithubIssues = "https://github.com/RenderHeads/UnityPlugin-AVProVideo/issues";
		public const string LinkGithubIssuesNew = "https://github.com/RenderHeads/UnityPlugin-AVProVideo/issues/new/choose";
		public const string LinkAssetStorePage = "https://www.assetstore.unity3d.com/#!/content/56355";
		public const string LinkUserManual = "http://downloads.renderheads.com/docs/UnityAVProVideo.pdf";
		public const string LinkScriptingClassReference = "http://www.renderheads.com/content/docs/AVProVideoClassReference/";

		private const string SupportMessage = "If you are reporting a bug, please include any relevant files and details so that we may remedy the problem as fast as possible.\n\n" +
			"Essential details:\n" +
			"+ Error message\n" +
			"      + The exact error message\n" +
			"      + The console/output log if possible\n" +
			"+ Hardware\n" +
			"      + Phone / tablet / device type and OS version\n" +
			"+ Development environment\n" +
			"      + Unity version\n" +
			"      + Development OS version\n" +
			"      + AVPro Video plugin version\n" +
			" + Video details\n" +
			"      + Resolution\n" +
			"      + Codec\n" +
			"      + Frame Rate\n" +
			"      + Better still, include a link to the video file\n";

		private static bool _showAlpha = false;
		private static string[] _platformNames;

		[MenuItem("GameObject/AVPro Video/Media Player", false, 10)]
		public static void CreateMediaPlayerEditor()
		{
			GameObject go = new GameObject("MediaPlayer");
			go.AddComponent<MediaPlayer>();
			Selection.activeGameObject = go;
		}

#if UNITY_5 || UNITY_5_4_OR_NEWER
		[MenuItem("GameObject/AVPro Video/Media Player with Unity Audio", false, 10)]
		public static void CreateMediaPlayerWithUnityAudioEditor()
		{
			GameObject go = new GameObject("MediaPlayer");
			go.AddComponent<MediaPlayer>();
			go.AddComponent<AudioSource>();
			AudioOutput ao = go.AddComponent<AudioOutput>();
			// Move the AudioOutput component above the AudioSource so that it acts as the audio generator
			UnityEditorInternal.ComponentUtility.MoveComponentUp(ao);
			Selection.activeGameObject = go;
		}
#endif

		private static void LoadSettings()
		{
			_expandPlatformOverrides = EditorPrefs.GetBool(SettingsPrefix + "ExpandPlatformOverrides", false);
			_expandMediaProperties = EditorPrefs.GetBool(SettingsPrefix + "ExpandMediaProperties", false);
			_expandGlobalSettings = EditorPrefs.GetBool(SettingsPrefix + "ExpandGlobalSettings", false);
			_expandMain = EditorPrefs.GetBool(SettingsPrefix + "ExpandMain", true);
			_expandAudio = EditorPrefs.GetBool(SettingsPrefix + "ExpandAudio", false);
			_expandEvents = EditorPrefs.GetBool(SettingsPrefix + "ExpandEvents", false);
			_expandPreview = EditorPrefs.GetBool(SettingsPrefix + "ExpandPreview", false);
			_expandSubtitles = EditorPrefs.GetBool(SettingsPrefix + "ExpandSubtitles", false);
			_platformIndex = EditorPrefs.GetInt(SettingsPrefix + "PlatformIndex", -1);
			_showAlpha = EditorPrefs.GetBool(SettingsPrefix + "ShowAlphaChannel", false);

			string recentFilesString = EditorPrefs.GetString(SettingsPrefix + "RecentFiles", string.Empty);
			_recentFiles = new List<string>(recentFilesString.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries));
		}

		private static void SaveSettings()
		{
			EditorPrefs.SetBool(SettingsPrefix + "ExpandPlatformOverrides", _expandPlatformOverrides);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandMediaProperties", _expandMediaProperties);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandGlobalSettings", _expandGlobalSettings);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandMain", _expandMain);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandAudio", _expandAudio);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandEvents", _expandEvents);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandPreview", _expandPreview);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandSubtitles", _expandSubtitles);
			EditorPrefs.SetInt(SettingsPrefix + "PlatformIndex", _platformIndex);
			EditorPrefs.SetBool(SettingsPrefix + "ShowAlphaChannel", _showAlpha);

			string recentFilesString = string.Empty;
			if (_recentFiles.Count > 0)
			{
				recentFilesString = string.Join(";", _recentFiles.ToArray());
			}
			EditorPrefs.SetString(SettingsPrefix + "RecentFiles", recentFilesString);
		}

		private static bool IsTrialVersion()
		{
			string version = GetPluginVersion();
			return version.Contains("t");
		}

		private void OnEnable()
		{
			LoadSettings();

			_isTrialVersion = IsTrialVersion();
			_platformNames = Helper.GetPlatformNames();

			_propLocation = serializedObject.FindProperty("m_VideoLocation");
			_propPath = serializedObject.FindProperty("m_VideoPath");
			_propAutoOpen = serializedObject.FindProperty("m_AutoOpen");
			_propAutoStart = serializedObject.FindProperty("m_AutoStart");
			_propLoop = serializedObject.FindProperty("m_Loop");
			_propRate = serializedObject.FindProperty("m_PlaybackRate");
			_propVolume = serializedObject.FindProperty("m_Volume");
			_propBalance = serializedObject.FindProperty("m_Balance");
			_propMuted = serializedObject.FindProperty("m_Muted");
			_propPersistent = serializedObject.FindProperty("m_Persistent");
			_propEvents = serializedObject.FindProperty("m_events");
			_propEventMask = serializedObject.FindProperty("m_eventMask");
			_propPauseMediaOnAppPause = serializedObject.FindProperty("_pauseMediaOnAppPause");
			_propPlayMediaOnAppUnpause = serializedObject.FindProperty("_playMediaOnAppUnpause");
			_propFilter = serializedObject.FindProperty("m_FilterMode");
			_propWrap = serializedObject.FindProperty("m_WrapMode");
			_propAniso = serializedObject.FindProperty("m_AnisoLevel");
			_propStereoPacking = serializedObject.FindProperty("m_StereoPacking");
			_propAlphaPacking = serializedObject.FindProperty("m_AlphaPacking");
			_propDisplayStereoTint = serializedObject.FindProperty("m_DisplayDebugStereoColorTint");
			_propVideoMapping = serializedObject.FindProperty("m_videoMapping");
			_propForceFileFormat = serializedObject.FindProperty("m_forceFileFormat");

			_propSubtitles = serializedObject.FindProperty("m_LoadSubtitles");
			_propSubtitleLocation = serializedObject.FindProperty("m_SubtitleLocation");
			_propSubtitlePath = serializedObject.FindProperty("m_SubtitlePath");
			_propResample = serializedObject.FindProperty("m_Resample");
			_propResampleMode = serializedObject.FindProperty("m_ResampleMode");
			_propResampleBufferSize = serializedObject.FindProperty("m_ResampleBufferSize");
			_propAudioHeadTransform = serializedObject.FindProperty("m_AudioHeadTransform");
			_propAudioEnableFocus = serializedObject.FindProperty("m_AudioFocusEnabled");
			_propAudioFocusOffLevelDB = serializedObject.FindProperty("m_AudioFocusOffLevelDB");
			_propAudioFocusWidthDegrees = serializedObject.FindProperty("m_AudioFocusWidthDegrees");
			_propAudioFocusTransform = serializedObject.FindProperty("m_AudioFocusTransform");
			_propSourceAudioSampleRate = serializedObject.FindProperty("m_sourceSampleRate");
			_propSourceAudioChannels = serializedObject.FindProperty("m_sourceChannels");
			_propManualSetAudioProps = serializedObject.FindProperty("m_manuallySetAudioSourceProperties");

			CheckStereoPackingField();
		}

		private void OnDisable()
		{
			SaveSettings();
		}

		private static bool IsPathWithin(string fullPath, string targetPath)
		{
			return fullPath.StartsWith(targetPath);
		}

		private static string GetPathRelativeTo(string root, string fullPath)
		{
			string result = fullPath.Remove(0, root.Length);
			if (result.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) || result.StartsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
			{
				result = result.Remove(0, 1);
			}
			return result;
		}

		public override bool RequiresConstantRepaint()
		{
			MediaPlayer media = (this.target) as MediaPlayer;
			return (_expandPreview && media != null && media.Control != null && media.isActiveAndEnabled);
		}

		public override void OnInspectorGUI()
		{
			MediaPlayer media = (this.target) as MediaPlayer;

			serializedObject.Update();

			if (media == null || _propLocation == null)
			{
				return;
			}

			if (_sectionBoxStyle == null)
			{
				_sectionBoxStyle = new GUIStyle(GUI.skin.box);
				_sectionBoxStyle.padding.top = 0;
				_sectionBoxStyle.padding.bottom = 0;
			}


			GUILayout.Space(6f);

			_icon = GetIcon(_icon);
			if (_icon != null)
			{
				GUI.backgroundColor = new Color(0.96f, 0.25f, 0.47f);
				if (GUILayout.Button("◄ AVPro Video ►\nHelp & Support"))
				{
					SupportWindow.Init();
				}
				GUI.backgroundColor = Color.white;
			}

			// Describe the watermark for trial version
			if (_isTrialVersion && Application.isPlaying)
			{
				string message = string.Empty;
#if UNITY_EDITOR_WIN
				message = "The watermark is the horizontal bar that moves vertically and the small 'AVPRO TRIAL' text.";
				if (media.Info != null && media.Info.GetPlayerDescription().Contains("MF-MediaEngine-Hardware"))
				{
					message = "The watermark is the RenderHeads logo that moves around the image.";
				}
#elif UNITY_EDITOR_OSX
				message = "The RenderHeads logo is the watermark.";
#endif

				GUI.backgroundColor = Color.yellow;
				EditorGUILayout.BeginVertical(GUI.skin.box);
				GUI.color = Color.yellow;
				GUILayout.Label("AVPRO VIDEO - TRIAL WATERMARK", EditorStyles.boldLabel);
				GUI.color = Color.white;
				GUILayout.Label(message, EditorStyles.wordWrappedLabel);
				EditorGUILayout.EndVertical();
				GUI.backgroundColor = Color.white;
				GUI.color = Color.white;
			}

			// Warning about not using multi-threaded rendering
			{
				bool showWarningMT = false;

				if (/*EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.iOS ||
#if AVPRO_UNITY_PLATFORM_TVOS
					EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.tvOS ||
#endif*/
					EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
				{
#if UNITY_2017_2_OR_NEWER
					showWarningMT = !UnityEditor.PlayerSettings.GetMobileMTRendering(BuildTargetGroup.Android);
#else
					showWarningMT = !UnityEditor.PlayerSettings.mobileMTRendering;
#endif
				}
				/*if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.WSA)
				{
				}*/
				if (showWarningMT)
				{
					GUI.backgroundColor = Color.yellow;
					EditorGUILayout.BeginVertical(GUI.skin.box);
					GUI.color = Color.yellow;
					GUILayout.Label("Performance Warning", EditorStyles.boldLabel);
					GUI.color = Color.white;
					GUILayout.Label("Deploying to Android with multi-threaded rendering disabled is not recommended.  Enable multi-threaded rendering in the Player Settings > Other Settings panel.", EditorStyles.wordWrappedLabel);
					EditorGUILayout.EndVertical();
					GUI.backgroundColor = Color.white;
					GUI.color = Color.white;
				}
			}

			// Warn about using Vulkan graphics API
#if UNITY_2018_1_OR_NEWER
			{
				if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
				{
					bool showWarningVulkan = false;
					if (!UnityEditor.PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android))
					{
						UnityEngine.Rendering.GraphicsDeviceType[] devices = UnityEditor.PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
						foreach (UnityEngine.Rendering.GraphicsDeviceType device in devices)
						{
							if (device == UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
							{
								showWarningVulkan = true;
								break;
							}
						}
					}
					if (showWarningVulkan)
					{
						GUI.backgroundColor = Color.yellow;
						EditorGUILayout.BeginVertical(GUI.skin.box);
						GUI.color = Color.yellow;
						GUILayout.Label("Compatibility Warning", EditorStyles.boldLabel);
						GUI.color = Color.white;
						GUILayout.Label("Vulkan graphics API is not supported.  Please go to Player Settings > Android > Auto Graphics API and remove Vulkan from the list.  Only OpenGL 2.0 and 3.0 are supported on Android.", EditorStyles.wordWrappedLabel);
						EditorGUILayout.EndVertical();
						GUI.backgroundColor = Color.white;
						GUI.color = Color.white;
					}
				}
			}
#endif


			/*
#if UNITY_WEBGL
			// Warning about not using WebGL 2.0 or above
			{
				bool showWarningWebGL2 = false;
				if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.WebGL)
				{
#if UNITY_2017_1_OR_NEWER
					showWarningWebGL2 = UnityEditor.PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.WebGL);
					if (!showWarningWebGL2)
					{
						UnityEngine.Rendering.GraphicsDeviceType[] devices = UnityEditor.PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL);
						foreach (UnityEngine.Rendering.GraphicsDeviceType device in devices)
						{
							if (device != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2)
							{
								showWarningWebGL2 = true;
								break;
							}
						}
					}
#else
					showWarningWebGL2 = 
#endif
				}
				if (showWarningWebGL2)
				{
					GUI.backgroundColor = Color.yellow;
					EditorGUILayout.BeginVertical(GUI.skin.box);
					GUI.color = Color.yellow;
					GUILayout.Label("Compatibility Warning", EditorStyles.boldLabel);
					GUI.color = Color.white;
					GUILayout.Label("WebGL 2.0 is not supported.  Please go to Player Settings > Other Settings > Auto Graphics API and remove WebGL 2.0 from the list.  Only WebGL 1.0 is supported.", EditorStyles.wordWrappedLabel);
					EditorGUILayout.EndVertical();
					GUI.backgroundColor = Color.white;
					GUI.color = Color.white;
				}
			}
#endif*/

			// Warning about linear colour space with GPU decoding
			/*if (Application.isPlaying && media.Control != null)
			{
				if (QualitySettings.activeColorSpace == ColorSpace.Linear && media.Info.GetPlayerDescription().Contains("MF-MediaEngine-Hardware"))
				{
					GUI.backgroundColor = Color.magenta;
					EditorGUILayout.BeginVertical("box");
					GUILayout.Label("NOTE", EditorStyles.boldLabel);
					GUILayout.Label("You're using the GPU video decoder with linear color-space set in Unity.  This can cause videos to become washed out due to our GPU decoder path not supporting sRGB textures.\n\nThis can be fixed easily by:\n1) Switching back to Gamma colour space in Player Settings\n2) Disabling hardware decoding\n3) Adding 'col.rgb = pow(col.rgb, 2.2);' to any shader rendering the video texture.\n\nIf you're using the InsideSphere shader, make sure to tick 'Apply Gamma' on the material.", EditorStyles.wordWrappedLabel);
					EditorGUILayout.EndVertical();
					GUI.backgroundColor = Color.white;
				}
			}*/

			/////////////////// FILE PATH

			// Display the file name and buttons to load new files
			{ 
				EditorGUILayout.BeginVertical("box");

				OnInspectorGUI_CopyableFilename(media.m_VideoPath);

				EditorGUILayout.LabelField("Source Path", EditorStyles.boldLabel);

				EditorGUILayout.PropertyField(_propLocation, GUIContent.none);

				{
					string oldPath = _propPath.stringValue;
					string newPath = EditorGUILayout.TextField(string.Empty, _propPath.stringValue);
					if (newPath != oldPath)
					{
						// Check for invalid characters
						if (0 > newPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()))
						{
							_propPath.stringValue = newPath.Replace("\\", "/");
							EditorUtility.SetDirty(target);
						}
					}
				}

				//if (!Application.isPlaying)
				{
					GUILayout.BeginHorizontal();
					OnInspectorGUI_RecentButton(_propPath, _propLocation);
					OnInspectorGUI_StreamingAssetsButton(_propPath, _propLocation);
					GUI.color = Color.green;
					if (GUILayout.Button("BROWSE"))
					{
						string startFolder = GetStartFolder(_propPath.stringValue, (MediaPlayer.FileLocation)_propLocation.enumValueIndex);
						string videoPath = media.m_VideoPath;
						string fullPath = string.Empty;
						MediaPlayer.FileLocation fileLocation = media.m_VideoLocation;
						if (Browse(startFolder, ref videoPath, ref fileLocation, ref fullPath, MediaExtensions))
						{
							_propPath.stringValue = videoPath.Replace("\\", "/");
							_propLocation.enumValueIndex = (int)fileLocation;
							EditorUtility.SetDirty(target);

							AddToRecentFiles(fullPath);
						}
					}
					GUI.color = Color.white;
					GUILayout.EndHorizontal();

					ShowFileWarningMessages(_propPath.stringValue, (MediaPlayer.FileLocation)_propLocation.enumValueIndex, media.m_AutoOpen, Platform.Unknown);
					GUI.color = Color.white;
				}

				if (Application.isPlaying)
				{
					if (GUILayout.Button("Load"))
					{
						media.OpenVideoFromFile(media.m_VideoLocation, media.m_VideoPath, media.m_AutoStart);
					}
				}

				EditorGUILayout.EndVertical();
			}

			/////////////////// MAIN

			OnInspectorGUI_Main();

			/////////////////// AUDIO

			OnInspectorGUI_Audio();

			/////////////////// MEDIA PROPERTIES

			if (!Application.isPlaying)
			{
				OnInspectorGUI_MediaProperties();
			}

			/////////////////// SUBTITLES

			OnInspectorGUI_Subtitles();

			/////////////////// GLOBAL SETTINGS

			if (!Application.isPlaying)
			{
				OnInspectorGUI_GlobalSettings();
			}

			//////////////////// PREVIEW

			OnInspectorGUI_Preview();

			/////////////////// EVENTS

			OnInspectorGUI_Events();

			/////////////////// PLATFORM OVERRIDES

			//if (!Application.isPlaying)
			{
				OnInspectorGUI_PlatformOverrides();
			}

			if (serializedObject.ApplyModifiedProperties())
			{
				EditorUtility.SetDirty(target);
			}

			if (!Application.isPlaying)
			{
				OnInspectorGUI_About();
			}
		}

		struct RecentFileData
		{
			public RecentFileData(string path, SerializedProperty propPath, SerializedProperty propLocation, Object target)
			{
				this.path = path;
				this.propPath = propPath;
				this.propLocation = propLocation;
				this.target = target;
			}

			public string path;
			public SerializedProperty propPath;
			public SerializedProperty propLocation;
			public Object target;
		}

		private static void AddToRecentFiles(string path)
		{
			if (!_recentFiles.Contains(path))
			{
				_recentFiles.Insert(0, path);
				if (_recentFiles.Count > MaxRecentFiles)
				{
					// Remove the oldest item from the list
					_recentFiles.RemoveAt(_recentFiles.Count - 1);
				}
			}
			else
			{
				// If it already contains the item, then move it to the top
				_recentFiles.Remove(path);
				_recentFiles.Insert(0, path);
			}
		}

		void RecentMenuCallback_Select(object obj)
		{
			RecentFileData data = (RecentFileData)obj;

			string videoPath = string.Empty;
			MediaPlayer.FileLocation fileLocation = MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder;
			GetRelativeLocationFromPath(data.path, ref videoPath, ref fileLocation);

			// Move it to the top of the list
			AddToRecentFiles(data.path);

			data.propPath.stringValue = videoPath.Replace("\\", "/");
			data.propLocation.enumValueIndex = (int)fileLocation;

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(data.target);
		}

		private void RecentMenuCallback_Clear()
		{
			_recentFiles.Clear();
		}

		private void RecentMenuCallback_ClearMissing()
		{
			if (_recentFiles != null && _recentFiles.Count > 0)
			{
				List<string> newList = new List<string>(_recentFiles.Count);
				for (int i = 0; i < _recentFiles.Count; i++)
				{
					string path = _recentFiles[i];
					if (System.IO.File.Exists(path))
					{
						newList.Add(path);
					}
				}
				_recentFiles = newList;
			}
		}

		private void RecentMenuCallback_Add()
		{
			// TODO: implement me
		}

		private void OnInspectorGUI_CopyableFilename(string path)
		{
			// Display the file name so it's easy to read and copy to the clipboard
			if (!string.IsNullOrEmpty(path) && 0 > path.IndexOfAny(System.IO.Path.GetInvalidPathChars()))
			{
				// Some GUI hacks here because SelectableLabel wants to be double height and it doesn't want to be centered because it's an EditorGUILayout function...
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
					GUI.color = Color.cyan;
				}
				string text = System.IO.Path.GetFileName(path);

				if (_mediaNameStyle == null)
				{
					_mediaNameStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
					_mediaNameStyle.fontStyle = FontStyle.Bold;
					_mediaNameStyle.stretchWidth = true;
					_mediaNameStyle.stretchHeight = true;
					_mediaNameStyle.alignment = TextAnchor.MiddleCenter;
					_mediaNameStyle.margin.top = 8;
					_mediaNameStyle.margin.bottom = 16;
				}

				float height = _mediaNameStyle.CalcHeight(new GUIContent(text), Screen.width)*1.5f;
				EditorGUILayout.SelectableLabel(text, _mediaNameStyle, GUILayout.Height(height), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));

				GUI.color = Color.white;
				GUI.backgroundColor = Color.white;
			}
		}

		private void OnInspectorGUI_RecentButton(SerializedProperty propPath, SerializedProperty propLocation)
		{
			GUI.color = Color.white;

			if (GUILayout.Button("RECENT", GUILayout.Width(60f)))
			{
				GenericMenu toolsMenu = new GenericMenu();
				toolsMenu.AddDisabledItem(new GUIContent("Recent Files:"));

				// TODO: allow current path to be added.  Perhaps add it automatically when file is loaded?
				/*if (!string.IsNullOrEmpty(propPath.stringValue))
				{
					string path = propPath.stringValue.Replace("/", ">").Replace("\\", ">");
					toolsMenu.AddItem(new GUIContent("Add Current: " + path), false, RecentMenuCallback_Add);
				}*/
				toolsMenu.AddSeparator("");

				int missingCount = 0;
				for (int i = 0; i < _recentFiles.Count; i++)
				{
					string path = _recentFiles[i];
					string itemName = path.Replace("/", ">").Replace("\\", ">");
					if (System.IO.File.Exists(path))
					{
						toolsMenu.AddItem(new GUIContent(itemName), false, RecentMenuCallback_Select, new RecentFileData(path, propPath, propLocation, target));
					}
					else
					{
						toolsMenu.AddDisabledItem(new GUIContent(itemName));
						missingCount++;
					}
				}
				if (_recentFiles.Count > 0)
				{
					toolsMenu.AddSeparator("");
					toolsMenu.AddItem(new GUIContent("Clear"), false, RecentMenuCallback_Clear);
					if (missingCount > 0)
					{
						toolsMenu.AddItem(new GUIContent("Clear Missing (" + missingCount + ")"), false, RecentMenuCallback_ClearMissing);
					}
				}

				toolsMenu.ShowAsContext();
			}
		}

		private void OnInspectorGUI_StreamingAssetsButton(SerializedProperty propPath, SerializedProperty propLocation)
		{
			GUI.color = Color.white;

			if (GUILayout.Button("SA", GUILayout.Width(32f)))
			{
				GenericMenu toolsMenu = new GenericMenu();
				toolsMenu.AddDisabledItem(new GUIContent("StreamingAssets Files:"));
				toolsMenu.AddSeparator("");

				if (System.IO.Directory.Exists(Application.streamingAssetsPath))
				{
					List<string> files = new List<string>();

					string[] allFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*", System.IO.SearchOption.AllDirectories);
					if (allFiles != null && allFiles.Length > 0)
					{
						// Filter by type
						for (int i = 0; i < allFiles.Length; i++)
						{
							bool remove = false;
							if (allFiles[i].EndsWith(".meta", System.StringComparison.InvariantCultureIgnoreCase))
							{
								remove = true;
							}
							if (!remove)
							{
								files.Add(allFiles[i]);
							}
						}
					}

					if (files.Count > 0)
					{
						for (int i = 0; i < files.Count; i++)
						{
							string path = files[i];
							if (System.IO.File.Exists(path))
							{
								string itemName = path.Replace(Application.streamingAssetsPath, "");
								if (itemName.StartsWith("/") || itemName.StartsWith("\\"))
								{
									itemName = itemName.Remove(0, 1);
								}
								itemName = itemName.Replace("\\", "/");

								toolsMenu.AddItem(new GUIContent(itemName), false, RecentMenuCallback_Select, new RecentFileData(path, propPath, propLocation, target));
							}
						}
					}
					else
					{
						toolsMenu.AddDisabledItem(new GUIContent("StreamingAssets folder contains no files"));
					}
				}
				else
				{
					toolsMenu.AddDisabledItem(new GUIContent("StreamingAssets folder doesn't exist"));
				}

				toolsMenu.ShowAsContext();
			}
		}

		private static void ShowNoticeBox(MessageType messageType, string message)
		{
			//GUI.backgroundColor = Color.yellow;
			//EditorGUILayout.HelpBox(message, messageType);

			switch (messageType)
			{
				case MessageType.Error:
					GUI.color = Color.red;
					message = "Error: " + message;
					break;
				case MessageType.Warning:
					GUI.color = Color.yellow;
					message = "Warning: " + message;
					break;
			}

			//GUI.color = Color.yellow;
			GUILayout.TextArea(message);
			GUI.color = Color.white;
		}

		private static void ShowFileWarningMessages(string filePath, MediaPlayer.FileLocation fileLocation, bool isAutoOpen, Platform platform)
		{
			string finalPath = MediaPlayer.GetFilePath(filePath, fileLocation);

			if (string.IsNullOrEmpty(filePath))
			{
				if (isAutoOpen)
				{
					ShowNoticeBox(MessageType.Error, "No file specified");
				}
				else
				{
					ShowNoticeBox(MessageType.Warning, "No file specified");
				}
			}
			else
			{
				bool isPlatformAndroid = (platform == Platform.Android) || (platform == Platform.Unknown && BuildTargetGroup.Android == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);
				bool isPlatformIOS = (platform == Platform.iOS);
#if AVPRO_UNITY_IOS_ALLOWHTTPDOWNLOAD
				isPlatformIOS |= (platform == Platform.Unknown && BuildTargetGroup.iOS == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);
#endif
#if AVPRO_UNITY_PLATFORM_TVOS
				bool isPlatformTVOS = (platform == Platform.tvOS);

				isPlatformTVOS |= (platform == Platform.Unknown && BuildTargetGroup.tvOS == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);
#endif

				// Test file extensions
				{
					bool isExtensionAVI = filePath.ToLower().EndsWith(".avi");
					bool isExtensionMOV = filePath.ToLower().EndsWith(".mov");
					bool isExtensionMKV = filePath.ToLower().EndsWith(".mkv");

					if (isPlatformAndroid && isExtensionMOV)
					{
						ShowNoticeBox(MessageType.Warning, "MOV file detected. Android doesn't support MOV files, you should change the container file.");
					}
					if (isPlatformAndroid && isExtensionAVI)
					{
						ShowNoticeBox(MessageType.Warning, "AVI file detected. Android doesn't support AVI files, you should change the container file.");
					}
					if (isPlatformAndroid && isExtensionMKV)
					{
						ShowNoticeBox(MessageType.Warning, "MKV file detected. Android doesn't support MKV files until Android 5.0.");
					}
					if (isPlatformIOS && isExtensionAVI)
					{
						ShowNoticeBox(MessageType.Warning, "AVI file detected. iOS doesn't support AVI files, you should change the container file.");
					}
				}

				if (finalPath.Contains("://"))
				{
					if (filePath.ToLower().Contains("rtmp://"))
					{
						ShowNoticeBox(MessageType.Warning, "RTMP protocol is not supported by AVPro Video, except when Windows DirectShow is used with an external codec library (eg LAV Filters)");
					}
					if (filePath.ToLower().Contains("youtube.com/watch"))
					{
						ShowNoticeBox(MessageType.Warning, "YouTube URL detected. YouTube website URL contains no media, a direct media file URL (eg MP4 or M3U8) is required.  See the documentation FAQ for YouTube support.");
					}
					if (fileLocation != MediaPlayer.FileLocation.AbsolutePathOrURL)
					{
						ShowNoticeBox(MessageType.Warning, "URL detected, change location type to URL?");
					}
					else
					{
#if AVPRO_UNITY_IOS_ALLOWHTTPDOWNLOAD
						// Display warning to iOS users if they're trying to use HTTP url without setting the permission

						if (isPlatformIOS
#if AVPRO_UNITY_PLATFORM_TVOS
				|| isPlatformTVOS
#endif
			)
						{
							if (!PlayerSettings.iOS.allowHTTPDownload && filePath.StartsWith("http://"))
							{
								ShowNoticeBox(MessageType.Warning, "Starting with iOS 9 'allow HTTP downloads' must be enabled for HTTP connections (see Player Settings)");
							}
						}
#endif
						// Display warning for Android users if they're trying to use a URL without setting permission
						if (isPlatformAndroid && !PlayerSettings.Android.forceInternetPermission)
						{
							ShowNoticeBox(MessageType.Warning, "You need to set 'Internet Access' to 'require' in your Player Settings for Android builds when using URLs");
						}

						// Display warning for UWP users if they're trying to use a URL without setting permission
						if (platform == Platform.WindowsUWP || (platform == Platform.Unknown && (
#if !AVPRO_UNITY_WP8_DEPRECATED
							BuildTargetGroup.WP8 == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup ||
#endif
#if AVPRO_UNITY_METRO
							BuildTargetGroup.Metro == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup
#else
							BuildTargetGroup.WSA == UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup
#endif
							)))
						{
#if AVPRO_UNITY_METRO
							if (!PlayerSettings.Metro.GetCapability(PlayerSettings.MetroCapability.InternetClient))
#else
							if (!PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.InternetClient))
#endif
							{
								ShowNoticeBox(MessageType.Warning, "You need to set 'InternetClient' capability in your Player Settings when using URLs");
							}
						}
					}
				}
				else
				{
					if (fileLocation != MediaPlayer.FileLocation.AbsolutePathOrURL && filePath.StartsWith("/"))
					{
						ShowNoticeBox(MessageType.Warning, "Absolute path detected, change location to Absolute path?");
					}

					// Display warning for Android users if they're trying to use absolute file path without permission
					if (isPlatformAndroid && !PlayerSettings.Android.forceSDCardPermission)
					{
						ShowNoticeBox(MessageType.Warning, "You may need to access the local file system you may need to set 'Write Access' to 'External(SDCard)' in your Player Settings for Android");
					}

					if (platform == Platform.Unknown || platform == MediaPlayer.GetPlatform())
					{
						if (!System.IO.File.Exists(finalPath))
						{
							ShowNoticeBox(MessageType.Error, "File not found");
						}
						else
						{
							// Check the case
							// This approach is very slow, so we only run it when the app isn't playing
							if (!Application.isPlaying)
							{
								string comparePath = finalPath.Replace('\\', '/');
								string folderPath = System.IO.Path.GetDirectoryName(comparePath);
								if (!string.IsNullOrEmpty(folderPath))
								{

									string[] files = System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly);
									bool caseMatch = false;
									if (files != null && files.Length > 0)
									{
										//Debug.Log("final: " + comparePath);
										for (int i = 0; i < files.Length; i++)
										{
											//Debug.Log("comp: " + files[i].Replace('\\', '/'));
											if (files[i].Replace('\\', '/') == comparePath)
											{
												caseMatch = true;
												break;
											}
										}
									}
									if (!caseMatch)
									{
										ShowNoticeBox(MessageType.Warning, "File found but case doesn't match");
									}
								}
							}
						}
					}
				}
			}

			if (fileLocation == MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder)
			{
				if (!System.IO.Directory.Exists(Application.streamingAssetsPath))
				{
					GUILayout.BeginHorizontal();
					GUI.color = Color.yellow;
					GUILayout.TextArea("Warning: No StreamingAssets folder found");

					if (GUILayout.Button("Create Folder"))
					{
						System.IO.Directory.CreateDirectory(Application.streamingAssetsPath);
						AssetDatabase.Refresh();
					}
					GUILayout.EndHorizontal();
				}
				else
				{
					bool checkAndroidFileSize = false;
#if UNITY_ANDROID
					if (platform == Platform.Unknown)
					{
						checkAndroidFileSize = true;
					}
#endif
					if (platform == Platform.Android)
					{
						checkAndroidFileSize = true;
					}

					if (checkAndroidFileSize)
					{
						try
						{
							System.IO.FileInfo info = new System.IO.FileInfo(finalPath);
							if (info != null && info.Length > (1024 * 1024 * 512))
							{
								ShowNoticeBox(MessageType.Warning, "Using this very large file inside StreamingAssets folder on Android isn't recommended.  Deployments will be slow and mapping the file from the StreamingAssets JAR may cause storage and memory issues.  We recommend loading from another folder on the device.");
							}
						}
						catch (System.Exception)
						{
						}
					}
				}
			}

			GUI.color = Color.white;
		}

		private void OnInspectorGUI_VideoPreview(MediaPlayer media, IMediaProducer textureSource)
		{
			GUILayout.Label("* Inspector preview affects playback performance");
			
			Texture texture = null;
			if (textureSource != null)
			{
				texture = textureSource.GetTexture();
			}
			if (texture == null)
			{
				texture = EditorGUIUtility.whiteTexture;
			}

			float ratio = (float)texture.width / (float)texture.height;

			// Reserve rectangle for texture
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			Rect textureRect;
			Rect alphaRect = new Rect(0f, 0f, 1f, 1f);
			if (texture != EditorGUIUtility.whiteTexture)
			{
				textureRect = GUILayoutUtility.GetRect(Screen.width / 2, Screen.width / 2, (Screen.width / 2) / ratio, (Screen.width / 2) / ratio);
				if (_showAlpha)
				{
					alphaRect = GUILayoutUtility.GetRect(Screen.width / 2, Screen.width / 2, (Screen.width / 2) / ratio, (Screen.width / 2) / ratio);
				}
			}
			else
			{
				textureRect = GUILayoutUtility.GetRect(1920f / 40f, 1080f / 40f);
				if (_showAlpha)
				{
					alphaRect = GUILayoutUtility.GetRect(1920f / 40f, 1080f / 40f);
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			// Dimensions
			string dimensionText = string.Format("{0}x{1}@{2}", 0, 0, 0.0f);
			if (texture != EditorGUIUtility.whiteTexture && media.Info != null)
			{
				dimensionText = string.Format("{0}x{1}@{2:F2}", texture.width, texture.height, media.Info.GetVideoFrameRate());
			}

			CentreLabel(dimensionText);

			string rateText = "0";
			string playerText = string.Empty;
			if (media.Info != null)
			{
				rateText = media.Info.GetVideoDisplayRate().ToString("F2");
				playerText = media.Info.GetPlayerDescription();
			}

			EditorGUILayout.LabelField("Display Rate", rateText);
			EditorGUILayout.LabelField("Using", playerText);
			_showAlpha = EditorGUILayout.Toggle("Show Alpha", _showAlpha);

			// Draw the texture
			Matrix4x4 prevMatrix = GUI.matrix;
			if (textureSource != null && textureSource.RequiresVerticalFlip())
			{
				GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0, textureRect.y + (textureRect.height / 2)));
			}

			if (!GUI.enabled)
			{
				GUI.color = Color.grey;
				GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, false);
				GUI.color = Color.white;
			}
			else
			{
				if (!_showAlpha)
				{
					// TODO: In Linear mode, this displays the texture too bright, but GUI.DrawTexture displays it correctly
					EditorGUI.DrawTextureTransparent(textureRect, texture, ScaleMode.ScaleToFit);
				}
				else
				{
					GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, false);
					EditorGUI.DrawTextureAlpha(alphaRect, texture, ScaleMode.ScaleToFit);
				}
			}
			GUI.matrix = prevMatrix;

			// Select texture button
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Select Texture", GUILayout.ExpandWidth(false)))
			{
				Selection.activeObject = texture;
			}
			if (GUILayout.Button("Save PNG", GUILayout.ExpandWidth(true)))
			{
				media.SaveFrameToPng();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void OnInspectorGUI_PlayControls(IMediaControl control, IMediaInfo info)
		{
			GUILayout.Space(8.0f);

			// Slider
			EditorGUILayout.BeginHorizontal();
			bool isPlaying = false;
			if (control != null)
			{
				isPlaying = control.IsPlaying();
			}
			float currentTime = 0f;
			if (control != null)
			{
				currentTime = control.GetCurrentTimeMs();
			}

			float durationTime = 0f;
			if (info != null)
			{
				durationTime = info.GetDurationMs();
				if (float.IsNaN(durationTime))
				{
					durationTime = 0f;
				}
			}
			string timeUsed = Helper.GetTimeString(currentTime / 1000f, true);
			GUILayout.Label(timeUsed, GUILayout.ExpandWidth(false));

			float newTime = GUILayout.HorizontalSlider(currentTime, 0f, durationTime, GUILayout.ExpandWidth(true));
			if (newTime != currentTime)
			{
				control.Seek(newTime);
			}

			string timeTotal = "Infinity";
			if (!float.IsInfinity(durationTime))
			{
				timeTotal = Helper.GetTimeString(durationTime / 1000f, true);
			}

			GUILayout.Label(timeTotal, GUILayout.ExpandWidth(false));

			EditorGUILayout.EndHorizontal();

			// Buttons
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Rewind", GUILayout.ExpandWidth(false)))
			{
				control.Rewind();
			}

			if (!isPlaying)
			{
				GUI.color = Color.green;
				if (GUILayout.Button("Play", GUILayout.ExpandWidth(true)))
				{
					control.Play();
				}
			}
			else
			{
				GUI.color = Color.yellow;
				if (GUILayout.Button("Pause", GUILayout.ExpandWidth(true)))
				{
					control.Pause();
				}
			}
			GUI.color = Color.white;
			EditorGUILayout.EndHorizontal();
		}

		private struct Native
		{
#if UNITY_EDITOR_WIN
			[System.Runtime.InteropServices.DllImport("AVProVideo")]
			public static extern System.IntPtr GetPluginVersion();
#elif UNITY_EDITOR_OSX
			[System.Runtime.InteropServices.DllImport("AVProVideo")]
			public static extern string AVPGetVersion();
#endif
		}

		private static string GetPluginVersion()
		{
			string version = "Unknown";
			try
			{
#if UNITY_EDITOR_WIN
				version = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Native.GetPluginVersion());
#elif UNITY_EDITOR_OSX
				version = Native.AVPGetVersion();
#endif
			}
			catch (System.DllNotFoundException e)
			{
				Debug.LogError("[AVProVideo] Failed to load DLL. " + e.Message);
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
#if !UNITY_5 && !UNITY_5_4_OR_NEWER
				Debug.LogError("[AVProVideo] You may need to copy the Audio360 DLL into the root folder of your project (the folder above Assets)");
#endif
#endif
			}
			return version;
		}

		private static Texture2D GetIcon(Texture2D icon)
		{
			if (icon == null)
			{
				icon = Resources.Load<Texture2D>("AVProVideoIcon");
			}
			return icon;
		}

		private void OnInspectorGUI_About()
		{
			//GUILayout.Space(8f);

			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (_expandAbout)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}
			GUILayout.BeginVertical(_sectionBoxStyle);
			GUI.backgroundColor = Color.white;
			if (GUILayout.Button("About / Help", EditorStyles.toolbarButton))
			{
				_expandAbout = !_expandAbout;
			}
			GUI.color = Color.white;

			if (_expandAbout)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				_icon = GetIcon(_icon);
				if (_icon != null)
				{
					GUILayout.Label(new GUIContent(_icon));
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUI.color = Color.yellow;
				CentreLabel("AVPro Video by RenderHeads Ltd", EditorStyles.boldLabel);
				CentreLabel("version " + GetPluginVersion() + " (scripts v" + Helper.ScriptVersion + ")");
				GUI.color = Color.white;
								

				GUILayout.Space(32f);
				GUI.backgroundColor = Color.white;

				EditorGUILayout.LabelField("Links", EditorStyles.boldLabel);

				GUILayout.Space(8f);

				EditorGUILayout.LabelField("Documentation");
				if (GUILayout.Button("User Manual, FAQ, Release Notes", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(LinkUserManual);
				}
				if (GUILayout.Button("Scripting Class Reference", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(LinkScriptingClassReference);
				}

				GUILayout.Space(16f);

				GUILayout.Label("Rate and Review (★★★★☆)", GUILayout.ExpandWidth(false));
				if (GUILayout.Button("Unity Asset Store Page", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(LinkAssetStorePage);
				}

				GUILayout.Space(16f);

				GUILayout.Label("Community");
				if (GUILayout.Button("Unity Forum Page", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(LinkForumPage);
				}

				GUILayout.Space(16f);

				GUILayout.Label("Homepage", GUILayout.ExpandWidth(false));
				if (GUILayout.Button("AVPro Video Website", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(LinkPluginWebsite);
				}

				GUILayout.Space(16f);

				GUILayout.Label("Bugs and Support");
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Open Help & Support", GUILayout.ExpandWidth(false)))
				{
					SupportWindow.Init();
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(32f);

				EditorGUILayout.LabelField("Credits", EditorStyles.boldLabel);
				GUILayout.Space(8f);

				CentreLabel("Programming", EditorStyles.boldLabel);
				CentreLabel("Andrew Griffiths");
				CentreLabel("Morris Butler");
				CentreLabel("Sunrise Wang");
				CentreLabel("Ste Butcher");
				CentreLabel("Muano Mainganye");
				CentreLabel("Shane Marks");
				GUILayout.Space(8f);
				CentreLabel("Graphics", EditorStyles.boldLabel);
				GUILayout.Space(8f);
				CentreLabel("Jeff Rusch");
				CentreLabel("Luke Godward");

				GUILayout.Space(32f);

				EditorGUILayout.LabelField("Bug Reporting Notes", EditorStyles.boldLabel);

				EditorGUILayout.SelectableLabel(SupportMessage, EditorStyles.wordWrappedLabel, GUILayout.Height(300f));
			}

			EditorGUILayout.EndVertical();
		}

		private void OnInspectorGUI_Events()
		{
			//GUILayout.Space(8f);
			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (_expandEvents)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}

			GUILayout.BeginVertical(_sectionBoxStyle);
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Events", EditorStyles.toolbarButton))
			{
				_expandEvents = !_expandEvents;
			}
			GUI.color = Color.white;

			if (_expandEvents)
			{
				EditorGUILayout.PropertyField(_propEvents);

				_propEventMask.intValue = EditorGUILayout.MaskField("Triggered Events", _propEventMask.intValue, System.Enum.GetNames(typeof(MediaPlayerEvent.EventType)));

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Pause Media On App Pause");
				_propPauseMediaOnAppPause.boolValue = EditorGUILayout.Toggle(_propPauseMediaOnAppPause.boolValue);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Play Media On App Unpause");
				_propPlayMediaOnAppUnpause.boolValue = EditorGUILayout.Toggle(_propPlayMediaOnAppUnpause.boolValue);
				EditorGUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();
		}

		private readonly static GUIContent[] _fileFormatGuiNames =
		{
			new GUIContent("Automatic (by extension)"),
			new GUIContent("Apple HLS (.m3u8)"),
			new GUIContent("MPEG-DASH (.mdp)"),
			new GUIContent("MS Smooth Streaming (.ism)"),
		};		

		private void OnInspectorGUI_Main()
		{
			//GUILayout.Space(8f);
			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (_expandMain)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}

			GUILayout.BeginVertical(_sectionBoxStyle);
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Main", EditorStyles.toolbarButton))
			{
				_expandMain = !_expandMain;
			}
			GUI.color = Color.white;

			if (_expandMain)
			{
				MediaPlayer media = (this.target) as MediaPlayer;

				/////////////////// STARTUP FIELDS

				EditorGUILayout.BeginVertical("box");
				GUILayout.Label("Startup", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_propAutoOpen);
				EditorGUILayout.PropertyField(_propAutoStart, new GUIContent("Auto Play"));
				EditorGUILayout.EndVertical();

				/////////////////// PLAYBACK FIELDS

				EditorGUILayout.BeginVertical("box");
				GUILayout.Label("Playback", EditorStyles.boldLabel);

				if (!Application.isPlaying || !media.VideoOpened)
				{
					EditorGUILayout.PropertyField(_propLoop);
					EditorGUILayout.PropertyField(_propRate);
				}
				else if (media.Control != null)
				{
					media.m_Loop = media.Control.IsLooping();
					bool newLooping = EditorGUILayout.Toggle("Loop", media.m_Loop);
					if (newLooping != media.m_Loop)
					{
						media.Control.SetLooping(newLooping);
					}

					media.m_PlaybackRate = media.Control.GetPlaybackRate();
					float newPlaybackRate = EditorGUILayout.Slider("Rate", media.m_PlaybackRate, -4f, 4f);
					if (newPlaybackRate != media.m_PlaybackRate)
					{
						media.Control.SetPlaybackRate(newPlaybackRate);
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical("box");
				GUILayout.Label("Other", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_propPersistent, new GUIContent("Persistent", "Use DontDestroyOnLoad so this object isn't destroyed between level loads"));

				if (_propForceFileFormat != null)
				{
					GUIContent label = new GUIContent("Force File Format", "Override automatic format detection when using non-standard file extensions");
					_propForceFileFormat.enumValueIndex = EditorGUILayout.Popup(label, _propForceFileFormat.enumValueIndex, _fileFormatGuiNames);
				}

				EditorGUILayout.EndVertical();
			}
			GUILayout.EndVertical();
		}

		private void OnInspectorGUI_Audio()
		{
			//GUILayout.Space(8f);
			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (_expandAudio)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}

			GUILayout.BeginVertical(_sectionBoxStyle);
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Audio", EditorStyles.toolbarButton))
			{
				_expandAudio = !_expandAudio;
			}
			GUI.color = Color.white;

			if (_expandAudio)
			{
				MediaPlayer media = (this.target) as MediaPlayer;

				EditorGUILayout.BeginVertical("box");
				GUILayout.Label("Audio", EditorStyles.boldLabel);
				if (!Application.isPlaying || !media.VideoOpened)
				{
					EditorGUILayout.PropertyField(_propVolume);
					EditorGUILayout.PropertyField(_propBalance);
					EditorGUILayout.PropertyField(_propMuted);
				}
				else if (media.Control != null)
				{
					media.m_Volume = media.Control.GetVolume();
					float newVolume = EditorGUILayout.Slider("Volume", media.m_Volume, 0f, 1f);
					if (newVolume != media.m_Volume)
					{
						media.Control.SetVolume(newVolume);
					}

					float balance = media.Control.GetBalance();
					float newBalance = EditorGUILayout.Slider("Balance", balance, -1f, 1f);
					if (newBalance != balance)
					{
						media.Control.SetBalance(newBalance);
						_propBalance.floatValue = newBalance;
					}

					media.m_Muted = media.Control.IsMuted();
					bool newMuted = EditorGUILayout.Toggle("Muted", media.m_Muted);
					if (newMuted != media.m_Muted)
					{
						media.Control.MuteAudio(newMuted);
					}

					
					/*
										int selectedTrackIndex = media.Control.GetCurrentAudioTrack();
										int numTracks = media.Info.GetAudioTrackCount();
										if (numTracks > 0)
										{
											string[] trackNames = new string[numTracks];
											for (int i = 0; i < numTracks; i++)
											{
												trackNames[i] = (i+1).ToString();
											}
											int newSelectedTrackIndex = EditorGUILayout.Popup("Audio Track", selectedTrackIndex, trackNames);
											if (newSelectedTrackIndex != selectedTrackIndex)
											{
												media.Control.SetAudioTrack(newSelectedTrackIndex);
											}
										}*/
				}

				EditorGUILayout.EndVertical();

				{
					EditorGUILayout.BeginVertical("box");
					GUILayout.Label("Facebook Audio 360", EditorStyles.boldLabel);
					EditorGUILayout.PropertyField(_propAudioHeadTransform, new GUIContent("Head Transform", "Set this to your head camera transform. Only currently used for TBE Audio360"));
					EditorGUILayout.PropertyField(_propAudioEnableFocus, new GUIContent("Enable Focus", "Enables focus control. Only currently used for TBE Audio360"));
					if (_propAudioEnableFocus.boolValue)
					{
						EditorGUILayout.PropertyField(_propAudioFocusOffLevelDB, new GUIContent("Off Focus Level DB", "Sets the off-focus level in DB, with the range being between -24 to 0 DB. Only currently used for TBE Audio360"));
						EditorGUILayout.PropertyField(_propAudioFocusWidthDegrees, new GUIContent("Focus Width Degrees", "Set the focus width in degrees, with the range being between 40 and 120 degrees. Only currently used for TBE Audio360"));
						EditorGUILayout.PropertyField(_propAudioFocusTransform, new GUIContent("Focus Transform", "Set this to where you wish to focus on the video. Only currently used for TBE Audio360"));
					}
					EditorGUILayout.EndVertical();
				}

				
			}

			GUILayout.EndVertical();
		}

		private void OnInspectorGUI_MediaProperties()
		{
			//GUILayout.Space(8f);
			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (_expandMediaProperties)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}

			GUILayout.BeginVertical(_sectionBoxStyle);
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Visual", EditorStyles.toolbarButton))
			{
				_expandMediaProperties = !_expandMediaProperties;
			}
			GUI.color = Color.white;

			if (_expandMediaProperties)
			{
				MediaPlayer media = (this.target) as MediaPlayer;

				EditorGUILayout.BeginVertical();
				GUILayout.Label("Texture", EditorStyles.boldLabel);

				EditorGUILayout.PropertyField(_propFilter, new GUIContent("Filter"));
				EditorGUILayout.PropertyField(_propWrap, new GUIContent("Wrap"));
				EditorGUILayout.PropertyField(_propAniso, new GUIContent("Aniso"));

				if (_propWrap.enumValueIndex != (int)media.m_WrapMode ||
					_propFilter.enumValueIndex != (int)media.m_FilterMode ||
					_propAniso.intValue != media.m_AnisoLevel)
				{
					if (media.Control != null)
					{
						media.Control.SetTextureProperties((FilterMode)_propFilter.enumValueIndex, (TextureWrapMode)_propWrap.enumValueIndex, _propAniso.intValue);
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical();
				GUILayout.Label("Layout Mapping", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_propVideoMapping);
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical();
				GUILayout.Label("Transparency", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_propAlphaPacking, new GUIContent("Packing"));
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical();
				GUILayout.Label("Stereo", EditorStyles.boldLabel);

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(_propStereoPacking, new GUIContent("Packing"));
				if (EditorGUI.EndChangeCheck())
				{
					CheckStereoPackingField();
				}

				if (_showMessage_UpdatStereoMaterial)
				{
					ShowNoticeBox(MessageType.Warning, "No UpdateStereoMaterial component found in scene. UpdateStereoMaterial is required for stereo display.");
				}

				EditorGUI.BeginDisabledGroup(_propStereoPacking.enumValueIndex == 0);
				EditorGUILayout.PropertyField(_propDisplayStereoTint, new GUIContent("Debug Eye Tint", "Tints the left eye green and the right eye red so you can confirm stereo is working"));
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.EndVertical();

				{
					//EditorGUILayout.BeginVertical("box");
					GUILayout.Label("Resampler (BETA)", EditorStyles.boldLabel);
					EditorGUILayout.PropertyField(_propResample);
					EditorGUI.BeginDisabledGroup(!_propResample.boolValue);

					EditorGUILayout.PropertyField(_propResampleMode);
					EditorGUILayout.PropertyField(_propResampleBufferSize);

					EditorGUI.EndDisabledGroup();
					//EditorGUILayout.EndVertical();
				}
			}

			GUILayout.EndVertical();
		}

		private void CheckStereoPackingField()
		{
			if (_propStereoPacking != null)
			{
				_showMessage_UpdatStereoMaterial = false;
				if (_propStereoPacking.enumValueIndex != 0 && null == FindObjectOfType<UpdateStereoMaterial>())
				{
					_showMessage_UpdatStereoMaterial = true;
				}
			}
		}

		private void OnInspectorGUI_Subtitles()
		{
			//GUILayout.Space(8f);
			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (_expandSubtitles)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}

			GUILayout.BeginVertical(_sectionBoxStyle);
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Subtitles", EditorStyles.toolbarButton))
			{
				_expandSubtitles = !_expandSubtitles;
			}
			GUI.color = Color.white;

			if (_expandSubtitles)
			{
				MediaPlayer media = (this.target) as MediaPlayer;

				EditorGUILayout.BeginVertical();
				EditorGUILayout.PropertyField(_propSubtitles, new GUIContent("Load External Subtitles"));
				{
					EditorGUI.BeginDisabledGroup(!_propSubtitles.boolValue);

					EditorGUILayout.BeginVertical("box");

					OnInspectorGUI_CopyableFilename(_propSubtitlePath.stringValue);

					EditorGUILayout.LabelField("Source Path", EditorStyles.boldLabel);

					EditorGUILayout.PropertyField(_propSubtitleLocation, GUIContent.none);

					{
						string oldPath = _propSubtitlePath.stringValue;
						string newPath = EditorGUILayout.TextField(string.Empty, _propSubtitlePath.stringValue);
						if (newPath != oldPath)
						{
							// Check for invalid characters
							if (0 > newPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()))
							{
								_propSubtitlePath.stringValue = newPath.Replace("\\", "/");
								EditorUtility.SetDirty(target);
							}
						}
					}

					//if (!Application.isPlaying)
					{
						GUILayout.BeginHorizontal();
						OnInspectorGUI_RecentButton(_propSubtitlePath, _propSubtitleLocation);
						OnInspectorGUI_StreamingAssetsButton(_propSubtitlePath, _propSubtitleLocation);
						GUI.color = Color.green;
						if (GUILayout.Button("BROWSE"))
						{
							string startFolder = GetStartFolder(_propSubtitlePath.stringValue, (MediaPlayer.FileLocation)_propSubtitleLocation.enumValueIndex);
							string videoPath = media.SubtitlePath;
							string fullPath = string.Empty;
							MediaPlayer.FileLocation fileLocation = media.SubtitleLocation;
							if (Browse(startFolder, ref videoPath, ref fileLocation, ref fullPath, SubtitleExtensions))
							{
								_propSubtitlePath.stringValue = videoPath.Replace("\\", "/");
								_propSubtitleLocation.enumValueIndex = (int)fileLocation;
								EditorUtility.SetDirty(target);

								AddToRecentFiles(fullPath);
							}
						}
						GUI.color = Color.white;
						GUILayout.EndHorizontal();

						ShowFileWarningMessages(_propSubtitlePath.stringValue, (MediaPlayer.FileLocation)_propSubtitleLocation.enumValueIndex, media.m_AutoOpen, Platform.Unknown);
						GUI.color = Color.white;
					}

					if (Application.isPlaying)
					{
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Load"))
						{
							media.EnableSubtitles((MediaPlayer.FileLocation)_propSubtitleLocation.enumValueIndex, _propSubtitlePath.stringValue);
						}
						if (GUILayout.Button("Clear"))
						{
							media.DisableSubtitles();
						}
						GUILayout.EndHorizontal();
					}

					EditorGUILayout.EndVertical();
				}

				EditorGUILayout.EndVertical();
				EditorGUI.EndDisabledGroup();
			}

			GUILayout.EndVertical();
		}

		private static void CentreLabel(string text, GUIStyle style = null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (style == null)
			{
				GUILayout.Label(text);
			}
			else
			{
				GUILayout.Label(text, style);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void OnInspectorGUI_GlobalSettings()
		{
			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (_expandGlobalSettings)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}

			GUILayout.BeginVertical(_sectionBoxStyle);
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Global Settings", EditorStyles.toolbarButton))
			{
				_expandGlobalSettings = !_expandGlobalSettings;
			}
			GUI.color = Color.white;

			if (_expandGlobalSettings)
			{
				EditorGUI.BeginDisabledGroup(Application.isPlaying);
				EditorGUILayout.LabelField("Current Platform", EditorUserBuildSettings.selectedBuildTargetGroup.ToString());

				GUILayout.Label("BETA", EditorStyles.boldLabel);

				// TimeScale
				{
					const string TimeScaleDefine = "AVPROVIDEO_BETA_SUPPORT_TIMESCALE";

					string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
					bool supportsTimeScale = defines.Contains(TimeScaleDefine);
					bool supportsTimeScaleNew = EditorGUILayout.Toggle("TimeScale Support", supportsTimeScale);
					if (supportsTimeScale != supportsTimeScaleNew)
					{
						if (supportsTimeScaleNew)
						{
							defines += ";" + TimeScaleDefine + ";";
						}
						else
						{
							defines = defines.Replace(TimeScaleDefine, "");
						}

						PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
					}			

					if (supportsTimeScaleNew)
					{
						ShowNoticeBox(MessageType.Warning, "This will affect performance if you change Time.timeScale or Time.captureFramerate.  This feature is useful for supporting video capture system that adjust time scale during capturing.");
					}
				}

				GUILayout.Label("Performance", EditorStyles.boldLabel);

				// Disable Debug GUI
				{
					const string DisableDebugGUIDefine = "AVPROVIDEO_DISABLE_DEBUG_GUI";

					string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
					bool disableDebugGUI = defines.Contains(DisableDebugGUIDefine);
					EditorGUILayout.BeginHorizontal();
					bool disableDebugGUINew = EditorGUILayout.Toggle("Disable Debug GUI", disableDebugGUI);
					GUILayout.Label("(in builds only)");
					EditorGUILayout.EndHorizontal();
					if (disableDebugGUI != disableDebugGUINew)
					{
						if (disableDebugGUINew)
						{
							defines += ";" + DisableDebugGUIDefine + ";";
						}
						else
						{
							defines = defines.Replace(DisableDebugGUIDefine, "");
						}

						PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
					}
					if (!disableDebugGUI)
					{
						GUI.color = Color.white;
						GUILayout.TextArea("The Debug GUI can be disabled globally for builds to help reduce garbage generation each frame.");
						GUI.color = Color.white;
					}
				}

				// Disable Logging
				{
					const string DisableLogging = "AVPROVIDEO_DISABLE_LOGGING";

					string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
					bool disableLogging = defines.Contains(DisableLogging);
					bool disableLoggingNew = EditorGUILayout.Toggle("Disable Logging", disableLogging);
					if (disableLogging != disableLoggingNew)
					{
						if (disableLoggingNew)
						{
							defines += ";" + DisableLogging + ";";
						}
						else
						{
							defines = defines.Replace(DisableLogging, "");
						}

						PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
					}
				}

				EditorGUI.EndDisabledGroup();
			}

			GUILayout.EndVertical();
		}

		private void OnInspectorGUI_PlatformOverrides()
		{
			MediaPlayer media = (this.target) as MediaPlayer;

			//GUILayout.Space(8f);
			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (_expandPlatformOverrides)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}
			GUILayout.BeginVertical(_sectionBoxStyle);
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Platform Specific", EditorStyles.toolbarButton))
			{
				_expandPlatformOverrides = !_expandPlatformOverrides;
			}
			GUI.color = Color.white;

			if (_expandPlatformOverrides)
			{
				int rowCount = 0;
				int platformIndex = _platformIndex;
				for (int i = 0; i < _platformNames.Length; i++)
				{
					if (i % 3 == 0)
					{
						GUILayout.BeginHorizontal();
						rowCount++;
					}
					MediaPlayer.PlatformOptions options = media.GetPlatformOptions((Platform)i);

					Color hilight = Color.yellow;
					
					if (i == _platformIndex)
					{
						// Selected, unmodified
						if (!options.IsModified())
						{
							GUI.contentColor = Color.white;
						}
						else
						{
							// Selected, modified
							GUI.color = hilight;
							GUI.contentColor = Color.white;
						}	
					}
					else if (options.IsModified())
					{
						// Unselected, modified
						GUI.backgroundColor = Color.grey* hilight;
						GUI.contentColor = hilight;
					}
					else
					{
						// Unselected, unmodified
						if (EditorGUIUtility.isProSkin)
						{
							GUI.backgroundColor = Color.grey;
							GUI.color = new Color(0.65f, 0.66f, 0.65f);// Color.grey;
						}
					}

					if (i == _platformIndex)
					{
						if (!GUILayout.Toggle(true, _platformNames[i], GUI.skin.button))
						{
							platformIndex = -1;
						}
					}
					else
					{
						if (GUILayout.Button(_platformNames[i]))
						{
							platformIndex = i;
						}
					}
					if ((i+1) % 3 == 0)
					{
						rowCount--;
						GUILayout.EndHorizontal();
					}
					GUI.backgroundColor = Color.white;
					GUI.contentColor = Color.white;
					GUI.color = Color.white;
				}

				if (rowCount > 0)
				{
					GUILayout.EndHorizontal();
				}

				//platformIndex = GUILayout.SelectionGrid(_platformIndex, Helper.GetPlatformNames(), 3);
				//int platformIndex = GUILayout.Toolbar(_platformIndex, Helper.GetPlatformNames());
				
				if (platformIndex != _platformIndex)
				{
					_platformIndex = platformIndex;

					// We do this to clear the focus, otherwise a focused text field will not change when the Toolbar index changes
					EditorGUI.FocusTextInControl("ClearFocus");
				}

				OnInspectorGUI_PathOverrides();
				switch ((Platform)_platformIndex)
				{
					case Platform.Windows:
						OnInspectorGUI_Override_Windows();
						break;
					case Platform.MacOSX:
						OnInspectorGUI_Override_MacOSX();
						break;
					case Platform.iOS:
						OnInspectorGUI_Override_iOS();
						break;
					case Platform.tvOS:
						OnInspectorGUI_Override_tvOS();
						break;
					case Platform.Android:
						OnInspectorGUI_Override_Android();
						break;
					case Platform.WindowsPhone:
						OnInspectorGUI_Override_WindowsPhone();
						break;
					case Platform.WindowsUWP:
						OnInspectorGUI_Override_WindowsUWP();
						break;
					case Platform.WebGL:
						OnInspectorGUI_Override_WebGL();
						break;
				}
			}
			GUILayout.EndVertical();
		}

		void OnInspectorGUI_Preview()
		{
			MediaPlayer media = (this.target) as MediaPlayer;

			//GUILayout.Space(8f);
			GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;

			if (_expandPreview)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}
			GUILayout.BeginVertical(_sectionBoxStyle);
			GUI.backgroundColor = Color.white;

			GUI.backgroundColor = Color.cyan;
			if (GUILayout.Button("Preview", EditorStyles.toolbarButton))
			{
				_expandPreview = !_expandPreview;
			}
			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;

			if (_expandPreview)
			{
				EditorGUI.BeginDisabledGroup(!(media.TextureProducer != null && media.Info.HasVideo()));
				OnInspectorGUI_VideoPreview(media, media.TextureProducer);
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(!(media.Control != null && media.Control.CanPlay() && media.isActiveAndEnabled && !EditorApplication.isPaused));
				OnInspectorGUI_PlayControls(media.Control, media.Info);
				EditorGUI.EndDisabledGroup();
			}

			GUILayout.EndVertical();
		}

		private static string GetStartFolder(string path, MediaPlayer.FileLocation fileLocation)
		{
			// Try to resolve based on file path + file location
			string result = MediaPlayer.GetFilePath(path, fileLocation);
			if (!string.IsNullOrEmpty(result))
			{
				if (System.IO.File.Exists(result))
				{
					result = System.IO.Path.GetDirectoryName(result);
				}
			}

			if (!System.IO.Directory.Exists(result))
			{
				// Just resolve on file location
				result = MediaPlayer.GetPath(fileLocation);
			}
			if (string.IsNullOrEmpty(result))
			{
				// Fallback
				result = Application.streamingAssetsPath;
			}
			return result;
		}

		private void GUI_OverridePath(int platformIndex)
		{
			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)platformIndex);

			SerializedProperty propLocation = serializedObject.FindProperty(optionsVarName + ".pathLocation");
			if (propLocation != null)
			{
				EditorGUILayout.PropertyField(propLocation, GUIContent.none);
			}
			SerializedProperty propPath = serializedObject.FindProperty(optionsVarName + ".path");
			if (propPath != null)
			{

				{
					string oldPath = propPath.stringValue;
					string newPath = EditorGUILayout.TextField(string.Empty, propPath.stringValue);
					if (newPath != oldPath)
					{
						// Check for invalid characters
						if (0 > newPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()))
						{
							propPath.stringValue = newPath.Replace("\\", "/");
							EditorUtility.SetDirty(target);
						}
					}
				}
			}

			GUILayout.BeginHorizontal();
			OnInspectorGUI_RecentButton(propPath, propLocation);
			OnInspectorGUI_StreamingAssetsButton(propPath, propLocation);
			GUI.color = Color.green;
			if (GUILayout.Button("BROWSE"))
			{
				string result = string.Empty;
				MediaPlayer.FileLocation fileLocation = MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder;
				string startFolder = GetStartFolder(propPath.stringValue, (MediaPlayer.FileLocation)propLocation.enumValueIndex);
				string fullPath = string.Empty;
				if (Browse(startFolder, ref result, ref fileLocation, ref fullPath, MediaExtensions))
				{
					propPath.stringValue = result.Replace("\\", "/");

					propLocation.enumValueIndex = (int)fileLocation;
					EditorUtility.SetDirty(target);			// TODO: not sure if we need this any more.  Was put here to help prefabs save values I think
				}
			}

			GUILayout.EndHorizontal();

			GUI.color = Color.white;

			// Display the file name so it's easy to read and copy to the clipboard
			OnInspectorGUI_CopyableFilename(propPath.stringValue);

			if (GUI.enabled)
			{
				ShowFileWarningMessages(propPath.stringValue, (MediaPlayer.FileLocation)propLocation.enumValueIndex, false, (Platform)platformIndex);
			}
		}

		private void OnInspectorGUI_PathOverrides()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;

			//MediaPlayer.PlatformOptions options = media.GetPlatformOptions((Platform)_platformIndex);
			//if (options != null)

			if (_platformIndex >= 0)
			{
				EditorGUILayout.BeginVertical("box");
				//GUILayout.Label("Media Foundation Options", EditorStyles.boldLabel);
				{
					string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)_platformIndex);
					SerializedProperty propOverridePath = serializedObject.FindProperty(optionsVarName + ".overridePath");
					if (propOverridePath != null)
					{
						EditorGUILayout.PropertyField(propOverridePath, new GUIContent("Override Path"));

						//if (propOverridePath.boolValue)
						{
							EditorGUI.BeginDisabledGroup(!propOverridePath.boolValue);
							GUI_OverridePath(_platformIndex);
							EditorGUI.EndDisabledGroup();
						}
					}
				}
				EditorGUILayout.EndVertical();
			}
		}

		private readonly static GUIContent[] _audioModesWindows =
		{
			new GUIContent("System Direct"),
			new GUIContent("Facebook Audio 360", "Initialises player with Facebook Audio 360 support"),
			new GUIContent("Unity", "Allows the AudioOutput component to grab audio from the video and play it through Unity to the AudioListener"),
		};

		private readonly static GUIContent[] _audioModesUWP =
		{
			new GUIContent("System Direct"),
			new GUIContent("Unity", "Allows the AudioOutput component to grab audio from the video and play it through Unity to the AudioListener"),
		};

		private readonly static GUIContent[] _audioModesAndroid =
		{
			new GUIContent("System Direct"),
			new GUIContent("Facebook Audio 360", "Initialises player with Facebook Audio 360 support"),
		};

		private readonly static GUIContent[] _audio360ChannelMapGuiNames =
		{
			new GUIContent("(TBE_8_2) 8 channels of hybrid TBE ambisonics and 2 channels of head-locked stereo audio"),
			new GUIContent("(TBE_8) 8 channels of hybrid TBE ambisonics. NO head-locked stereo audio"),
			new GUIContent("(TBE_6_2) 6 channels of hybrid TBE ambisonics and 2 channels of head-locked stereo audio"),
			new GUIContent("(TBE_6) 6 channels of hybrid TBE ambisonics. NO head-locked stereo audio"),
			new GUIContent("(TBE_4_2) 4 channels of hybrid TBE ambisonics and 2 channels of head-locked stereo audio"),
			new GUIContent("(TBE_4) 4 channels of hybrid TBE ambisonics. NO head-locked stereo audio"),

			new GUIContent("(TBE_8_PAIR0) Channels 1 and 2 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_8_PAIR1) Channels 3 and 4 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_8_PAIR2) Channels 5 and 6 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_8_PAIR3) Channels 7 and 8 of TBE hybrid ambisonics"),

			new GUIContent("(TBE_CHANNEL0) Channels 1 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL1) Channels 2 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL2) Channels 3 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL3) Channels 4 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL4) Channels 5 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL5) Channels 6 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL6) Channels 7 of TBE hybrid ambisonics"),
			new GUIContent("(TBE_CHANNEL7) Channels 8 of TBE hybrid ambisonics"),

			new GUIContent("(HEADLOCKED_STEREO) Head-locked stereo audio"),
			new GUIContent("(HEADLOCKED_CHANNEL0) Channels 1 or left of head-locked stereo audio"),
			new GUIContent("(HEADLOCKED_CHANNEL1) Channels 2 or right of head-locked stereo audio"),

			new GUIContent("(AMBIX_4) 4 channels of first order ambiX"),
			new GUIContent("(AMBIX_9) 9 channels of second order ambiX"),
			new GUIContent("(AMBIX_9_2) 9 channels of second order ambiX with 2 channels of head-locked audio"),
			new GUIContent("(AMBIX_16) 16 channels of third order ambiX"),
			new GUIContent("(AMBIX_16_2) 16 channels of third order ambiX with 2 channels of head-locked audio"),

			new GUIContent("(STEREO)  Stereo audio"),
		};

		private void OnInspectorGUI_AudioOutput()
		{
			EditorGUILayout.PropertyField(_propManualSetAudioProps, new GUIContent("Specify Properties", "Manually set source audio properties, in case auto detection fails and the audio needs to be resampled"));

			if (_propManualSetAudioProps.boolValue)
			{
				EditorGUILayout.PropertyField(_propSourceAudioSampleRate, new GUIContent("Sample rate", "Sample rate of source video"));
				EditorGUILayout.PropertyField(_propSourceAudioChannels, new GUIContent("Channel count", "Number of channels in source video"));
			}
		}

		private void OnInspectorGUI_Override_Windows()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsWindows options = media._optionsWindows;

			GUILayout.Space(8f);

			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)_platformIndex);
			SerializedProperty propVideoApi = serializedObject.FindProperty(optionsVarName + ".videoApi");
			if (propVideoApi != null)
			{
				EditorGUILayout.PropertyField(propVideoApi, new GUIContent("Preferred Video API", "The preferred video API to use"));

				GUILayout.Space(8f);

				{
					SerializedProperty propUseTextureMips = serializedObject.FindProperty(optionsVarName + ".useTextureMips");
					if (propUseTextureMips != null)
					{
						EditorGUILayout.PropertyField(propUseTextureMips, new GUIContent("Generate Texture Mips", "Automatically create mip-maps for the texture to reducing aliasing when texture is scaled down"));
						if (propUseTextureMips.boolValue && ((FilterMode)_propFilter.enumValueIndex) != FilterMode.Trilinear)
						{
							ShowNoticeBox(MessageType.Info, "Recommend changing the texture filtering mode to Trilinear when using mip-maps.");
						}
					}
				}

				GUILayout.Space(8f);

				EditorGUILayout.BeginVertical("box");
				GUILayout.Label("Media Foundation Options", EditorStyles.boldLabel);
				SerializedProperty propUseHardwareDecoding = serializedObject.FindProperty(optionsVarName + ".useHardwareDecoding");
				if (propUseHardwareDecoding != null)
				{
					EditorGUILayout.PropertyField(propUseHardwareDecoding, new GUIContent("Hardware Decoding"));
				}

				{
					SerializedProperty propUseLowLatency = serializedObject.FindProperty(optionsVarName + ".useLowLatency");
					if (propUseLowLatency != null)
					{
						EditorGUILayout.PropertyField(propUseLowLatency, new GUIContent("Use Low Latency", "Provides a hint to the decoder to use less buffering"));
					}
				}

				int audioModeIndex = 0;
				{
					SerializedProperty propUseUnityAudio = serializedObject.FindProperty(optionsVarName + ".useUnityAudio");
					SerializedProperty propEnableAudio360 = serializedObject.FindProperty(optionsVarName + ".enableAudio360");
					
					if (propEnableAudio360.boolValue)
					{
						audioModeIndex = 1;
					}
					if (propUseUnityAudio.boolValue)
					{
						audioModeIndex = 2;
					}
					int newAudioModeIndex = EditorGUILayout.Popup(new GUIContent("Audio Mode"), audioModeIndex, _audioModesWindows);
					if (newAudioModeIndex != audioModeIndex)
					{
						switch (newAudioModeIndex)
						{
							case 0:
								propUseUnityAudio.boolValue = false;
								propEnableAudio360.boolValue = false;
								break;
							case 1:
								propUseUnityAudio.boolValue = false;
								propEnableAudio360.boolValue = true;
								break;
							case 2:
								propUseUnityAudio.boolValue = true;
								propEnableAudio360.boolValue = false;
								break;
						}
					}
				}

				if (audioModeIndex == 2)
				{
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Unity Audio", EditorStyles.boldLabel);

					SerializedProperty propForceAudioResample = serializedObject.FindProperty(optionsVarName + ".forceAudioResample");
					if (propForceAudioResample != null)
					{
						EditorGUILayout.PropertyField(propForceAudioResample, new GUIContent("Stereo", "Forces plugin to resample the video's audio to 2 channels"));
					}

					OnInspectorGUI_AudioOutput();
				}
				else if (audioModeIndex == 1)
				{
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Facebook Audio 360", EditorStyles.boldLabel);
					
					SerializedProperty prop360AudioChannelMode = serializedObject.FindProperty(optionsVarName + ".audio360ChannelMode");
					if (prop360AudioChannelMode != null)
					{
						GUIContent label = new GUIContent("Channel Mode", "Specifies what channel mode Facebook Audio 360 needs to be initialised with");
						prop360AudioChannelMode.enumValueIndex = EditorGUILayout.Popup(label, prop360AudioChannelMode.enumValueIndex, _audio360ChannelMapGuiNames);
					}

					SerializedProperty propForceAudioOutputDeviceName = serializedObject.FindProperty(optionsVarName + ".forceAudioOutputDeviceName");
					if (propForceAudioOutputDeviceName != null)
					{
						string[] deviceNames = { "Default", Windows.AudioDeviceOutputName_Rift, Windows.AudioDeviceOutputName_Vive, "Custom" };
						int index = 0;
						if (!string.IsNullOrEmpty(propForceAudioOutputDeviceName.stringValue))
						{
							switch (propForceAudioOutputDeviceName.stringValue)
							{
								case Windows.AudioDeviceOutputName_Rift:
									index = 1;
									break;
								case Windows.AudioDeviceOutputName_Vive:
									index = 2;
									break;
								default:
									index = 3;
									break;
							}
						}
						int newIndex = EditorGUILayout.Popup("Audio Device Name", index, deviceNames);
						if (newIndex == 0)
						{
							propForceAudioOutputDeviceName.stringValue = string.Empty;
						}
						else if (newIndex == 3)
						{
							if (index != newIndex)
							{
								if (string.IsNullOrEmpty(propForceAudioOutputDeviceName.stringValue) ||
										 propForceAudioOutputDeviceName.stringValue == Windows.AudioDeviceOutputName_Rift ||
										 propForceAudioOutputDeviceName.stringValue == Windows.AudioDeviceOutputName_Vive)
								{
									propForceAudioOutputDeviceName.stringValue = "?";
								}
							}
							EditorGUILayout.PropertyField(propForceAudioOutputDeviceName, new GUIContent("Audio Device Name", "Useful for VR when you need to output to the VR audio device"));
						}
						else
						{
							propForceAudioOutputDeviceName.stringValue = deviceNames[newIndex];
						}
					}
				}

				EditorGUILayout.EndVertical();

				GUILayout.Space(8f);

				
				EditorGUILayout.BeginVertical("box");
				GUILayout.Label("DirectShow Options", EditorStyles.boldLabel);
				{
					SerializedProperty propHintAlphaChannel = serializedObject.FindProperty(optionsVarName + ".hintAlphaChannel");
					if (propHintAlphaChannel != null)
					{
						EditorGUILayout.PropertyField(propHintAlphaChannel, new GUIContent("Alpha Channel Hint", "If a video is detected as 32-bit, use or ignore the alpha channel"));
					}
				}
				{
					SerializedProperty propForceAudioOutputDeviceName = serializedObject.FindProperty(optionsVarName + ".forceAudioOutputDeviceName");
					if (propForceAudioOutputDeviceName != null)
					{
						EditorGUILayout.PropertyField(propForceAudioOutputDeviceName, new GUIContent("Force Audio Output Device Name", "Useful for VR when you need to output to the VR audio device"));
					}
				}
				{
					int prevIndentLevel = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 1;
					SerializedProperty propPreferredFilter = serializedObject.FindProperty(optionsVarName + ".preferredFilters");
					if (propPreferredFilter != null)
					{
						EditorGUILayout.PropertyField(propPreferredFilter, new GUIContent("Preferred Filters", "Priority list for preferred filters to be used instead of default"), true);
						if (propPreferredFilter.arraySize > 0)
						{
							ShowNoticeBox(MessageType.Info, "Command filter names are:\n1) \"Microsoft DTV-DVD Video Decoder\" (best for compatibility when playing H.264 videos)\n2) \"LAV Video Decoder\"\n3) \"LAV Audio Decoder\"");
						}
					}
					EditorGUI.indentLevel = prevIndentLevel;
				}

				EditorGUILayout.EndVertical();
				
			}
		}

		private void OnInspectorGUI_Override_Apple(string optionsVarName)
		{
			// HTTP header fields
			SerializedProperty prop = serializedObject.FindProperty(optionsVarName + ".httpHeaderJson");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("HTTP Header (JSON)", "Allows custom http fields."));
			}

			GUILayout.Space(8f);

			EditorGUILayout.BeginVertical("box");
			GUILayout.Label("HLS Decryption", EditorStyles.boldLabel);

			// Key server auth token
			prop = serializedObject.FindProperty(optionsVarName + ".keyServerAuthToken");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("Key Server Authorization Token", "Token to pass to the key server in the 'Authorization' header field"));
			}

			GUILayout.Label("Overrides");
			EditorGUI.indentLevel++;

			// Key server override
			prop = serializedObject.FindProperty(optionsVarName + ".keyServerURLOverride");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("Key Server URL", "Overrides the key server URL if present in a HLS manifest."));
			}
			
			// Key data blob override
			prop = serializedObject.FindProperty(optionsVarName + ".base64EncodedKeyBlob");
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent("Key (Base64)", "Override Base64 encoded key to use for decoding encrypted HLS streams."));
			}

			EditorGUI.indentLevel--;

			EditorGUILayout.EndVertical();
		}

		private void OnInspectorGUI_Override_MacOSX()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsMacOSX options = media._optionsMacOSX;

			GUILayout.Space(8f);

			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)_platformIndex);
			OnInspectorGUI_Override_Apple(optionsVarName);
		}

		private void OnInspectorGUI_Override_iOS()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsIOS options = media._optionsIOS;

			GUILayout.Space(8f);
			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)_platformIndex);

			SerializedProperty prop = serializedObject.FindProperty(optionsVarName + ".useYpCbCr420Textures");
			if (prop != null)
				EditorGUILayout.PropertyField(prop, new GUIContent("Use YpCbCr420", "Reduces memory usage, REQUIRES shader support."));

			GUILayout.Space(8f);
			OnInspectorGUI_Override_Apple(optionsVarName);
		}

		private void OnInspectorGUI_Override_tvOS()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsTVOS options = media._optionsTVOS;

			GUILayout.Space(8f);
			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)_platformIndex);

			SerializedProperty prop = serializedObject.FindProperty(optionsVarName + ".useYpCbCr420Textures");
			if (prop != null)
				EditorGUILayout.PropertyField(prop, new GUIContent("Use YpCbCr420", "Reduces memory usage, REQUIRES shader support."));

			GUILayout.Space(8f);
			OnInspectorGUI_Override_Apple(optionsVarName);
		}

		private void OnInspectorGUI_Override_Android()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsAndroid options = media._optionsAndroid;

			GUILayout.Space(8f);

			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)_platformIndex);
			 
			SerializedProperty propVideoApi = serializedObject.FindProperty(optionsVarName + ".videoApi");
			if (propVideoApi != null)
			{
				EditorGUILayout.PropertyField(propVideoApi, new GUIContent("Preferred Video API", "The preferred video API to use"));

				GUILayout.Space(8f);

				SerializedProperty propFileOffset = serializedObject.FindProperty(optionsVarName + ".fileOffset");
				if (propFileOffset != null)
				{
					EditorGUILayout.PropertyField(propFileOffset);
					propFileOffset.intValue = Mathf.Max(0, propFileOffset.intValue);
				}

				SerializedProperty propUseFastOesPath = serializedObject.FindProperty(optionsVarName + ".useFastOesPath");
				if (propUseFastOesPath != null)
				{
					EditorGUILayout.PropertyField(propUseFastOesPath, new GUIContent("Use Fast OES Path", "Enables a faster rendering path using OES textures.  This requires that all rendering in Unity uses special GLSL shaders."));
					if (propUseFastOesPath.boolValue)
					{
						ShowNoticeBox(MessageType.Info, "OES can require special shaders.  Make sure you assign an AVPro Video OES shader to your meshes/materials that need to display video.");
#if UNITY_5_5_OR_NEWER
						if (PlayerSettings.virtualRealitySupported && PlayerSettings.stereoRenderingPath != StereoRenderingPath.MultiPass)
						{
							ShowNoticeBox(MessageType.Error, "OES only supports multi-pass stereo rendering path, please change in Player Settings.");
						}
#elif UNITY_5_4_OR_NEWER
						if (PlayerSettings.virtualRealitySupported && PlayerSettings.singlePassStereoRendering )
						{
							ShowNoticeBox(MessageType.Error, "OES only supports multi-pass stereo rendering path, please change in Player Settings.");
						}
#endif

#if UNITY_5_6_0 || UNITY_5_6_1
						ShowNoticeBox(MessageType.Warning, "Unity 5.6.0 and 5.6.1 have a known bug with OES path.  Please use another version of Unity for this feature and vote to fix bug #899502.");
#endif
						ShowNoticeBox(MessageType.Warning, "OES is not supported in the trial version.  If your Android plugin is not trial then you can ignore this warning.");
					}
				}

				SerializedProperty propHttpHeaderJson = serializedObject.FindProperty(optionsVarName + ".httpHeaderJson");
				if (propHttpHeaderJson != null)
				{
					EditorGUILayout.PropertyField(propHttpHeaderJson, new GUIContent("HTTP Header (JSON)", "Allows custom http fields."));
				}

				// MediaPlayer API options
				{
					EditorGUILayout.BeginVertical("box");
					GUILayout.Label("MediaPlayer Options", EditorStyles.boldLabel);					
					
					SerializedProperty propShowPosterFrame = serializedObject.FindProperty(optionsVarName + ".showPosterFrame");
					if (propShowPosterFrame != null)
					{
						EditorGUILayout.PropertyField(propShowPosterFrame, new GUIContent("Show Poster Frame", "Allows a paused loaded video to display the initial frame. This uses up decoder resources."));
					}

					EditorGUILayout.EndVertical();
				}

				// ExoPlayer API options
				{
					EditorGUILayout.BeginVertical("box");
					GUILayout.Label("ExoPlayer Options", EditorStyles.boldLabel);

					SerializedProperty propPreferSoftwareDecoder = serializedObject.FindProperty(optionsVarName + ".preferSoftwareDecoder");
					if(propPreferSoftwareDecoder != null)
					{
						EditorGUILayout.PropertyField(propPreferSoftwareDecoder);
					}

					int audioModeIndex = 0;
					{
						SerializedProperty propEnableAudio360 = serializedObject.FindProperty(optionsVarName + ".enableAudio360");

						if (propEnableAudio360.boolValue)
						{
							audioModeIndex = 1;
						}
						int newAudioModeIndex = EditorGUILayout.Popup(new GUIContent("Audio Mode"), audioModeIndex, _audioModesAndroid);
						if (newAudioModeIndex != audioModeIndex)
						{
							switch (newAudioModeIndex)
							{
								case 0:
									propEnableAudio360.boolValue = false;
									break;
								case 1:
									propEnableAudio360.boolValue = true;
									break;
							}
						}
					}

					if (audioModeIndex == 1)
					{
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Facebook Audio 360", EditorStyles.boldLabel);

						SerializedProperty prop360AudioChannelMode = serializedObject.FindProperty(optionsVarName + ".audio360ChannelMode");
						if (prop360AudioChannelMode != null)
						{
							GUIContent label = new GUIContent("Channel Mode", "Specifies what channel mode Facebook Audio 360 needs to be initialised with");
							prop360AudioChannelMode.enumValueIndex = EditorGUILayout.Popup(label, prop360AudioChannelMode.enumValueIndex, _audio360ChannelMapGuiNames);
						}
					}

					EditorGUILayout.EndVertical();
				}
				GUI.enabled = true;
			}

			/*
			SerializedProperty propFileOffsetLow = serializedObject.FindProperty(optionsVarName + ".fileOffsetLow");
			SerializedProperty propFileOffsetHigh = serializedObject.FindProperty(optionsVarName + ".fileOffsetHigh");
			if (propFileOffsetLow != null && propFileOffsetHigh != null)
			{
				propFileOffsetLow.intValue = ;

				EditorGUILayout.PropertyField(propFileOFfset);
			}*/
		}

		private void OnInspectorGUI_Override_WindowsPhone()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsWindowsPhone options = media._optionsWindowsPhone;

			GUILayout.Space(8f);

			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)_platformIndex);

			{
				SerializedProperty propUseHardwareDecoding = serializedObject.FindProperty(optionsVarName + ".useHardwareDecoding");
				if (propUseHardwareDecoding != null)
				{
					EditorGUILayout.PropertyField(propUseHardwareDecoding, new GUIContent("Hardware Decoding"));
				}
			}
			{
				SerializedProperty propUseTextureMips = serializedObject.FindProperty(optionsVarName + ".useTextureMips");
				if (propUseTextureMips != null)
				{
					EditorGUILayout.PropertyField(propUseTextureMips, new GUIContent("Generate Texture Mips", "Automatically create mip-maps for the texture to reducing aliasing when texture is scaled down"));
					if (propUseTextureMips.boolValue && ((FilterMode)_propFilter.enumValueIndex) != FilterMode.Trilinear)
					{
						ShowNoticeBox(MessageType.Info, "Recommend changing the texture filtering mode to Trilinear when using mip-maps.");
					}
				}
			}
			{
				SerializedProperty propUseLowLatency = serializedObject.FindProperty(optionsVarName + ".useLowLatency");
				if (propUseLowLatency != null)
				{
					EditorGUILayout.PropertyField(propUseLowLatency, new GUIContent("Use Low Latency", "Provides a hint to the decoder to use less buffering"));
				}
			}

			int audioModeIndex = 0;
			{
				SerializedProperty propUseUnityAudio = serializedObject.FindProperty(optionsVarName + ".useUnityAudio");
				if (propUseUnityAudio.boolValue)
				{
					audioModeIndex = 1;
				}
				int newAudioModeIndex = EditorGUILayout.Popup(new GUIContent("Audio Mode"), audioModeIndex, _audioModesUWP);
				if (newAudioModeIndex != audioModeIndex)
				{
					switch (newAudioModeIndex)
					{
						case 0:
							propUseUnityAudio.boolValue = false;
							break;
						case 1:
							propUseUnityAudio.boolValue = true;
							break;
					}
				}
			}

			if (audioModeIndex == 1)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Unity Audio", EditorStyles.boldLabel);

				SerializedProperty propForceAudioResample = serializedObject.FindProperty(optionsVarName + ".forceAudioResample");
				if (propForceAudioResample != null)
				{
					EditorGUILayout.PropertyField(propForceAudioResample, new GUIContent("Stereo", "Forces plugin to resample the video's audio to 2 channels"));
				}

				OnInspectorGUI_AudioOutput();
			}

			GUI.enabled = true;
		}

		private void OnInspectorGUI_Override_WindowsUWP()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsWindowsUWP options = media._optionsWindowsUWP;

			GUILayout.Space(8f);

			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)_platformIndex);

			{
				SerializedProperty propUseHardwareDecoding = serializedObject.FindProperty(optionsVarName + ".useHardwareDecoding");
				if (propUseHardwareDecoding != null)
				{
					EditorGUILayout.PropertyField(propUseHardwareDecoding, new GUIContent("Hardware Decoding"));
				}
			}
			{
				SerializedProperty propUseTextureMips = serializedObject.FindProperty(optionsVarName + ".useTextureMips");
				if (propUseTextureMips != null)
				{
					EditorGUILayout.PropertyField(propUseTextureMips, new GUIContent("Generate Texture Mips", "Automatically create mip-maps for the texture to reducing aliasing when texture is scaled down"));
					if (propUseTextureMips.boolValue && ((FilterMode)_propFilter.enumValueIndex) != FilterMode.Trilinear)
					{
						ShowNoticeBox(MessageType.Info, "Recommend changing the texture filtering mode to Trilinear when using mip-maps.");
					}
				}
			}
			{
				SerializedProperty propUseLowLatency = serializedObject.FindProperty(optionsVarName + ".useLowLatency");
				if (propUseLowLatency != null)
				{
					EditorGUILayout.PropertyField(propUseLowLatency, new GUIContent("Use Low Latency", "Provides a hint to the decoder to use less buffering"));
				}
			}

			int audioModeIndex = 0;
			{
				SerializedProperty propUseUnityAudio = serializedObject.FindProperty(optionsVarName + ".useUnityAudio");
				if (propUseUnityAudio.boolValue)
				{
					audioModeIndex = 1;
				}
				int newAudioModeIndex = EditorGUILayout.Popup(new GUIContent("Audio Mode"), audioModeIndex, _audioModesUWP);
				if (newAudioModeIndex != audioModeIndex)
				{
					switch (newAudioModeIndex)
					{
						case 0:
							propUseUnityAudio.boolValue = false;
							break;
						case 1:
							propUseUnityAudio.boolValue = true;
							break;
					}
				}
			}

			if (audioModeIndex == 1)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Unity Audio", EditorStyles.boldLabel);

				SerializedProperty propForceAudioResample = serializedObject.FindProperty(optionsVarName + ".forceAudioResample");
				if (propForceAudioResample != null)
				{
					EditorGUILayout.PropertyField(propForceAudioResample, new GUIContent("Stereo", "Forces plugin to resample the video's audio to 2 channels"));
				}

				OnInspectorGUI_AudioOutput();
			}

			GUI.enabled = true;
		}

		private void OnInspectorGUI_Override_WebGL()
		{
			//MediaPlayer media = (this.target) as MediaPlayer;
			//MediaPlayer.OptionsWebGL options = media._optionsWebGL;
			string optionsVarName = MediaPlayer.GetPlatformOptionsVariable((Platform)_platformIndex);

			SerializedProperty propExternalLibrary = serializedObject.FindProperty(optionsVarName + ".externalLibrary");
			if (propExternalLibrary != null)
			{
				EditorGUILayout.PropertyField(propExternalLibrary);
			}

			{
				SerializedProperty propUseTextureMips = serializedObject.FindProperty(optionsVarName + ".useTextureMips");
				if (propUseTextureMips != null)
				{
					EditorGUILayout.PropertyField(propUseTextureMips, new GUIContent("Generate Texture Mips", "Automatically create mip-maps for the texture to reducing aliasing when texture is scaled down"));
					if (propUseTextureMips.boolValue && ((FilterMode)_propFilter.enumValueIndex) != FilterMode.Trilinear)
					{
						ShowNoticeBox(MessageType.Info, "Recommend changing the texture filtering mode to Trilinear when using mip-maps.");
					}
				}
			}			
		}

		private static bool Browse(string startPath, ref string filePath, ref MediaPlayer.FileLocation fileLocation, ref string fullPath, string extensions)
		{
			bool result = false;

			string path = UnityEditor.EditorUtility.OpenFilePanel("Browse Video File", startPath, extensions);
			if (!string.IsNullOrEmpty(path) && !path.EndsWith(".meta"))
			{
				fullPath = path;
				GetRelativeLocationFromPath(path, ref filePath, ref fileLocation);
				result = true;
			}

			return result;
		}

		private static void GetRelativeLocationFromPath(string path, ref string filePath, ref MediaPlayer.FileLocation fileLocation)
		{
			string projectRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));
			projectRoot = projectRoot.Replace('\\', '/');

			if (path.StartsWith(projectRoot))
			{
				if (path.StartsWith(Application.streamingAssetsPath))
				{
					// Must be StreamingAssets relative path
					filePath = GetPathRelativeTo(Application.streamingAssetsPath, path);
					fileLocation = MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder;
				}
				else if (path.StartsWith(Application.dataPath))
				{
					// Must be Assets relative path
					filePath = GetPathRelativeTo(Application.dataPath, path);
					fileLocation = MediaPlayer.FileLocation.RelativeToDataFolder;
				}
				else
				{
					// Must be project relative path
					filePath = GetPathRelativeTo(projectRoot, path);
					fileLocation = MediaPlayer.FileLocation.RelativeToProjectFolder;
				}
			}
			else
			{
				// Must be persistant data
				if (path.StartsWith(Application.persistentDataPath))
				{
					filePath = GetPathRelativeTo(Application.persistentDataPath, path);
					fileLocation = MediaPlayer.FileLocation.RelativeToPeristentDataFolder;
				}

				// Must be absolute path
				filePath = path;
				fileLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
			}
		}
	}
}