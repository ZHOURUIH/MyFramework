using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static MathUtility;
using static FrameUtility;
using static FrameBaseUtility;

// 只能在主线程中使用的数组池
// 专为byte[]实现的对象池
public class ByteArrayPool : FrameSystem
{
	protected Dictionary<int, HashSet<byte[]>> mPersistentInuseList = new();    // 持久使用的数组对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<int, HashSet<byte[]>> mInusedList = new();             // 仅当前栈帧中使用的数组对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<int, Queue<byte[]>> mUnusedList = new();               // 未使用数组的列表
	protected Dictionary<Array, string> mObjectStack = new();                   // 堆栈信息存储列表
	protected int mCreatedCount;									            // 累计创建的对象数量
	public ByteArrayPool()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<ArrayPoolDebug>();
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isEditor())
		{
			foreach (var itemList in mInusedList.Values)
			{
				foreach (byte[] itemArray in itemList)
				{
					string stack = mObjectStack.get(itemArray);
					if (stack.isEmpty())
					{
						stack = "当前未开启对象池的堆栈追踪,可在对象分配前使用F4键开启堆栈追踪,然后就可以在此错误提示中看到对象分配时所在的堆栈\n";
					}
					else
					{
						stack = "create stack:\n" + stack + "\n";
					}
					Debug.LogError("有临时对象正在使用中,是否在申请后忘记回收到池中! \n" + stack);
					break;
				}
			}
		}
	}
	public void clearUnused()
	{
		foreach (var item in mUnusedList.Values)
		{
			item.Clear();
		}
	}
	public Dictionary<int, HashSet<byte[]>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<int, HashSet<byte[]>> getInusedList() { return mInusedList; }
	public Dictionary<int, Queue<byte[]>> getUnusedList() { return mUnusedList; }
	public byte[] newArray(int size, bool onlyOnce = true)
	{
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用ArrayPool");
			return null;
		}
		if (!isPow2(size))
		{
			Debug.LogError("只有长度为2的n次方的数组才能使用ArrayPool");
			return null;
		}
		bool isNew = false;
		byte[] array;
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
		// 标记为已使用,为了提高运行时效率,仅在编辑器下才会添加到已使用列表
		if (isEditor())
		{
			addInuse(array, onlyOnce);
			mObjectStack.Add(array, GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY);
			if (isNew && mCreatedCount % 1000 == 0)
			{
				logNoLock("byte[" + size + "]" + "数量已经达到了" + mCreatedCount + "个");
			}
		}
		return array;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyArray(ref byte[] array, bool destroyReally = false)
	{
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用ArrayPool");
			return;
		}
		if (array == null)
		{
			return;
		}
		if (isEditor())
		{
			mObjectStack.Remove(array);
		}
		if (!destroyReally)
		{
			addUnuse(array);
		}
		if (isEditor())
		{
			removeInuse(array);
		}
		array = null;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyArrayList(ICollection<byte[]> arrayList, bool destroyReally = false)
	{
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用ArrayPool");
			return;
		}
		if (isEditor())
		{
			foreach (byte[] item in arrayList)
			{
				mObjectStack.Remove(item);
			}
		}
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
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(byte[] array, bool onlyOnce)
	{
		var typeList = onlyOnce ? mInusedList : mPersistentInuseList;
		// 加入使用列表
		if (!typeList.getOrAddNew(array.Length).Add(array))
		{
			Debug.LogError("array is in inuse list!");
		}
	}
	protected void removeInuse(byte[] array)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		int length = array.Length;
		if (mInusedList.TryGetValue(length, out var arrayList) && arrayList.Remove(array))
		{
			return;
		}
		if (mPersistentInuseList.TryGetValue(length, out arrayList) && arrayList.Remove(array))
		{
			return;
		}
		Debug.LogError("can not find size in Inused List! size : " + length);
	}
	protected void removeInuse(ICollection<byte[]> allArrayList)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		foreach (byte[] array in allArrayList)
		{
			int length = array.Length;
			if (mInusedList.TryGetValue(length, out var arrayList) && arrayList.Remove(array))
			{
				continue;
			}
			if (mPersistentInuseList.TryGetValue(length, out arrayList) && arrayList.Remove(array))
			{
				continue;
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