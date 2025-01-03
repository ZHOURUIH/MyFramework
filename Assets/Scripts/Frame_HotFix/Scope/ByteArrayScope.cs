using System;
using static FrameBase;

// 用于自动从对象池中获取一个byte[],不再使用时会自动释放,需要搭配using来使用,比如using(new ByteArrayScope(out var list, int))
public struct ByteArrayScope : IDisposable
{
	private byte[] mValue;	// 分配的数组
	public ByteArrayScope(out byte[] value, int count)
	{
		if (mArrayPool == null)
		{
			value = new byte[count];
			mValue = null;
			return;
		}
		value = mByteArrayPool.newArray(count, true);
		mValue = value;
	}
	public void Dispose()
	{
		if (mValue == null)
		{
			return;
		}
		mByteArrayPool?.destroyArray(ref mValue, false);
	}
}