using System;
using System.Collections.Generic;

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