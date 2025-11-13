using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static MathUtility;
using static FrameBaseUtility;

// 负责窗口对象池,UsedList是有序的
public class WindowStructPool<T> : WindowStructPoolBase where T : WindowObjectBase, IRecycleable
{
	protected HashSet<T> mUnusedItemList = new();	// 未使用列表
	protected List<T> mUsedItemList = new();		// 正在使用的列表
	public WindowStructPool(IWindowObjectOwner parent) : base(parent) { }
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
	// 这个init需要在界面中手动调用,因为参数跟默认的init不一样
	public void init(bool newItemToLast)
	{
		init(mTemplate.getParent(), typeof(T), newItemToLast);
	}
	public void init(myUGUIObject parent, bool newItemToLast)
	{
		init(parent, typeof(T), newItemToLast);
	}
	public void init(Type type, bool newItemToLast)
	{
		init(mTemplate.getParent(), type, newItemToLast);
	}
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
	// 将source从sourcePool中移动到当前池中,inUsed表示移动到当前池以后是处于正在使用的状态还是未使用状态
	public void moveItem(WindowStructPool<T> sourcePool, T source, bool inUsed, bool moveParent = true)
	{
		// 从原来的池中移除
		sourcePool.mUsedItemList.Remove(source);
		sourcePool.mUnusedItemList.Remove(source);
		// 加入新的池中
		if (inUsed)
		{
			mUsedItemList.Add(source);
		}
		else
		{
			mUnusedItemList.Add(source);
		}
		if (moveParent)
		{
			source.setParent(mItemParent);
		}
		// 检查分配ID种子,确保后面池中的已分配ID一定小于分配ID种子
		mAssignIDSeed = getMax(source.getAssignID(), mAssignIDSeed);
	}
	public void newItem(int count)
	{
		for (int i = 0; i < count; ++i)
		{
			newItem(mItemParent);
		}
	}
	public T newItem(out T item)
	{
		return item = newItem(mItemParent);
	}
	// 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用对象池的refreshUIDepth()来刷新深度
	// 如果是调用了自动排列函数,则会在排列函数中自动调用刷新深度,无需再手动调用
	public T newItem(myUGUIObject parent = null)
	{
		if (!mInited)
		{
			logError("还未执行初始化,不能newItem");
			return null;
		}
		parent ??= mItemParent;
		T item;
		if (mUnusedItemList.Count > 0)
		{
			item = mUnusedItemList.popFirst();
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
		mUsedItemList.Add(item);
		return item;
	}
	public override void unuseAll()
	{
		if (mUsedItemList.Count == 0)
		{
			return;
		}
		foreach (T item in mUsedItemList)
		{
			item.recycle();
			item.setActive(false);
			mUnusedItemList.Add(item);
		}
		mUsedItemList.Clear();
	}
	public bool unuseItem(T item, bool showError = true)
	{
		if (item == null)
		{
			return false;
		}
		if (item.GetType() != mObjectType)
		{
			logError("物体类型与池类型不一致,无法回收,物体类型:" + item.GetType() + ",池类型:" + mObjectType);
			return false;
		}
		if (item.getAssignID() <= 0)
		{
			logError("物体已经回收过了,无法重复回收,type:" + item.GetType());
			return false;
		}
		if (!mUsedItemList.Remove(item))
		{
			if (showError)
			{
				logError("此窗口物体不属于当前窗口物体池,无法回收,type:" + item.GetType());
			}
			return false;
		}
		item.recycle();
		item.setActive(false);
		mUnusedItemList.Add(item);
		return true;
	}
	public void unuseIndex(int index)
	{
		unuseRange(index, 1);
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
			T item = mUsedItemList[startIndex + i];
			item.recycle();
			item.setActive(false);
			mUnusedItemList.Add(item);
		}
		mUsedItemList.RemoveRange(startIndex, count);
	}
}