using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static UnityUtility;

// 用于自动从对象池中获取一个T,不再使用时会自动释放,需要搭配using来使用,比如using(new ClassScope<T>(out var value))
public struct ClassScope<T> : IDisposable where T : ClassObject
{
	public T mValue;
	public ClassScope(out T value)
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS_ONCE<T>");
			value = null;
			mValue = null;
			return;
		}
		if (mClassPool == null)
		{
			value = createInstanceDirect<T>(type);
			mValue = null;
			return;
		}
		value = mClassPool?.newClass(type, out _, true, true) as T;
		mValue = value;
	}
	public void Dispose()
	{
		if (mValue == null)
		{
			return;
		}
		mClassPool?.destroyClass(ref mValue);
	}
}