using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UObject = UnityEngine.Object;
using UDebug = UnityEngine.Debug;
using static FrameBaseDefine;

public class FrameBaseUtility
{
	private static bool mShowMessageBox = true;     // 是否显示报错提示框,用来判断提示框显示次数
	private static int mMainThreadID;				// 主线程ID
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
	public static bool isWeiXin()
	{
#if UNITY_WEIXINMINIGAME
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
		return isAndroid() || isIOS() || isWeiXin();
	}
	public static bool isRealMobile()
	{
		return !isEditor() && isMobile();
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
#if UNITY_WEBGL || WEIXINMINIGAME
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
	public static void logExceptionBase(Exception e, string info = null)
	{
		if (info == null || info.Length == 0)
		{
			info = "";
		}
		else
		{
			info += ", ";
		}
		info += e.Message + ", stack:" + e.StackTrace;
		if (e.InnerException != null)
		{
			info += "\ninner exception:" + e.InnerException.Message + ", stack:" + e.InnerException.StackTrace;
		}
		logErrorBase(info);
	}
	public static void logErrorBase(string info)
	{
		if (mShowMessageBox)
		{
			// 在编辑器中显示对话框
			displayDialog("错误", info, "确认");
			setPause(true);
			// 运行一次只显示一次提示框,避免在循环中报错时一直弹窗
			mShowMessageBox = false;
		}
		UDebug.LogError("error: " + info + "\nstack: " + new StackTrace().ToString());
	}
	public static void logBase(string info)
	{
		UDebug.Log(info);
	}
	public static void logWarningBase(string info)
	{
		UDebug.LogWarning(info);
	}
	public static void setScreenSizeBase(Vector2Int size, bool fullScreen)
	{
		Screen.SetResolution(size.x, size.y, fullScreen);

		// UGUI
		GameObject uguiRootObj = getRootGameObject(UGUI_ROOT);
		uguiRootObj.TryGetComponent<RectTransform>(out var uguiRectTransform);
		uguiRectTransform.offsetMin = -size / 2;
		uguiRectTransform.offsetMax = size / 2;
		uguiRectTransform.anchorMax = Vector2.one * 0.5f;
		uguiRectTransform.anchorMin = Vector2.one * 0.5f;
		Camera camera = getGameObject("UICamera", uguiRootObj).GetComponent<Camera>();
		camera.transform.localPosition = new(0.0f, 0.0f, -size.y * 0.5f / Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f));
	}
	public static GameObject getRootGameObject(string name, bool errorIfNull = false)
	{
		if (name == null || name.Length == 0)
		{
			return null;
		}
		GameObject go = GameObject.Find(name);
		if (go == null && errorIfNull)
		{
			logErrorBase("找不到物体,请确认物体存在且是已激活状态,parent为空时无法查找到未激活的物体:" + name);
		}
		return go;
	}
	public static GameObject getGameObject(string name, GameObject parent, bool errorIfNull = false, bool recursive = true)
	{
		if (name == null || name.Length == 0)
		{
			return null;
		}
		if (parent == null)
		{
			logErrorBase("parent不能为空,查找无父节点的GameObject请使用getRootGameObject");
			return null;
		}
		GameObject go = null;
		do
		{
			Transform trans = parent.transform.Find(name);
			if (trans != null)
			{
				go = trans.gameObject;
				break;
			}
			// 如果父节点的第一级子节点中找不到,就递归查找
			if (recursive)
			{
				int childCount = parent.transform.childCount;
				for (int i = 0; i < childCount; ++i)
				{
					GameObject thisParent = parent.transform.GetChild(i).gameObject;
					go = getGameObject(name, thisParent, false, recursive);
					if (go != null)
					{
						break;
					}
				}
			}
		} while (false);

		if (go == null && errorIfNull)
		{
			logErrorBase("找不到物体,请确认是否存在:" + name + ", parent:" + (parent != null ? parent.name : ""));
		}
		return go;
	}
	public static Vector2 getScreenScale(Vector2 rootSize)
	{
		return new(rootSize.x * (1.0f / STANDARD_WIDTH), rootSize.y * (1.0f / STANDARD_HEIGHT));
	}
	public static void destroyUnityObject(UObject obj, bool immediately = false, bool allowDestroyAssets = false)
	{
		destroyUnityObject(ref obj, immediately, allowDestroyAssets);
	}
	public static void destroyUnityObject<T>(ref T obj, bool immediately = false, bool allowDestroyAssets = false) where T : UObject
	{
		if (obj == null)
		{
			return;
		}
		if (immediately)
		{
			UObject.DestroyImmediate(obj, allowDestroyAssets);
		}
		else
		{
			UObject.Destroy(obj);
		}
		obj = null;
	}
	public static void destroyComponent<T>(GameObject go) where T : Component
	{
		if (go.TryGetComponent<T>(out var com))
		{
			destroyUnityObject(com, true);
		}
	}
	public static T getOrAddComponent<T>(GameObject go) where T : Component
	{
		if (!go.TryGetComponent(out T com))
		{
			com = go.AddComponent<T>();
		}
		return com;
	}
	// 返回值表示是否是新添加的组件
	public static bool getOrAddComponent<T>(GameObject go, out T com) where T : Component
	{
		if (!go.TryGetComponent(out com))
		{
			com = go.AddComponent<T>();
			return true;
		}
		return false;
	}
	public static void setMainThreadID(int mainThreadID) { mMainThreadID = mainThreadID; }
	public static bool isMainThread() { return Thread.CurrentThread.ManagedThreadId == mMainThreadID; }
	// 对比两个版本号,返回值表示整个版本号的大小比较结果,lowerVersion表示小版本号的比较结果,higherVersion表示大版本号比较的结果
	// 此函数只判断3位的版本号,也就是版本号0.版本号1.版本号2的格式,不支持2位的版本号
	public static VERSION_COMPARE compareVersion3(string remote, string local, out VERSION_COMPARE lowerVersion, out VERSION_COMPARE higherVersion)
	{
		if (string.IsNullOrEmpty(remote))
		{
			lowerVersion = VERSION_COMPARE.REMOTE_LOWER;
			higherVersion = VERSION_COMPARE.REMOTE_LOWER;
			return VERSION_COMPARE.REMOTE_LOWER;
		}
		if (string.IsNullOrEmpty(local))
		{
			lowerVersion = VERSION_COMPARE.LOCAL_LOWER;
			higherVersion = VERSION_COMPARE.LOCAL_LOWER;
			return VERSION_COMPARE.LOCAL_LOWER;
		}
		string[] sourceFormatStr = remote.Split('.');
		string[] targetFormatStr = local.Split('.');
		if (sourceFormatStr.Length != 3)
		{
			lowerVersion = VERSION_COMPARE.REMOTE_LOWER;
			higherVersion = VERSION_COMPARE.REMOTE_LOWER;
			return VERSION_COMPARE.REMOTE_LOWER;
		}
		if (targetFormatStr.Length != 3)
		{
			lowerVersion = VERSION_COMPARE.LOCAL_LOWER;
			higherVersion = VERSION_COMPARE.LOCAL_LOWER;
			return VERSION_COMPARE.LOCAL_LOWER;
		}
		lowerVersion = VERSION_COMPARE.EQUAL;
		higherVersion = VERSION_COMPARE.EQUAL;
		if (remote == local)
		{
			return VERSION_COMPARE.EQUAL;
		}
		const long MaxMiddleVersion = 100000000000;
		long[] source = new long[3] { long.Parse(sourceFormatStr[0]), long.Parse(sourceFormatStr[1]), long.Parse(sourceFormatStr[2]) };
		long[] target = new long[3] { long.Parse(targetFormatStr[0]), long.Parse(targetFormatStr[1]), long.Parse(targetFormatStr[2]) };
		long sourceFullVersion = source[0] * MaxMiddleVersion * MaxMiddleVersion + source[1] * MaxMiddleVersion + source[2];
		long targetFullVersion = target[0] * MaxMiddleVersion * MaxMiddleVersion + target[1] * MaxMiddleVersion + target[2];
		long sourceBigVersion = source[0] * MaxMiddleVersion + source[1];
		long targetBigVersion = target[0] * MaxMiddleVersion + target[1];
		if (sourceBigVersion > targetBigVersion)
		{
			higherVersion = VERSION_COMPARE.LOCAL_LOWER;
		}
		else if (sourceBigVersion < targetBigVersion)
		{
			higherVersion = VERSION_COMPARE.REMOTE_LOWER;
		}
		else
		{
			higherVersion = VERSION_COMPARE.EQUAL;
		}
		if (source[2] > target[2])
		{
			lowerVersion = VERSION_COMPARE.LOCAL_LOWER;
		}
		else if (source[2] < target[2])
		{
			lowerVersion = VERSION_COMPARE.REMOTE_LOWER;
		}
		else
		{
			lowerVersion = VERSION_COMPARE.EQUAL;
		}
		if (sourceFullVersion > targetFullVersion)
		{
			return VERSION_COMPARE.LOCAL_LOWER;
		}
		else if (sourceFullVersion < targetFullVersion)
		{
			return VERSION_COMPARE.REMOTE_LOWER;
		}
		else
		{
			return VERSION_COMPARE.EQUAL;
		}
	}
}