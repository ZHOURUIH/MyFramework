using System;
using System.Collections.Generic;
using static FrameBaseHotFix;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取一个List<T>,不再使用时会自动释放,需要搭配using来使用,比如using(new ListScope<T>(out var list))
public struct ListScope<T> : IDisposable
{
	private List<T> mList;       // 分配的对象
	public ListScope(out List<T> list, IEnumerable<T> initList = null)
	{
		if (GameEntry.getInstance() == null || mListPool == null)
		{
			list = new();
			mList = null;
			return;
		}
		string stackTrace = GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		mList = list;
		if (initList != null)
		{
			mList.AddRange(initList);
		}
	}
	public void Dispose()
	{
		mListPool?.destroyList(ref mList, typeof(T));
	}
}