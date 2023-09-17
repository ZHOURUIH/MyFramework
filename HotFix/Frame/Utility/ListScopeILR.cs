using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取一个List<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new ListScopeILR<T>(out var list))
public class ListScopeILR<T> : IDisposable
{
	public List<T> mList;
	public ListScopeILR(out List<T> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		mList = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		list = mList;
	}
	public void Dispose()
	{
		if (mList != null)
		{
			mListPool?.destroyList(ref mList, typeof(T));
		}
	}
}