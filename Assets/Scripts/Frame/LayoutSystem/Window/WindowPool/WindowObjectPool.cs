using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowObjectPool<T> : GameBase where T : PooledWindow, new()
{
	protected List<T> mUsedItemList;
	protected List<T> mUnusedItemList;
	protected LayoutScript mScript;
	protected txUIObject mItemParentInuse;
	protected txUIObject mItemParentUnuse;
	protected txUIObject mTemplate;
	protected string mPreName;
	protected int mAssignIDSeed;		// 分配ID种子,用于设置唯一分配ID,只会递增,不会减少
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
	public void setItemPreName(string preName) { mPreName = preName; }
	public List<T> getUsedList() { return mUsedItemList; }
	public bool isUsed(T item) { return mUsedItemList.Contains(item); }
	public void checkCapacity(int capacity)
	{
		int needCount = capacity - mUsedItemList.Count;
		for (int i = 0; i < needCount; ++i)
		{
			newItem();
		}
	}
	public void moveItem(WindowObjectPool<T> sourcePool, T source, bool inUsed)
	{
		// 从原来的池中移除
		sourcePool.mUsedItemList.Remove(source);
		sourcePool.mUnusedItemList.Remove(source);
		// 加入新的池中
		if (inUsed)
		{
			mUsedItemList.Add(source);
			source.setParent(mItemParentInuse);
		}
		else
		{
			mUnusedItemList.Add(source);
			source.setParent(mItemParentUnuse);
		}
		// 检查分配ID种子,确保后面池中的已分配ID一定小于分配ID种子
		mAssignIDSeed = getMax(source.getAssignID(), mAssignIDSeed);
	}
	// 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用mLayout.notifyObjectOrderChanged()来刷新深度
	public T newItem()
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
		item.setAssignID(++mAssignIDSeed);
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
			item.recycle();
			item.setVisible(false);
			item.setParent(mItemParentUnuse);
			item.setAssignID(-1);
		}
		mUnusedItemList.AddRange(mUsedItemList);
		mUsedItemList.Clear();
	}
	public void unuseItem(T item)
	{
		item.recycle();
		item.setVisible(false);
		item.setParent(mItemParentUnuse);
		item.setAssignID(-1);
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