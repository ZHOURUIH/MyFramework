#if UNITY_5_4_OR_NEWER || UNITY_5
	#if !UNITY_5_0
		#define AVPROVIDEO_WINDOWTITLE_51
	#endif
#endif

using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2016-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// A window to display options to the user to help them report bugs
	/// Also collects some metadata about the machine specs, plugin version etc
	/// </summary>
	public class SupportWindow : EditorWindow
	{
		private class MyPopupWindow : PopupWindowContent
		{
			private string _text;
			private string _url;
			private string _buttonMessage;

			public MyPopupWindow(string text, string buttonMessage,string url)
			{
				_text = text;
				_url = url;
				_buttonMessage = buttonMessage;
			}

			public override Vector2 GetWindowSize()
			{
				return new Vector2(400, 520);
			}

			public override void OnGUI(Rect rect)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Copy-Paste this text, then ", EditorStyles.boldLabel);
				GUI.color = Color.green;
				if (GUILayout.Button(_buttonMessage, GUILayout.ExpandWidth(true)))
				{
					Application.OpenURL(_url);
				}
				GUILayout.EndHorizontal();
				GUI.color = Color.white;
				EditorGUILayout.TextArea(_text);
			}
		}

		private static bool _isCreated = false;
		private static bool _isInit = false;

		private const string SettingsPrefix = "AVProVideo.SupportWindow.";

		private string _emailDescription = string.Empty;
		private string _emailTopic = string.Empty;
		private string _emailVideoFormat = string.Empty;
		private string _emailDeviceSpecs = string.Empty;

		//private bool _askForHelp = false;
		private bool _trySelfSolve = false;
		private Vector2 _scroll = Vector2.zero;

		private int _selectionIndex = 0;
		private static string[] _gridNames = { "Ask for Help", "Help Resources", "FAQ" };

		[MenuItem("Window/AVPro Video Support")]
		public static void Init()
		{
			// Close window if it is already open
			if (_isInit || _isCreated)
			{
				SupportWindow window = (SupportWindow)EditorWindow.GetWindow(typeof(SupportWindow));
				window.Close();
				return;
			}

			_isCreated = true;

			// Get existing open window or if none, make a new one:
			SupportWindow window2 = ScriptableObject.CreateInstance<SupportWindow>();
			if (window2 != null)
			{
				window2.SetupWindow();
			}
		}

		private void SetupWindow()
		{
			_isCreated = true;
			float width = 512f;
			float height = 512f;
			this.position = new Rect((Screen.width / 2) - (width / 2f), (Screen.height / 2) - (height / 2f), width, height);
			this.minSize = new Vector2(530f, 510f);
#if AVPROVIDEO_WINDOWTITLE_51
			this.titleContent = new GUIContent("AVPro Video - Help & Support");
#else
			this.title = "AVPro Video - Support";
#endif
			this.CreateGUI();
			LoadSettings();
			this.ShowUtility();
			this.Repaint();
		}

		private void CreateGUI()
		{
			_isInit = true;
		}

		void OnEnable()
		{
			if (!_isCreated)
			{
				SetupWindow();
			}
		}

		void OnDisable()
		{
			_isInit = false;
			_isCreated = false;
			SaveSettings();
			Repaint();
		}

		private void SaveSettings()
		{
			EditorPrefs.SetString(SettingsPrefix + "EmailTopic", _emailTopic);
			EditorPrefs.SetString(SettingsPrefix + "EmailDescription", _emailDescription);
			EditorPrefs.SetString(SettingsPrefix + "EmailDeviceSpecs", _emailDeviceSpecs);
			EditorPrefs.SetString(SettingsPrefix + "EmailVideoSpecs", _emailVideoFormat);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandSelfSolve", _trySelfSolve);
			EditorPrefs.SetInt(SettingsPrefix + "SelectionIndex", _selectionIndex);
		}

		private void LoadSettings()
		{
			_emailTopic = EditorPrefs.GetString(SettingsPrefix + "EmailTopic", _emailTopic);
			_emailDescription = EditorPrefs.GetString(SettingsPrefix + "EmailDescription", _emailDescription);
			_emailDeviceSpecs = EditorPrefs.GetString(SettingsPrefix + "EmailDeviceSpecs", _emailDeviceSpecs);
			_emailVideoFormat = EditorPrefs.GetString(SettingsPrefix + "EmailVideoSpecs", _emailVideoFormat);
			_trySelfSolve = EditorPrefs.GetBool(SettingsPrefix + "ExpandSelfSolve", _trySelfSolve);
			_selectionIndex = EditorPrefs.GetInt(SettingsPrefix + "SelectionIndex", _selectionIndex);
		}

		private string CollectSupportData()
		{
			string nl = System.Environment.NewLine;

			string version = string.Format("AVPro Video: plugin v{0} scripts v{1}", GetPluginVersion(), Helper.ScriptVersion);
			string targetPlatform = "Target Platform: " + EditorUserBuildSettings.selectedBuildTargetGroup.ToString();
			string unityVersion = "Unity: v" + Application.unityVersion + " " + Application.platform.ToString();

			string deviceInfo = "OS: " + SystemInfo.deviceType + " - " + SystemInfo.deviceModel + " - " + SystemInfo.operatingSystem + " - " + Application.systemLanguage;
			string cpuInfo = "CPU: " + SystemInfo.processorType + " - " + SystemInfo.processorCount + " threads - " + + SystemInfo.systemMemorySize + "KB";
			string gfxInfo = "GPU: " + SystemInfo.graphicsDeviceName + " - " + SystemInfo.graphicsDeviceVendor + " - " + SystemInfo.graphicsDeviceVersion + " - " + SystemInfo.graphicsMemorySize + "KB - " + SystemInfo.maxTextureSize;

			return version + nl + targetPlatform + nl + unityVersion + nl + deviceInfo + nl + cpuInfo + nl + gfxInfo;
		}

		void OnGUI()
		{
			if (!_isInit)
			{
				EditorGUILayout.LabelField("Initialising...");
				return;
			}

			GUILayout.Label("Having problems? We'll do our best to help.\n\nBelow is a collection of resources to help solve any issues you may encounter.", EditorStyles.wordWrappedLabel);
			GUILayout.Space(16f);

			/*GUI.color = Color.white;
			GUI.backgroundColor = Color.clear;
			if (_trySelfSolve)
			{
				GUI.color = Color.white;
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = Color.black;
				}
			}
			GUILayout.BeginVertical("box");
			GUI.backgroundColor = Color.white;*/

			_selectionIndex = GUILayout.Toolbar(_selectionIndex, _gridNames);

			GUILayout.Space(16f);
			/*if (GUILayout.Button("Try Solve the Issue Yourself", EditorStyles.toolbarButton))
			{
				//_trySelfSolve = !_trySelfSolve;
				_trySelfSolve = true;
			}
			GUI.color = Color.white;
			if (_trySelfSolve)*/
			if (_selectionIndex == 1)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("1) ");
				GUILayout.Label("Check you're using the latest version of AVPro Video via the Asset Store.  This is version " + Helper.ScriptVersion, EditorStyles.wordWrappedLabel);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("2) ");
				GUILayout.Label("Look at the example projects and scripts in the Demos folder");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("3) ");
				GUI.color = Color.green;
				if (GUILayout.Button("Read the Documentation", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(MediaPlayerEditor.LinkUserManual);
				}
				GUI.color = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("4) ");
				GUI.color = Color.green;
				if (GUILayout.Button("Read the GitHub Issues", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(MediaPlayerEditor.LinkGithubIssues);
				}
				GUI.color = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("5) ");
				GUI.color = Color.green;
				if (GUILayout.Button("Read the Scripting Reference", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(MediaPlayerEditor.LinkScriptingClassReference);
				}
				GUI.color = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("6) ");
				GUI.color = Color.green;
				if (GUILayout.Button("Visit the AVPro Video Website", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(MediaPlayerEditor.LinkPluginWebsite);
				}
				GUI.color = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("7) ");
				GUI.color = Color.green;
				if (GUILayout.Button("Browse the Unity Forum", GUILayout.ExpandWidth(false)))
				{
					Application.OpenURL(MediaPlayerEditor.LinkForumPage);
				}
				GUI.color = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			else if (_selectionIndex == 2)
			{
				GUILayout.Label("Coming soon...");
			}
			else if (_selectionIndex == 0)
			{
				GUILayout.Label("Please fill out these fields when sending us a new issue.\nThis makes it much easier and faster to resolve the issue.", EditorStyles.wordWrappedLabel);
				GUILayout.Space(16f);

				GUILayout.BeginVertical("box");
				_scroll = GUILayout.BeginScrollView(_scroll);

				GUILayout.Label("Issue/Question Title", EditorStyles.boldLabel);
				_emailTopic = GUILayout.TextField(_emailTopic);

				GUILayout.Space(8f);
				GUILayout.Label("What's the problem?", EditorStyles.boldLabel);
				_emailDescription = EditorGUILayout.TextArea(_emailDescription, GUILayout.Height(64f));

				GUILayout.Space(8f);
				GUILayout.BeginHorizontal();
				GUILayout.Label("Tell us about your videos", EditorStyles.boldLabel);
				GUILayout.Label("- Number of videos, resolution, codec, frame-rate, example URLs", EditorStyles.miniBoldLabel);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				_emailVideoFormat = EditorGUILayout.TextArea(_emailVideoFormat, GUILayout.Height(32f));

				GUILayout.Space(8f);
				GUILayout.BeginHorizontal();
				GUILayout.Label("Which devices are you having the issue with?", EditorStyles.boldLabel);
				GUILayout.Label("- Model, OS version number", EditorStyles.miniBoldLabel);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				_emailDeviceSpecs = EditorGUILayout.TextField(_emailDeviceSpecs);

				//GUILayout.Space(16f);
				////GUILayout.Label("System Information");
				//GUILayout.TextArea(CollectSupportData());

				string emailBody = System.Environment.NewLine + System.Environment.NewLine;
				emailBody += "Problem description:" + System.Environment.NewLine + System.Environment.NewLine + _emailDescription + System.Environment.NewLine + System.Environment.NewLine;
				emailBody += "Device (which devices are you having the issue with - model, OS version number):" + System.Environment.NewLine + System.Environment.NewLine + _emailDeviceSpecs + System.Environment.NewLine + System.Environment.NewLine;
				emailBody += "Media (tell us about your videos - number of videos, resolution, codec, frame-rate, example URLs):" + System.Environment.NewLine + System.Environment.NewLine + _emailVideoFormat + System.Environment.NewLine + System.Environment.NewLine;
				emailBody += "System Information:" + System.Environment.NewLine + System.Environment.NewLine + CollectSupportData() + System.Environment.NewLine + System.Environment.NewLine;

				//GUILayout.Space(16f);
//
				//GUILayout.Label("Email Content");
				//EditorGUILayout.TextArea(emailBody);

				GUILayout.EndScrollView();
				GUILayout.EndVertical();

				GUILayout.Space(16f);

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUI.color = Color.green;
				if (GUILayout.Button("Send at GitHub Issues ➔", GUILayout.ExpandWidth(false), GUILayout.Height(32f)))
				{
					PopupWindow.Show(buttonRect, new MyPopupWindow(emailBody, "Go to GitHub", MediaPlayerEditor.LinkGithubIssuesNew));
				}
				if (GUILayout.Button("Send at the Unity Forum ➔", GUILayout.ExpandWidth(false), GUILayout.Height(32f)))
				{
					PopupWindow.Show(buttonRect, new MyPopupWindow(emailBody, "Go to Forum", MediaPlayerEditor.LinkForumLastPage));
				}

				if (Event.current.type == EventType.Repaint)
				{
					buttonRect = GUILayoutUtility.GetLastRect();
				}

				GUI.color = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			//GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Close"))
			{
				this.Close();
			}			
		}

		private Rect buttonRect;

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
	}
}