using System;
using System.Collections;
using System.Collections.Generic;

// 线程安全的字典列表池,但是效率较低
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
public class DictionaryPoolThread : FrameSystem
{
	protected Dictionary<DictionaryType, HashSet<IEnumerable>> mInusedList;
	protected Dictionary<DictionaryType, HashSet<IEnumerable>> mUnusedList;
	protected ThreadLock mListLock;
	public DictionaryPoolThread()
	{
		mInusedList = new Dictionary<DictionaryType, HashSet<IEnumerable>>();
		mUnusedList = new Dictionary<DictionaryType, HashSet<IEnumerable>>();
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
	public Dictionary<DictionaryType, HashSet<IEnumerable>> getInusedList() { return mInusedList; }
	public Dictionary<DictionaryType, HashSet<IEnumerable>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public Dictionary<K, V> newList<K, V>(out Dictionary<K, V> list)
	{
		list = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			var type = new DictionaryType(Typeof<K>(), Typeof<V>());
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.TryGetValue(type, out HashSet<IEnumerable> valueList) && valueList.Count > 0)
			{
				foreach (var item in valueList)
				{
					list = item as Dictionary<K, V>;
					break;
				}
				valueList.Remove(list);
			}
			// 未使用列表中没有,创建一个新的
			else
			{
				list = new Dictionary<K, V>();
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
	public void destroyList<K, V>(Dictionary<K, V> list)
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
	protected void addInuse<K, V>(Dictionary<K, V> list)
	{
		var type = new DictionaryType(Typeof<K>(), Typeof<V>());
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
	protected void removeInuse<K, V>(Dictionary<K, V> list)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		var type = new DictionaryType(Typeof<K>(), Typeof<V>());
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
	protected void addUnuse<K, V>(Dictionary<K, V> list)
	{
		// 加入未使用列表
		var type = new DictionaryType(Typeof<K>(), Typeof<V>());
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