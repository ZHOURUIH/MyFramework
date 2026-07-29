using System;
using System.Collections.Generic;

// 深度安全字典的只读遍历辅助,搭配SafeDeepDictionary使用,using释放
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