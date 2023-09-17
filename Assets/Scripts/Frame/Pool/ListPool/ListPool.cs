using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static CSharpUtility;

// 仅能在主线程使用的列表池
public class ListPool : FrameSystem
{
	protected Dictionary<Type, HashSet<IList>> mPersistentInuseList;    // 持久使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, HashSet<IList>> mInusedList;             // 仅当前栈帧中使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, List<IList>> mUnusedList;				// 未使用列表
	protected Dictionary<IList, string> mObjectStack;                   // 对象分配的堆栈信息列表
	public ListPool()
	{
		mPersistentInuseList = new Dictionary<Type, HashSet<IList>>();
		mInusedList = new Dictionary<Type, HashSet<IList>>();
		mUnusedList = new Dictionary<Type, List<IList>>();
		mObjectStack = new Dictionary<IList, string>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject?.AddComponent<ListPoolDebug>();
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
	public Dictionary<Type, HashSet<IList>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, HashSet<IList>> getInusedList() { return mInusedList; }
	public Dictionary<Type, List<IList>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public IList newList(Type elementType, Type listType, string stackTrace, bool onlyOnce = true)
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程使用ListPool,子线程请使用ListPoolThread代替");
			return null;
		}
#endif
		IList list;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(elementType, out List<IList> valueList) && valueList.Count > 0)
		{
			list = valueList[valueList.Count - 1];
			valueList.RemoveAt(valueList.Count - 1);
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			list = createInstance<IList>(listType);
#if UNITY_EDITOR
			mInusedList.TryGetValue(listType, out HashSet<IList> temp0);
			mPersistentInuseList.TryGetValue(listType, out HashSet<IList> temp1);
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
				Debug.Log("创建的List总数量已经达到:" + totalCount + "个,type:" + elementType);
			}
#endif
		}
#if UNITY_EDITOR
		// 标记为已使用
		mObjectStack.Add(list, stackTrace);
		addInuse(list, elementType, onlyOnce);
#endif
		return list;
	}
	public void destroyList<T>(ref List<T> list, Type type)
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程使用ListPool,子线程请使用ListPoolThread代替");
			return;
		}
#endif
		list.Clear();
		addUnuse(list, type);
#if UNITY_EDITOR
		removeInuse(list, type);
		mObjectStack.Remove(list);
#endif
		list = null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(IList list, Type type, bool onlyOnce)
	{
		// 加入仅临时使用的列表对象的列表中
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.TryGetValue(type, out HashSet<IList> valueList))
		{
			valueList = new HashSet<IList>();
			inuseList.Add(type, valueList);
		}
		if (!valueList.Add(list))
		{
			Debug.LogError("list object is in inuse list!");
			return;
		}
	}
	protected void removeInuse(IList list, Type type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		HashSet<IList> valueList;
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
	protected void addUnuse(IList list, Type type)
	{
		// 加入未使用列表
		if (!mUnusedList.TryGetValue(type, out List<IList> valueList))
		{
			valueList = new List<IList>();
			mUnusedList.Add(type, valueList);
		}
#if UNITY_EDITOR
		if (valueList.Contains(list))
		{
			Debug.LogError("list Object is in Unused list! can not add again!");
			return;
		}
#endif
		valueList.Add(list);
	}
}