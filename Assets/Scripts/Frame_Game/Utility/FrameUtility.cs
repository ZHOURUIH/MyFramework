using System;
using System.Collections.Generic;
using static BinaryUtility;
using static CSharpUtility;
using static FileUtility;
using static StringUtility;
using static FrameBase;
using static FrameDefineBase;
using static FrameEditorUtility;

// 一些框架层的方便使用的工具函数,包含命令,对象池,列表池,字符串拼接池,延迟执行函数,以及其他的与框架层逻辑有关的工具函数
public class FrameUtility
{
	public static GameCamera getMainCamera() { return mCameraManager?.getMainCamera(); }
	public static GameScene getCurScene() { return mGameSceneManager.getCurScene(); }
	public static string toPercent(float value, int precision = 1) { return FToS(value * 100, precision); }
	public static myUGUIObject getUGUIRoot() { return mLayoutManager?.getUIRoot(); }
	// 获得一个合适的文件加载路径,fileName是StreamingAssets下的相对路径,带后缀
	public static string availableReadPath(string fileName)
	{
		if (isEditor())
		{
			// 编辑器中从StreamingAssets读取
			return F_ASSET_BUNDLE_PATH + fileName;
		}
		else
		{
			// 非编辑器中时,根据文件对比结果来判断从哪儿加载
			return mAssetVersionSystem.getFileReadPath(fileName);
		}
	}
	public static void writeFileList(string path, string content)
	{
		writeTxtFile(path + FILE_LIST, content);
		// 再生成此文件的MD5文件,用于客户端校验文件内容是否改变
		writeTxtFile(path + FILE_LIST_MD5, generateFileMD5(stringToBytes(content), -1));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 跳转流程或场景的工具函数
	public static void changeProcedure<T>(string intent = null) where T : SceneProcedure
	{
		getCurScene().changeProcedure(typeof(T), intent);
	}
	public static void enterScene<T>(Type startProcedure = null, string intent = null) where T : GameScene
	{
		mGameSceneManager.enterScene(typeof(T), startProcedure, intent);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 命令
	// 在主线程中创建立即执行的命令
	public static Command CMD(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		if (mClassPool == null)
		{
			return null;
		}
		Command cmd = mClassPool.newClass(type, false) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(false);
		cmd.setThreadCommand(false);
		return cmd;
	}
	// 在主线程中创建延迟执行的命令
	public static Command CMD_DELAY(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		if (mClassPool == null)
		{
			return null;
		}
		Command cmd = mClassPool.newClass(type, false) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(true);
		cmd.setThreadCommand(false);
		return cmd;
	}
	// 在子线程中创建延迟执行的命令
	public static Command CMD_DELAY_THREAD(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		if (mClassPoolThread == null)
		{
			return null;
		}
		Command cmd = mClassPoolThread.newClass(type) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(true);
		cmd.setThreadCommand(true);
		return cmd;
	}
	// 在主线程中创建立即执行的命令
	public static void CMD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		cmd = CMD(typeof(T), logLevel) as T;
	}
	// 在主线程中创建延迟执行的命令
	public static void CMD_DELAY<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		cmd = CMD_DELAY(typeof(T), logLevel) as T;
	}
	// 在子线程中创建延迟执行的命令
	public static void CMD_DELAY_THREAD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		cmd = CMD_DELAY_THREAD(typeof(T), logLevel) as T;
	}
	// 在主线程中发送一个指定类型的命令
	public static void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		mCommandSystem?.pushCommand(cmd, cmdReceiver);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute, DelayCmdWatcher watcher)
	{
		mCommandSystem?.pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 列表对象池
	public static void LIST<T>(out List<T> list, IEnumerable<T> initList = null)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new();
			return;
		}
		string stackTrace = mGameFramework.mParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		if (initList != null)
		{
			list.AddRange(initList);
		}
	}
	public static void LIST_PERSIST<T>(out List<T> list, IEnumerable<T> initList = null)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new();
			return;
		}
		string stackTrace = mGameFramework.mParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, false) as List<T>;
		if (initList != null)
		{
			list.AddRange(initList);
		}
	}
	public static void UN_LIST<T>(ref List<T> list)
	{
		if (mGameFramework == null || mListPool == null || list == null)
		{
			return;
		}
		mListPool.destroyList(ref list, typeof(T));
	}
	public static void SET_PERSIST<T>(out HashSet<T> list, IEnumerable<T> initList = null)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new();
			return;
		}
		string stackTrace = mGameFramework.mParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list = mHashSetPool.newList(typeof(T), typeof(HashSet<T>), stackTrace, false) as HashSet<T>;
		if (initList != null)
		{
			list.addRange(initList);
		}
	}
	public static void DIC_PERSIST<K, V>(out Dictionary<K, V> list)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new();
			return;
		}
		string stackTrace = mGameFramework.mParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list = mDictionaryPool.newList(typeof(K), typeof(V), typeof(Dictionary<K, V>), stackTrace, false) as Dictionary<K, V>;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 对象池
	public static T CLASS<T>(out T value) where T : ClassObject, new()
	{
		if (mClassPool == null)
		{
			value = new();
			return null;
		}
		value = mClassPool?.newClass(typeof(T), false) as T;
		return value;
	}
	public static T CLASS<T>() where T : ClassObject, new()
	{
		if (mClassPool == null)
		{
			return new();
		}
		return mClassPool?.newClass(typeof(T), false) as T;
	}
	public static ClassObject CLASS(Type type)
	{
		if (mClassPool == null)
		{
			return createInstance<ClassObject>(type);
		}
		return mClassPool?.newClass(type, false);
	}
	public static void UN_CLASS<T>(ref T obj) where T : ClassObject
	{
		mClassPool?.destroyClass(ref obj);
	}
	public static void UN_CLASS<T>(T obj) where T : ClassObject
	{
		mClassPool?.destroyClass(ref obj);
	}
	public static void UN_CLASS_LIST<T>(IList<T> objList) where T : ClassObject
	{
		mClassPool?.destroyClassList(objList);
		objList.Clear();
	}
	public static void UN_CLASS_LIST<T>(HashSet<T> objList) where T : ClassObject
	{
		mClassPool?.destroyClassList(objList);
		objList.Clear();
	}
	public static void UN_CLASS_LIST<T0, T1>(IDictionary<T0, T1> objList) where T1 : ClassObject
	{
		mClassPool?.destroyClassList(objList.Values);
		objList.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 数组对象池
	public static void ARRAY_THREAD<T>(out T[] array, int count)
	{
		if (mArrayPoolThread == null)
		{
			array = new T[count];
			return;
		}
		array = mArrayPoolThread.newArray<T>(count);
	}
	public static void UN_ARRAY_THREAD<T>(ref T[] array, bool destroyReally = false)
	{
		mArrayPoolThread?.destroyArray(ref array, destroyReally);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// delayCall和delayCallSafe都可以实现在一定条件下阻止延迟函数执行,只不过通过delayCall的DelayCmdWatcher可以决定任意时刻终止延迟执行
	// 而delayCallSafe只有在guard销毁以后才能终止延迟执行
	// 在主线程中发起延迟调用函数,函数将在主线程中调用,如果watcher在开始执行命令时被销毁了,则命令不会被执行
	public static long delayCall(Action function, float delayTime = 0.0f, DelayCmdWatcher watcher = null)
	{
		if (function == null || mGlobalCmdReceiver == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCall cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCall cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
	}
	public static void backupCrossParam()
	{
		FrameCrossParam.mPersistentDataVersion = mAssetVersionSystem.getPersistentAssetsVersion();
		FrameCrossParam.mStreamingAssetsVersion = mAssetVersionSystem.getStreamingAssetsVersion();
		FrameCrossParam.mRemoteVersion = mAssetVersionSystem.getRemoteAssetsVersion();
		FrameCrossParam.mDownloadURL = mResourceManager.getDownloadURL();
		FrameCrossParam.mStreamingAssetsFileList.setRange(mAssetVersionSystem.getStreamingAssetsFile());
		FrameCrossParam.mPersistentAssetsFileList.setRange(mAssetVersionSystem.getPersistentAssetsFile());
		FrameCrossParam.mRemoteAssetsFileList.setRange(mAssetVersionSystem.getRemoteAssetsFile());
		FrameCrossParam.mTotalDownloadedFiles.setRange(mAssetVersionSystem.getDownloadedFiles());
		FrameCrossParam.mReadPathType = AssetVersionSystem.getReadPathType();
		FrameCrossParam.mLocalizationName = ResLocalizationText.mCurLanguage;
		FrameCrossParam.mFramworkParam = mGameFramework.mParam;
	}
}