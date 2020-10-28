using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WindowObjectPool<T> : FrameBase where T : PooledWindow
{
	protected List<T> mUsedItemList;
	protected List<T> mUnusedItemList;
	protected LayoutScript mScript;
	protected myUIObject mItemParentInuse;
	protected myUIObject mItemParentUnuse;
	protected myUIObject mTemplate;
	protected Type mObjectType;
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
	// 当使用ILRuntime热更时,T类型很可能就是一个热更工程中的类,在主工程中无法根据T获得真正的类型,所以只能由外部传进来类型
	public void setNode(myUIObject parent, myUIObject template, Type objectType)
	{
		setNode(parent, parent, template, objectType);
	}
	public void setNode(myUIObject parentInuse, myUIObject parentUnuse, myUIObject template, Type objectType)
	{
		mItemParentInuse = parentInuse;
		mItemParentUnuse = parentUnuse;
		mTemplate = template;
		mPreName = template?.getName();
		mObjectType = objectType;
#if UNITY_EDITOR
		if(mObjectType != null)
		{
			bool hasNoneParamConstructor = false;
			ConstructorInfo[] info = mObjectType.GetConstructors();
			if (info != null)
			{
				foreach (var item in info)
				{
					if (item.GetParameters().Length == 0)
					{
						hasNoneParamConstructor = true;
					}
				}
			}
			if (!hasNoneParamConstructor && info != null && info.Length > 0)
			{
				logError("WindowObjectPool需要有无参构造的类作为节点类型, Type:" + mObjectType.Name);
			}
		}
#endif
	}
	public myUIObject getInUseParent() { return mItemParentInuse; }
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
	// 将source从sourcePool中移动到当前池中
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
			item = createInstance<T>(mObjectType);
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