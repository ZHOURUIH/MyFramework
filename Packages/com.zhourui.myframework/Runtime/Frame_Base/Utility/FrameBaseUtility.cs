using System;
using System.Diagnostics;
using System.Threading;
using System.Collections;
#if BYTE_DANCE
using TTSDK;
#endif
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UObject = UnityEngine.Object;
using UDebug = UnityEngine.Debug;
using static FrameBaseDefine;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Text;

public class FrameBaseUtility
{
	private static bool mShowMessageBox = true;     // 是否显示报错提示框,用来判断提示框显示次数
	private static int mMainThreadID;               // 主线程ID
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
	public static string assetPathToGUID(string assetPath)
	{
#if UNITY_EDITOR
		return AssetDatabase.AssetPathToGUID(assetPath);
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
	public static bool isByteDance()
	{
#if BYTE_DANCE
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
	public static bool isDevOrEditor()
	{
		return isEditor() || isDevelopment();
	}
	public static bool isMobile()
	{
		return isAndroid() || isIOS() || isWeiXin();
	}
	public static bool isRealMobile()
	{
		return !isEditor() && isMobile();
	}
	public static bool isEnableHotFix()
	{
#if ENABLE_HOTFIX
		return true;
#else
		return false;
#endif
	}
	public static bool isOfficialClient()
	{
#if UNITY_EDITOR || TEST
		return false;
#else
		return true;
#endif
	}
	public static bool isNoHotFixTestClient()
	{
		return isTestClient() && !isEnableHotFix();
	}
	public static bool isTestClient()
	{
#if TEST
		return true;
#else
		return false;
#endif
	}
	public static bool isHotFixTestClient()
	{
		return isTestClient() && isEnableHotFix();
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
	public static bool isUseHybridCLR()
	{
#if USE_HYBRID_CLR
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
	[HideInCallstack]
	public static void logExceptionBase(Exception e, string info = null)
	{
		if (e == null)
		{
			UDebug.LogError(string.IsNullOrEmpty(info) ? "异常对象为空" : info);
			return;
		}
		string originInfo = info;
		string errorInfo = info ?? "";
		Exception curException = e;
		int depth = 0;
		while (curException != null)
		{
			errorInfo +=
				"\nexception[" + depth + "]:" +
				curException.GetType().FullName +
				"\nmessage:" + curException.Message +
				"\nstack:" + curException.StackTrace;

			curException = curException.InnerException;
			++depth;
		}
		if (isEditor())
		{
			errorInfo += ",编辑器中双击下一条日志可跳转到抛异常的具体代码位置";
		}
		logErrorBase(errorInfo);
		if (isEditor())
		{
			makeExceptionStack(e, info);
		}
	}
	[HideInCallstack]
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
		info = "error: " + info + "\nstack: " + new StackTrace().ToString();
		if (isIOS() && !isEditor())
		{
			iOSDllImportFrameBase.iOSLog(info);
			// 这里需要手动触发bugly的上报,因为没有调用unity的LogError,无法自动捕获到错误
			BuglyForwarder.reportError(info, "", LogType.Error);
		}
		else
		{
			UDebug.LogError(info);
		}
	}
	[HideInCallstack]
	public static void logBase(string info)
	{
		if (isIOS() && !isEditor())
		{
			iOSDllImportFrameBase.iOSLog(info);
		}
		else
		{
			UDebug.Log(info);
		}
	}
	[HideInCallstack]
	public static void logWarningBase(string info)
	{
		if (isIOS() && !isEditor())
		{
			iOSDllImportFrameBase.iOSLog(info);
		}
		else
		{
			UDebug.LogWarning(info);
		}
	}
	// 在Unity编辑器中根据异常调用栈创建一条可定位的错误日志。
	// 日志会显示异常类型、错误信息和完整调用栈，双击日志时会直接跳转到异常实际发生的代码位置，
	// 而不是跳转到捕获异常或调用logException的位置。
	// 此功能依赖UnityEditor内部接口，仅在UNITY_EDITOR环境下生效；
	// 无法获取有效文件和行号或内部接口不可用时，不会创建可定位日志。
	protected static void makeExceptionStack(Exception e, string info = null)
	{
#if UNITY_EDITOR
		try
		{
			// 优先查找最深层内部异常,因为真正的错误可能被外层异常包装
			Exception targetException = e;
			while (targetException.InnerException != null)
			{
				targetException = targetException.InnerException;
			}
			StackFrame targetFrame = null;
			Exception frameException = targetException;
			while (frameException != null && targetFrame == null)
			{
				StackFrame[] frames = new StackTrace(frameException, true).GetFrames();
				if (frames != null)
				{
					foreach (StackFrame frame in frames)
					{
						if (!string.IsNullOrEmpty(frame.GetFileName()) && frame.GetFileLineNumber() > 0)
						{
							targetFrame = frame;
							break;
						}
					}
				}
				// 内部异常中没有可定位的调试信息时,再尝试最外层异常
				if (ReferenceEquals(frameException, e))
				{
					break;
				}
				frameException = e;
			}
			if (targetFrame == null)
			{
				return;
			}
			string filePath = targetFrame.GetFileName();
			int line = targetFrame.GetFileLineNumber();
			int column = targetFrame.GetFileColumnNumber();
			if (column <= 0)
			{
				column = 1;
			}
			Assembly editorAssembly = typeof(EditorApplication).Assembly;
			Type logEntriesType = editorAssembly.GetType("UnityEditor.LogEntries");
			Type logEntryType = editorAssembly.GetType("UnityEditor.LogEntry");
			Type consoleWindowType = editorAssembly.GetType("UnityEditor.ConsoleWindow");
			if (logEntriesType == null || logEntryType == null || consoleWindowType == null)
			{
				return;
			}
			const int logIdentifier = 0x4D4C4558;
			const string callbackRegisterKey = "UnityUtility.ExceptionLogDoubleClickCallback";
			BindingFlags instanceFieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			BindingFlags staticFieldFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			bool callbackRegistered = AppDomain.CurrentDomain.GetData(callbackRegisterKey) is bool registered && registered;
			if (!callbackRegistered)
			{
				EventInfo callbackEvent = consoleWindowType.GetEvent("entryWithManagedCallbackDoubleClicked", staticFieldFlags);
				FieldInfo callbackField = null;
				Type callbackType = null;
				if (callbackEvent != null)
				{
					callbackType = callbackEvent.EventHandlerType;
				}
				else
				{
					callbackField = consoleWindowType.GetField("entryWithManagedCallbackDoubleClicked", staticFieldFlags);
					callbackType = callbackField?.FieldType;
				}
				if (callbackType != null)
				{
					ParameterInfo[] callbackParameters = callbackType.GetMethod("Invoke")?.GetParameters();
					if (callbackParameters != null && callbackParameters.Length == 1)
					{
						Type callbackEntryType = callbackParameters[0].ParameterType;
						FieldInfo identifierField = callbackEntryType.GetField("identifier", instanceFieldFlags);
						FieldInfo fileField = callbackEntryType.GetField("file", instanceFieldFlags);
						FieldInfo lineField = callbackEntryType.GetField("line", instanceFieldFlags);
						FieldInfo columnField = callbackEntryType.GetField("column", instanceFieldFlags);
						MethodInfo openFileMethod = logEntriesType.GetMethod("OpenFileOnSpecificLineAndColumn", staticFieldFlags);
						if (identifierField != null && fileField != null && lineField != null && columnField != null && openFileMethod != null)
						{
							DynamicMethod callbackMethod = new("openExceptionFile", typeof(void), new Type[] { callbackEntryType }, typeof(FrameBaseUtility).Module, true);
							ILGenerator il = callbackMethod.GetILGenerator();
							Label returnLabel = il.DefineLabel();
							// 只处理UnityUtility创建的异常日志,不影响其他系统的双击回调
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldfld, identifierField);
							il.Emit(OpCodes.Ldc_I4, logIdentifier);
							il.Emit(OpCodes.Bne_Un_S, returnLabel);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldfld, fileField);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldfld, lineField);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldfld, columnField);
							il.Emit(OpCodes.Call, openFileMethod);
							il.MarkLabel(returnLabel);
							il.Emit(OpCodes.Ret);
							Delegate callback = callbackMethod.CreateDelegate(callbackType);
							if (callbackEvent != null)
							{
								callbackEvent.GetAddMethod(true)?.Invoke(null, new object[] { callback });
							}
							else if (callbackField != null)
							{
								Delegate oldCallback = callbackField.GetValue(null) as Delegate;
								callbackField.SetValue(null, Delegate.Combine(oldCallback, callback));
							}
							AppDomain.CurrentDomain.SetData(callbackRegisterKey, true);
						}
					}
				}
			}
			object logEntry = Activator.CreateInstance(logEntryType, true);
			string logMessage = makeUnityExceptionMessage(e, info, out int callstackStartUTF16, out int callstackStartUTF8);
			logEntryType.GetField("message", instanceFieldFlags)?.SetValue(logEntry, logMessage);
			logEntryType.GetField("file", instanceFieldFlags)?.SetValue(logEntry, filePath);
			logEntryType.GetField("line", instanceFieldFlags)?.SetValue(logEntry, line);
			logEntryType.GetField("column", instanceFieldFlags)?.SetValue(logEntry, column);
			logEntryType.GetField("identifier", instanceFieldFlags)?.SetValue(logEntry, logIdentifier);
			// 告诉Unity Console从哪个字符开始属于调用栈
			logEntryType.GetField("callstackTextStartUTF16", instanceFieldFlags)?.SetValue(logEntry, callstackStartUTF16);
			logEntryType.GetField("callstackTextStartUTF8", instanceFieldFlags)?.SetValue(logEntry, callstackStartUTF8);
			// ScriptingException | DontExtractStacktrace
			logEntryType.GetField("mode", instanceFieldFlags)?.SetValue(logEntry, (1 << 17) | (1 << 18));
			MethodInfo addMessageMethod = logEntriesType.GetMethod("AddMessageWithDoubleClickCallback", staticFieldFlags);
			addMessageMethod?.Invoke(null, new object[] { logEntry });
		}
		catch (Exception editorException)
		{
			UDebug.LogError("创建可定位异常日志失败:" + editorException);
		}
#endif
	}
	public static void setScreenSizeBase(Vector2Int size, bool fullScreen)
	{
		Screen.SetResolution(size.x, size.y, fullScreen);

		// UGUI
		GameObject uguiRootObj = findRootGameObject(UGUI_ROOT);
		uguiRootObj.TryGetComponent<RectTransform>(out var uguiRectTransform);
		uguiRectTransform.offsetMin = -size / 2;
		uguiRectTransform.offsetMax = size / 2;
		uguiRectTransform.anchorMax = Vector2.one * 0.5f;
		uguiRectTransform.anchorMin = Vector2.one * 0.5f;
		Camera camera = findGameObject("UICamera", uguiRootObj).GetComponent<Camera>();
		if (camera.orthographic)
		{
			camera.orthographicSize = size.y * 0.5f;
		}
		else
		{
			camera.transform.localPosition = new(0.0f, 0.0f, -size.y * 0.5f / Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f));
		}
	}
	public static GameObject findRootGameObject(string name, bool errorIfNull = false)
	{
		if (string.IsNullOrEmpty(name))
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
	public static GameObject findGameObject(string name, GameObject parent, bool errorIfNull = false, bool recursive = true)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (parent == null)
		{
			logErrorBase("parent不能为空,查找无父节点的GameObject请使用getRootGameObject");
			return null;
		}
		GameObject go = null;
		Transform trans = parent.transform.Find(name);
		if (trans != null)
		{
			go = trans.gameObject;
		}
		// 如果父节点的第一级子节点中找不到,就递归查找
		else if (recursive)
		{
			int childCount = parent.transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				go = findGameObject(name, parent.transform.GetChild(i).gameObject, false, recursive);
				if (go != null)
				{
					break;
				}
			}
		}

		if (go == null && errorIfNull)
		{
			logErrorBase("找不到物体,请确认是否存在:" + name + ", path:" + getGameObjectPathBase(parent) + "/" + name);
		}
		return go;
	}
	public static string getGameObjectPathBase(GameObject go)
	{
		if (go == null)
		{
			return "";
		}
		Transform transform = go.transform;
		string path = go.name;
		while (transform != null)
		{
			Transform parentTrans = transform.parent;
			if (parentTrans == null)
			{
				break;
			}
			path = parentTrans.name + "/" + path;
			transform = transform.parent;
		}
		return path;
	}
	public static Vector2 getScreenScale(Vector2 rootSize)
	{
		Vector2Int uiSize = FrameSettings.getUISize();
		return new(rootSize.x / uiSize.x, rootSize.y / uiSize.y);
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
		if (go == null)
		{
			return;
		}
		if (go.TryGetComponent<T>(out var com))
		{
			destroyUnityObject(com, true);
		}
	}
	public static T getOrAddComponent<T>(GameObject go) where T : Component
	{
		if (go == null)
		{
			return null;
		}
		if (!go.TryGetComponent(out T com))
		{
			com = go.AddComponent<T>();
		}
		return com;
	}
	// 返回值表示是否是新添加的组件
	public static bool getOrAddComponent<T>(GameObject go, out T com) where T : Component
	{
		if (go == null)
		{
			com = null;
			return false;
		}
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
		long[] source = new long[3];
		long[] target = new long[3];
		for (int i = 0; i < 3; ++i)
		{
			if (!long.TryParse(sourceFormatStr[i], out source[i]))
			{
				lowerVersion = VERSION_COMPARE.REMOTE_LOWER;
				higherVersion = VERSION_COMPARE.REMOTE_LOWER;
				return VERSION_COMPARE.REMOTE_LOWER;
			}
			if (!long.TryParse(targetFormatStr[i], out target[i]))
			{
				lowerVersion = VERSION_COMPARE.LOCAL_LOWER;
				higherVersion = VERSION_COMPARE.LOCAL_LOWER;
				return VERSION_COMPARE.LOCAL_LOWER;
			}
		}

		if (source[0] != target[0])
		{
			higherVersion = source[0] > target[0] ? VERSION_COMPARE.LOCAL_LOWER : VERSION_COMPARE.REMOTE_LOWER;
		}
		else if (source[1] != target[1])
		{
			higherVersion = source[1] > target[1] ? VERSION_COMPARE.LOCAL_LOWER : VERSION_COMPARE.REMOTE_LOWER;
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

		for (int i = 0; i < 3; ++i)
		{
			if (source[i] > target[i])
			{
				return VERSION_COMPARE.LOCAL_LOWER;
			}
			if (source[i] < target[i])
			{
				return VERSION_COMPARE.REMOTE_LOWER;
			}
		}
		return VERSION_COMPARE.EQUAL;
	}
	public static string checkValidVersion(string version)
	{
		checkValidVersion(ref version);
		return version;
	}
	public static bool checkValidVersion(ref string version)
	{
		if (string.IsNullOrEmpty(version))
		{
			version = "0.0.0";
			return false;
		}
		string[] elements = version.Split(".");
		if (elements.Length != 3)
		{
			version = "0.0.0";
			return false;
		}
		for (int i = 0; i < elements.Length; ++i)
		{
			if (!int.TryParse(elements[i], out _))
			{
				version = "0.0.0";
				return false;
			}
		}
		return true;
	}
#if BYTE_DANCE
	public static string GetTTPersistantPath()
	{
		switch (TT.GetSystemInfo().platform)
		{
			case "devtools":
			case "windows":
				return "ttfile://user/";
			default:
				return "scfile://user/";
		}
	}
#endif
	public static UnityWebRequest unityWebRequest(string url)
	{
		if (isWebGL() && url.StartsWith(F_PERSISTENT_ASSETS_PATH))
		{
			logErrorBase("webgl下无法使用UnityWebRequest读取PersistentDataPath中的资源");
			return null;
		}
		return UnityWebRequest.Get(url);
	}
	// 如果str不以prefix开头,则在str开头加上prefix
	public static string ensurePrefix(string str, string prefix)
	{
		if (!str.StartsWith(prefix))
		{
			return prefix + str;
		}
		return str;
	}
	// 通过WWW加载本地资源时,需要确保路径的前缀正确
	public static void checkDownloadPath(ref string path)
	{
		if (isEditor() || isWindows())
		{
			path = ensurePrefix(path, "file:///");
		}
		else if (isIOS() || isLinux() || isMacOS())
		{
			path = ensurePrefix(path, "file://");
		}
		else if (isAndroid())
		{
			// android本地加载需要添加jar:file://前缀
			path = ensurePrefix(path, "jar:file://");
		}
	}
	// fileName为绝对路径
	public static IEnumerator openFileAsyncInternal(string fileName, bool errorIfNull, BytesCallback callback)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			callBytesCallback(callback, null);
			yield break;
		}
		DateTime start = DateTime.Now;
		// 小游戏里面由于路径前缀不同,所以只能使用小游戏的sdk来读取persistPath
		if (!isEditor() && isWebGL() && fileName.StartsWith(F_PERSISTENT_DATA_PATH))
		{
			if (isByteDance())
			{
				yield return TTFileSystem.readBytesAsync(fileName, (byte[] bytes) =>
				{
					if (errorIfNull && bytes == null)
					{
						logErrorBase("open file failed:" + fileName);
					}
					logBase("打开文件耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒,file:" + fileName);
					callBytesCallback(callback, bytes);
				});
			}
			else if (isWeiXin())
			{
				yield return WeChatFileSystem.readBytesAsync(fileName, (byte[] bytes) =>
				{
					if (errorIfNull && bytes == null)
					{
						logErrorBase("open file failed:" + fileName);
					}
					logBase("打开文件耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒,file:" + fileName);
					callBytesCallback(callback, bytes);
				});
			}
			else
			{
				logErrorBase("not supported");
				callBytesCallback(callback, null);
			}
		}
		else
		{
			checkDownloadPath(ref fileName);
			using var www = unityWebRequest(fileName);
			yield return www.SendWebRequest();
			if (errorIfNull && www.downloadHandler.data == null)
			{
				logErrorBase("open file failed:" + fileName + ", info:" + www.error + ", error:" + www.downloadHandler.error);
			}
			logBase("打开文件耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒,file:" + fileName);
			callBytesCallback(callback, www.downloadHandler.data);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void callBytesCallback(BytesCallback callback, byte[] bytes)
	{
		try
		{
			callback?.Invoke(bytes);
		}
		catch (Exception e)
		{
			logExceptionBase(e);
		}
	}
	// 将异常转换为Unity Console可显示和点击的日志格式。
	// 每一层包含源码文件和行号的调用栈都会被转换为可点击链接。
	protected static string makeUnityExceptionMessage(Exception e, string info, out int callstackStartUTF16, out int callstackStartUTF8)
	{
		string exceptionText = e.ToString().Replace("\r\n", "\n").Replace('\r', '\n');

		// Mono异常调用栈格式:
		// at Test.execute() [0x00000] in E:\Project\Test.cs:10
		//
		// 转换为Unity Console的超链接格式:
		// at Test.execute() [0x00000] (at <link="href='E:\Project\Test.cs' line='10'">E:\Project\Test.cs:10</link>)

		string hyperlinkColor = getUnityConsoleHyperlinkColor();
		// 将Mono格式的调用栈转换为Unity Console超链接格式
		exceptionText = Regex.Replace
		(
			exceptionText,
			@"\s+in\s+(.+):(?:line\s+)?(\d+)\s*$",
			match =>
			{
				string filePath = match.Groups[1].Value.Trim();
				string lineString = match.Groups[2].Value;
				string linkFilePath = escapeUnityConsoleLink(filePath);
				string linkText = escapeUnityConsoleLink(filePath + ":" + lineString);
				return " (at <color=" + hyperlinkColor + "><link=\"href='" + linkFilePath + "' line='" + lineString + "'\">" + linkText + "</link></color>)";
			},
			RegexOptions.Multiline
		);

		// 兼容已经是(at 文件:行号),但还没有添加link标签的调用栈
		exceptionText = Regex.Replace
		(
			exceptionText,
			@"\(at\s+(?!<color|<link)(.+):(\d+)\)",
			match =>
			{
				string filePath = match.Groups[1].Value.Trim();
				string lineString = match.Groups[2].Value;
				string linkFilePath = escapeUnityConsoleLink(filePath);
				string linkText = escapeUnityConsoleLink(filePath + ":" + lineString);
				return "(at <color=" + hyperlinkColor + "><link=\"href='" + linkFilePath + "' line='" + lineString + "'\">" + linkText + "</link></color>)";
			}
		);

		string logMessage;
		if (string.IsNullOrEmpty(info))
		{
			logMessage = exceptionText;
		}
		else
		{
			logMessage = info + "\n" + exceptionText;
		}

		// 调用栈超链接已经手动生成,不再让Unity重复解析调用栈
		callstackStartUTF16 = logMessage.Length;
		callstackStartUTF8 = Encoding.UTF8.GetByteCount(logMessage);
		return logMessage;
	}
	// 转义Unity Console超链接中的特殊字符。
	protected static string escapeUnityConsoleLink(string value)
	{
		if (value == null)
		{
			return "";
		}
		return value.Replace("&", "&amp;")
					.Replace("\"", "&quot;")
					.Replace("'", "&apos;")
					.Replace("<", "&lt;")
					.Replace(">", "&gt;");
	}
	// 获取与Unity Console一致的堆栈超链接颜色。
	protected static string getUnityConsoleHyperlinkColor()
	{
#if UNITY_EDITOR
		try
		{
			MethodInfo method = typeof(EditorGUIUtility).GetMethod("GetHyperlinkColorForSkin", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method?.Invoke(null, null) is string color && !string.IsNullOrEmpty(color))
			{
				return color;
			}
		}
		catch { }
		return EditorGUIUtility.isProSkin ? "#40a0ff" : "#0000FF";
#else
		return "#0000FF";
#endif
	}
}