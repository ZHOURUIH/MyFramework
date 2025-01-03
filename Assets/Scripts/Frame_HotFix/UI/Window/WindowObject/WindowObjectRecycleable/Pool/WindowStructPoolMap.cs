using System.Collections.Generic;
using static UnityUtility;
using static CSharpUtility;
using static FrameEditorUtility;

// 可通过Key索引的复杂窗口对象池
public class WindowStructPoolMap<Key, T> : WindowStructPoolBase where T : WindowObjectBase, IRecycleable
{
	protected Dictionary<Key, T> mUsedItemList = new(); // 正在使用的列表
	protected Stack<T> mUnusedItemList = new();         // 未使用列表
	public WindowStructPoolMap(LayoutScript script):
		base(script){}
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
	public bool hasKey(Key key) { return mUsedItemList.ContainsKey(key); }
	public T getItem(Key key) { return mUsedItemList.get(key); }
	public Dictionary<Key, T> getUsedList() { return mUsedItemList; }
	public T newItem(Key key) { return newItem(mItemParent, key); }
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
	public void unuseAll()
	{
		foreach (T item in mUsedItemList.Values)
		{
			item.recycle();
			item.setActive(false);
			mUnusedItemList.Push(item);
		}
		mUsedItemList.Clear();
	}
	public void unuseItem(Key key, bool showError = true)
	{
		if (key == null)
		{
			return;
		}
		if (!mUsedItemList.Remove(key, out T item))
		{
			if (showError)
			{
				logError("此窗口物体不属于当前窗口物体池,无法回收,type:" + item.GetType());
			}
			return;
		}
		item.recycle();
		item.setActive(false);
		mUnusedItemList.Push(item);
	}
}