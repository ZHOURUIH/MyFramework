using System;
using System.Collections;
using System.Collections.Generic;

// 线程安全的列表池,但效率较低
public class HashSetPoolThread : FrameSystem
{
	protected Dictionary<Type, HashSet<IEnumerable>> mInusedList;	// 已使用列表
	protected Dictionary<Type, HashSet<IEnumerable>> mUnusedList;	// 未使用列表
	protected ThreadLock mListLock;									// 列表的线程锁
	public HashSetPoolThread()
	{
		mInusedList = new Dictionary<Type, HashSet<IEnumerable>>();
		mUnusedList = new Dictionary<Type, HashSet<IEnumerable>>();
		mListLock = new ThreadLock();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject?.AddComponent<HashSetPoolThreadDebug>();
#endif
	}
	public void lockList() { mListLock.waitForUnlock(); }
	public void unlockList() { mListLock.unlock(); }
	public Dictionary<Type, HashSet<IEnumerable>> getInusedList() { return mInusedList; }
	public Dictionary<Type, HashSet<IEnumerable>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public IEnumerable newList(Type elementType, Type listType)
	{
		IEnumerable list = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
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
			addInuse(list, elementType);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		return list;
	}
	// 销毁列表时需要手动清空列表,因为内部无法调用列表的Clear
	public void destroyList(IEnumerable list, Type elementType)
	{
		mListLock.waitForUnlock();
		try
		{
			addUnuse(list, elementType);
			removeInuse(list, elementType);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(IEnumerable list, Type type)
	{
		if (mInusedList.TryGetValue(type, out HashSet<IEnumerable> valueList))
		{
			if (valueList.Contains(list))
			{
				logError("list object is in inuse list!");
				return;
			}
		}
		else
		{
			valueList = new HashSet<IEnumerable>();
			mInusedList.Add(type, valueList);
		}
		valueList.Add(list);
	}
	protected void removeInuse(IEnumerable list, Type type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (mInusedList.TryGetValue(type, out HashSet<IEnumerable> valueList))
		{
			if (!valueList.Remove(list))
			{
				logError("Inused List not contains class object!");
				return;
			}
		}
		logError("can not find class type in Inused List! type : " + type);
	}
	protected void addUnuse(IEnumerable list, Type type)
	{
		// 加入未使用列表
		if (mUnusedList.TryGetValue(type, out HashSet<IEnumerable> valueList))
		{
			if (valueList.Contains(list))
			{
				logError("list Object is in Unused list! can not add again!");
				return;
			}
		}
		else
		{
			valueList = new HashSet<IEnumerable>();
			mUnusedList.Add(type, valueList);
		}
		valueList.Add(list);
	}
}