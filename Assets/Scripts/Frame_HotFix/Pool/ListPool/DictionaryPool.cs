using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;
using static FrameEditorUtility;

// 仅能在主线程中使用的字典列表池
public class DictionaryPool : FrameSystem
{
	protected Dictionary<DictionaryType, HashSet<ICollection>> mPersistentInuseList = new();    // 持久使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<DictionaryType, HashSet<ICollection>> mInusedList = new();             // 仅当前栈帧中使用的列表对象,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<DictionaryType, Queue<ICollection>> mUnusedList = new();				// 未使用的列表
	protected Dictionary<ICollection, string> mObjectStack = new();                             // 存储对象分配的堆栈信息的列表
	public DictionaryPool()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<DictionaryPoolDebug>();
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isEditor())
		{
			foreach (var item in mInusedList.Values)
			{
				foreach (ICollection itemList in item)
				{
					string stack = mObjectStack.get(itemList);
					if (stack.isEmpty())
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
		}
	}
	public void clearUnused() { mUnusedList.Clear(); }
	public Dictionary<DictionaryType, HashSet<ICollection>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<DictionaryType, HashSet<ICollection>> getInusedList() { return mInusedList; }
	public Dictionary<DictionaryType, Queue<ICollection>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public ICollection newList(Type keyType, Type valueType, Type listType, string stackTrace, bool onlyOnce = true)
	{
		if (isEditor() && !isMainThread())
		{
			Debug.LogError("只能在主线程中使用DictionaryPool,子线程中请使用DictionaryPoolThread代替");
			return null;
		}

		ICollection list;
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
			if (isEditor())
			{
				int totalCount = 1;
				totalCount += mInusedList.get(type)?.Count ?? 0;
				totalCount += mPersistentInuseList.get(type)?.Count ?? 0;
				if (totalCount % 1000 == 0)
				{
					Debug.Log("创建的Dictionary总数量已经达到:" + totalCount + "个,key:" + keyType + ",value:" + valueType);
				}
			}
		}
		if (isEditor())
		{
			// 标记为已使用
			addInuse(list, type, onlyOnce);
			mObjectStack.Add(list, stackTrace);
		}
		return list;
	}
	public void destroyList<K, V>(ref Dictionary<K, V> list, Type keyType, Type valueType)
	{
		if (list == null)
		{
			return;
		}
		if (isEditor() && !isMainThread())
		{
			Debug.LogError("只能在主线程中使用DictionaryPool,子线程中请使用DictionaryPoolThread代替");
			return;
		}
		list.Clear();
		DictionaryType type = new(keyType, valueType);
		addUnuse(list, type);
		if (isEditor())
		{
			removeInuse(list, type);
			mObjectStack.Remove(list);
		}
		list = null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(ICollection list, DictionaryType type, bool onlyOnce)
	{
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.tryGetOrAddNew(type).Add(list))
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
		var valueList = mUnusedList.tryGetOrAddNew(type);
		if (isEditor() && valueList.Contains(list))
		{
			Debug.LogError("list Object is in Unused list! can not add again!");
			return;
		}
		valueList.Enqueue(list);
	}
}