using System.Collections;
using System.Collections.Generic;
using System;

// 仅能在主线程使用的列表池
public class HashSetPool : FrameSystem
{
	protected Dictionary<Type, HashSet<IEnumerable>> mPersistentInuseList;	// 持久使用的列表对象
	protected Dictionary<Type, HashSet<IEnumerable>> mInusedList;			// 仅当前栈帧中使用的列表对象
	protected Dictionary<Type, HashSet<IEnumerable>> mUnusedList;			// 未使用对象的列表
	protected Dictionary<IEnumerable, string> mObjectStack;					// 存储对象分配堆栈的列表
	public HashSetPool()
	{
		mPersistentInuseList = new Dictionary<Type, HashSet<IEnumerable>>();
		mInusedList = new Dictionary<Type, HashSet<IEnumerable>>();
		mUnusedList = new Dictionary<Type, HashSet<IEnumerable>>();
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
	public Dictionary<Type, HashSet<IEnumerable>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, HashSet<IEnumerable>> getInusedList() { return mInusedList; }
	public Dictionary<Type, HashSet<IEnumerable>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public IEnumerable newList(Type elementType, Type listType, string stackTrace, bool onlyOnce = true)
	{
		if(!isMainThread())
		{
			logError("只能在主线程使用HashSetPool,子线程请使用HashSetPoolThread代替");
			return null;
		}
		IEnumerable list;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(elementType, out HashSet<IEnumerable> valueList) && valueList.Count > 0)
		{
			list = popFirstElement(valueList);
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			list = createInstance<IEnumerable>(listType);
		}
		// 标记为已使用
		addInuse(list, elementType, onlyOnce);
#if UNITY_EDITOR
		mObjectStack.Add(list, stackTrace);
#endif
		return list;
	}
	// 回收时需要外部手动清空,因为内部无法调用Clear函数
	public void destroyList(IEnumerable list, Type type)
	{
		if (!isMainThread())
		{
			logError("只能在主线程使用HashSetPool,子线程请使用HashSetPoolThread代替");
			return;
		}
#if UNITY_EDITOR
		mObjectStack.Remove(list);
#endif
		addUnuse(list, type);
		removeInuse(list, type);
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
			logError("list object is in inuse list!");
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
		logError("can not find class type in Inused List! type : " + type);
	}
	protected void addUnuse(IEnumerable list, Type type)
	{
		// 加入未使用列表
		if (!mUnusedList.TryGetValue(type, out HashSet<IEnumerable> valueList))
		{
			valueList = new HashSet<IEnumerable>();
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