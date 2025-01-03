using UnityEngine;
using UObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FrameEditorUtility
{
	public static void stopApplication()
	{
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
	public static T loadAssetAtPath<T>(string filePath) where T : UObject
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadAssetAtPath<T>(filePath);
#else
		return null;
#endif
	}
	public static UObject[] loadAllAssetsAtPath(string filePath)
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadAllAssetsAtPath(filePath);
#else
		return null;
#endif
	}
	public static UObject loadMainAssetAtPath(string filePath)
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadMainAssetAtPath(filePath);
#else
		return null;
#endif
	}
	public static string getAssetPath(UObject obj)
	{
#if UNITY_EDITOR
		return AssetDatabase.GetAssetPath(obj);
#else
		return string.Empty;
#endif
	}
	public static bool displayDialog(string title, string info, string okText)
	{
#if UNITY_EDITOR
		return EditorUtility.DisplayDialog(title, info, okText);
#else
		return false;
#endif
	}
	public static bool isPlaying()
	{
#if UNITY_EDITOR
		return EditorApplication.isPlaying;
#else
		return true;
#endif
	}
	public static void setPause(bool pause)
	{
#if UNITY_EDITOR
		EditorApplication.isPaused = pause;
#endif
	}
	public static UObject getSelection()
	{
#if UNITY_EDITOR
		return Selection.activeObject;
#else
		return null;
#endif
	}
	public static void setDirty(UObject obj)
	{
#if UNITY_EDITOR
		EditorUtility.SetDirty(obj);
#endif
	}
	public static bool isIOS()
	{
#if UNITY_IOS
		return true;
#else
		return false;
#endif
	}
	public static bool isMacOS()
	{
#if UNITY_STANDALONE_OSX
		return true;
#else
		return false;
#endif
	}
	public static bool isLinux()
	{
#if UNITY_STANDALONE_LINUX
		return true;
#else
		return false;
#endif
	}
	public static bool isStandalone()
	{
#if UNITY_STANDALONE
		return true;
#else
		return false;
#endif
	}
	public static bool isDevelopment()
	{
#if DEVELOPMENT_BUILD
		return true;
#else
		return false;
#endif
	}
	public static bool isMobile()
	{
		return isAndroid() || isIOS();
	}
	public static bool isRealMobile()
	{
		return !isEditor() && (isAndroid() || isIOS());
	}
	public static bool isEditor()
	{
#if UNITY_EDITOR
		return true;
#else
		return false;
#endif
	}
	public static bool isWindows()
	{
#if UNITY_STANDALONE_WIN
		return true;
#else
		return false;
#endif
	}
	public static bool isAndroid()
	{
#if UNITY_ANDROID
		return true;
#else
		return false;
#endif
	}
	public static bool isWebGL()
	{
#if UNITY_WEBGL
		return true;
#else
		return false;
#endif
	}
	public static string getPlatformName()
	{
		if (isIOS())
		{
			return "iOS";
		}
		if (isMacOS())
		{
			return "MacOS";
		}
		if (isAndroid())
		{
			return "Android";
		}
		if (isWindows())
		{
			return "Windows";
		}
		if (isWebGL())
		{
			return "WebGL";
		}
		if (isLinux())
		{
			return "Linux";
		}
		return "";
	}
}