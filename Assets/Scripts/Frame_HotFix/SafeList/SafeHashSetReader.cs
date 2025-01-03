using System;
using System.Collections.Generic;

public struct SafeHashSetReader<T> : IDisposable
{
	private SafeHashSet<T> mSafeList;
	public HashSet<T> mReadList;
	public SafeHashSetReader(SafeHashSet<T> list)
	{
		mSafeList = list;
		mReadList = mSafeList.startForeach();
	}
	public void Dispose()
	{
		mSafeList.endForeach();
	}
}