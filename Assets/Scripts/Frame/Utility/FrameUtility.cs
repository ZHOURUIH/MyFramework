using System;
using System.Collections.Generic;
using UnityEngine;

public class FrameUtility : WidgetUtility
{
	public static GameCamera getMainCamera() { return FrameBase.mCameraManager.getMainCamera(); }
	// 主工程中可调用的跳转流程的函数
	public static void changeProcedure<T>(string intent = null) where T : SceneProcedure
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用changeProcedure");
		}
#else
		Type type = typeof(T);
#endif
		CMD(out CmdGameSceneChangeProcedure cmd);
		cmd.mProcedure = type;
		cmd.mIntent = intent;
		pushCommand(cmd, FrameBase.mGameSceneManager.getCurScene());
	}
	public static CmdGameSceneChangeProcedure changeProcedureDelay<T>(float delayTime = 0.001f, string intent = null) where T : SceneProcedure
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用changeProcedure");
		}
#else
		Type type = typeof(T);
#endif
		CMD_DELAY(out CmdGameSceneChangeProcedure cmd, true);
		cmd.mProcedure = type;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, FrameBase.mGameSceneManager.getCurScene(), delayTime);
		return cmd;
	}
	public static void prepareChangeProcedure<T>(float prepareTime = 0.001f, string intent = null) where T : SceneProcedure
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用changeProcedure");
		}
#else
		Type type = typeof(T);
#endif
		CMD(out CmdGameScenePrepareChangeProcedure cmd);
		cmd.mProcedure = type;
		cmd.mIntent = intent;
		cmd.mPrepareTime = prepareTime;
		pushCommand(cmd, FrameBase.mGameSceneManager.getCurScene());
	}
	public static void enterScene<T>(Type startProcedure = null, string intent = null) where T : GameScene
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用enterScene");
		}
#else
		Type type = typeof(T);
#endif
		CMD(out CmdGameSceneManagerEnter cmd);
		cmd.mSceneType = type;
		cmd.mStartProcedure = startProcedure;
		cmd.mIntent = intent;
		pushCommand(cmd, FrameBase.mGameSceneManager);
	}
	public static void changeProcedure(Type procedure, string intent = null)
	{
		CMD(out CmdGameSceneChangeProcedure cmd);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushCommand(cmd, FrameBase.mGameSceneManager.getCurScene());
	}
	public static CmdGameSceneChangeProcedure changeProcedureDelay(Type procedure, float delayTime = 0.001f, string intent = null)
	{
		CMD_DELAY(out CmdGameSceneChangeProcedure cmd, true);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, FrameBase.mGameSceneManager.getCurScene(), delayTime);
		return cmd;
	}
	public static void prepareChangeProcedure(Type procedure, float prepareTime = 0.001f, string intent = null)
	{
		CMD(out CmdGameScenePrepareChangeProcedure cmd);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		cmd.mPrepareTime = prepareTime;
		pushCommand(cmd, FrameBase.mGameSceneManager.getCurScene());
	}
	public static void enterScene(Type sceneType, Type startProcedure = null, string intent = null)
	{
		CMD(out CmdGameSceneManagerEnter cmd);
		cmd.mSceneType = sceneType;
		cmd.mStartProcedure = startProcedure;
		cmd.mIntent = intent;
		pushCommand(cmd, FrameBase.mGameSceneManager);
	}
	public static bool isKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return FrameBase.mInputSystem.isKeyCurrentDown(key, mask); }
	public static bool isKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return FrameBase.mInputSystem.isKeyCurrentUp(key, mask); }
	public static bool isKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return FrameBase.mInputSystem.isKeyDown(key, mask); }
	public static bool isKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return FrameBase.mInputSystem.isKeyUp(key, mask); }
	public static Vector3 getMousePosition(bool leftBottomAsOrigin = true)
	{
		if (leftBottomAsOrigin)
		{
			return FrameBase.mGlobalTouchSystem.getCurMousePosition();
		}
		else
		{
			return FrameBase.mGlobalTouchSystem.getCurMousePosition() - (Vector3)getScreenSize() * 0.5f;
		}
	}
	public static GameScene getCurScene() { return FrameBase.mGameSceneManager.getCurScene(); }
	// 百分比一般用于属性增幅之类的
	public static string toPercent(string value, int precision = 1) { return FToS(SToF(value) * 100, precision); }
	public static string toPercent(float value, int precision = 1) { return FToS(value * 100, precision); }
	// 几率类的一般是万分比的格式填写的
	public static string toProbability(string value) { return FToS(SToF(value) * 0.01f); }
	public static T PACKET<T>(out T packet) where T : SocketPacket
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用PACKET");
		}
