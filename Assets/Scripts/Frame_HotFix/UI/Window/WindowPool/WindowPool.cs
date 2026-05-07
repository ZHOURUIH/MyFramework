using System.Collections.Generic;
using UnityEngine;
using static FrameBaseUtility;
using static MathUtility;
using static UnityUtility;

[CommonWindowPool]
public class WindowPool<T> : WindowPoolBase where T : myUGUIObject, new()
{
	protected List<T> mUnusedList = new();			// 未使用的窗口列表
	protected List<T> mInusedList = new();          // 正在使用的窗口列表,因为需要保证物体的存储顺序,所以使用List
	protected myUGUIObject mParent;                 // 创建出的窗口的默认父节点
	protected T mTemplate;							// 窗口模板
	public WindowPool(IWindowObjectOwner parent) : base(parent){}
	public void assignTemplate(myUGUIObject parent, string name)
	{
		mScript.newObject(out mTemplate, parent, name);
	}
	public void assignTemplate(T template)
	{
		mTemplate = template;
	}
	public override void init()
	{
		base.init();
		mParent = mTemplate.getParent();
		mTemplate.setActive(false);
	}
	public void newItemListHorizontal(int count)
	{
		unuseAll();
		newItem(count);
		autoGridHorizontal();
	}
	public void newItemListVertical(int count)
	{
		unuseAll();
		newItem(count);
		autoGridVertical();
	}
	public void newItem(int count)
	{
		if (mParent == null)
		{
			logError("窗口池的父节点为空, 是否忘了调用init?");
			return;
		}
		for (int i = 0; i < count; ++i)
		{
			newItem(mParent, null);
		}
	}
	public T newItemIf(bool condition)
	{
		if (condition)
		{
			return newItem();
		}
		return null;
	}
	public T newItem(string name = null)
	{
		if (mParent == null)
		{
			logError("窗口池的父节点为空, 是否忘了调用init?");
			return null;
		}
		return newItem(mParent, name);
	}
	// 新创建的窗口会自动移动到父节点的最后一个子节点的位置
	public T newItem(myUGUIObject parent, string name = null)
	{
		name ??= mTemplate.getName();
		T window = null;
		// 从未使用列表中获取
		if (mUnusedList.Count > 0)
		{
			window = mUnusedList.popBack();
			window.setParent(parent, false);
		}
		// 未使用列表中没有就创建新窗口
		if (window == null)
		{
			mScript.cloneObject(out window, parent, mTemplate, name, true);
		}
		window.setActive(true);
		window.setName(name);
		if (mMoveToLast)
		{
			window.setAsLastSibling(mAutoRefreshDepth);
		}
		return mInusedList.add(window);
	}
	public override void unuseAll()
	{
		foreach (T item in mInusedList)
		{
			if (mDestroyCallback != null)
			{
				mDestroyCallback(item);
			}
			else
			{
				item.setActive(false);
			}
			mUnusedList.Add(item);
		}
		mInusedList.Clear();
	}
	public bool tryUnuseItem(T window)
	{
		if (!mInusedList.contains(window))
		{
			return false;
		}
		return unuseItem(window);
	}
	public bool unuseItem(T window)
	{
		if (window == null)
		{
			return false;
		}
		if (isEditor() && mUnusedList.Contains(window))
		{
			logError("重复回收窗口,name:" + window.getName());
			return false;
		}
		if (!mInusedList.Remove(window))
		{
			logError("要回收的窗口不属于当前对象池, name:" + window.getName());
			return false;
		}
		mUnusedList.Add(window);
		if (mDestroyCallback != null)
		{
			mDestroyCallback(window);
		}
		else
		{
			window.setActive(false);
		}
		return true;
	}
	public void unuseIndex(int index)
	{
		unuseRange(index, 1);
	}
	// 回收一定下标范围的对象,count小于0表示回收从startIndex到结尾的所有对象
	public void unuseRange(int startIndex, int count = -1)
	{
		int usedCount = mInusedList.Count;
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
			mUnusedList.add(mInusedList[startIndex + i]).setActive(false);
		}
		mInusedList.RemoveRange(startIndex, count);
	}
	public void checkCapacity(int capacity)
	{
		int needCount = capacity - mInusedList.Count;
		for (int i = 0; i < needCount; ++i)
		{
			newItem();
		}
	}
	public override int getInUseCount() { return mInusedList.Count; }
	public List<T> getWindowList() { return mInusedList; }
	public void autoGridHorizontal()
	{
		WidgetUtility.autoGridHorizontal(mParent, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridHorizontal(float interval)
	{
		WidgetUtility.autoGridHorizontal(mParent, true, true, interval, true, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridHorizontal(bool keepLeftSide)
	{
		WidgetUtility.autoGridHorizontal(mParent, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, keepLeftSide);
	}
	public void autoGridHorizontal(float interval, bool keepLeftSide)
	{
		WidgetUtility.autoGridHorizontal(mParent, true, true, interval, true, 0.0f, 0.0f, 0.0f, keepLeftSide);
	}
	public void autoGridHorizontal(bool autoRefreshUIDepth, bool refreshIgnoreInactive)
	{
		WidgetUtility.autoGridHorizontal(mParent, autoRefreshUIDepth, refreshIgnoreInactive, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
	}
	// 自动排列一个节点下的所有子节点的位置,从左往右紧密排列,并且不改变子节点的大小,keepLeftSide为true表示改变大小后保持父节点的左边界位置不变,false表示保持右边界位置不变
	public void autoGridHorizontal(bool autoRefreshUIDepth, bool refreshIgnoreInactive, float interval, bool changeRootPosSize = true, float minWidth = 0.0f, float extraLeftWidth = 0.0f, float extraRightWidth = 0.0f, bool keepLeftSide = true)
	{
		WidgetUtility.autoGridHorizontal(mParent, autoRefreshUIDepth, refreshIgnoreInactive, interval, changeRootPosSize, minWidth, extraLeftWidth, extraRightWidth, keepLeftSide);
	}
	// 将自动排列的方法直接写到对象池中,方便使用
	// 一般对于排列是有两个地方有需求,1:滑动列表,2:不可滑动,但是有多个相似节点需要动态创建后排列好
	// 一般滑动列表都会需要在排列好后将边对齐父节点,而对于不可滑动的列表,需求会根据实际情况有不同
	public void autoGridHorizontalForDragView()
	{
		WidgetUtility.autoGridHorizontal(mParent, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
		mParent.setLeftCenterToParentLeftCenter();
	}
	public void autoGridHorizontalCenter()
	{
		WidgetUtility.autoGridHorizontalCenter(mParent, true, true, 0.0f);
	}
	public void autoGridVertical()
	{
		WidgetUtility.autoGridVertical(mParent, true, true, 0.0f, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridVertical(bool keepTopSide)
	{
		WidgetUtility.autoGridVertical(mParent, true, true, 0.0f, 0.0f, 0.0f, 0.0f, keepTopSide);
	}
	public void autoGridVertical(float interval)
	{
		WidgetUtility.autoGridVertical(mParent, true, true, interval, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridVertical(float interval, bool keepTopSide)
	{
		WidgetUtility.autoGridVertical(mParent, true, true, interval, 0.0f, 0.0f, 0.0f, keepTopSide);
	}
	public void autoGridVertical(bool autoRefreshUIDepth, bool refreshIgnoreInactive)
	{
		WidgetUtility.autoGridVertical(mParent, autoRefreshUIDepth, refreshIgnoreInactive, 0.0f, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridVertical(bool autoRefreshUIDepth, bool refreshIgnoreInactive, float interval, float minHeight = 0.0f, float extraTopHeight = 0.0f, float extraBottomHeight = 0.0f, bool keepTopSide = true)
	{
		WidgetUtility.autoGridVertical(mParent, autoRefreshUIDepth, refreshIgnoreInactive, interval, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridVerticalForDragView()
	{
		WidgetUtility.autoGridVertical(mParent, true, true, 0.0f, 0.0f, 0.0f, 0.0f, true);
		mParent.setTopCenterToParentTopCenter();
	}
	public void autoGridVerticalForDragView(float interval)
	{
		WidgetUtility.autoGridVertical(mParent, true, true, interval, 0.0f, 0.0f, 0.0f, true);
		mParent.setTopCenterToParentTopCenter();
	}
	public void autoGridForDragView()
	{
		WidgetUtility.autoGrid(mParent, mTemplate.getSize(), Vector2.zero, true, true, true, HORIZONTAL_DIRECTION.LEFT, VERTICAL_DIRECTION.TOP);
		// 根据排列后的子节点,计算出父节点的高度
		WidgetUtility.setWindowBestHeight(mParent, true, true);
		mParent.setTopCenterToParentTopCenter();
	}
	public void autoGrid()
	{
		WidgetUtility.autoGrid(mParent, mTemplate.getSize(), Vector2.zero, true, true, true, HORIZONTAL_DIRECTION.LEFT, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(bool autoRefreshUIDepth)
	{
		WidgetUtility.autoGrid(mParent, mTemplate.getSize(), Vector2.zero, autoRefreshUIDepth, true, true, HORIZONTAL_DIRECTION.LEFT, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(Vector2 interval)
	{
		WidgetUtility.autoGrid(mParent, mTemplate.getSize(), interval, true, true, true, HORIZONTAL_DIRECTION.LEFT, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(HORIZONTAL_DIRECTION horizontal)
	{
		WidgetUtility.autoGrid(mParent, mTemplate.getSize(), Vector2.zero, true, true, true, horizontal, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(VERTICAL_DIRECTION vertical)
	{
		WidgetUtility.autoGrid(mParent, mTemplate.getSize(), Vector2.zero, true, true, true, HORIZONTAL_DIRECTION.LEFT, vertical);
	}
	public void autoGrid(Vector2 interval, HORIZONTAL_DIRECTION horizontal)
	{
		WidgetUtility.autoGrid(mParent, mTemplate.getSize(), interval, true, true, true, horizontal, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(Vector2 interval, VERTICAL_DIRECTION vertical)
	{
		WidgetUtility.autoGrid(mParent, mTemplate.getSize(), interval, true, true, true, HORIZONTAL_DIRECTION.LEFT, vertical);
	}
}