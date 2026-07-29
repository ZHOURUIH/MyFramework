using System;
using System.Collections.Generic;

// 安全列表的只读遍历辅助,搭配SafeList使用,using释放
public struct SafeListReader<T> : IDisposable
{
	private SafeList<T> mSafeList;
	public List<T> mReadList;
	public SafeListReader(SafeList<T> list)
	{
		mSafeList = list;
		mReadList = mSafeList.startForeach();
	}
	public void Dispose()
	{
		mSafeList.endForeach();
	}
}