#else
		Type type = typeof(T);
#endif
		return packet = FrameBase.mSocketFactory.createSocketPacket(type) as T;
	}
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 命令
	public static T CMD<T>(out T cmd, bool show = true) where T : Command
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD");
		}
#else
		Type type = typeof(T);
#endif
		return cmd = FrameBase.mCommandSystem.newCmd(type, show) as T;
	}
	public static T CMD_DELAY<T>(out T cmd, bool show = true) where T : Command
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD_DELAY");
		}
#else
		Type type = typeof(T);
#endif
		return cmd = FrameBase.mCommandSystem.newCmd(type, show, true) as T;
	}
	public static void pushMainCommand<T>(CommandReceiver cmdReceiver, bool show = true) where T : Command
	{
		CMD(out T cmd, show);
		FrameBase.mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		FrameBase.mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static T pushDelayMainCommand<T>(IDelayCmdWatcher watcher, CommandReceiver cmdReceiver, float delayExecute = 0.001f, bool show = true) where T : Command
	{
		CMD_DELAY(out T cmd, show);
		FrameBase.mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute, IDelayCmdWatcher watcher)
	{
		FrameBase.mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute)
	{
		FrameBase.mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute, null);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		FrameBase.mCommandSystem.pushDelayCommand(cmd, cmdReceiver, 0.0f, null);
	}
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 列表对象池
	public static void LIST<T>(out List<T> list)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST");
		}
#else
		Type type = typeof(T);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mListPool.newList(type, Typeof<List<T>>(), stackTrace, true) as List<T>;
	}
	public static void LIST_PERSIST<T>(out List<T> list)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST_PERSIST");
		}
#else
		Type type = typeof(T);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mListPool.newList(type, Typeof<List<T>>(), stackTrace, false) as List<T>;
	}
	public static void UN_LIST<T>(List<T> list)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用UN_LIST");
		}
#else
		Type type = typeof(T);
#endif
		FrameBase.mListPool?.destroyList(list, type);
	}
	public static void LIST<T>(out HashSet<T> list)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST");
		}
#else
		Type type = typeof(T);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mHashSetPool.newList(type, Typeof<HashSet<T>>(), stackTrace, true) as HashSet<T>;
	}
	public static void LIST_PERSIST<T>(out HashSet<T> list)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST_PERSIST");
		}
#else
		Type type = typeof(T);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mHashSetPool.newList(type, Typeof<HashSet<T>>(), stackTrace, false) as HashSet<T>;
	}
	public static void UN_LIST<T>(HashSet<T> list)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用UN_LIST");
		}
#else
		Type type = typeof(T);
#endif
		list.Clear();
		FrameBase.mHashSetPool?.destroyList(list, type);
	}
	public static void LIST<K, V>(out Dictionary<K, V> list)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type typeKey = Typeof<K>();
		Type typeValue = Typeof<V>();
		if (typeKey == null || typeValue == null)
		{
			logError("热更工程无法使用LIST");
		}
#else
		Type typeKey = typeof(K);
		Type typeValue = typeof(V);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mDictionaryPool.newList(typeKey, typeValue, Typeof<Dictionary<K, V>>(), stackTrace, true) as Dictionary<K, V>;
	}
	public static void LIST_PERSIST<K, V>(out Dictionary<K, V> list)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type typeKey = Typeof<K>();
		Type typeValue = Typeof<V>();
		if (typeKey == null || typeValue == null)
		{
			logError("热更工程无法使用LIST_PERSIST");
		}
#else
		Type typeKey = typeof(K);
		Type typeValue = typeof(V);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mDictionaryPool.newList(typeKey, typeValue, Typeof<Dictionary<K, V>>(), stackTrace, false) as Dictionary<K, V>;
	}
	public static void UN_LIST<K, V>(Dictionary<K, V> list)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type typeKey = Typeof<K>();
		Type typeValue = Typeof<V>();
		if (typeKey == null || typeValue == null)
		{
			logError("热更工程无法使用UN_LIST");
		}
#else
		Type typeKey = typeof(K);
		Type typeValue = typeof(V);
#endif
		list.Clear();
		FrameBase.mDictionaryPool?.destroyList(list, typeKey, typeValue);
	}
	//----------------------------------------------------------------------------------------------------------------------------------------
	// 对象池
	public static void CLASS_ONCE<T>(out T value) where T : ClassObject
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS_ONCE");
		}
