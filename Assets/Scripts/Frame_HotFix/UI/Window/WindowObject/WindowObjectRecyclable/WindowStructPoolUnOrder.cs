using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseUtility;

// 负责窗口对象池,效率稍微高一些,但是功能会比普通的WindowStructPool少一点,UsedList是无序的
public class WindowStructPoolUnOrder<T> : WindowStructPoolBase where T : WindowObjectBase, IRecyclable
{
	protected HashSet<T> mUsedItemList = new();		// 正在使用的列表
	protected Queue<T> mUnusedItemList = new();		// 未使用列表
	public WindowStructPoolUnOrder(IWindowObjectOwner parent) : base(parent) { }
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
	public void init(bool newItemToLast)
	{
		init(mTemplate.getParent(), typeof(T), newItemToLast);
	}
	public void init(myUGUIObject parent, bool newItemToLast)
	{
		init(parent, typeof(T), newItemToLast);
	}
	public HashSet<T> getUsedList() { return mUsedItemList; }
	public void checkCapacity(int capacity)
	{
		int needCount = capacity - mUsedItemList.Count;
		for (int i = 0; i < needCount; ++i)
		{
			newItem();
		}
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
	public T newItem()
	{
		return newItem(mItemParent);
	}
	// 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用对象池的refreshUIDepth()来刷新深度
	// 如果是调用了自动排列函数,则会在排列函数中自动调用刷新深度,无需再手动调用
	public T newItem(myUGUIObject parent)
	{
		if (!mInited)
		{
			logError("还未执行初始化,不能newItem");
			return null;
		}
		T item;
		if (mUnusedItemList.Count > 0)
		{
			item = mUnusedItemList.Dequeue();
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
		foreach (T item in mUsedItemList)
		{
			item.recycle();
			item.setActive(false);
			mUnusedItemList.Enqueue(item);
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
		mUnusedItemList.Enqueue(item);
		return true;
	}
}