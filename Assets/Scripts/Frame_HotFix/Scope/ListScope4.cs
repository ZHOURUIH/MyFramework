using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取4个List<T>,不再使用时会自动释放
// 需要搭配using来使用,比如using(new ListScope4<T>(out var list0, out var list1, out var list2, out var list3))
public struct ListScope4<T> : IDisposable
{
	private List<T> mList0;      // 分配的对象
	private List<T> mList1;      // 分配的对象
	private List<T> mList2;      // 分配的对象
	private List<T> mList3;      // 分配的对象
	public ListScope4(out List<T> list0, out List<T> list1, out List<T> list2, out List<T> list3)
	{
		if (mGameFramework == null || mListPool == null)
		{
			list0 = new();
			list1 = new();
			list2 = new();
			list3 = new();
			mList0 = null;
			mList1 = null;
			mList2 = null;
			mList3 = null;
			return;
		}
		string stackTrace = mGameFramework.mParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list0 = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		list1 = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		list2 = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		list3 = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		mList0 = list0;
		mList1 = list1;
		mList2 = list2;
		mList3 = list3;
	}
	public void Dispose()
	{
		if (mGameFramework == null || mListPool == null)
		{
			return;
		}
		Type type = typeof(T);
		mListPool.destroyList(ref mList0, type);
		mListPool.destroyList(ref mList1, type);
		mListPool.destroyList(ref mList2, type);
		mListPool.destroyList(ref mList3, type);
	}
}