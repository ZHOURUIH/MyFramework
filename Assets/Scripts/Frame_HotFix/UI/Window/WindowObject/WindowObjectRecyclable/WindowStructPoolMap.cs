using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseUtility;

// 可通过Key索引的复杂窗口对象池
public class WindowStructPoolMap<Key, T> : WindowStructPoolBase where T : WindowObjectBase, IRecyclable
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
	public override void init()
	{
		base.init();
		init(mTemplate.getParent(), typeof(T), true);
	}
	public bool hasKey(Key key) { return mUsedItemList.ContainsKey(key); }
	public T getItem(Key key) { return mUsedItemList.get(key); }
	public Dictionary<Key, T> getUsedList() { return mUsedItemList; }
	public T newItem(Key key) { return newItem(mItemParent, key); }
	// 因为添加窗口可能会影响所有窗口的深度值,所以如果有需求,需要在完成添加窗口以后手动调用对象池的refreshUIDepth()来刷新深度
	// 如果是调用了自动排列函数,则会在排列函数中自动调用刷新深度,无需再手动调用
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
			item = createInstance<T>(mObjectType, this);
			item.assignWindow(parent, mTemplate, isEditor() ? mPreName + makeID() : mPreName);
			item.init();
			item.postInit();
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
			item.setActive(false);
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
		item.setActive(false);
		mUnusedItemList.Push(item);
		return true;
	}
	public override int getInUseCount() { return mUsedItemList.count(); }
}