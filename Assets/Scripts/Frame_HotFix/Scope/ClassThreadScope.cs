using System;
using static FrameBase;

// 用于自动从对象池中获取一个T,可用于在子线程中访问,不再使用时会自动释放
// 需要搭配using来使用,比如using(new ClassThreadScope<T>(out var list))
public struct ClassThreadScope<T> : IDisposable where T : ClassObject, new()
{
	private T mValue;	// 分配的对象
	public ClassThreadScope(out T value)
	{
		if (mClassPoolThread == null)
		{
			value = new();
			mValue = null;
			return;
		}
		value = mClassPoolThread?.newClass<T>();
		mValue = value;
	}
	public void Dispose()
	{
		if (mValue == null)
		{
			return;
		}
		mClassPoolThread?.destroyClass(ref mValue);
	}
}