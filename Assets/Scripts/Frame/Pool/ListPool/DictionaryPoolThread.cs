using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;

// 线程安全的字典列表池,但是效率较低
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
public class DictionaryPoolThread : FrameSystem
{
	protected Dictionary<DictionaryType, HashSet<ICollection>> mInusedList; // 已使用的列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<DictionaryType, List<ICollection>> mUnusedList;	// 未使用的列表
	protected ThreadLock mListLock;                                         // 列表的线程锁
	protected Dictionary<DictionaryType, int> mCreatedCount;
	public DictionaryPoolThread()
	{
		mInusedList = new Dictionary<DictionaryType, HashSet<ICollection>>();
		mUnusedList = new Dictionary<DictionaryType, List<ICollection>>();
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
	public ThreadLock getLock() { return mListLock; }
	public void clearUnused()
	{
		using (new ThreadLockScope(mListLock))
		{
			mUnusedList.Clear();
		}
	}
	public Dictionary<DictionaryType, HashSet<ICollection>> getInusedList() { return mInusedList; }
	public Dictionary<DictionaryType, List<ICollection>> getUnusedList() { return mUnusedList; }
	public Dictionary<DictionaryType, int> getCreateCount() { return mCreatedCount; }
	// onlyOnce表示是否仅当作临时列表使用
	public ICollection newList(Type keyType, Type valueType, Type listType)
	{
		ICollection list = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				var type = new DictionaryType(keyType, valueType);
				// 先从未使用的列表中查找是否有可用的对象
				if (mUnusedList.TryGetValue(type, out List<ICollection> valueList) && valueList.Count > 0)
				{
					list = valueList[valueList.Count - 1];
					valueList.RemoveAt(valueList.Count - 1);
				}
				// 未使用列表中没有,创建一个新的
				else
				{
					list = createInstance<ICollection>(listType);
#if UNITY_EDITOR
					mInusedList.TryGetValue(type, out HashSet<ICollection> temp0);
					int totalCount = 1;
					if (temp0 != null)
					{
						totalCount += temp0.Count;
					}
					if (totalCount % 1000 == 0)
					{
						Debug.Log("创建的Dictionary总数量已经达到:" + totalCount + "个,key:" + keyType + ",value:" + valueType);
					}
#endif
				}
#if UNITY_EDITOR
				// 标记为已使用
				addInuse(list, type);
#endif
			}
			catch (Exception e)
			{
				logException(e);
			}
		}
		return list;
	}
	public void destroyList(ref ICollection list, DictionaryType type)
	{
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				if (list.Count > 0)
				{
					Debug.LogError("销毁列表时需要手动清空列表");
				}
				addUnuse(list, type);
#if UNITY_EDITOR
				removeInuse(list, type);
#endif
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
		if (!mInusedList.TryGetValue(type, out HashSet<ICollection> valueList))
		{
			valueList = new HashSet<ICollection>();
			mInusedList.Add(type, valueList);
		}
		else if (valueList.Contains(list))
		{
			Debug.LogError("list object is in inuse list!");
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
		Debug.LogError("can not find class type in Inused List! type : " + type);
	}
	protected void addUnuse(ICollection list, DictionaryType type)
	{
		// 加入未使用列表
		if (!mUnusedList.TryGetValue(type, out List<ICollection> valueList))
		{
			valueList = new List<ICollection>();
			mUnusedList.Add(type, valueList);
		}
		else if (valueList.Contains(list))
		{
			Debug.LogError("list Object is in Unused list! can not add again!");
			return;
		}
		valueList.Add(list);
	}
}