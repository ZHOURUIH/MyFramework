using System;
using System.Collections.Generic;
using static FrameBaseHotFix;
using static FrameUtility;
using static StringUtility;

// 用于自动从对象池中获取一个HashSet<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new HashSetScope<T>(out var list))
public struct HashSetScope<T> : IDisposable
{
	private HashSet<T> mList;		// 分配的对象
	public HashSetScope(out HashSet<T> list, IEnumerable<T> initList = null)
	{
		if (GameEntry.getInstance() == null || mHashSetPool == null)
		{
			list = new();
			mList = null;
			return;
		}
		string stackTrace = GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list = mHashSetPool.newList(typeof(T), typeof(HashSet<T>), stackTrace, true) as HashSet<T>;
		mList = list;
		if (initList != null)
		{
			mList.addRange(initList);
		}
	}
	public void Dispose()
	{
		mHashSetPool?.destroyList(ref mList, typeof(T));
	}
}