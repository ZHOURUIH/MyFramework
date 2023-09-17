using System;
using static FrameBase;

// 用于自动从对象池中获取两个T类型的对象,不再使用时会自动释放,需要搭配using来使用,比如using(new ClassScopeILR<T>(out var obj0, out var obj1))
public class ClassScopeILR2<T> : IDisposable where T : ClassObject
{
	public T mValue0;
	public T mValue1;
	public ClassScopeILR2(out T value0, out T value1)
	{
		value0 = mClassPool?.newClass(typeof(T), out _, true, false) as T;
		value1 = mClassPool?.newClass(typeof(T), out _, true, false) as T;
		mValue0 = value0;
		mValue1 = value1;
	}
	public void Dispose()
	{
		if (mValue0 != null)
		{
			mClassPool?.destroyClass(ref mValue0);
		}
		if (mValue1 != null)
		{
			mClassPool?.destroyClass(ref mValue1);
		}
	}
}