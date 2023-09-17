using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static CSharpUtility;

// 仅能在主线程中使用的字典列表池
public class DictionaryPool : FrameSystem
{
	protected Dictionary<DictionaryType, HashSet<ICollection>> mPersistentInuseList;    // 持久使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<DictionaryType, HashSet<ICollection>> mInusedList;             // 仅当前栈帧中使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<DictionaryType, List<ICollection>> mUnusedList;				// 未使用的列表
	protected Dictionary<ICollection, string> mObjectStack;                             // 存储对象分配的堆栈信息的列表
	public DictionaryPool()
	{
		mPersistentInuseList = new Dictionary<DictionaryType, HashSet<ICollection>>();
		mInusedList = new Dictionary<DictionaryType, HashSet<ICollection>>();
		mUnusedList = new Dictionary<DictionaryType, List<ICollection>>();
		mObjectStack = new Dictionary<ICollection, string>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<DictionaryPoolDebug>();
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
				string stack = mObjectStack[itemList];
				if (isEmpty(stack))
				{
					stack = "当前未开启对象池的堆栈追踪,可在对象分配前使用F4键开启堆栈追踪,然后就可以在此错误提示中看到对象分配时所在的堆栈\n";
				}
				else
				{
					stack = "create stack:\n" + stack + "\n";
				}
				logError("有临时对象正在使用中,是否在申请后忘记回收到池中! \n" + stack);
				break;
			}
		}
#endif
	}
	public void clearUnused() { mUnusedList.Clear(); }
	public Dictionary<DictionaryType, HashSet<ICollection>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<DictionaryType, HashSet<ICollection>> getInusedList() { return mInusedList; }
	public Dictionary<DictionaryType, List<ICollection>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public ICollection newList(Type keyType, Type valueType, Type listType, string stackTrace, bool onlyOnce = true)
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用DictionaryPool,子线程中请使用DictionaryPoolThread代替");
			return null;
		}
#endif

		ICollection list;
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
			mPersistentInuseList.TryGetValue(type, out HashSet<ICollection> temp1);
			int totalCount = 1;
			if (temp0 != null)
			{
				totalCount += temp0.Count;
			}
			if (temp1 != null)
			{
				totalCount += temp1.Count;
			}
			if (totalCount % 1000 == 0)
			{
				Debug.Log("创建的Dictionary总数量已经达到:" + totalCount + "个,key:" + keyType + ",value:" + valueType);
			}
#endif
		}
#if UNITY_EDITOR
		// 标记为已使用
		addInuse(list, type, onlyOnce);
		mObjectStack.Add(list, stackTrace);
#endif
		return list;
	}
	public void destroyList<K, V>(ref Dictionary<K, V> list, Type keyType, Type valueType)
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用DictionaryPool,子线程中请使用DictionaryPoolThread代替");
			return;
		}
#endif
		if(list.Count > 0)
		{
			Debug.LogError("销毁列表时需要手动清空列表");
		}
		var type = new DictionaryType(keyType, valueType);
		addUnuse(list, type);
#if UNITY_EDITOR
		removeInuse(list, type);
		mObjectStack.Remove(list);
#endif
		list = null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(ICollection list, DictionaryType type, bool onlyOnce)
	{
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.TryGetValue(type, out HashSet<ICollection> valueList))
		{
			valueList = new HashSet<ICollection>();
			inuseList.Add(type, valueList);
		}
		if (!valueList.Add(list))
		{
			Debug.LogError("list object is in inuse list!");
			return;
		}
	}
	protected void removeInuse(ICollection list, DictionaryType type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		HashSet<ICollection> valueList;
		if (mInusedList.TryGetValue(type, out valueList) && valueList.Remove(list))
		{
			return;
		}
		if (mPersistentInuseList.TryGetValue(type, out valueList) && valueList.Remove(list))
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
#if UNITY_EDITOR
		if (valueList.Contains(list))
		{
			Debug.LogError("list Object is in Unused list! can not add again!");
			return;
		}
#endif
		valueList.Add(list);
	}
}