using System;
using System.Collections.Generic;
using static FrameBaseHotFix;
using static FrameUtility;
using static StringUtility;

// 用于自动从对象池中获取3个List<T>,不再使用时会自动释放
// 需要搭配using来使用,比如using(new ListScope3<T>(out var list0, out var list1, out var list2))
public struct ListScope3<T> : IDisposable
{
	private List<T> mList0;      // 分配的对象
	private List<T> mList1;      // 分配的对象
	private List<T> mList2;      // 分配的对象
	public ListScope3(out List<T> list0, out List<T> list1, out List<T> list2)
	{
		if (GameEntry.getInstance() == null || mListPool == null)
		{
			list0 = new();
			list1 = new();
			list2 = new();
			mList0 = null;
			mList1 = null;
			mList2 = null;
			return;
		}
		string stackTrace = GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list0 = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		list1 = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		list2 = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		mList0 = list0;
		mList1 = list1;
		mList2 = list2;
	}
	public void Dispose()
	{
		Type type = typeof(T);
		mListPool?.destroyList(ref mList0, type);
		mListPool?.destroyList(ref mList1, type);
		mListPool?.destroyList(ref mList2, type);
	}
}