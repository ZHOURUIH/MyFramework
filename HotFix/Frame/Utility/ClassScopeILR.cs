using System;
using System.Collections.Generic;
using static FrameBase;

// 用于自动从对象池中获取一个T类型的对象,不再使用时会自动释放,需要搭配using来使用,比如using(new ClassScopeILR<T>(out var obj))
public class ClassScopeILR<T> : IDisposable where T : ClassObject
{
	public T mValue;
	public ClassScopeILR(out T value)
	{
		value = mClassPool?.newClass(typeof(T), out _, true, false) as T;
		mValue = value;
	}
	public void Dispose()
	{
		if (mValue != null)
		{
			mClassPool?.destroyClass(ref mValue);
		}
	}
}