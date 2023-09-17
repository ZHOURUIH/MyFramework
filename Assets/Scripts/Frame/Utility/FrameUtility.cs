using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;
using static StringUtility;
using static MathUtility;
using static BinaryUtility;
using static FrameBase;
using static FrameDefine;

// 一些框架层的方便使用的工具函数,包含命令,对象池,列表池,字符串拼接池,延迟执行函数,以及其他的与框架层逻辑有关的工具函数
public class FrameUtility
{
	protected static LONG mTempStateID = new LONG();	// 避免GC
	// 移除第一个指定类型的状态
	public static void characterRemoveState<T>(Character character, string param = null) where T : CharacterState
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用characterRemoveState<T>");
		}
		CharacterState state = character.getFirstState(type);
		if (state == null)
		{
			return;
		}
		CmdCharacterRemoveState.execute(character, state, param);
	}
	public static GameCamera getMainCamera() { return mCameraManager.getMainCamera(); }
	public static bool isKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputSystem.isKeyCurrentDown(key, mask); }
	public static bool isKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputSystem.isKeyCurrentUp(key, mask); }
	public static bool isKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputSystem.isKeyDown(key, mask); }
	public static bool isKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputSystem.isKeyUp(key, mask); }
	// 仅编辑器或桌面端使用,与多点触控相关的逻辑时不能使用此鼠标坐标,应该获取相应触点的坐标,否则会出错误
#if UNITY_EDITOR || UNITY_STANDALONE
	public static Vector3 getMousePosition(bool leftBottomAsOrigin = true)
	{
		Vector3 mousePos = mInputSystem.getTouchPoint((int)MOUSE_BUTTON.LEFT).getCurPosition();
		if (leftBottomAsOrigin)
		{
			return mousePos;
		}
		else
		{
			return mousePos - (Vector3)getHalfScreenSize();
		}
	}