#else
		Type type = typeof(T);
#endif
		value = FrameBase.mClassPool?.newClass(type, true) as T;
	}
	public static ClassObject CLASS_ONCE(Type type)
	{
		return FrameBase.mClassPool?.newClass(type, true);
	}
	public static void CLASS<T>(out T value) where T : ClassObject
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS");
		}
#else
		Type type = typeof(T);
#endif
		value = FrameBase.mClassPool?.newClass(type, false) as T;
	}
	public static ClassObject CLASS(Type type)
	{
		return FrameBase.mClassPool?.newClass(type, false);
	}
	// 由于热更工程无法使用多线程,所以此处不考虑热更工程
	public static void CLASS_THREAD<T>(out T value) where T : ClassObject
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用CLASS_THREAD");
		}
#endif
		value = FrameBase.mClassPoolThread?.newClass(typeof(T), out _) as T;
	}
	public static ClassObject CLASS_THREAD(Type type)
	{
		return FrameBase.mClassPoolThread?.newClass(type, out _);
	}
	public static void UN_CLASS(ClassObject obj)
	{
		FrameBase.mClassPool?.destroyClass(obj);
	}
	public static void UN_CLASS_THREAD(ClassObject obj)
	{
		FrameBase.mClassPoolThread?.destroyClass(obj);
	}
	//----------------------------------------------------------------------------------------------------------------------------------------
	// 数组对象池
	public static void ARRAY<T>(out T[] array, int count)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用ARRAY");
		}
#endif
		array = FrameBase.mArrayPool.newArray<T>(count, true);
	}
	public static void ARRAY_PERSIST<T>(out T[] array, int count)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用ARRAY_PERSIST");
		}
#endif
		array = FrameBase.mArrayPool.newArray<T>(count, false);
	}
	public static void UN_ARRAY<T>(T[] array, bool destroyReally = false)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用UN_ARRAY");
		}
#endif
		FrameBase.mArrayPool.destroyArray(array, destroyReally);
	}
	public static void ARRAY_THREAD<T>(out T[] array, int count)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用ARRAY_THREAD");
		}
#endif
		array = FrameBase.mArrayPoolThread.newArray<T>(count);
	}
	public static void UN_ARRAY_THREAD<T>(T[] array, bool destroyReally = false)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用UN_ARRAY_THREAD");
		}
#endif
		FrameBase.mArrayPoolThread.destroyArray(array, destroyReally);
	}
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 字符串拼接
	public static MyStringBuilder STRING()
	{
		if (FrameBase.mClassPool == null)
		{
			return new MyStringBuilder();
		}
		return CLASS_ONCE(typeof(MyStringBuilder)) as MyStringBuilder;
	}
	public static MyStringBuilder STRING(string str)
	{
		return STRING().Append(str);
	}
	public static MyStringBuilder STRING(string str0, string str1)
	{
		return STRING().Append(str0, str1);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2)
	{
		return STRING().Append(str0, str1, str2);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3)
	{
		return STRING().Append(str0, str1, str2, str3);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4)
	{
		return STRING().Append(str0, str1, str2, str3, str4);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5, str6);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5, str6, str7);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5, str6, str7, str8);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9);
	}
	public static MyStringBuilder STRING_THREAD()
	{
		if (FrameBase.mClassPoolThread == null)
		{
			return new MyStringBuilder();
		}
		return CLASS_THREAD(typeof(MyStringBuilder)) as MyStringBuilder;
	}
	public static MyStringBuilder STRING_THREAD(string str)
	{
		return STRING_THREAD().Append(str);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1)
	{
		return STRING_THREAD().Append(str0, str1);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2)
	{
		return STRING_THREAD().Append(str0, str1, str2);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4, str5);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4, str5, str6);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4, str5, str6, str7);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4, str5, str6, str7, str8);
	}
	public static string END_STRING_THREAD(MyStringBuilder builder)
	{
		string str = builder.ToString();
		UN_CLASS_THREAD(builder);
		return str;
	}
	public static string END_STRING(MyStringBuilder builder)
	{
		string str = builder.ToString();
		UN_CLASS(builder);
		return str;
	}
	public static void DESTROY_STRING(MyStringBuilder builder)
	{
		UN_CLASS(builder);
	}
	public static long delayCall(Action function, float delayTime)
	{
		CMD_DELAY(out CmdGlobalDelayCall cmd);
		cmd.mFunction = function;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
}