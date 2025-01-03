using System.Collections.Generic;
using static UnityUtility;
using static CSharpUtility;
using static FrameEditorUtility;

// 负责窗口对象池,效率稍微高一些,但是功能会比普通的WindowStructPool少一点,UsedList是无序的
public class WindowStructPoolUnOrder<T> : WindowStructPoolBase where T : WindowObjectBase, IRecycleable
{
	protected HashSet<T> mUsedItemList = new();		// 正在使用的列表
	protected Queue<T> mUnusedItemList = new();		// 未使用列表
	public WindowStructPoolUnOrder(LayoutScript script) : base(script){}
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
	public void init(myUIObject parent, myUIObject template, bool newItemToLast = true)
	{
		init(parent, template, typeof(T), newItemToLast);
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
	// 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用mLayout.refreshUIDepth()来刷新深度
	public T newItem(myUIObject parent)
	{
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
	public void unuseAll()
	{
		foreach (T item in mUsedItemList)
		{
			item.recycle();
			item.setActive(false);
			mUnusedItemList.Enqueue(item);
		}
		mUsedItemList.Clear();
	}
	public void unuseItem(T item)
	{
		if (item == null)
		{
			return;
		}
		if (item.GetType() != mObjectType)
		{
			logError("物体类型与池类型不一致,无法回收,物体类型:" + item.GetType() + ",池类型:" + mObjectType);
			return;
		}
		if (item.getAssignID() <= 0)
		{
			logError("物体已经回收过了,无法重复回收,type:" + item.GetType());
			return;
		}
		if (!mUsedItemList.Remove(item))
		{
			logError("此窗口物体不属于当前窗口物体池,无法回收,type:" + item.GetType());
			return;
		}
		item.recycle();
		item.setActive(false);
		mUnusedItemList.Enqueue(item);
	}
}