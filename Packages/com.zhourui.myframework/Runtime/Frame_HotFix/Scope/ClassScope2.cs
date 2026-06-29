using System;
using static FrameBaseHotFix;

// 用于自动从对象池中获取一个T,不再使用时会自动释放,需要搭配using来使用,比如using(new ClassScope<T>(out var value))
public struct ClassScope2<T> : IDisposable where T : ClassObject, new()
{
	private T mValue0;  // 分配的对象
	private T mValue1;	// 分配的对象
	public ClassScope2(out T value0, out T value1)
	{
		if (mClassPool == null)
		{
			value0 = new();
			value1 = new();
			mValue0 = null;
			mValue1 = null;
			return;
		}
		value0 = mClassPool?.newClass<T>(true);
		value1 = mClassPool?.newClass<T>(true);
		mValue0 = value0;
		mValue1 = value1;
	}
	public void Dispose()
	{
		mClassPool?.destroyClass(ref mValue0);
		mClassPool?.destroyClass(ref mValue1);
	}
}