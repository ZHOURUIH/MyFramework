using System.Collections;
using System.Collections.Generic;
using System;

// 仅能在主线程使用的列表池
public class ListPool : FrameSystem
{
	protected Dictionary<Type, HashSet<IList>> mPersistentInuseList;	// 持久使用的列表对象
	protected Dictionary<Type, HashSet<IList>> mInusedList;				// 仅当前栈帧中使用的列表对象
	protected Dictionary<Type, HashSet<IList>> mUnusedList;
	protected Dictionary<IList, string> mObjectStack;
	public ListPool()
	{
		mPersistentInuseList = new Dictionary<Type, HashSet<IList>>();
		mInusedList = new Dictionary<Type, HashSet<IList>>();
		mUnusedList = new Dictionary<Type, HashSet<IList>>();
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
	public Dictionary<Type, HashSet<IList>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, HashSet<IList>> getInusedList() { return mInusedList; }
	public Dictionary<Type, HashSet<IList>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public IList newList(Type elementType, Type listType, string stackTrace, bool onlyOnce = true)
	{
		if(!isMainThread())
		{
			logError("只能在主线程使用ListPool,子线程请使用ListPoolThread代替");
			return null;
		}
		IList list = null;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(elementType, out HashSet<IList> valueList) && valueList.Count > 0)
		{
			foreach(var item in valueList)
			{
				list = item;
				break;
			}
			valueList.Remove(list);
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			list = createInstance<IList>(listType);
		}
		// 标记为已使用
		addInuse(list, elementType, onlyOnce);
#if UNITY_EDITOR
		mObjectStack.Add(list, stackTrace);
#endif
		return list;
	}
	public void destroyList(IList list, Type type)
	{
		if (!isMainThread())
		{
			logError("只能在主线程使用ListPool,子线程请使用ListPoolThread代替");
			return;
		}
#if UNITY_EDITOR
		mObjectStack.Remove(list);
#endif
		list.Clear();
		addUnuse(list, type);
		removeInuse(list, type);
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(IList list, Type type, bool onlyOnce)
	{
		// 加入仅临时使用的列表对象的列表中
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.TryGetValue(type, out HashSet<IList> valueList))
		{
			valueList = new HashSet<IList>();
			inuseList.Add(type, valueList);
		}
		else if (valueList.Contains(list))
		{
			logError("list object is in inuse list!");
			return;
		}
		valueList.Add(list);
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
		logError("can not find class type in Inused List! type : " + type);
	}
	protected void addUnuse(IList list, Type type)
	{
		// 加入未使用列表
		if (!mUnusedList.TryGetValue(type, out HashSet<IList> valueList))
		{
			valueList = new HashSet<IList>();
			mUnusedList.Add(type, valueList);
		}
		else if (valueList.Contains(list))
		{
			logError("list Object is in Unused list! can not add again!");
			return;
		}
		valueList.Add(list);
	}
}