using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static UnityUtility;
using static StringUtility;

// 用于自动从对象池中获取一个Dictionary<K, V>,不再使用时会自动释放,需要搭配using来使用,比如using(new DicScope<K, V>(out var list))
public struct DicScope<K, V> : IDisposable
{
	public Dictionary<K, V> mList;
	public DicScope(out Dictionary<K, V> list)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new Dictionary<K, V>();
			mList = null;
			return;
		}
		Type typeKey = Typeof<K>();
		Type typeValue = Typeof<V>();
		if (typeKey == null || typeValue == null)
		{
			logError("热更工程无法使用LIST<K, V>");
		}
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = mDictionaryPool.newList(typeKey, typeValue, typeof(Dictionary<K, V>), stackTrace, true) as Dictionary<K, V>;
		mList = list;
	}
	public void Dispose()
	{
		if (mGameFramework == null || mListPool == null || mList == null)
		{
			return;
		}
		Type typeKey = Typeof<K>();
		Type typeValue = Typeof<V>();
		if (typeKey == null || typeValue == null)
		{
			logError("热更工程无法使用UN_LIST<K, V>");
		}
		mList.Clear();
		mDictionaryPool?.destroyList(ref mList, typeKey, typeValue);
	}
}