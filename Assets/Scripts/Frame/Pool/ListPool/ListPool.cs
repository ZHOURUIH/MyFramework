using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ListPool : FrameComponent
{
	protected Dictionary<Type, List<IList>> mInusedList;
	protected Dictionary<Type, List<IList>> mUnusedList;
	protected ThreadLock mListLock;
	public ListPool(string name)
		: base(name)
	{
		mInusedList = new Dictionary<Type, List<IList>>();
		mUnusedList = new Dictionary<Type, List<IList>>();
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
		foreach(var item in mInusedList)
		{
			if(item.Value.Count > 0)
			{
				logError("有临时列表对象正在使用中,是否在申请后忘记回收到池中! type:" + item.Value[0].GetType());
			}
		}
	}
	public Dictionary<Type, List<IList>> getInusedList() { return mInusedList; }
	public Dictionary<Type, List<IList>> getUnusedList() { return mUnusedList; }
	// 返回值表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public List<T> newList<T>(out List<T> obj)
	{
		string info = null;
		obj = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			Type elementType = typeof(T);
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.ContainsKey(elementType) && mUnusedList[elementType].Count > 0)
			{
				obj = mUnusedList[elementType][0] as List<T>;
			}
			// 未使用列表中没有,创建一个新的
			else
			{
				obj = createInstance<List<T>>(typeof(List<T>));
			}
			// 标记为已使用
			if (!markUsed(obj))
			{
				info = "List<T> is in Inused list! can not add again!";
			}
		}
		catch(Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		if (!isEmpty(info))
		{
			logError(info);
		}
		return obj;
	}
	public void destroyList<T>(List<T> list)
	{
		string info = null;
		bool ret = false;
		mListLock.waitForUnlock();
		try
		{
			list.Clear();
			ret = markUnused(list, ref info);
		}
		catch(Exception e)
		{
			logError(e.Message);
		}	
		mListLock.unlock();
		if(!ret)
		{
			logError(info);
		}
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected bool checkUsed<T>(List<T> classObject)
	{
		Type type = typeof(T);
		return mInusedList.ContainsKey(type) && mInusedList[type].Contains(classObject);
	}
	protected bool markUsed<T>(List<T> classObject)
	{
		Type type = typeof(T);
		if (mInusedList.ContainsKey(type))
		{
			if (mInusedList[type].Contains(classObject))
			{
				return false;
			}
		}
		else
		{
			mInusedList.Add(type, new List<IList>());
		}
		// 加入使用列表
		mInusedList[type].Add(classObject);
		// 从未使用列表移除
		if (mUnusedList.ContainsKey(type))
		{
			mUnusedList[type].Remove(classObject);
		}
		return true;
	}
	protected bool markUnused<T>(List<T> list, ref string info)
	{
		// 加入未使用列表
		Type type = typeof(T);
		if (mUnusedList.ContainsKey(type))
		{
			if (mUnusedList[type].Contains(list))
			{
				info = "list Object is in Unused list! can not add again!";
				return false;
			}
		}
		else
		{
			mUnusedList.Add(type, new List<IList>());
		}
		mUnusedList[type].Add(list);
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (!mInusedList.ContainsKey(type))
		{
			info = "can not find class type in Inused List! type : " + type.ToString();
			return false;
		}
		if (!mInusedList[type].Remove(list))
		{
			info = "Inused List not contains class object!";
			return false;
		}
		return true;
	}
}