using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;
using static FrameEditorUtility;

// 仅能在主线程使用的列表池
public class ListPool : FrameSystem
{
	protected Dictionary<Type, HashSet<IList>> mPersistentInuseList = new();    // 持久使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, HashSet<IList>> mInusedList = new();             // 仅当前栈帧中使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Queue<IList>> mUnusedList = new();				// 未使用列表
	protected Dictionary<IList, string> mObjectStack = new();                   // 对象分配的堆栈信息列表
	public ListPool()
	{
		mCreateObject = true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isEditor())
		{
			foreach (var item in mInusedList.Values)
			{
				foreach (IList itemList in item)
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
	// onlyOnce表示是否仅当作临时列表使用
	public IList newList(Type elementType, Type listType, string stackTrace, bool onlyOnce = true)
	{
		if (isEditor() && !isMainThread())
		{
			Debug.LogError("只能在主线程使用ListPool,子线程请使用ListPoolThread代替");
			return null;
		}
		IList list;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(elementType, out var valueList) && valueList.Count > 0)
		{
			list = valueList.Dequeue();
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			list = createInstance<IList>(listType);
			if (isEditor())
			{
				int totalCount = 1;
				totalCount += mInusedList.get(listType)?.Count ?? 0;
				totalCount += mPersistentInuseList.get(listType)?.Count ?? 0;
				if (totalCount % 1000 == 0)
				{
					Debug.Log("创建的List总数量已经达到:" + totalCount + "个,type:" + elementType);
				}
			}
		}
		if (isEditor())
		{
			// 标记为已使用
			mObjectStack.Add(list, stackTrace);
			addInuse(list, elementType, onlyOnce);
		}
		return list;
	}
	public void destroyList<T>(ref List<T> list, Type elementType)
	{
		if (list == null)
		{
			return;
		}
		if (isEditor() && !isMainThread())
		{
			Debug.LogError("只能在主线程使用ListPool,子线程请使用ListPoolThread代替");
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
	protected void addInuse(IList list, Type elementType, bool onlyOnce)
	{
		// 加入仅临时使用的列表对象的列表中
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.tryGetOrAddNew(elementType).Add(list))
		{
			Debug.LogError("list object is in inuse list!");
			return;
		}
	}
	protected void removeInuse(IList list, Type elementType)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		HashSet<IList> valueList;
		if (mInusedList.TryGetValue(elementType, out valueList) && valueList.Remove(list))
		{
			return;
		}
		if (mPersistentInuseList.TryGetValue(elementType, out valueList) && valueList.Remove(list))
		{
			return;
		}
		Debug.LogError("can not find class type in Inused List! elementType : " + elementType);
	}
	protected void addUnuse(IList list, Type elementType)
	{
		// 加入未使用列表
		var valueList = mUnusedList.tryGetOrAddNew(elementType);
		if (isEditor() && valueList.Contains(list))
		{
			Debug.LogError("list Object is in Unused list! can not add again!");
			return;
		}
		valueList.Enqueue(list);
	}
}