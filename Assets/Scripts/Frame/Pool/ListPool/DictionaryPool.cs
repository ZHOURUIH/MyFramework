using System;
using System.Collections;
using System.Collections.Generic;

// 仅能在主线程中使用的字典列表池
public class DictionaryPool : FrameSystem
{
	protected Dictionary<DictionaryType, HashSet<ICollection>> mPersistentInuseList;	// 持久使用的列表对象
	protected Dictionary<DictionaryType, HashSet<ICollection>> mInusedList;				// 仅当前栈帧中使用的列表对象
	protected Dictionary<DictionaryType, HashSet<ICollection>> mUnusedList;				// 未使用的列表
	protected Dictionary<ICollection, string> mObjectStack;								// 存储对象分配的堆栈信息的列表
	public DictionaryPool()
	{
		mPersistentInuseList = new Dictionary<DictionaryType, HashSet<ICollection>>();
		mInusedList = new Dictionary<DictionaryType, HashSet<ICollection>>();
		mUnusedList = new Dictionary<DictionaryType, HashSet<ICollection>>();
		mObjectStack = new Dictionary<ICollection, string>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<DictionaryPoolDebug>();
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
	public Dictionary<DictionaryType, HashSet<ICollection>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<DictionaryType, HashSet<ICollection>> getInusedList() { return mInusedList; }
	public Dictionary<DictionaryType, HashSet<ICollection>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public ICollection newList(Type keyType, Type valueType, Type listType, string stackTrace, bool onlyOnce = true)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用DictionaryPool,子线程中请使用DictionaryPoolThread代替");
			return null;
		}
		ICollection list;
		var type = new DictionaryType(keyType, valueType);
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(type, out HashSet<ICollection> valueList) && valueList.Count > 0)
		{
			list = popFirstElement(valueList);
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			list = createInstance<ICollection>(listType);
		}
		// 标记为已使用
		addInuse(list, type, onlyOnce);
#if UNITY_EDITOR
		mObjectStack.Add(list, stackTrace);
#endif
		return list;
	}
	public void destroyList(ICollection list, Type keyType, Type valueType)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用DictionaryPool,子线程中请使用DictionaryPoolThread代替");
			return;
		}
#if UNITY_EDITOR
		mObjectStack.Remove(list);
#endif
		if(list.Count > 0)
		{
			logError("销毁列表时需要手动清空列表");
		}
		var type = new DictionaryType(keyType, valueType);
		addUnuse(list, type);
		removeInuse(list, type);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(ICollection list, DictionaryType type, bool onlyOnce)
	{
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.TryGetValue(type, out HashSet<ICollection> valueList))
		{
			valueList = new HashSet<ICollection>();
			inuseList.Add(type, valueList);
		}
		else if (valueList.Contains(list))
		{
			logError("list object is in inuse list!");
			return;
		}
		valueList.Add(list);
	}
	protected void removeInuse(ICollection list, DictionaryType type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		HashSet<ICollection> valueList;
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
	protected void addUnuse(ICollection list, DictionaryType type)
	{
		// 加入未使用列表
		if (!mUnusedList.TryGetValue(type, out HashSet<ICollection> valueList))
		{
			valueList = new HashSet<ICollection>();
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