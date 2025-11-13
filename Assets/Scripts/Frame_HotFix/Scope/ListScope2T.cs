using System;
using System.Collections.Generic;
using static FrameBaseHotFix;
using static FrameUtility;
using static StringUtility;

// 用于自动从对象池中获取两个List,不再使用时会自动释放,需要搭配using来使用
// 比如using(new ListScope2T<T0, T1>(out var list0, out var list1))
public struct ListScope2T<T0, T1> : IDisposable
{
	private List<T0> mList0;     // 分配的对象
	private List<T1> mList1;     // 分配的对象
	public ListScope2T(out List<T0> list0, out List<T1> list1)
	{
		if (GameEntry.getInstance() == null || mListPool == null)
		{
			list0 = new();
			list1 = new();
			mList0 = null;
			mList1 = null;
			return;
		}
		string stackTrace = GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list0 = mListPool.newList(typeof(T0), typeof(List<T0>), stackTrace, true) as List<T0>;
		list1 = mListPool.newList(typeof(T1), typeof(List<T1>), stackTrace, true) as List<T1>;
		mList0 = list0;
		mList1 = list1;
	}
	public void Dispose()
	{
		mListPool?.destroyList(ref mList0, typeof(T0));
		mListPool?.destroyList(ref mList1, typeof(T1));
	}
}