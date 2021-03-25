using System.Collections;
using System.Collections.Generic;
using System;

// 仅能在主线程使用的列表池
public class ListPool : FrameSystem
{
	protected Dictionary<Type, HashSet<IList>> mPersistentInuseList;	// 持久使用的列表对象
	protected Dictionary<Type, HashSet<IList>> mInusedList;				// 仅当前栈帧中使用的列表对象
	protected Dictionary<Type, HashSet<IList>> mUnusedList;
	protected Dictionary<IList, string> mObjectStack;
	public ListPool()
	{
		mPersistentInuseList = new Dictionary<Type, HashSet<IList>>();
		mInusedList = new Dictionary<Type, HashSet<IList>>();
		mUnusedList = new Dictionary<Type, HashSet<IList>>();
		mObjectStack = new Dictionary<IList, string>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject?.AddComponent<ListPoolDebug>();
#endif
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
#if UNITY_EDITOR
		foreach (var item in mInusedList)
		{
			foreach (var itemList in item.Value)
			{
				logError("有临时对象正在使用中,是否在申请后忘记回收到池中! create stack:" + mObjectStack[itemList] + "\n");
				break;
			}
		}
#endif
	}
	public Dictionary<Type, HashSet<IList>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, HashSet<IList>> getInusedList() { return mInusedList; }
	public Dictionary<Type, HashSet<IList>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public List<T> newList<T>(out List<T> list, bool onlyOnce = true)
	{
		list = null;
		if(!isMainThread())
		{
			logError("只能在主线程使用ListPool,子线程请使用ListPoolThread代替");
			return null;
		}
		Type elementType = Typeof<T>();
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(elementType, out HashSet<IList> valueList) && valueList.Count > 0)
		{
			foreach(var item in valueList)
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
		addInuse(list, onlyOnce);
#if UNITY_EDITOR
		if (mGameFramework.isEnablePoolStackTrace())
		{
			mObjectStack.Add(list, getStackTrace());
		}
		else
		{
			mObjectStack.Add(list, EMPTY);
		}
#endif
		return list;
	}
	public void destroyList<T>(List<T> list)
	{
		if (!isMainThread())
		{
			logError("只能在主线程使用ListPool,子线程请使用ListPoolThread代替");
			return;
		}
#if UNITY_EDITOR
		mObjectStack.Remove(list);
#endif
		list.Clear();
		addUnuse(list);
		removeInuse(list);
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse<T>(List<T> list, bool onlyOnce)
	{
		Type type = Typeof<T>();
		// 加入仅临时使用的列表对象的列表中
		if(onlyOnce)
		{
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
		// 加入持久使用的列表对象的列表中
		else
		{
			if (mPersistentInuseList.TryGetValue(type, out HashSet<IList> valueList))
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
				mPersistentInuseList.Add(type, valueList);
			}
			valueList.Add(list);
		}
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
		else if(mPersistentInuseList.TryGetValue(type, out HashSet<IList> persistentValueList))
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