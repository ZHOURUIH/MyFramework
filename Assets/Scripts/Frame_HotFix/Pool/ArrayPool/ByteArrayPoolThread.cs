using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 线程安全的数组池,但是效率较低
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
// 专为byte[]实现的对象池
public class ByteArrayPoolThread : FrameSystem
{
	protected Dictionary<int, HashSet<byte[]>> mInusedList = new();		// 正在使用数组的列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<int, Queue<byte[]>> mUnusedList = new();		// 未使用数组的列表
	protected ThreadLock mListLock = new();								// 列表锁
	protected int mCreatedCount;										// 累计创建的对象数量
	public ByteArrayPoolThread()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<ArrayPoolThreadDebug>();
		}
	}
	public ThreadLock getLock() { return mListLock; }
	public void clearUnused()
	{
		using (new ThreadLockScope(mListLock))
		{
			foreach (var item in mUnusedList.Values)
			{
				item.Clear();
			}
		}
	}
	public Dictionary<int, HashSet<byte[]>> getInusedList() { return mInusedList; }
	public Dictionary<int, Queue<byte[]>> getUnusedList() { return mUnusedList; }
	public byte[] newArray(int size)
	{
		if (!isPow2(size))
		{
			logError("只有长度为2的n次方的数组才能使用ByteArrayPoolThread");
			return null;
		}
		byte[] array = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				bool isNew = false;
				// 先从未使用的列表中查找是否有可用的对象
				if (mUnusedList.Count > 0 && 
					mUnusedList.TryGetValue(size, out var arrayList) &&
					arrayList.Count > 0)
				{
					array = arrayList.Dequeue();
				}
				// 未使用列表中没有,创建一个新的
				else
				{
					array = new byte[size];
					++mCreatedCount;
					isNew = true;
				}
				// 标记为已使用,为了提高运行时效率,仅在编辑器下才会加入到已使用列表
				if (isEditor())
				{
					addInuse(array);
					if (isNew && mCreatedCount % 1000 == 0)
					{
						logNoLock("byte[" + size + "]" + "数量已经达到了" + mCreatedCount + "个");
					}
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
	public void destroyArray(ref byte[] array, bool destroyReally = false)
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
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyArrayList(ICollection<byte[]> arrayList, bool destroyReally = false)
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
				if (isEditor())
				{
					removeInuse(arrayList);
				}
				arrayList.Clear();
			}
			catch (Exception e)
			{
				logException(e);
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(byte[] array)
	{
		// 加入使用列表
		if (!mInusedList.getOrAddNew(array.Length).Add(array))
		{
			Debug.LogError("array is in inuse list!");
		}
	}
	protected void removeInuse(byte[] array)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (mInusedList.TryGetValue(array.Length, out var arrayList) && arrayList.Remove(array))
		{
			return;
		}
		Debug.LogError("remove from Inused List failed! size : " + array.Length);
	}
	protected void removeInuse(ICollection<byte[]> allArrayList)
	{
		foreach (byte[] array in allArrayList)
		{
			// 从使用列表移除,要确保操作的都是从本类创建的实例
			if (!mInusedList.TryGetValue(array.Length, out var arrayList) || !arrayList.Remove(array))
			{
				Debug.LogError("remove from Inused List failed! size : " + array.Length);
			}
		}
	}
	protected void addUnuse(byte[] array)
	{
		// 加入未使用列表
		var arrayList = mUnusedList.getOrAddNew(array.Length);
		if (isEditor() && arrayList.Contains(array))
		{
			Debug.LogError("array is in Unused list! can not add again!");
		}
		arrayList.Enqueue(array);
	}
	protected void addUnuse(ICollection<byte[]> allArrayList)
	{
		// 加入未使用列表
		foreach (byte[] array in allArrayList)
		{
			var arrayList = mUnusedList.getOrAddNew(array.Length);
			if (isEditor() && arrayList.Contains(array))
			{
				Debug.LogError("array is in Unused list! can not add again!");
			}
			arrayList.Enqueue(array);
		}
	}
}