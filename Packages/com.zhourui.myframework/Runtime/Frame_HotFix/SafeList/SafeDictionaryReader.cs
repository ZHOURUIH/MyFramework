using System;
using System.Collections.Generic;

// 安全字典的只读遍历辅助,搭配SafeDictionary使用,using释放
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