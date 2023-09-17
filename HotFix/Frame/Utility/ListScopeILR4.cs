using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取一个List<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new ListScope<T>(out List<T> list))
// 可同时获取4个相同类型的列表
public class ListScopeILR4<T> : IDisposable
{
	public List<T> mList0;
	public List<T> mList1;
	public List<T> mList2;
	public List<T> mList3;
	public ListScopeILR4(out List<T> list0, out List<T> list1, out List<T> list2, out List<T> list3)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		Type type = typeof(T);
		mList0 = mListPool.newList(type, typeof(List<T>), stackTrace, true) as List<T>;
		mList1 = mListPool.newList(type, typeof(List<T>), stackTrace, true) as List<T>;
		mList2 = mListPool.newList(type, typeof(List<T>), stackTrace, true) as List<T>;
		mList3 = mListPool.newList(type, typeof(List<T>), stackTrace, true) as List<T>;
		list0 = mList0;
		list1 = mList1;
		list2 = mList2;
		list3 = mList3;
	}
	public void Dispose()
	{
		Type type = typeof(T);
		if (mList0 != null)
		{
			mListPool?.destroyList(ref mList0, type);
		}
		if (mList1 != null)
		{
			mListPool?.destroyList(ref mList1, type);
		}
		if (mList2 != null)
		{
			mListPool?.destroyList(ref mList2, type);
		}
		if (mList3 != null)
		{
			mListPool?.destroyList(ref mList3, type);
		}
	}
}