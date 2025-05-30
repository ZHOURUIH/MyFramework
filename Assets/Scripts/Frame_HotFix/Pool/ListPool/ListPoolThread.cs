﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;
using static FrameBaseUtility;

// 线程安全的列表池,但效率较低
public class ListPoolThread : FrameSystem
{
	protected Dictionary<Type, HashSet<IList>> mInusedList = new(); // 已使用列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Queue<IList>> mUnusedList = new();	// 未使用列表
	protected ThreadLock mListLock = new();                         // 列表的线程锁
	public ListPoolThread()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<ListPoolThreadDebug>();
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
	public Dictionary<Type, HashSet<IList>> getInusedList() { return mInusedList; }
	public Dictionary<Type, Queue<IList>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public IList newList(Type elementType, Type listType)
	{
		IList list = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				bool isNew = false;
				// 先从未使用的列表中查找是否有可用的对象
				if (mUnusedList.TryGetValue(elementType, out var valueList) && valueList.Count > 0)
				{
					list = valueList.Dequeue();
				}
				// 未使用列表中没有,创建一个新的
				else
				{
					list = createInstance<IList>(listType);
					isNew = true;
				}
				if (isEditor())
				{
					// 标记为已使用
					addInuse(list, elementType);
					if (isNew)
					{
						int totalCount = mInusedList.get(elementType)?.Count ?? 0;
						if (totalCount % 1000 == 0)
						{
							Debug.Log("创建的List总数量已经达到:" + totalCount + "个,type:" + elementType);
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
	public void destroyList(ref IList list, Type type)
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
	protected void addInuse(IList list, Type type)
	{
		if (!mInusedList.getOrAddNew(type).Add(list))
		{
			Debug.LogError("list object is in inuse list!");
		}
	}
	protected void removeInuse(IList list, Type type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (mInusedList.TryGetValue(type, out var valueList) && !valueList.Remove(list))
		{
			Debug.LogError("Inused List not contains class object!");
			return;
		}
		logError("can not find class type in Inused List! type : " + type);
	}
	protected void addUnuse(IList list, Type type)
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