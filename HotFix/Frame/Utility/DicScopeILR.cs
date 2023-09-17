using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 用于自动从对象池中获取一个Dictionary<K, V>,不再使用时会自动释放,需要搭配using来使用,比如using(new DicScopeILR<K, V>(out var list))
public class DicScopeILR<K, V> : IDisposable
{
	public Dictionary<K, V> mList;
	public DicScopeILR(out Dictionary<K, V> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getILRStackTrace();
		}
		list = mDictionaryPool.newList(typeof(K), typeof(V), typeof(Dictionary<K, V>), stackTrace, true) as Dictionary<K, V>;
		mList = list;
	}
	public void Dispose()
	{
		if (mList != null)
		{
			mList.Clear();
			mDictionaryPool?.destroyList(ref mList, typeof(K), typeof(V));
		}
	}
}