#endif
	public static GameScene getCurScene() { return mGameSceneManager.getCurScene(); }
	public static bool atProcedure(Type type) { return getCurScene().atProcedure(type); }
	public static bool atProcedure<T>() where T : SceneProcedure { return getCurScene().atProcedure(typeof(T)); }
	// 百分比一般用于属性增幅之类的
	public static string toPercent(string value, int precision = 1) { return FToS(SToF(value) * 100, precision); }
	public static string toPercent(float value, int precision = 1) { return FToS(value * 100, precision); }
	// 几率类的一般是万分比的格式填写的
	public static string toProbability(string value) { return FToS(SToF(value) * 0.01f); }
	public static string toProbability(float value) { return FToS(value * 0.01f); }
	public static myUGUIObject getUGUIRoot() { return mLayoutManager?.getUIRoot(); }
	public static Canvas getUGUIRootComponent() { return mLayoutManager?.getUIRoot()?.getCanvas(); }
	public static Camera getUICamera() { return mCameraManager.getUICamera()?.getCamera(); }
	// 将UI的宽高调整为偶数
	public static void makeSizeEven(myUGUIObject obj)
	{
		Vector2 scrollRectSize = obj.getWindowSize();
		int intScrollSizeX = ceil(scrollRectSize.x);
		int intScrollSizeY = ceil(scrollRectSize.y);
		float newScrollSizeX = intScrollSizeX + (intScrollSizeX & 1);
		float newScrollSizeY = intScrollSizeY + (intScrollSizeY & 1);
		if (!isFloatEqual(newScrollSizeX, scrollRectSize.x) || !isFloatEqual(newScrollSizeY, scrollRectSize.y))
		{
			obj.setWindowSize(new Vector2(newScrollSizeX, newScrollSizeY));
		}
	}
	public static T getData<T>(int id) where T : ExcelData
	{
		return mExcelManager.getData<T>(id);
	}
	public static T PACKET<T>(out T packet) where T : NetPacket
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用PACKET<T>");
		}
		return packet = mSocketFactory.createSocketPacket(type) as T;
	}
	// 获得一个合适的文件写入路径,fileName是StreamingAssets下的相对路径,带后缀
	public static string availableWritePath(string fileName)
	{
		// 安卓真机和iOS真机上是优先从PersistentDataPath中加载,其他的都是在StreamingAssets中
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		return F_PERSISTENT_ASSETS_PATH + fileName;
#else
		return F_ASSET_BUNDLE_PATH + fileName;
#endif
	}
	// 获得一个合适的文件加载路径,fileName是StreamingAssets下的相对路径,带后缀
	public static string availableReadPath(string fileName)
	{
		// 安卓真机和iOS真机上是优先从PersistentDataPath中加载,其他的都是在StreamingAssets中
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		// 优先从PersistentDataPath中加载
		string path = F_PERSISTENT_ASSETS_PATH + fileName;
		if (FileUtility.isFileExist(path))
		{
			return path;
		}
		// PersistentDataPath中没有,再从AssetBundlePath中加载,原本在StreamingAssets中的文件也都在AssetBundlePath中
		return F_ASSET_BUNDLE_PATH + fileName;
#else
		return F_ASSET_BUNDLE_PATH + fileName;
#endif
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 跳转流程或场景的工具函数
	// 主工程中可调用的跳转流程的函数
	public static void changeProcedure<T>(string intent = null) where T : SceneProcedure
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用changeProcedure<T>");
		}
		CMD(out CmdGameSceneChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = type;
		cmd.mIntent = intent;
		pushCommand(cmd, getCurScene());
	}
	public static CmdGameSceneChangeProcedure changeProcedureDelay<T>(float delayTime = 0.001f, string intent = null) where T : SceneProcedure
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用changeProcedure<T>");
		}
		CMD_DELAY(out CmdGameSceneChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = type;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, getCurScene(), delayTime);
		return cmd;
	}
	public static void prepareChangeProcedure<T>(float prepareTime = 0.001f, string intent = null) where T : SceneProcedure
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用changeProcedure<T>");
		}
		CmdGameScenePrepareChangeProcedure.execute(getCurScene(), type, intent, prepareTime);
	}
	public static void enterScene<T>(Type startProcedure = null, string intent = null) where T : GameScene
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用enterScene<T>");
		}
		CMD(out CmdGameSceneManagerEnter cmd, LOG_LEVEL.FORCE);
		cmd.mSceneType = type;
		cmd.mStartProcedure = startProcedure;
		cmd.mIntent = intent;
		pushCommand(cmd, mGameSceneManager);
	}
	// 延迟到下一帧跳转
	public static void enterSceneDelay<T>(Type startProcedure = null, string intent = null) where T : GameScene
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用enterScene<T>");
		}
		CMD_DELAY(out CmdGameSceneManagerEnter cmd, LOG_LEVEL.FORCE);
		cmd.mSceneType = type;
		cmd.mStartProcedure = startProcedure;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, mGameSceneManager);
	}
	public static void changeProcedure(Type procedure, string intent = null)
	{
		CMD(out CmdGameSceneChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushCommand(cmd, getCurScene());
	}
	public static CmdGameSceneChangeProcedure changeProcedureDelay(Type procedure, float delayTime = 0.001f, string intent = null)
	{
		CMD_DELAY(out CmdGameSceneChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, getCurScene(), delayTime);
		return cmd;
	}
	public static void prepareChangeProcedure(Type procedure, float prepareTime = 0.001f, string intent = null)
	{
		CmdGameScenePrepareChangeProcedure.execute(getCurScene(), procedure, intent, prepareTime);
	}
	public static void enterScene(Type sceneType, Type startProcedure = null, string intent = null)
	{
		CMD(out CmdGameSceneManagerEnter cmd, LOG_LEVEL.FORCE);
		cmd.mSceneType = sceneType;
		cmd.mStartProcedure = startProcedure;
		cmd.mIntent = intent;
		pushCommand(cmd, mGameSceneManager);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 命令
	// 在主线程中创建立即执行的命令
	public static Command CMD(Type type, bool newDirect, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		Command cmd = mClassPool.newClass(type, out _, false, newDirect) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(false);
		cmd.setThreadCommand(false);
		return cmd;
	}
	// 在主线程中创建延迟执行的命令
	public static Command CMD_DELAY(Type type, bool newDirect, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		Command cmd = mClassPool.newClass(type, out _, false, newDirect) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(true);
		cmd.setThreadCommand(false);
		return cmd;
	}
	// 在子线程中创建立即执行的命令
	public static Command CMD_THREAD(Type type, bool newDirect, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		Command cmd = mClassPoolThread.newClass(type, out _, newDirect) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(false);
		cmd.setThreadCommand(true);
		return cmd;
	}
	// 在子线程中创建延迟执行的命令
	public static Command CMD_DELAY_THREAD(Type type, bool newDirect, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		Command cmd = mClassPoolThread.newClass(type, out _, newDirect) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(true);
		cmd.setThreadCommand(true);
		return cmd;
	}
	// 在主线程中创建立即执行的命令
	public static void CMD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD<T>");
		}
		cmd = CMD(type, true, logLevel) as T;
	}
	// 在主线程中创建延迟执行的命令
	public static void CMD_DELAY<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD_DELAY<T>");
		}
		cmd = CMD_DELAY(type, true, logLevel) as T;
	}
	// 在子线程中创建立即执行的命令
	public static void CMD_THREAD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD_THREAD<T>");
		}
		cmd = CMD_THREAD(type, true, logLevel) as T;
	}
	// 在子线程中创建延迟执行的命令
	public static void CMD_DELAY_THREAD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD_DELAY_THREAD<T>");
		}
		cmd = CMD_DELAY_THREAD(type, true, logLevel) as T;
	}
	// 在子线程中发送一个指定类型的命令
	public static void pushCommandThread<T>(CommandReceiver cmdReceiver, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD_THREAD(out T cmd, logLevel);
		pushCommand(cmd, cmdReceiver);
	}
	// 在主线程中发送一个指定类型的命令
	public static void pushCommand<T>(CommandReceiver cmdReceiver, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD(out T cmd, logLevel);
		pushCommand(cmd, cmdReceiver);
	}
	// 在主线程中发送一个指定类型的命令
	public static void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	// 在子线程中发送一个指定类型的命令,并且会延迟到主线程执行
	public static T pushDelayCommandThread<T>(DelayCmdWatcher watcher, CommandReceiver cmdReceiver, float delayExecute = 0.001f, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD_DELAY_THREAD(out T cmd, logLevel);
		pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	// 在主线程中发送一个指定类型的命令,并且在主线程中延迟执行
	public static T pushDelayCommand<T>(DelayCmdWatcher watcher, CommandReceiver cmdReceiver, float delayExecute = 0.001f, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD_DELAY(out T cmd, logLevel);
		pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute)
	{
		pushDelayCommand(cmd, cmdReceiver, delayExecute, null);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		pushDelayCommand(cmd, cmdReceiver, 0.0f, null);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute, DelayCmdWatcher watcher)
	{
		mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 列表对象池
	public static void LIST<T>(out List<T> list)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new List<T>();
			return;
		}
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST<T>");
		}
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = mListPool.newList(type, typeof(List<T>), stackTrace, true) as List<T>;
	}
	public static void LIST_PERSIST<T>(out List<T> list)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new List<T>();
			return;
		}
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST_PERSIST<T>");
		}
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = mListPool.newList(type, typeof(List<T>), stackTrace, false) as List<T>;
	}
	public static void UN_LIST<T>(ref List<T> list)
	{
		if (mGameFramework == null || mListPool == null)
		{
			return;
		}
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用UN_LIST<T>");
		}
		mListPool?.destroyList(ref list, type);
	}
	public static void LIST_PERSIST<T>(out HashSet<T> list)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new HashSet<T>();
			return;
		}
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST_PERSIST<T>");
		}
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = mHashSetPool.newList(type, typeof(HashSet<T>), stackTrace, false) as HashSet<T>;
	}
	public static void LIST_PERSIST<K, V>(out Dictionary<K, V> list)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new Dictionary<K, V>();
			return;
		}
		Type typeKey = Typeof<K>();
		Type typeValue = Typeof<V>();
		if (typeKey == null || typeValue == null)
		{
			logError("热更工程无法使用LIST_PERSIST<K, V>");
		}
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = mDictionaryPool.newList(typeKey, typeValue, typeof(Dictionary<K, V>), stackTrace, false) as Dictionary<K, V>;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 对象池
	public static ClassObject CLASS_ONCE(Type type, out bool isNew)
	{
		isNew = true;
		if (mClassPool == null)
		{
			return createInstance<ClassObject>(type);
		}
		return mClassPool?.newClass(type, out isNew, true, false);
	}
	public static void CLASS_ONCE<T>(out T value) where T : ClassObject
	{
		if (mClassPool == null)
		{
			value = createInstanceDirect<T>(typeof(T));
			return;
		}
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS<T>");
		}
		value = mClassPool?.newClass(type, out _, true, true) as T;
	}
	public static void CLASS<T>(out T value) where T : ClassObject
	{
		if (mClassPool == null)
		{
			value = createInstanceDirect<T>(typeof(T));
			return;
		}
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS<T>");
		}
		value = mClassPool?.newClass(type, out _, false, true) as T;
	}
	public static T CLASS<T>() where T : ClassObject
	{
		if (mClassPool == null)
		{
			return createInstanceDirect<T>(typeof(T));
		}
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS<T>");
		}
		return mClassPool?.newClass(type, out _, false, true) as T;
	}
	public static ClassObject CLASS(Type type)
	{
		if (mClassPool == null)
		{
			return createInstance<ClassObject>(type);
		}
		return mClassPool?.newClass(type, out _, false, false);
	}
	public static ClassObject CLASS(Type type, out bool isNew)
	{
		if (mClassPool == null)
		{
			isNew = true;
			return createInstance<ClassObject>(type);
		}
		isNew = false;
		return mClassPool?.newClass(type, out isNew, false, false);
	}
	// 由于热更工程无法使用多线程,所以此处不考虑热更工程
	public static T CLASS_THREAD<T>() where T : ClassObject
	{
		if (mClassPoolThread == null)
		{
			return createInstanceDirect<T>(typeof(T));
		}
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用CLASS_THREAD<T>");
		}
		return mClassPoolThread?.newClass(typeof(T), out _, true) as T;
	}
	public static void UN_CLASS<T>(ref T obj) where T : ClassObject
	{
		mClassPool?.destroyClass(ref obj);
	}
	public static void UN_CLASS_LIST<T>(List<T> objList) where T : ClassObject
	{
		mClassPool?.destroyClass(objList);
	}
	public static void UN_CLASS_LIST<K, T>(Dictionary<K, T> objList) where T : ClassObject
	{
		mClassPool?.destroyClass(objList);
	}
	public static void UN_CLASS_THREAD<T>(ref T obj) where T : ClassObject
	{
		mClassPoolThread?.destroyClass(ref obj);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 数组对象池
	public static void ARRAY_PERSIST<T>(out T[] array, int count)
	{
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用ARRAY_PERSIST<T>");
		}
		if (mArrayPool == null)
		{
			array = new T[count];
			return;
		}
		array = mArrayPool.newArray<T>(count, false);
	}
	public static void UN_ARRAY<T>(ref T[] array, bool destroyReally = false)
	{
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用UN_ARRAY<T>");
		}
		mArrayPool?.destroyArray(ref array, destroyReally);
	}
	public static void UN_ARRAY<T>(List<T[]> arrayList, bool destroyReally = false)
	{
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用UN_ARRAY<T>");
		}
		mArrayPool?.destroyArray(arrayList, destroyReally);
	}
	public static void ARRAY_THREAD<T>(out T[] array, int count)
	{
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用ARRAY_THREAD<T>");
		}
		if (mArrayPoolThread == null)
		{
			array = new T[count];
			return;
		}
		array = mArrayPoolThread.newArray<T>(count);
	}
	public static void UN_ARRAY_THREAD<T>(ref T[] array, bool destroyReally = false)
	{
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用UN_ARRAY_THREAD<T>");
		}
		mArrayPoolThread?.destroyArray(ref array, destroyReally);
	}
	public static void UN_ARRAY_THREAD<T>(List<T[]> arrayList, bool destroyReally = false)
	{
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用UN_ARRAY_THREAD<T>");
		}
		mArrayPoolThread?.destroyArray(arrayList, destroyReally);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 在主线程中发起延迟调用函数,函数将在主线程中调用
	public static long delayCall(Action function, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCall cmd);
		cmd.mFunction = function;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0>(Action<T0> function, T0 param0, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam1<T0> cmd);
		cmd.mFunction = function;
		cmd.mParam = param0;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0, T1>(Action<T0, T1> function, T0 param0, T1 param1, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam2<T0, T1> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0, T1, T2>(Action<T0, T1, T2> function, T0 param0, T1 param1, T2 param2, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam3<T0, T1, T2> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0, T1, T2, T3>(Action<T0, T1, T2, T3> function, T0 param0, T1 param1, T2 param2, T3 param3, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam4<T0, T1, T2, T3> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		cmd.mParam3 = param3;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> function, T0 param0, T1 param1, T2 param2, T3 param3, T4 param4, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam5<T0, T1, T2, T3, T4> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		cmd.mParam3 = param3;
		cmd.mParam4 = param4;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	// 在子线程中发起延迟调用函数,函数将在主线程中调用
	public static long delayCallThread(Action function, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCall cmd);
		cmd.mFunction = function;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0>(Action<T0> function, T0 param0, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam1<T0> cmd);
		cmd.mFunction = function;
		cmd.mParam = param0;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0, T1>(Action<T0, T1> function, T0 param0, T1 param1, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam2<T0, T1> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0, T1, T2>(Action<T0, T1, T2> function, T0 param0, T1 param1, T2 param2, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam3<T0, T1, T2> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0, T1, T2, T3>(Action<T0, T1, T2, T3> function, T0 param0, T1 param1, T2 param2, T3 param3, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam4<T0, T1, T2, T3> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		cmd.mParam3 = param3;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> function, T0 param0, T1 param1, T2 param2, T3 param3, T4 param4, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam5<T0, T1, T2, T3, T4> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		cmd.mParam3 = param3;
		cmd.mParam4 = param4;
		pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static ushort generateCRC16(byte[] buffer, int count, int bufferOffset = 0)
	{
		return (ushort)(crc16(0x1F, buffer, count, bufferOffset) ^ 0x123F);
	}
	public static ushort generateCRC16(ushort value)
	{
		ushortToBytes(value, out byte byte0, out byte byte1);
		return (ushort)(crc16(0x1F, byte0, byte1) ^ 0x123F);
	}
	public static ushort generateCRC16(int value)
	{
		intToBytes(value, out byte byte0, out byte byte1, out byte byte2, out byte byte3);
		return (ushort)(crc16(0x1F, byte0, byte1, byte2, byte3) ^ 0x123F);
	}
}