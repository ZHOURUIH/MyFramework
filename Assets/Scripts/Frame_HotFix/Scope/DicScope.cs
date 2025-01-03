using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取一个Dictionary<K, V>,不再使用时会自动释放,需要搭配using来使用,比如using(new DicScope<K, V>(out var list))
public struct DicScope<K, V> : IDisposable
{
	private Dictionary<K, V> mList;	// 分配的对象
	public DicScope(out Dictionary<K, V> list)
	{
		if (mGameFramework == null || mDictionaryPool == null)
		{
			list = new();
			mList = null;
			return;
		}
		string stackTrace = mGameFramework.mParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY;
		list = mDictionaryPool.newList(typeof(K), typeof(V), typeof(Dictionary<K, V>), stackTrace, true) as Dictionary<K, V>;
		mList = list;
	}
	public void Dispose()
	{
		if (mGameFramework == null || mDictionaryPool == null)
		{
			return;
		}
		mDictionaryPool?.destroyList(ref mList, typeof(K), typeof(V));
	}
}