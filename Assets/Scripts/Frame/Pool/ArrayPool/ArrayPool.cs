using System;
using System.Collections.Generic;
using static UnityUtility;
using static StringUtility;
using static MathUtility;
using static CSharpUtility;
using static FrameBase;

// 只能在主线程中使用的数组池
public class ArrayPool : FrameSystem
{
	protected Dictionary<Type, Dictionary<int, HashSet<Array>>> mPersistentInuseList;   // 持久使用的数组对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Dictionary<int, HashSet<Array>>> mInusedList;            // 仅当前栈帧中使用的数组对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Dictionary<int, List<Array>>> mUnusedList;				// 未使用数组的列表
	protected Dictionary<Array, string> mObjectStack;                                   // 堆栈信息存储列表
	protected int mCreatedCount;
	public ArrayPool()
	{
		mPersistentInuseList = new Dictionary<Type, Dictionary<int, HashSet<Array>>>();
		mInusedList = new Dictionary<Type, Dictionary<int, HashSet<Array>>>();
		mUnusedList = new Dictionary<Type, Dictionary<int, List<Array>>>();
		mObjectStack = new Dictionary<Array, string>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<ArrayPoolDebug>();
#endif
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
#if UNITY_EDITOR
		foreach (var item in mInusedList)
		{
			foreach (var itemList in item.Value)
			{
				foreach(var itemArray in itemList.Value)
				{
					string stack = mObjectStack[itemArray];
					if (isEmpty(stack))
					{
						stack = "当前未开启对象池的堆栈追踪,可在对象分配前使用F4键开启堆栈追踪,然后就可以在此错误提示中看到对象分配时所在的堆栈\n";
					}
					else
					{
						stack = "create stack:\n" + stack + "\n";
					}
					logError("有临时对象正在使用中,是否在申请后忘记回收到池中! \n" + stack);
					break;
				}
			}
		}
#endif
	}
	public void clearUnused()
	{
		foreach (var item in mUnusedList)
		{
			item.Value.Clear();
		}
	}
	public Dictionary<Type, Dictionary<int, HashSet<Array>>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, Dictionary<int, HashSet<Array>>> getInusedList() { return mInusedList; }
	public Dictionary<Type, Dictionary<int, List<Array>>> getUnusedList() { return mUnusedList; }
	public T[] newArray<T>(int size, bool onlyOnce = true)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用ArrayPool");
			return null;
		}
		if (!isPow2(size))
		{
			logError("只有长度为2的n次方的数组才能使用ArrayPool");
			return null;
		}
		Type type = typeof(T);
		T[] array;
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
		// 标记为已使用,为了提高运行时效率,仅在编辑器下才会添加到已使用列表
#if UNITY_EDITOR
		addInuse(array, onlyOnce);
#endif

#if UNITY_EDITOR
		if (mGameFramework.mEnablePoolStackTrace)
		{
			mObjectStack.Add(array, getStackTrace());
		}
		else
		{
			mObjectStack.Add(array, EMPTY);
		}
#endif
		return array;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyArray<T>(ref T[] array, bool destroyReally = false)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用ArrayPool");
			return;
		}
		if (array == null)
		{
			return;
		}
#if UNITY_EDITOR
		mObjectStack.Remove(array);
#endif
		if (!destroyReally)
		{
			addUnuse(array);
		}
#if UNITY_EDITOR
		removeInuse(array);
#endif
		array = null;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public void destroyArray<T>(List<T[]> arrayList, bool destroyReally = false)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用ArrayPool");
			return;
		}
#if UNITY_EDITOR
		int count = arrayList.Count;
		for (int i = 0; i < count; ++i)
		{
			mObjectStack.Remove(arrayList[i]);
		}
#endif
		if (!destroyReally)
		{
			addUnuse(arrayList);
		}
#if UNITY_EDITOR
		removeInuse(arrayList);
#endif
		arrayList.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse<T>(T[] array, bool onlyOnce)
	{
		Type type = typeof(T);
		int length = array.Length;
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.TryGetValue(type, out Dictionary<int, HashSet<Array>> typeList))
		{
			typeList = new Dictionary<int, HashSet<Array>>();
			inuseList.Add(type, typeList);
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
		Type type = typeof(T);
		int length = array.Length;
		Dictionary<int, HashSet<Array>> typeList0;
		Dictionary<int, HashSet<Array>> typeList1;
		HashSet<Array> arrayList;
		if (mInusedList.TryGetValue(type, out typeList0) &&
			typeList0.TryGetValue(length, out arrayList) &&
			arrayList.Remove(array))
		{
			return;
		}
		if (mPersistentInuseList.TryGetValue(type, out typeList1) &&
			typeList1.TryGetValue(length, out arrayList) &&
			arrayList.Remove(array))
		{
			return;
		}
		logError("can not find size in Inused List! size : " + length);
	}
	protected void removeInuse<T>(List<T[]> allArrayList)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		Type type = typeof(T);
		Dictionary<int, HashSet<Array>> typeList0;
		Dictionary<int, HashSet<Array>> typeList1;
		mInusedList.TryGetValue(type, out typeList0);
		mPersistentInuseList.TryGetValue(type, out typeList1);
		HashSet<Array> arrayList;
		int count = allArrayList.Count;
		for (int i = 0; i < count; ++i)
		{
			T[] array = allArrayList[i];
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
		Type type = typeof(T);
		int length = array.Length;
		if (!mUnusedList.TryGetValue(type, out Dictionary<int, List<Array>> typeList))
		{
			typeList = new Dictionary<int, List<Array>>();
			mUnusedList.Add(type, typeList);
		}
		if(!typeList.TryGetValue(length, out List<Array> arrayList))
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

		int count = allArrayList.Count;
		for (int i = 0; i < count; ++i)
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