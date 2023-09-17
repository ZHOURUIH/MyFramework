using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取一个List<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new ListScope<T>(out List<T> list))
// 可同时获取2个不同类型的列表
public class ListScopeILR2T<T0, T1> : IDisposable
{
	public List<T0> mList0;
	public List<T1> mList1;
	public ListScopeILR2T(out List<T0> list0, out List<T1> list1)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		mList0 = mListPool.newList(typeof(T0), typeof(List<T0>), stackTrace, true) as List<T0>;
		list0 = mList0;

		mList1 = mListPool.newList(typeof(T1), typeof(List<T1>), stackTrace, true) as List<T1>;
		list1 = mList1;
	}
	public void Dispose()
	{
		if (mList0 != null)
		{
			mListPool?.destroyList(ref mList0, typeof(T0));
		}
		if (mList1 != null)
		{
			mListPool?.destroyList(ref mList1, typeof(T1));
		}
	}
}