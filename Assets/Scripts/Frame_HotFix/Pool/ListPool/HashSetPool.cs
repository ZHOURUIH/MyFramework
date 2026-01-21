using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseUtility;

// 仅能在主线程使用的列表池
public class HashSetPool : FrameSystem
{
	protected Dictionary<Type, HashSet<IEnumerable>> mPersistentInuseList = new();  // 持久使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, HashSet<IEnumerable>> mInusedList = new();           // 仅当前栈帧中使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Queue<IEnumerable>> mUnusedList = new();				// 未使用对象的列表
	protected Dictionary<IEnumerable, string> mObjectStack = new();                 // 存储对象分配堆栈的列表
	public HashSetPool()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<HashSetPoolDebug>();
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isEditor())
		{
			foreach (var item in mInusedList.Values)
			{
				foreach (IEnumerable itemList in item)
				{
					string stack = mObjectStack.get(itemList);
					if (stack.isEmpty())
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
	}
	public void clearUnused() { mUnusedList.Clear(); }
	public Dictionary<Type, HashSet<IEnumerable>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, HashSet<IEnumerable>> getInusedList() { return mInusedList; }
	public Dictionary<Type, Queue<IEnumerable>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public IEnumerable newList(Type elementType, Type listType, string stackTrace, bool onlyOnce = true)
	{
		if (mHasDestroy)
		{
			return null;
		}
		if (isEditor() && !isMainThread())
		{
			Debug.LogError("只能在主线程使用HashSetPool,子线程请使用HashSetPoolThread代替");
			return null;
		}

		bool isNew = false;
		IEnumerable list;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(elementType, out var valueList) && valueList.Count > 0)
		{
			list = valueList.Dequeue();
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			list = createInstance<IEnumerable>(listType);
			isNew = true;
		}
		if (isEditor())
		{
			// 标记为已使用
			addInuse(list, elementType, onlyOnce);
			mObjectStack.Add(list, stackTrace);
			if (isNew)
			{
				int totalCount = 0;
				totalCount += mInusedList.get(elementType)?.Count ?? 0;
				totalCount += mPersistentInuseList.get(elementType)?.Count ?? 0;
				if (totalCount % 1000 == 0)
				{
					Debug.Log("创建的Set总数量已经达到:" + totalCount + "个,type:" + elementType);
				}
			}
		}
		return list;
	}
	public void destroyList<T>(ref HashSet<T> list, Type elementType)
	{
		if (mHasDestroy)
		{
			return;
		}
		if (isEditor() && !isMainThread())
		{
			Debug.LogError("只能在主线程使用HashSetPool,子线程请使用HashSetPoolThread代替");
			return;
		}
		list.Clear();
		addUnuse(list, elementType);
		if (isEditor())
		{
			removeInuse(list, elementType);
			mObjectStack.Remove(list);
		}
		list = null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(IEnumerable list, Type elementType, bool onlyOnce)
	{
		// 加入仅临时使用的列表对象的列表中
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.getOrAddNew(elementType).Add(list))
		{
			Debug.LogError("list object is in inuse list!");
		}
	}
	protected void removeInuse(IEnumerable list, Type elementType)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		HashSet<IEnumerable> valueList;
		if (mInusedList.TryGetValue(elementType, out valueList) && valueList.Remove(list))
		{
			return;
		}
		if (mPersistentInuseList.TryGetValue(elementType, out valueList) && valueList.Remove(list))
		{
			return;
		}
		Debug.LogError("can not find class type in Inused List! type : " + elementType);
	}
	protected void addUnuse(IEnumerable list, Type elementType)
	{
		// 加入未使用列表
		var valueList = mUnusedList.getOrAddNew(elementType);
		if (isEditor() && valueList.Contains(list))
		{
			Debug.LogError("list Object is in Unused list! can not add again!");
			return;
		}
		valueList.Enqueue(list);
	}
}