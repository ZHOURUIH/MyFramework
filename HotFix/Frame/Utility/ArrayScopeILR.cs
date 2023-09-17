using System;
using System.Collections.Generic;
using static FrameBase;

// 用于自动从对象池中获取一个T[],不再使用时会自动释放,需要搭配using来使用,比如using(new ArrayScopeILR<T>(out var list, int))
public class ArrayScopeILR<T> : IDisposable
{
	public T[] mValue;
	public ArrayScopeILR(out T[] value, int count)
	{
		if (mArrayPool == null)
		{
			value = new T[count];
			mValue = value;
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