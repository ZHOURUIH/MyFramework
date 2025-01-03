using System;
using System.Collections.Generic;

public struct SafeDictionaryReader<Key, Value> : IDisposable
{
	private SafeDictionary<Key, Value> mSafeList;
	public Dictionary<Key, Value> mReadList;
	public SafeDictionaryReader(SafeDictionary<Key, Value> list)
	{
		mSafeList = list;
		mReadList = mSafeList.startForeach();
	}
	public void Dispose()
	{
		mSafeList.endForeach();
	}
}