using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static UnityUtility;
using static StringUtility;

// 用于自动从对象池中获取一个HashSet<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new HashSetScope<T>(out var list))
public struct HashSetScope2<T> : IDisposable
{
	public HashSet<T> mList0;
	public HashSet<T> mList1;
	public HashSetScope2(out HashSet<T> list0, out HashSet<T> list1)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list0 = new HashSet<T>();
			list1 = new HashSet<T>();
			mList0 = null;
			mList1 = null;
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
		list0 = mHashSetPool.newList(type, typeof(HashSet<T>), stackTrace, true) as HashSet<T>;
		list1 = mHashSetPool.newList(type, typeof(HashSet<T>), stackTrace, true) as HashSet<T>;
		mList0 = list0;
		mList1 = list1;
	}
	public void Dispose()
	{
		if (mGameFramework == null || mListPool == null || mList0 == null || mList1 == null)
		{
			return;
		}
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用UN_LIST<T>");
		}
		mList0.Clear();
		mList1.Clear();
		mHashSetPool?.destroyList(ref mList0, type);
		mHashSetPool?.destroyList(ref mList1, type);
	}
}