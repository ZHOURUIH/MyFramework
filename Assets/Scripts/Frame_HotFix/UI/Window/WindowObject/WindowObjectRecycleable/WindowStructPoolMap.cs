﻿using System.Collections.Generic;
using static UnityUtility;
using static CSharpUtility;
using static FrameBaseUtility;

// 可通过Key索引的复杂窗口对象池
public class WindowStructPoolMap<Key, T> : WindowStructPoolBase where T : WindowObjectBase, IRecycleable
{
	protected Dictionary<Key, T> mUsedItemList = new(); // 正在使用的列表
	protected Stack<T> mUnusedItemList = new();         // 未使用列表
	public WindowStructPoolMap(IWindowObjectOwner parent) : base(parent) { }
	public override void destroy()
	{
		base.destroy();
		unuseAll();
		foreach (T item in mUnusedItemList)
		{
			item.destroy();
		}
		mUnusedItemList.Clear();
	}
	public void init()
	{
		init(mTemplate.getParent(), typeof(T), true);
	}
	public void init(myUGUIObject parent)
	{
		init(parent, typeof(T), true);
	}
	public void init(bool newItemToLast)
	{
		init(mTemplate.getParent(), typeof(T), newItemToLast);
	}
	public void init1(myUGUIObject parent, bool newItemToLast)
	{
		init(parent, typeof(T), newItemToLast);
	}
	public bool hasKey(Key key) { return mUsedItemList.ContainsKey(key); }
	public T getItem(Key key) { return mUsedItemList.get(key); }
	public Dictionary<Key, T> getUsedList() { return mUsedItemList; }
	public T newItem(Key key) { return newItem(mItemParent, key); }
	// 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用mLayout.refreshUIDepth()来刷新深度
	public T newItem(myUGUIObject parent, Key key)
	{
		if (!mInited)
		{
			logError("还未执行初始化,不能newItem");
			return null;
		}
		T item;
		if (mUnusedItemList.Count > 0)
		{
			item = mUnusedItemList.Pop();
			item.setParent(parent, false);
		}
		else
		{
			item = createInstance<T>(mObjectType, mScript);
			item.assignWindow(parent, mTemplate, isEditor() ? mPreName + makeID() : mPreName);
			item.init();
		}
		item.setAssignID(++mAssignIDSeed);
		item.reset();
		item.setActive(true);
		if (mNewItemMoveToLast)
		{
			item.setAsLastSibling(false);
		}
		mUsedItemList.Add(key, item);
		return item;
	}
	public override void unuseAll()
	{
		foreach (T item in mUsedItemList.Values)
		{
			item.recycle();
			if (item.isActive())
			{
				item.setActive(false);
			}
			mUnusedItemList.Push(item);
		}
		mUsedItemList.Clear();
	}
	public bool unuseItem(Key key, bool showError = true)
	{
		if (key == null)
		{
			return false;
		}
		if (!mUsedItemList.Remove(key, out T item))
		{
			if (showError)
			{
				logError("此窗口物体不属于当前窗口物体池,无法回收,key:" + key + ", type:" + typeof(T));
			}
			return false;
		}
		item.recycle();
		if (item.isActive())
		{
			item.setActive(false);
		}
		mUnusedItemList.Push(item);
		return true;
	}
}