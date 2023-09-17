using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static UnityUtility;
using static StringUtility;

// 用于自动从对象池中获取一个List<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new ListScope<T>(out var list))
public struct ListScope2<T> : IDisposable
{
	public List<T> mList0;
	public List<T> mList1;
	public ListScope2(out List<T> list0, out List<T> list1)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list0 = new List<T>();
			list1 = new List<T>();
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
		list0 = mListPool.newList(type, typeof(List<T>), stackTrace, true) as List<T>;
		list1 = mListPool.newList(type, typeof(List<T>), stackTrace, true) as List<T>;
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
		mListPool?.destroyList(ref mList0, type);
		mListPool?.destroyList(ref mList1, type);
	}
}