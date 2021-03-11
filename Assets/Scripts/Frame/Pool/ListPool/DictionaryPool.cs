using System;
using System.Collections;
using System.Collections.Generic;

// 仅能在主线程中使用的字典列表池
public class DictionaryPool : FrameSystem
{
	protected Dictionary<DictionaryType, HashSet<IEnumerable>> mPersistentInuseList;	// 持久使用的列表对象
	protected Dictionary<DictionaryType, HashSet<IEnumerable>> mInusedList;			// 仅当前栈帧中使用的列表对象
	protected Dictionary<DictionaryType, HashSet<IEnumerable>> mUnusedList;
	public DictionaryPool()
	{
		mPersistentInuseList = new Dictionary<DictionaryType, HashSet<IEnumerable>>();
		mInusedList = new Dictionary<DictionaryType, HashSet<IEnumerable>>();
		mUnusedList = new Dictionary<DictionaryType, HashSet<IEnumerable>>();
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
			if (item.Value.Count == 0)
			{
				continue;
			}
			foreach (var itemList in item.Value)
			{
				logError("有临时列表对象正在使用中,是否在申请后忘记回收到池中! type:" + Typeof(itemList));
				break;
			}
		}
#endif
	}
	public Dictionary<DictionaryType, HashSet<IEnumerable>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<DictionaryType, HashSet<IEnumerable>> getInusedList() { return mInusedList; }
	public Dictionary<DictionaryType, HashSet<IEnumerable>> getUnusedList() { return mUnusedList; }
	// onlyOnce表示是否仅当作临时列表使用
	public Dictionary<K, V> newList<K, V>(out Dictionary<K, V> obj, bool onlyOnce = true)
	{
		obj = null;
		if (!isMainThread())
		{
			logError("只能在主线程中使用DictionaryPool,子线程中请使用DictionaryPoolThread代替");
			return null;
		}
		var type = new DictionaryType(Typeof<K>(), Typeof<V>());
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(type, out HashSet<IEnumerable> valueList) && valueList.Count > 0)
		{
			foreach(var item in valueList)
			{
				obj = item as Dictionary<K, V>;
				break;
			}
			valueList.Remove(obj);
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			obj = new Dictionary<K, V>();
		}
		// 标记为已使用
		addInuse(obj, onlyOnce);
		return obj;
	}
	public void destroyList<K, V>(Dictionary<K, V> list)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用DictionaryPool,子线程中请使用DictionaryPoolThread代替");
			return;
		}
		list.Clear();
		addUnuse(list);
		removeInuse(list);
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse<K, V>(Dictionary<K, V> list, bool onlyOnce)
	{
		var type = new DictionaryType(Typeof<K>(), Typeof<V>());
		// 加入仅临时使用的列表对象的列表中
		if (onlyOnce)
		{
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
		// 加入持久使用的列表对象的列表中
		else
		{
			if (mPersistentInuseList.TryGetValue(type, out HashSet<IEnumerable> valueList))
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
				mPersistentInuseList.Add(type, valueList);
			}
			valueList.Add(list);
		}
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
		else if(mPersistentInuseList.TryGetValue(type, out HashSet<IEnumerable> persistentValueList))
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