using System.Collections.Generic;
using System;
using UnityEngine;

// 不支持带参构造的类,因为在再次利用时参数无法正确传递
// 线程安全的对象池,但是效率较低
public class ClassPoolThread : FrameSystem
{
	protected Dictionary<Type, ClassPoolSingle> mPoolList = new();	// 对象池列表,对应每一个类型的对象池
	protected ThreadLock mListLock = new();							// 列表的线程锁
	public ClassPoolThread()
	{
		mCreateObject = true;
	}
	public T newClass<T>() where T : ClassObject
	{
		return newClass(typeof(T)) as T;
	}
	// 返回值表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public ClassObject newClass(Type type)
	{
		if (type == null)
		{
			return null;
		}
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		ClassPoolSingle singlePool = null;
		using (new ThreadLockScope(mListLock))
		{
			// 先从未使用的列表中查找是否有可用的对象
			if (!mPoolList.tryGetOrAddNew(type, out singlePool))
			{
				singlePool.setType(type);
			}
		}
		return singlePool.newClass();
	}
	public void destroyClass<T>(ref T classObject) where T : ClassObject
	{
		if (classObject == null)
		{
			return;
		}
		using (new ThreadLockScope(mListLock))
		{
			if (!mPoolList.TryGetValue(classObject.GetType(), out ClassPoolSingle singlePool))
			{
				Debug.LogError("找不到类对象的对象池");
			}
			singlePool.destroyClass(ref classObject);
		}
	}
}