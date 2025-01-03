using System;
using System.Collections.Generic;

public struct SafeDeepDictionaryReader<Key, Value> : IDisposable
{
	private SafeDeepDictionary<Key, Value> mSafeList;
	public Dictionary<Key, Value> mList;
	public SafeDeepDictionaryReader(SafeDeepDictionary<Key, Value> list)
	{
		mSafeList = list;
		mList = mSafeList.startForeach();
	}
	public void Dispose()
	{
		mSafeList.endForeach(mList);
	}
}