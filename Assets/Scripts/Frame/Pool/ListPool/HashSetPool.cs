using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static CSharpUtility;

// 仅能在主线程使用的列表池
public class HashSetPool : FrameSystem
{
	protected Dictionary<Type, HashSet<IEnumerable>> mPersistentInuseList;  // 持久使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, HashSet<IEnumerable>> mInusedList;           // 仅当前栈帧中使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, List<IEnumerable>> mUnusedList;				// 未使用对象的列表
	protected Dictionary<IEnumerable, string> mObjectStack;                 // 存储对象分配堆栈的列表
	public HashSetPool()
	{
		mPersistentInuseList = new Dictionary<Type, HashSet<IEnumerable>>();
		mInusedList = new Dictionary<Type, HashSet<IEnumerable>>();
		mUnusedList = new Dictionary<Type, List<IEnumerable>>();
		mObjectStack = new Dictionary<IEnumerable, string>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject?.AddComponent<HashSetPoolDebug>();
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
				string stack = mObjectStack[itemList];
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
#endif
	}
	public void clearUnused() { mUnusedList.Clear(); }
	public Dictionary<Type, HashSet<IEnumerable>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, HashSet<IEnumerable>> getInusedList() { return mInusedList; }
	public Dictionary<Type, List<IEnumerable>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public IEnumerable newList(Type elementType, Type listType, string stackTrace, bool onlyOnce = true)
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程使用HashSetPool,子线程请使用HashSetPoolThread代替");
			return null;
		}
#endif

		IEnumerable list;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(elementType, out List<IEnumerable> valueList) && valueList.Count > 0)
		{
			list = valueList[valueList.Count - 1];
			valueList.RemoveAt(valueList.Count - 1);
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			list = createInstance<IEnumerable>(listType);
#if UNITY_EDITOR
			mInusedList.TryGetValue(listType, out HashSet<IEnumerable> temp0);
			mPersistentInuseList.TryGetValue(listType, out HashSet<IEnumerable> temp1);
			int totalCount = 1;
			if (temp0 != null)
			{
				totalCount += temp0.Count;
			}
			if (temp1 != null)
			{
				totalCount += temp1.Count;
			}
			if (totalCount % 1000 == 0)
			{
				Debug.Log("创建的Set总数量已经达到:" + totalCount + "个,type:" + elementType);
			}
#endif
		}
#if UNITY_EDITOR
		// 标记为已使用
		addInuse(list, elementType, onlyOnce);
		mObjectStack.Add(list, stackTrace);
#endif
		return list;
	}
	// 回收时需要外部手动清空,因为内部无法调用Clear函数
	public void destroyList<T>(ref HashSet<T> list, Type type)
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程使用HashSetPool,子线程请使用HashSetPoolThread代替");
			return;
		}
#endif
		addUnuse(list, type);
#if UNITY_EDITOR
		removeInuse(list, type);
		mObjectStack.Remove(list);
#endif
		list = null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(IEnumerable list, Type type, bool onlyOnce)
	{
		// 加入仅临时使用的列表对象的列表中
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.TryGetValue(type, out HashSet<IEnumerable> valueList))
		{
			valueList = new HashSet<IEnumerable>();
			inuseList.Add(type, valueList);
		}
		else if (valueList.Contains(list))
		{
			Debug.LogError("list object is in inuse list!");
			return;
		}
		valueList.Add(list);
	}
	protected void removeInuse(IEnumerable list, Type type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		HashSet<IEnumerable> valueList;
		if (mInusedList.TryGetValue(type, out valueList) && valueList.Remove(list))
		{
			return;
		}
		if (mPersistentInuseList.TryGetValue(type, out valueList) && valueList.Remove(list))
		{
			return;
		}
		Debug.LogError("can not find class type in Inused List! type : " + type);
	}
	protected void addUnuse(IEnumerable list, Type type)
	{
		// 加入未使用列表
		if (!mUnusedList.TryGetValue(type, out List<IEnumerable> valueList))
		{
			valueList = new List<IEnumerable>();
			mUnusedList.Add(type, valueList);
		}
		else if (valueList.Contains(list))
		{
			Debug.LogError("list Object is in Unused list! can not add again!");
			return;
		}
		valueList.Add(list);
	}
}