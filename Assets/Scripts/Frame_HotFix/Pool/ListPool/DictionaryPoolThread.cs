using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;
using static FrameBaseUtility;

// 线程安全的字典列表池,但是效率较低
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
public class DictionaryPoolThread : FrameSystem
{
	protected Dictionary<DictionaryType, HashSet<ICollection>> mInusedList = new(); // 已使用的列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<DictionaryType, Queue<ICollection>> mUnusedList = new();	// 未使用的列表
	protected ThreadLock mListLock = new();                                         // 列表的线程锁
	public DictionaryPoolThread()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<DictionaryPoolThreadDebug>();
		}
	}
	public ThreadLock getLock() { return mListLock; }
	public void clearUnused()
	{
		using (new ThreadLockScope(mListLock))
		{
			mUnusedList.Clear();
		}
	}
	public Dictionary<DictionaryType, HashSet<ICollection>> getInusedList() { return mInusedList; }
	public Dictionary<DictionaryType, Queue<ICollection>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public ICollection newList(Type keyType, Type valueType, Type listType)
	{
		ICollection list = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				bool isNew = false;
				DictionaryType type = new(keyType, valueType);
				// 先从未使用的列表中查找是否有可用的对象
				if (mUnusedList.TryGetValue(type, out var valueList) && valueList.Count > 0)
				{
					list = valueList.Dequeue();
				}
				// 未使用列表中没有,创建一个新的
				else
				{
					list = createInstance<ICollection>(listType);
					isNew = true;
				}
				if (isEditor())
				{
					// 标记为已使用
					addInuse(list, type);

					if (isNew)
					{
						int totalCount = mInusedList.get(type)?.Count ?? 0;
						if (totalCount % 1000 == 0)
						{
							Debug.Log("创建的Dictionary总数量已经达到:" + totalCount + "个,key:" + keyType + ",value:" + valueType);
						}
					}
				}
			}
			catch (Exception e)
			{
				logException(e);
			}
		}
		return list;
	}
	public void destroyList<K, V>(ref Dictionary<K, V> list, Type keyType, Type valueType)
	{
		if (list == null)
		{
			return;
		}
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				list.Clear();
				DictionaryType type = new(keyType, valueType);
				addUnuse(list, type);
				if (isEditor())
				{
					removeInuse(list, type);
				}
			}
			catch (Exception e)
			{
				logException(e);
			}
		}
		list = null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(ICollection list, DictionaryType type)
	{
		if (!mInusedList.getOrAddNew(type).Add(list))
		{
			Debug.LogError("list object is in inuse list!");
		}
	}
	protected void removeInuse(ICollection list, DictionaryType type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (mInusedList.TryGetValue(type, out var valueList) && valueList.Remove(list))
		{
			return;
		}
		Debug.LogError("can not find class type in Inused List! type : " + type);
	}
	protected void addUnuse(ICollection list, DictionaryType type)
	{
		// 加入未使用列表
		var valueList = mUnusedList.getOrAddNew(type);
		if (isEditor() && valueList.Contains(list))
		{
			Debug.LogError("list Object is in Unused list! can not add again!");
			return;
		}
		valueList.Enqueue(list);
	}
}