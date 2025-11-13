using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static MathUtility;
using static FrameUtility;
using static FrameBaseUtility;

// 只能在主线程中使用的数组池
public class ArrayPool : FrameSystem
{
	protected Dictionary<Type, Dictionary<int, HashSet<Array>>> mPersistentInuseList = new();   // 持久使用的数组对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Dictionary<int, HashSet<Array>>> mInusedList = new();            // 仅当前栈帧中使用的数组对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Dictionary<int, Queue<Array>>> mUnusedList = new();              // 未使用数组的列表
	protected Dictionary<Array, string> mObjectStack = new();                                   // 堆栈信息存储列表
	protected int mCreatedCount;																// 累计创建的对象数量
	public ArrayPool()
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
			foreach (var item in mInusedList.Values)
			{
				foreach (var itemList in item.Values)
				{
					foreach (Array itemArray in itemList)
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
	}
	public void clearUnused()
	{
		foreach (var item in mUnusedList.Values)
		{
			item.Clear();
		}
	}
	public Dictionary<Type, Dictionary<int, HashSet<Array>>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, Dictionary<int, HashSet<Array>>> getInusedList() { return mInusedList; }
	public Dictionary<Type, Dictionary<int, Queue<Array>>> getUnusedList() { return mUnusedList; }
	public T[] newArray<T>(int size, bool onlyOnce = true)
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
		Type type = typeof(T);
		T[] array;
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
			isNew = true;
			++mCreatedCount;
		}
		// 标记为已使用,为了提高运行时效率,仅在编辑器下才会添加到已使用列表
		if (isEditor())
		{
			addInuse(array, onlyOnce);
			mObjectStack.Add(array, GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY);
			if (isNew && mCreatedCount % 1000 == 0)
			{
				logNoLock(typeof(T).ToString() + "[" + size + "]" + "数量已经达到了" + mCreatedCount + "个");
			}
		}
		return array;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyArray<T>(ref T[] array, bool destroyReally = false)
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
	public void destroyArrayList<T>(ICollection<T[]> arrayList, bool destroyReally = false)
	{
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用ArrayPool");
			return;
		}
		if (isEditor())
		{
			foreach (T[] item in arrayList)
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
	protected void addInuse<T>(T[] array, bool onlyOnce)
	{
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		// 加入使用列表
		if (!inuseList.getOrAddNew(typeof(T)).getOrAddNew(array.Length).Add(array))
		{
			Debug.LogError("array is in inuse list!");
		}
	}
	protected void removeInuse<T>(T[] array)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		Type type = typeof(T);
		int length = array.Length;
		HashSet<Array> arrayList;
		if (mInusedList.TryGetValue(type, out var typeList0) &&
			typeList0.TryGetValue(length, out arrayList) &&
			arrayList.Remove(array))
		{
			return;
		}
		if (mPersistentInuseList.TryGetValue(type, out var typeList1) &&
			typeList1.TryGetValue(length, out arrayList) &&
			arrayList.Remove(array))
		{
			return;
		}
		Debug.LogError("can not find size in Inused List! size : " + length);
	}
	protected void removeInuse<T>(ICollection<T[]> allArrayList)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		Type type = typeof(T);
		var typeList0 = mInusedList.get(type);
		var typeList1 = mPersistentInuseList.get(type);
		HashSet<Array> arrayList;
		foreach (T[] array in allArrayList)
		{
			int length = array.Length;
			if (typeList0.TryGetValue(length, out arrayList) && arrayList.Remove(array))
			{
				continue;
			}
			if (typeList1.TryGetValue(length, out arrayList) && arrayList.Remove(array))
			{
				continue;
			}
		}
	}
	protected void addUnuse<T>(T[] array)
	{
		// 加入未使用列表
		var arrayList = mUnusedList.getOrAddNew(typeof(T)).getOrAddNew(array.Length);
		if (isEditor() && arrayList.Contains(array))
		{
			Debug.LogError("array is in Unused list! can not add again!");
		}
		arrayList.Enqueue(array);
	}
	protected void addUnuse<T>(ICollection<T[]> allArrayList)
	{
		// 加入未使用列表
		var typeList = mUnusedList.getOrAddNew(typeof(T));
		foreach (T[] array in allArrayList)
		{
			var arrayList = typeList.getOrAddNew(array.Length);
			if (isEditor() && arrayList.Contains(array))
			{
				Debug.LogError("array is in Unused list! can not add again!");
			}
			arrayList.Enqueue(array);
		}
	}
}