using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static UnityUtility;
using static StringUtility;

// 用于自动从对象池中获取一个List<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new ListScope<T>(out var list))
public struct ListScope<T> : IDisposable
{
	public List<T> mList;
	public ListScope(out List<T> list)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list = new List<T>();
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
		list = mListPool.newList(type, typeof(List<T>), stackTrace, true) as List<T>;
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
		mListPool?.destroyList(ref mList, type);
	}
}