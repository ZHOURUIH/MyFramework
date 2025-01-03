using System;
using System.Collections.Generic;

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