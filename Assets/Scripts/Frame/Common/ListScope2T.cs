using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static UnityUtility;
using static StringUtility;

// 用于自动从对象池中获取一个List<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new ListScope2T<T0, T1>(out var list0, out var list1))
public struct ListScope2T<T0, T1> : IDisposable
{
	public List<T0> mList0;
	public List<T1> mList1;
	public ListScope2T(out List<T0> list0, out List<T1> list1)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list0 = new List<T0>();
			list1 = new List<T1>();
			mList0 = null;
			mList1 = null;
			return;
		}
		Type type0 = Typeof<T0>();
		if (type0 == null)
		{
			logError("热更工程无法使用LIST<T>");
		}
		Type type1 = Typeof<T1>();
		if (type1 == null)
		{
			logError("热更工程无法使用LIST<T>");
		}
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list0 = mListPool.newList(type0, typeof(List<T0>), stackTrace, true) as List<T0>;
		list1 = mListPool.newList(type1, typeof(List<T1>), stackTrace, true) as List<T1>;
		mList0 = list0;
		mList1 = list1;
	}
	public void Dispose()
	{
		if (mGameFramework == null || mListPool == null || mList0 == null || mList1 == null)
		{
			return;
		}
		Type type0 = Typeof<T0>();
		if (type0 == null)
		{
			logError("热更工程无法使用UN_LIST<T>");
		}
		Type type1 = Typeof<T1>();
		if (type1 == null)
		{
			logError("热更工程无法使用UN_LIST<T>");
		}
		mListPool?.destroyList(ref mList0, type0);
		mListPool?.destroyList(ref mList1, type1);
	}
}