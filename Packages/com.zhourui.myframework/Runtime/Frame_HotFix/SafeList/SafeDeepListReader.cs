using System;
using System.Collections.Generic;

// 深度安全列表的只读遍历辅助,搭配SafeDeepList使用,using释放
public struct SafeDeepListReader<T> : IDisposable
{
	private SafeDeepList<T> mSafeList;
	public List<T> mReadList;
	public SafeDeepListReader(SafeDeepList<T> list)
	{
		mSafeList = list;
		mReadList = mSafeList.startForeach();
	}
	public void Dispose()
	{
		mSafeList.endForeach(mReadList);
	}
}