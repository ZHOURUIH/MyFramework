using System;
using static FrameBase;

// 用于自动从对象池中获取两个MyStringBuilder,不再使用时会自动释放,需要搭配using来使用,比如using var a = new MyStringBuilderScope2(out var value0, out var value1);
public struct MyStringBuilderScope2 : IDisposable
{
	private MyStringBuilder mValue0;    // 分配的对象
	private MyStringBuilder mValue1;	// 分配的对象
	public MyStringBuilderScope2(out MyStringBuilder value0, out MyStringBuilder value1)
	{
		if (mClassPool == null)
		{
			value0 = new();
			value1 = new();
			mValue0 = null;
			mValue1 = null;
			return;
		}
		value0 = mClassPool?.newClass<MyStringBuilder>(true);
		value1 = mClassPool?.newClass<MyStringBuilder>(true);
		mValue0 = value0;
		mValue1 = value1;
	}
	public void Dispose()
	{
		mClassPool?.destroyClass(ref mValue0);
		mClassPool?.destroyClass(ref mValue1);
	}
}