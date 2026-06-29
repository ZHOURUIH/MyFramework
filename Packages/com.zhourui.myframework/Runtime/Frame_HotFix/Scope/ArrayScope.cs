using System;
using static FrameBaseHotFix;

// 用于自动从对象池中获取一个T[],不再使用时会自动释放,需要搭配using来使用,比如using(new ArrayScope<T>(out var list, int))
public struct ArrayScope<T> : IDisposable
{
	private T[] mValue;	// 分配的数组
	public ArrayScope(out T[] value, int count)
	{
		if (mArrayPool == null)
		{
			value = new T[count];
			mValue = null;
			return;
		}
		value = mArrayPool.newArray<T>(count, true);
		mValue = value;
	}
	public void Dispose()
	{
		if (mValue == null)
		{
			return;
		}
		mArrayPool?.destroyArray(ref mValue, false);
	}
}