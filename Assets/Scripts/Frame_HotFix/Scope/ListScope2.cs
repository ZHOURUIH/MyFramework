using System;
using System.Collections.Generic;
using static FrameBaseHotFix;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取两个List<T>,不再使用时会自动释放
// 需要搭配using来使用,比如using(new ListScope2<T>(out var list0, out var list1))
public struct ListScope2<T> : IDisposable
{
	private List<T> mList0;      // 分配的对象
	private List<T> mList1;      // 分配的对象
	public ListScope2(out List<T> list0, out List<T> list1)
	{
		if (mGameFrameworkHotFix == null || mListPool == null)
		{
			list0 = new();
			list1 = new();
			mList0 = null;
			mList1 = null;
			return;
		}
		string stackTrace = mGameFrameworkHotFix.mParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list0 = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		list1 = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		mList0 = list0;
		mList1 = list1;
	}
	public void Dispose()
	{
		if (mGameFrameworkHotFix == null || mListPool == null)
		{
			return;
		}
		Type type = typeof(T);
		mListPool?.destroyList(ref mList0, type);
		mListPool?.destroyList(ref mList1, type);
	}
}