using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameEditorUtility;

// 线程安全的数组池,但是效率较低
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
public class ArrayPoolThread : FrameSystem
{
	protected Dictionary<Type, Dictionary<int, HashSet<Array>>> mInusedList = new();    // 正在使用数组的列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Dictionary<int, Queue<Array>>> mUnusedList = new();		// 未使用数组的列表
	protected ThreadLock mListLock = new();                                             // 列表锁
	protected int mCreatedCount;														// 累计创建的对象数量
	public ArrayPoolThread()
	{
		mCreateObject = true;
	}
	public T[] newArray<T>(int size)
	{
		if (!isPow2(size))
		{
			Debug.LogError("只有长度为2的n次方的数组才能使用ArrayPoolThread");
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
				if (mUnusedList.TryGetValue(type, out var typeList) &&
					typeList.Count > 0 &&
					typeList.TryGetValue(size, out var arrayList) &&
					arrayList.Count > 0)
				{
					array = arrayList.Dequeue() as T[];
				}
				// 未使用列表中没有,创建一个新的
				else
				{
					array = new T[size];
					if (isEditor() && ++mCreatedCount % 1000 == 0)
					{
						logNoLock(typeof(T).ToString() + "[" + size + "]" + "数量已经达到了" + mCreatedCount + "个");
					}
				}
				// 标记为已使用,为了提高运行时效率,仅在编辑器下才会加入到已使用列表
				if (isEditor())
				{
					addInuse(array);
				}
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
				if (isEditor())
				{
					removeInuse(array);
				}
			}
			catch (Exception e)
			{
				logException(e);
			}
			array = null;
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse<T>(T[] array)
	{
		// 加入使用列表
		if (!mInusedList.tryGetOrAddNew(typeof(T)).tryGetOrAddNew(array.Length).Add(array))
		{
			Debug.LogError("array is in inuse list!");
		}
	}
	protected void removeInuse<T>(T[] array)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (mInusedList.TryGetValue(typeof(T), out var typeList) &&
			typeList.TryGetValue(array.Length, out var arrayList) &&
			arrayList.Remove(array))
		{
			return;
		}
		Debug.LogError("remove from Inused List failed! size : " + array.Length);
	}
	protected void addUnuse<T>(T[] array)
	{
		// 加入未使用列表
		var arrayList = mUnusedList.tryGetOrAddNew(typeof(T)).tryGetOrAddNew(array.Length);
		if (isEditor() && arrayList.Contains(array))
		{
			Debug.LogError("array is in Unused list! can not add again!");
		}
		arrayList.Enqueue(array);
	}
}