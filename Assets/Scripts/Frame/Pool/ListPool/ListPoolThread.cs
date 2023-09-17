using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;

// 线程安全的列表池,但效率较低
public class ListPoolThread : FrameSystem
{
	protected Dictionary<Type, HashSet<IList>> mInusedList; // 已使用列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, List<IList>> mUnusedList;	// 未使用列表
	protected ThreadLock mListLock;                         // 列表的线程锁
	public ListPoolThread()
	{
		mInusedList = new Dictionary<Type, HashSet<IList>>();
		mUnusedList = new Dictionary<Type, List<IList>>();
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
	public ThreadLock getLock() { return mListLock; }
	public void clearUnused() 
	{
		using (new ThreadLockScope(mListLock))
		{
			mUnusedList.Clear();
		}
	}
	public Dictionary<Type, HashSet<IList>> getInusedList() { return mInusedList; }
	public Dictionary<Type, List<IList>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public IList newList(Type elementType, Type listType)
	{
		IList list = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				// 先从未使用的列表中查找是否有可用的对象
				if (mUnusedList.TryGetValue(elementType, out List<IList> valueList) && valueList.Count > 0)
				{
					list = valueList[valueList.Count - 1];
					valueList.RemoveAt(valueList.Count - 1);
				}
				// 未使用列表中没有,创建一个新的
				else
				{
					list = createInstance<IList>(listType);
#if UNITY_EDITOR
					mInusedList.TryGetValue(listType, out HashSet<IList> temp0);
					int totalCount = 1;
					if (temp0 != null)
					{
						totalCount += temp0.Count;
					}
					if (totalCount % 1000 == 0)
					{
						Debug.Log("创建的List总数量已经达到:" + totalCount + "个,type:" + elementType);
					}
#endif
				}
#if UNITY_EDITOR
				// 标记为已使用
				addInuse(list, elementType);
#endif
			}
			catch (Exception e)
			{
				logException(e);
			}
		}
		return list;
	}
	public void destroyList(ref IList list, Type type)
	{
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				list.Clear();
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
	protected void addInuse(IList list, Type type)
	{
		if (mInusedList.TryGetValue(type, out HashSet<IList> valueList))
		{
			if (valueList.Contains(list))
			{
				Debug.LogError("list object is in inuse list!");
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
	protected void removeInuse(IList list, Type type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (mInusedList.TryGetValue(type, out HashSet<IList> valueList))
		{
			if (!valueList.Remove(list))
			{
				Debug.LogError("Inused List not contains class object!");
				return;
			}
		}
		logError("can not find class type in Inused List! type : " + type);
	}
	protected void addUnuse(IList list, Type type)
	{
		// 加入未使用列表
		if (mUnusedList.TryGetValue(type, out List<IList> valueList))
		{
			if (valueList.Contains(list))
			{
				Debug.LogError("list Object is in Unused list! can not add again!");
				return;
			}
		}
		else
		{
			valueList = new List<IList>();
			mUnusedList.Add(type, valueList);
		}
		valueList.Add(list);
	}
}