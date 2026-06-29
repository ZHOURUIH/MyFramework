using System;
using static FrameBaseHotFix;

// 用于自动从对象池中获取一个T,不再使用时会自动释放,需要搭配using来使用,比如using(new ClassScope<T>(out var value))
public struct ClassScope<T> : IDisposable where T : ClassObject, new()
{
	private T mValue;	// 分配的对象
	public ClassScope(out T value)
	{
		if (mClassPool == null)
		{
			value = new();
			mValue = null;
			return;
		}
		value = mClassPool?.newClass<T>(true);
		mValue = value;
	}
	public void Dispose()
	{
		mClassPool?.destroyClass(ref mValue);
	}
}