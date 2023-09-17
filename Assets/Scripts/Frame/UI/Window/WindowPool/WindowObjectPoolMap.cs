using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;

// 可通过Key索引的复杂窗口对象池
public class WindowObjectPoolMap<Key, T> where T : PooledWindow
{
	protected Dictionary<Key, T> mUsedItemList;	// 未使用列表
	protected Stack<T> mUnusedItemList;			// 正在使用的列表
	protected LayoutScript mScript;				// 所属布局脚本
	protected myUIObject mItemParent;			// 创建节点时默认的父节点
	protected myUIObject mTemplate;				// 创建节点时使用的模板
	protected Type mValueType;					// Value的类型,用于创建Value实例
	protected string mPreName;					// 创建物体的名字前缀
	protected int mAssignIDSeed;				// 分配ID种子,用于设置唯一分配ID,只会递增,不会减少
	protected bool mNewItemMoveToLast;			// 新创建的物体是否需要放到父节点的最后,也就是是否在意其渲染顺序
	public WindowObjectPoolMap(LayoutScript script)
	{
		mScript = script;
		mUsedItemList = new Dictionary<Key, T>();
		mUnusedItemList = new Stack<T>();
		mNewItemMoveToLast = true;
	}
	public void destroy()
	{
		unuseAll();
		foreach(var item in mUnusedItemList)
		{
			item.destroy();
		}
		mUnusedItemList.Clear();
	}
	public void init(myUIObject parentInuse, myUIObject template, Type valueType, bool newItemToLast = true)
	{
		mItemParent = parentInuse;
		mTemplate = template;
		mValueType = valueType;
		mNewItemMoveToLast = newItemToLast;
		mPreName = template.getName();
#if UNITY_EDITOR
		if(mValueType != null)
		{
			ConstructorInfo[] info = mValueType.GetConstructors();
			if (info != null)
			{
				bool hasNoneParamConstructor = false;
				int count = info.Length;
				for(int i= 0; i < count; ++i)
				{
					if (info[i].GetParameters().Length == 0)
					{
						hasNoneParamConstructor = true;
					}
				}
				if (!hasNoneParamConstructor && count > 0)
				{
					logError("WindowObjectPoolMap需要有无参构造的类作为节点类型, Type:" + mValueType.Name);
				}
			}
		}
#endif
	}
	public bool hasKey(Key key) { return mUsedItemList.ContainsKey(key); }
	public T getItem(Key key) 
	{
		if (mUsedItemList.TryGetValue(key, out T item))
		{
			return item;
		}
		return default;
	}
	public myUIObject getInUseParent() { return mItemParent; }
	public Dictionary<Key, T> getUseList() { return mUsedItemList; }
	public void setItemPreName(string preName) { mPreName = preName; }
	public T newItem(Key key)
	{
		return newItem(mItemParent, key);
	}
	// 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用mLayout.refreshUIDepth()来刷新深度
	public T newItem(myUIObject parent, Key key)
	{
		T item;
		if (mUnusedItemList.Count > 0)
		{
			item = mUnusedItemList.Pop();
			item.setParent(parent, false);
		}
		else
		{
			item = createInstance<T>(mValueType);
			item.setScript(mScript);
#if UNITY_EDITOR
			string name = mPreName + makeID();
#else
			string name = mPreName;
#endif
			item.assignWindow(parent, mTemplate, name);
			item.init();
		}
		item.setAssignID(++mAssignIDSeed);
		item.reset();
		item.setVisible(true);
		if (mNewItemMoveToLast)
		{
			item.setAsLastSibling(false);
		}
		mUsedItemList.Add(key, item);
		return item;
	}
	public void unuseAll()
	{
		foreach (var item in mUsedItemList)
		{
			item.Value.recycle();
			item.Value.setVisible(false);
			item.Value.setAssignID(-1);
			mUnusedItemList.Push(item.Value);
		}
		mUsedItemList.Clear();
	}
	public void unuseItem(Key key, bool showError = true)
	{
		if (key == null)
		{
			return;
		}
		if (!mUsedItemList.TryGetValue(key, out T item))
		{
			if(showError)
			{
				logError("此窗口物体不属于当前窗口物体池,无法回收");
			}
			return;
		}
		item.recycle();
		item.setVisible(false);
		item.setAssignID(-1);
		mUnusedItemList.Push(item);
		mUsedItemList.Remove(key);
	}
}