using System;

// 搭配SafeFastDeepList使用,using释放
public struct SafeFastDeepListReader<T> : IDisposable
{
	private SafeFastDeepList<T> mSafeList;
	public SafeFastDeepListReader(SafeFastDeepList<T> list, out int count)
	{
		mSafeList = list;
		mSafeList.startForeach();
		count = mSafeList.count();
	}
	public void Dispose()
	{
		mSafeList.endForeach();
	}
}