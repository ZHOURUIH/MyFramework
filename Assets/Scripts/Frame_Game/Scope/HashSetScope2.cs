using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取一个HashSet<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new HashSetScope<T>(out var list))
public struct HashSetScope2<T> : IDisposable
{
	private HashSet<T> mList0;		// 分配的对象
	private HashSet<T> mList1;		// 分配的对象
	public HashSetScope2(out HashSet<T> list0, out HashSet<T> list1)
	{
		if (mGameFramework == null || mHashSetPool == null)
		{
			list0 = new();
			list1 = new();
			mList0 = null;
			mList1 = null;
			return;
		}
		string stackTrace = mGameFramework.mParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list0 = mHashSetPool.newList(typeof(T), typeof(HashSet<T>), stackTrace, true) as HashSet<T>;
		list1 = mHashSetPool.newList(typeof(T), typeof(HashSet<T>), stackTrace, true) as HashSet<T>;
		mList0 = list0;
		mList1 = list1;
	}
	public void Dispose()
	{
		if (mGameFramework == null || mHashSetPool == null)
		{
			return;
		}
		Type type = typeof(T);
		mHashSetPool?.destroyList(ref mList0, type);
		mHashSetPool?.destroyList(ref mList1, type);
	}
}