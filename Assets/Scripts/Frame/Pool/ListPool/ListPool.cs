using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ListPool : FrameSystem
{
	protected Dictionary<Type, List<IList>> mPersistentInuseList;	// 持久使用的列表对象
	protected Dictionary<Type, List<IList>> mInusedList;			// 仅当前栈帧中使用的列表对象
	protected Dictionary<Type, Stack<IList>> mUnusedList;
	protected ThreadLock mListLock;
	public ListPool()
	{
		mPersistentInuseList = new Dictionary<Type, List<IList>>();
		mInusedList = new Dictionary<Type, List<IList>>();
		mUnusedList = new Dictionary<Type, Stack<IList>>();
		mListLock = new ThreadLock();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<ListPoolDebug>();
#endif
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
#if UNITY_EDITOR
		foreach (var item in mInusedList)
		{
			if (item.Value.Count > 0)
			{
				logError("有临时列表对象正在使用中,是否在申请后忘记回收到池中! type:" + Typeof(item.Value[0]));
			}
		}
#endif
	}
	public Dictionary<Type, List<IList>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, List<IList>> getInusedList() { return mInusedList; }
	public Dictionary<Type, Stack<IList>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public List<T> newList<T>(out List<T> obj, bool onlyOnce = true, int capacity = 0)
	{
		obj = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			Type elementType = Typeof<T>();
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.TryGetValue(elementType, out Stack<IList> valueList) && valueList.Count > 0)
			{
				obj = valueList.Pop() as List<T>;
				if(capacity > 0 && capacity > obj.Capacity)
				{
					obj.Capacity = capacity;
				}
			}
			// 未使用列表中没有,创建一个新的
			else
			{
				obj = new List<T>(capacity);
			}
			// 标记为已使用
			addInuse(obj, onlyOnce);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		return obj;
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
	protected void addInuse<T>(List<T> list, bool onlyOnce)
	{
		Type type = Typeof<T>();
		// 加入仅临时使用的列表对象的列表中
		if(onlyOnce)
		{
			if (mInusedList.TryGetValue(type, out List<IList> valueList))
			{
				if (valueList.Contains(list))
				{
					logError("list object is in inuse list!");
					return;
				}
			}
			else
			{
				valueList = new List<IList>();
				mInusedList.Add(type, valueList);
			}
			valueList.Add(list);
		}
		// 加入持久使用的列表对象的列表中
		else
		{
			if (mPersistentInuseList.TryGetValue(type, out List<IList> valueList))
			{
				if (valueList.Contains(list))
				{
					logError("list object is in inuse list!");
					return;
				}
			}
			else
			{
				valueList = new List<IList>();
				mPersistentInuseList.Add(type, valueList);
			}
			valueList.Add(list);
		}
	}
	protected void removeInuse<T>(List<T> list)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		Type type = Typeof<T>();
		if (mInusedList.TryGetValue(type, out List<IList> valueList))
		{
			if (!valueList.Remove(list))
			{
				logError("Inused List not contains class object!");
				return;
			}
		}
		else if(mPersistentInuseList.TryGetValue(type, out List<IList> persistentValueList))
		{
			if (!persistentValueList.Remove(list))
			{
				logError("Inused List not contains class object!");
				return;
			}
		}
		else
		{
			logError("can not find class type in Inused List! type : " + type);
			return;
		}
	}
	protected void addUnuse<T>(List<T> list)
	{
		// 加入未使用列表
		Type type = Typeof<T>();
		if (mUnusedList.TryGetValue(type, out Stack<IList> valueList))
		{
			if (valueList.Contains(list))
			{
				logError("list Object is in Unused list! can not add again!");
				return;
			}
		}
		else
		{
			valueList = new Stack<IList>();
			mUnusedList.Add(type, valueList);
		}
		valueList.Push(list);
	}
}