using System;
using System.Collections;
using System.Collections.Generic;

// 线程安全的列表池,但效率较低
public class ListPoolThread : FrameSystem
{
	protected Dictionary<Type, HashSet<IList>> mInusedList;
	protected Dictionary<Type, HashSet<IList>> mUnusedList;
	protected ThreadLock mListLock;
	public ListPoolThread()
	{
		mInusedList = new Dictionary<Type, HashSet<IList>>();
		mUnusedList = new Dictionary<Type, HashSet<IList>>();
		mListLock = new ThreadLock();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject?.AddComponent<ListPoolThreadDebug>();
#endif
	}
	public void lockList() { mListLock.waitForUnlock(); }
	public void unlockList() { mListLock.unlock(); }
	public Dictionary<Type, HashSet<IList>> getInusedList() { return mInusedList; }
	public Dictionary<Type, HashSet<IList>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public List<T> newList<T>(out List<T> list)
	{
		list = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			Type elementType = Typeof<T>();
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.TryGetValue(elementType, out HashSet<IList> valueList) && valueList.Count > 0)
			{
				foreach (var item in valueList)
				{
					list = item as List<T>;
					break;
				}
				valueList.Remove(list);
			}
			// 未使用列表中没有,创建一个新的
			else
			{
				list = new List<T>();
			}
			// 标记为已使用
			addInuse(list);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		return list;
	}
	public void destroyList<T>(List<T> list)
	{
		mListLock.waitForUnlock();
		try
		{
			list.Clear();
			addUnuse(list);
			removeInuse(list);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse<T>(List<T> list)
	{
		Type type = Typeof<T>();
		if (mInusedList.TryGetValue(type, out HashSet<IList> valueList))
		{
			if (valueList.Contains(list))
			{
				logError("list object is in inuse list!");
				return;
			}
		}
		else
		{
			valueList = new HashSet<IList>();
			mInusedList.Add(type, valueList);
		}
		valueList.Add(list);
	}
	protected void removeInuse<T>(List<T> list)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		Type type = Typeof<T>();
		if (mInusedList.TryGetValue(type, out HashSet<IList> valueList))
		{
			if (!valueList.Remove(list))
			{
				logError("Inused List not contains class object!");
				return;
			}
		}
		logError("can not find class type in Inused List! type : " + type);
	}
	protected void addUnuse<T>(List<T> list)
	{
		// 加入未使用列表
		Type type = Typeof<T>();
		if (mUnusedList.TryGetValue(type, out HashSet<IList> valueList))
		{
			if (valueList.Contains(list))
			{
				logError("list Object is in Unused list! can not add again!");
				return;
			}
		}
		else
		{
			valueList = new HashSet<IList>();
			mUnusedList.Add(type, valueList);
		}
		valueList.Add(list);
	}
}