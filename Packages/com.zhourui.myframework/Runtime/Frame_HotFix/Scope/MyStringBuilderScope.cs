using System;
using static FrameBaseHotFix;

// 用于自动从对象池中获取一个MyStringBuilder,不再使用时会自动释放,需要搭配using来使用,比如using var a = new MyStringBuilderScope(out var value);
public struct MyStringBuilderScope : IDisposable
{
	private MyStringBuilder mValue;	// 分配的对象
	public MyStringBuilderScope(out MyStringBuilder value)
	{
		if (mClassPool == null)
		{
			value = new();
			mValue = null;
			return;
		}
		value = mClassPool?.newClass<MyStringBuilder>(true);
		mValue = value;
	}
	public void Dispose()
	{
		mClassPool?.destroyClass(ref mValue);
	}
}