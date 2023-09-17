using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取一个HashSet<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new HashSetScopeILR<T>(out var list))
public class HashSetScopeILR<T> : IDisposable
{
	public HashSet<T> mList;
	public HashSetScopeILR(out HashSet<T> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		list = mHashSetPool.newList(typeof(T), typeof(HashSet<T>), stackTrace, true) as HashSet<T>;
		mList = list;
	}
	public void Dispose()
	{
		if (mList != null)
		{
			mList.Clear();
			mHashSetPool?.destroyList(ref mList, typeof(T));
		}
	}
}