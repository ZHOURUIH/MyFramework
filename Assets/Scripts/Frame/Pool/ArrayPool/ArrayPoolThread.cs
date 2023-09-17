using System.Collections.Generic;
using System;
using static UnityUtility;
using static MathUtility;

// 线程安全的数组池,但是效率较低
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
public class ArrayPoolThread : FrameSystem
{
	protected Dictionary<Type, Dictionary<int, HashSet<Array>>> mInusedList;    // 正在使用数组的列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Dictionary<int, List<Array>>> mUnusedList;		// 未使用数组的列表
	protected ThreadLock mListLock;                                             // 列表锁
	protected int mCreatedCount;
	public ArrayPoolThread()
	{
		mInusedList = new Dictionary<Type, Dictionary<int, HashSet<Array>>>();
		mUnusedList = new Dictionary<Type, Dictionary<int, List<Array>>>();
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
	public ThreadLock getLock() { return mListLock; }
	public void clearUnused()
	{
		using (new ThreadLockScope(mListLock))
		{
			foreach (var item in mUnusedList)
			{
				item.Value.Clear();
			}
		}
	}
	public Dictionary<Type, Dictionary<int, HashSet<Array>>> getInusedList() { return mInusedList; }
	public Dictionary<Type, Dictionary<int, List<Array>>> getUnusedList() { return mUnusedList; }
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
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				// 先从未使用的列表中查找是否有可用的对象
				if (mUnusedList.TryGetValue(type, out Dictionary<int, List<Array>> typeList) &&
					typeList.Count > 0 &&
					typeList.TryGetValue(size, out List<Array> arrayList) &&
					arrayList.Count > 0)
				{
					array = arrayList[arrayList.Count - 1] as T[];
					arrayList.RemoveAt(arrayList.Count - 1);
				}
				// 未使用列表中没有,创建一个新的
				else
				{
					array = new T[size];
#if UNITY_EDITOR
					++mCreatedCount;
					if (mCreatedCount % 1000 == 0)
					{
						logForceNoLock(typeof(T).ToString() + "[" + size + "]" + "数量已经达到了" + mCreatedCount + "个");
					}
#endif
				}
				// 标记为已使用,为了提高运行时效率,仅在编辑器下才会加入到已使用列表
#if UNITY_EDITOR
				addInuse(array);
#endif
			}
			catch (Exception e)
			{
				logException(e);
			}
		}
		return array;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyArray<T>(ref T[] array, bool destroyReally = false)
	{
		if (array == null)
		{
			return;
		}
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				if (!destroyReally)
				{
					addUnuse(array);
				}
#if UNITY_EDITOR
				removeInuse(array);
#endif
			}
			catch (Exception e)
			{
				logException(e);
			}
			array = null;
		}
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyArray<T>(List<T[]> arrayList, bool destroyReally = false)
	{
		if (arrayList == null)
		{
			return;
		}
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				if (!destroyReally)
				{
					addUnuse(arrayList);
				}
#if UNITY_EDITOR
				removeInuse(arrayList);
#endif
				arrayList.Clear();
			}
			catch (Exception e)
			{
				logException(e);
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
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
		else if (arrayList.Contains(array))
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
	protected void removeInuse<T>(List<T[]> allArrayList)
	{
		Type type = typeof(T);
		if (!mInusedList.TryGetValue(type, out Dictionary<int, HashSet<Array>> typeList))
		{
			logError("remove from Inused List failed! type : " + type);
			return;
		}
		int allCount = allArrayList.Count;
		for (int i = 0; i < allCount; ++i)
		{
			T[] array = allArrayList[i];
			// 从使用列表移除,要确保操作的都是从本类创建的实例
			int length = array.Length;
			if (!typeList.TryGetValue(length, out HashSet<Array> arrayList) ||
				!arrayList.Remove(array))
			{
				logError("remove from Inused List failed! size : " + length);
			}
		}
	}
	protected void addUnuse<T>(T[] array)
	{
		// 加入未使用列表
		Type type = typeof(T);
		int length = array.Length;
		if (!mUnusedList.TryGetValue(type, out Dictionary<int, List<Array>> typeList))
		{
			typeList = new Dictionary<int, List<Array>>();
			mUnusedList.Add(type, typeList);
		}
		if (!typeList.TryGetValue(length, out List<Array> arrayList))
		{
			arrayList = new List<Array>();
			typeList.Add(length, arrayList);
		}
		else
		{
#if UNITY_EDITOR
			if (arrayList.Contains(array))
			{
				logError("array is in Unused list! can not add again!");
			}
#endif
		}
		arrayList.Add(array);
	}
	protected void addUnuse<T>(List<T[]> allArrayList)
	{
		// 加入未使用列表
		Type type = typeof(T);
		if (!mUnusedList.TryGetValue(type, out Dictionary<int, List<Array>> typeList))
		{
			typeList = new Dictionary<int, List<Array>>();
			mUnusedList.Add(type, typeList);
		}

		int allCount = allArrayList.Count;
		for (int i = 0; i < allCount; ++i)
		{
			T[] array = allArrayList[i];
			int length = array.Length;
			if (!typeList.TryGetValue(length, out List<Array> arrayList))
			{
				arrayList = new List<Array>();
				typeList.Add(length, arrayList);
			}
			else
			{
#if UNITY_EDITOR
				if (arrayList.Contains(array))
				{
					logError("array is in Unused list! can not add again!");
				}
#endif
			}
			arrayList.Add(array);
		}
	}
}