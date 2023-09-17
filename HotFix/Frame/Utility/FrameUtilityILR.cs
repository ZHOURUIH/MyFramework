using System;
using System.Collections.Generic;
using static FrameUtility;
using static FrameBase;
using static StringUtility;
using static CSharpUtility;

// 主工程中FrameUtility的ILR版本
public class FrameUtilityILR
{
	public static T PACKET_ILR<T>(out T packet) where T : NetPacketFrame
	{
		return packet = mSocketFactory.createSocketPacket(typeof(T)) as T;
	}
	public static void changeProcedureILR<T>(string intent = null) where T : SceneProcedure
	{
		changeProcedure(typeof(T), intent);
	}
	public static void enterSceneILR<T>(Type startProcedure = null, string intent = null) where T : GameScene
	{
		enterScene(typeof(T), startProcedure, intent);
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
	public static void UN_LIST_ILR<T>(ref List<T> list)
	{
		mListPool?.destroyList(ref list, typeof(T));
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
	public static void UN_LIST_ILR<T>(ref HashSet<T> list)
	{
		list.Clear();
		mHashSetPool?.destroyList(ref list, typeof(T));
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
	public static void UN_LIST_ILR<K, V>(ref Dictionary<K, V> list)
	{
		list.Clear();
		mDictionaryPool?.destroyList(ref list, typeof(K), typeof(V));
	}
	public static void CLASS_ILR<T>(out T value) where T : ClassObject
	{
		value = mClassPool?.newClass(typeof(T), out _, false, false) as T;
	}
	public static ClassObject CLASS_ILR(Type type)
	{
		return mClassPool?.newClass(type, out _, false, false);
	}
	public static void CLASS_ILR_THREAD<T>(out T value) where T : ClassObject
	{
		value = mClassPoolThread?.newClass(typeof(T), out _, false) as T;
	}
	public static ClassObject CLASS_ILR_THREAD(Type type)
	{
		return mClassPoolThread?.newClass(type, out _, false);
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
		mArrayPool.destroyArray(ref array, destroyReally);
	}
	public static void ARRAY_ILR_THREAD<T>(out T[] array, int count)
	{
		array = mArrayPoolThread.newArray<T>(count);
	}
	public static void UN_ARRAY_ILR_THREAD<T>(T[] array, bool destroyReally = false)
	{
		mArrayPoolThread.destroyArray(ref array, destroyReally);
	}
}