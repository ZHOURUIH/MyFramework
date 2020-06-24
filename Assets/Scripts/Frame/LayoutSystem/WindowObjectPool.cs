using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowObjectPool<T> : GameBase where T : IPooledWindow, new()
{
	protected List<T> mUsedItemList;
	protected List<T> mUnusedItemList;
	protected LayoutScript mScript;
	protected txUIObject mItemParentInuse;
	protected txUIObject mItemParentUnuse;
	protected txUIObject mTemplate;
	protected string mPreName;
	public WindowObjectPool(LayoutScript script)
	{
		mScript = script;
		mUsedItemList = new List<T>();
		mUnusedItemList = new List<T>();
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
	public void setNode(txUIObject parent, txUIObject template)
	{
		setNode(parent, parent, template);
	}
	public void setNode(txUIObject parentInuse, txUIObject parentUnuse, txUIObject template)
	{
		mItemParentInuse = parentInuse;
		mItemParentUnuse = parentUnuse;
		mTemplate = template;
		mPreName = template.getName();
	}
	public void setItemPreName(string preName)
	{
		mPreName = preName;
	}
	public List<T> getUsedList() { return mUsedItemList; }
	public bool isUsed(T item) { return mUsedItemList.Contains(item); }
	public void checkCapacity(int capacity)
	{
		int needCount = capacity - mUsedItemList.Count;
		for (int i = 0; i < needCount; ++i)
		{
			getOneUnusedItem();
		}
	}
	public T getOneUnusedItem()
	{
		T item;
		if (mUnusedItemList.Count > 0)
		{
			item = mUnusedItemList[mUnusedItemList.Count - 1];
			mUnusedItemList.RemoveAt(mUnusedItemList.Count - 1);
		}
		else
		{
			item = new T();
			item.setScript(mScript);
			item.assignWindow(mItemParentInuse, mTemplate, mPreName + makeID());
			item.init();
		}
		item.reset();
		item.setVisible(true);
		item.setParent(mItemParentInuse);
		mUsedItemList.Add(item);
		return item;
	}
	public void unuseAll()
	{
		foreach (var item in mUsedItemList)
		{
			item.setVisible(false);
			item.setParent(mItemParentUnuse);
		}
		mUnusedItemList.AddRange(mUsedItemList);
		mUsedItemList.Clear();
	}
	public void unuseItem(T item)
	{
		item.setVisible(false);
		item.setParent(mItemParentUnuse);
		mUnusedItemList.Add(item);
		mUsedItemList.Remove(item);
	}
	// 回收一定下标范围的对象,count小于0表示回收从startIndex到结尾的所有对象
	public void unuseRange(int startIndex, int count = -1)
	{
		int usedCount = mUsedItemList.Count;
		if (count < 0)
		{
			count = usedCount - startIndex;
		}
		else
		{
			clampMax(ref count, usedCount - startIndex);
		}
		for (int i = 0; i < count; ++i)
		{
			int index = startIndex + i;
			mUsedItemList[index].setVisible(false);
			mUsedItemList[index].setParent(mItemParentUnuse);
			mUnusedItemList.Add(mUsedItemList[index]);
		}
		mUsedItemList.RemoveRange(startIndex, count);
	}
}