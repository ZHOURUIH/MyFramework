using System;
using System.Collections.Generic;
using static FrameBase;
using static CSharpUtility;
using static UnityUtility;

// 用于自动从对象池中获取一个T,可用于在子线程中访问,不再使用时会自动释放,需要搭配using来使用,比如using(new ClassThreadScope<T>(out var list))
public struct ClassThreadScope<T> : IDisposable where T : ClassObject
{
	public T mValue;
	public ClassThreadScope(out T value)
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS_THREAD<T>");
			value = null;
			mValue = null;
			return;
		}
		if (mClassPoolThread == null)
		{
			value = createInstanceDirect<T>(type);
			mValue = null;
			return;
		}
		value = mClassPoolThread?.newClass(type, out _, true) as T;
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