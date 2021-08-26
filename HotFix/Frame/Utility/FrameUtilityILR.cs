using UnityEngine;
using System;
using System.Collections.Generic;

// 主工程中FrameUtility的ILR版本
public class FrameUtilityILR : FrameBase
{
	public static void pushEvent(int eventType, GameEvent param)
	{
		mEventSystem.pushEvent(eventType, param);
	}
	public static void changeProcedureILR<T>(string intent = null) where T : SceneProcedure
	{
		changeProcedure(typeof(T), intent);
	}
	public static void enterSceneILR<T>(Type startProcedure = null, string intent = null) where T : GameScene
	{
		enterScene(typeof(T), startProcedure, intent);
	}
	// 命令
	//------------------------------------------------------------------------------------------------------------------------------
	public static void CMD_ILR<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		cmd = CMD(typeof(T), logLevel) as T;
	}
	public static void CMD_ILR_DELAY<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		cmd = CMD_DELAY(typeof(T), logLevel) as T;
	}
	public static void pushILRCommand<T>(CommandReceiver cmdReceiver, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD_ILR(out T cmd, logLevel);
		mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static T pushDelayILRCommand<T>(DelayCmdWatcher watcher, CommandReceiver cmdReceiver, float delayExecute = 0.001f, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD_ILR_DELAY(out T cmd, logLevel);
		mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 对象池
	public static void LIST_ILR<T>(out List<T> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		list = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
	}
	public static void LIST_ILR_PERSIST<T>(out List<T> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		list = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, false) as List<T>;
	}
	public static void UN_LIST_ILR<T>(List<T> list)
	{
		mListPool?.destroyList(list, typeof(T));
	}
	public static void LIST_ILR<T>(out HashSet<T> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		list = mHashSetPool.newList(typeof(T), typeof(HashSet<T>), stackTrace, true) as HashSet<T>;
	}
	public static void LIST_ILR_PERSIST<T>(out HashSet<T> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		list = mHashSetPool.newList(typeof(T), typeof(HashSet<T>), stackTrace, false) as HashSet<T>;
	}
	public static void UN_LIST_ILR<T>(HashSet<T> list)
	{
		list.Clear();
		mHashSetPool?.destroyList(list, typeof(T));
	}
	public static void LIST_ILR<K, V>(out Dictionary<K, V> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		list = mDictionaryPool.newList(typeof(K), typeof(V), typeof(Dictionary<K, V>), stackTrace, true) as Dictionary<K, V>;
	}
	public static void LIST_ILR_PERSIST<K, V>(out Dictionary<K, V> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		list = mDictionaryPool.newList(typeof(K), typeof(V), typeof(Dictionary<K, V>), stackTrace, false) as Dictionary<K, V>;
	}
	public static void UN_LIST_ILR<K, V>(Dictionary<K, V> list)
	{
		list.Clear();
		mDictionaryPool?.destroyList(list, typeof(K), typeof(V));
	}
	public static void CLASS_ILR_ONCE<T>(out T value) where T : ClassObject
	{
		value = mClassPool?.newClass(typeof(T), true) as T;
	}
	public static ClassObject CLASS_ILR_ONCE(Type type)
	{
		return mClassPool?.newClass(type, true);
	}
	public static void CLASS_ILR<T>(out T value) where T : ClassObject
	{
		value = mClassPool?.newClass(typeof(T), false) as T;
	}
	public static ClassObject CLASS_ILR(Type type)
	{
		return mClassPool?.newClass(type, false);
	}
	public static void CLASS_ILR_THREAD<T>(out T value) where T : ClassObject
	{
		value = mClassPoolThread?.newClass(typeof(T), out _) as T;
	}
	public static ClassObject CLASS_ILR_THREAD(Type type)
	{
		return mClassPoolThread?.newClass(type, out _);
	}
	public static void ARRAY_ILR<T>(out T[] array, int count)
	{
		array = mArrayPool.newArray<T>(count, true);
	}
	public static void ARRAY_ILR_PERSIST<T>(out T[] array, int count)
	{
		array = mArrayPool.newArray<T>(count, false);
	}
	public static void UN_ARRAY_ILR<T>(T[] array, bool destroyReally = false)
	{
		mArrayPool.destroyArray(array, destroyReally);
	}
	public static void ARRAY_ILR_THREAD<T>(out T[] array, int count)
	{
		array = mArrayPoolThread.newArray<T>(count);
	}
	public static void UN_ARRAY_ILR_THREAD<T>(T[] array, bool destroyReally = false)
	{
		mArrayPoolThread.destroyArray(array, destroyReally);
	}
}