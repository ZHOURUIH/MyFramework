using System;
using System.Collections;
using System.Collections.Generic;

// 线程安全的字典列表池,但是效率较低
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
public class DictionaryPoolThread : FrameSystem
{
	protected Dictionary<DictionaryType, HashSet<ICollection>> mInusedList;	// 已使用的列表
	protected Dictionary<DictionaryType, HashSet<ICollection>> mUnusedList;	// 未使用的列表
	protected ThreadLock mListLock;											// 列表的线程锁
	public DictionaryPoolThread()
	{
		mInusedList = new Dictionary<DictionaryType, HashSet<ICollection>>();
		mUnusedList = new Dictionary<DictionaryType, HashSet<ICollection>>();
		mListLock = new ThreadLock();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<DictionaryPoolThreadDebug>();
#endif
	}
	public void lockList() { mListLock.waitForUnlock(); }
	public void unlockList() { mListLock.unlock(); }
	public Dictionary<DictionaryType, HashSet<ICollection>> getInusedList() { return mInusedList; }
	public Dictionary<DictionaryType, HashSet<ICollection>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public ICollection newList(Type keyType, Type valueType, Type listType)
	{
		ICollection list = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
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
			addInuse(list, type);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		return list;
	}
	public void destroyList(ICollection list, DictionaryType type)
	{
		mListLock.waitForUnlock();
		try
		{
			if(list.Count > 0)
			{
				logError("销毁列表时需要手动清空列表");
			}
			addUnuse(list, type);
			removeInuse(list, type);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(ICollection list, DictionaryType type)
	{
		if (!mInusedList.TryGetValue(type, out HashSet<ICollection> valueList))
		{
			valueList = new HashSet<ICollection>();
			mInusedList.Add(type, valueList);
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
		if (mInusedList.TryGetValue(type, out HashSet<ICollection> valueList) && valueList.Remove(list))
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