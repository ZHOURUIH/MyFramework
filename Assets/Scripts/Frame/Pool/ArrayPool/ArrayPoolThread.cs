using System.Collections.Generic;
using System;

// 线程安全的数组池,但是效率较低
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
public class ArrayPoolThread : FrameSystem
{
	protected Dictionary<Type, Dictionary<int, HashSet<Array>>> mInusedList;
	protected Dictionary<Type, Dictionary<int, HashSet<Array>>> mUnusedList;
	protected ThreadLock mListLock;
	public ArrayPoolThread()
	{
		mInusedList = new Dictionary<Type, Dictionary<int, HashSet<Array>>>();
		mUnusedList = new Dictionary<Type, Dictionary<int, HashSet<Array>>>();
		mListLock = new ThreadLock();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<ArrayPoolThreadDebug>();
#endif
	}
	public void lockList() { mListLock.waitForUnlock(); }
	public void unlockList() { mListLock.unlock(); }
	public Dictionary<Type, Dictionary<int, HashSet<Array>>> getInusedList() { return mInusedList; }
	public Dictionary<Type, Dictionary<int, HashSet<Array>>> getUnusedList() { return mUnusedList; }
	public T[] newArray<T>(int size)
	{
		if (!isPow2(size))
		{
			logError("只有长度为2的n次方的数组才能使用ArrayPoolThread");
			return null;
		}
		Type type = typeof(T);
		T[] array = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.TryGetValue(type, out Dictionary<int, HashSet<Array>> typeList) &&
				typeList.Count > 0 &&
				typeList.TryGetValue(size, out HashSet<Array> arrayList) &&
				arrayList.Count > 0)
			{
				foreach (var item in arrayList)
				{
					array = item as T[];
					break;
				}
				arrayList.Remove(array);
			}
			// 未使用列表中没有,创建一个新的
			else
			{
				array = new T[size];
			}
			// 标记为已使用
			addInuse(array);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		return array;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyArray<T>(T[] array, bool destroyReally = false)
	{
		if (array == null)
		{
			return;
		}
		mListLock.waitForUnlock();
		try
		{
			if(!destroyReally)
			{
				addUnuse(array);
			}
			removeInuse(array);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse<T>(T[] array)
	{
		Type type = typeof(T);
		int length = array.Length;
		if (!mInusedList.TryGetValue(type, out Dictionary<int, HashSet<Array>> typeList))
		{
			typeList = new Dictionary<int, HashSet<Array>>();
			mInusedList.Add(type, typeList);
		}
		if (!typeList.TryGetValue(length, out HashSet<Array> arrayList))
		{
			arrayList = new HashSet<Array>();
			typeList.Add(length, arrayList);
		}
		if (arrayList.Count > 0 && arrayList.Contains(array))
		{
			logError("array is in inuse list!");
			return;
		}
		// 加入使用列表
		arrayList.Add(array);
	}
	protected void removeInuse<T>(T[] array)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		int length = array.Length;
		if (mInusedList.TryGetValue(typeof(T), out Dictionary<int, HashSet<Array>> typeList) &&
			typeList.TryGetValue(length, out HashSet<Array> arrayList) &&
			arrayList.Remove(array))
		{
			return;
		}
		logError("remove from Inused List failed! size : " + length);
	}
	protected void addUnuse<T>(T[] array)
	{
		// 加入未使用列表
		Type type = typeof(T);
		int length = array.Length;
		if (!mUnusedList.TryGetValue(type, out Dictionary<int, HashSet<Array>> typeList))
		{
			typeList = new Dictionary<int, HashSet<Array>>();
			mUnusedList.Add(type, typeList);
		}
		if (!typeList.TryGetValue(length, out HashSet<Array> arrayList))
		{
			arrayList = new HashSet<Array>();
			typeList.Add(length, arrayList);
		}
		if (arrayList.Count > 0 && arrayList.Contains(array))
		{
			logError("array is in Unused list! can not add again!");
		}
		arrayList.Add(array);
	}
}