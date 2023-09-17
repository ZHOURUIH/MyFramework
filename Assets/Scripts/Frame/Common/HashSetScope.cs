using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static UnityUtility;
using static StringUtility;

// 用于自动从对象池中获取一个HashSet<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new HashSetScope<T>(out var list))
public struct HashSetScope<T> : IDisposable
{
	public HashSet<T> mList;
	public HashSetScope(out HashSet<T> list)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new HashSet<T>();
			mList = null;
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
		list = mHashSetPool.newList(type, typeof(HashSet<T>), stackTrace, true) as HashSet<T>;
		mList = list;
	}
	public void Dispose()
	{
		if (mGameFramework == null || mListPool == null || mList == null)
		{
			return;
		}
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用UN_LIST<T>");
		}
		mList.Clear();
		mHashSetPool?.destroyList(ref mList, type);
	}
}