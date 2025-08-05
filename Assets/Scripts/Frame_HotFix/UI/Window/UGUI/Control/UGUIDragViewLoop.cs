using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static FrameUtility;
using static StringUtility;
using static UnityUtility;

// 自定义的循环滚动列表,暂时只支持从上往下纵向排列的滚动列表
public class UGUIDragViewLoop<T, DataType> : WindowObjectUGUI, IDragViewLoop, ICommonUI where T : DragViewItem<DataType> where DataType : ClassObject
{
	protected WindowStructPool<T> mDisplayItemPool;			// 只用作显示的节点列表
	protected List<DataType> mDataList = new();				// 所有节点的数据列表
	protected List<Vector3> mAllItemPos = new();            // 所有节点的位置
	protected myUGUIObject mViewport;						// 视口节点,也就是根节点
	protected myUGUIDragView mContent;                      // 所有节点的父节点,也是主要用于实现滑动的节点
	protected Vector3 mLastRefreshedContentPos;				// 上一次刷新时Content的位置,只有当Content位置改变时才会自动刷新
	protected Vector2 mItemSize;                            // 节点的大小
	protected Vector2 mInterval;                            // 适配后的排列间隔
	protected int mColCount;                                // 节点的排列列数
	protected int mLastStartItemIndex;						// 用于判断是否刷新
	protected bool mNeedUpdateItems;                        // 是否需要调用所有节点的update
	protected static List<DataType> mTempDataList;			// 用于外部临时构造一个数据列表的
	public UGUIDragViewLoop(IWindowObjectOwner parent) : base(parent)
	{
		mDisplayItemPool = new(this);
	}
	protected override void assignWindowInternal()
	{
		base.assignWindowInternal();
		mViewport = mRoot;
		newObject(out mContent, mViewport, "Content");
	}
	public void assignTemplate(string templateName)
	{
		mDisplayItemPool.assignTemplate(mContent, templateName);
	}
	public void initDragView(DRAG_DIRECTION direction = DRAG_DIRECTION.VERTICAL)
	{
		initDragView(direction, Vector2.zero, true);
	}
	// interval是适配前的间隔,默认根据Content的适配方式进行计算
	// refreshDepthIgnoreInactive刷新节点深度时,是否忽略没有激活的节点,默认是忽略的
	public void initDragView(DRAG_DIRECTION direction, Vector2 interval, bool refreshDepthIgnoreInactive)
	{
		if (mContent == null || mViewport == null)
		{
			logError("是否忘了调用UGUIDragViewLoop的assignWindow?");
			return;
		}
		if (mDisplayItemPool.getTemplate() == null)
		{
			logError("是否忘了调用UGUIDragViewLoop的initTemplate?");
			return;
		}
		mContent.initDragView(direction);
		mDisplayItemPool.init();
		mInterval = mContent.tryGetUnityComponent<ScaleAnchor>().getRealScale() * interval;
		mItemSize = mDisplayItemPool.getTemplate().getWindowSize();
		if (mItemSize.x < 1 || mItemSize.y < 1)
		{
			logError("无法获取节点的显示大小");
			return;
		}
		Vector2 viewportSize = mViewport.getWindowSize();
		mColCount = floor((viewportSize.x + mInterval.x) / (mItemSize.x + mInterval.x));
		// 这里+2比较重要,需要算出在任意情况下一定能够显示完整个viewport所需要的数量,最极限的就是中间已经铺了若干个完整的节点,上下留了一些空间,所以需要+2
		int maxDisplayRowCount = floor((viewportSize.y + mInterval.y) / (mItemSize.y + mInterval.y)) + 2;
		int maxDisplayItemCount = mColCount * maxDisplayRowCount;
		for (int i = 0; i < maxDisplayItemCount; ++i)
		{
			mDisplayItemPool.newItem();
		}
		mScript.getLayout().refreshUIDepth(mContent, refreshDepthIgnoreInactive);
	}
	// 设置显示的数据,dataList的元素需要从对象池中创建,会在内部进行回收
	// keepContentTopPos表示是否保持Content的顶部的位置不变,否则将会自动是Content顶部对齐Viewport顶部
	public void setDataList(List<DataType> dataList, bool keepContentTopPos = false)
	{
		UN_CLASS_LIST(mDataList);
		mAllItemPos.Clear();
		if (dataList.count() == 0)
		{
			mContent.setWindowHeight(0);
			updateDisplayItem();
			return;
		}
		mDataList.setRange(dataList);
		float contentWidth = mContent.getWindowSize().x;
		Vector2 curLeftTop = Vector2.zero;
		// 为了避免浮点计算的误差,使用计数的方式来获得行数,如果通过(最大高度 + 高度间隔) / (节点高度 + 高度间隔)的方式计算,可能会由于误差而算错
		int rowCount = dataList.Count > 0 ? 1 : 0;
		for (int i = 0; i < dataList.Count; ++i)
		{
			// 这一排已经放不下了, 放到下一排
			if (curLeftTop.x + mItemSize.x > contentWidth)
			{
				curLeftTop = new(0, curLeftTop.y - mInterval.y - mItemSize.y);
				++rowCount;
			}
			mAllItemPos.add(new(curLeftTop.x + mItemSize.x * 0.5f, curLeftTop.y - mItemSize.y * 0.5f));
			curLeftTop.x += mItemSize.x + mInterval.x;
		}
		float maxHeight = mItemSize.y * rowCount + mInterval.y * (rowCount - 1);
		// 计算完高度, 重新计算所有节点在Content下的位置
		for (int i = 0; i < mAllItemPos.Count; ++i)
		{
			mAllItemPos[i] = new(mAllItemPos[i].x - mContent.getPivot().x * contentWidth, mAllItemPos[i].y + (1 - mContent.getPivot().y) * maxHeight);
		}
		// Content的顶部在Viewport中的位置
		float contentTop = mContent.getPosition().y + mContent.getPivot().y * mContent.getWindowSize().y;
		// 计算出Content节点的总高度
		mContent.setWindowHeight(maxHeight);
		// 只有当top不在Viewport内时才会调整,否则即使调整了,也会自动被拉回
		if (keepContentTopPos && contentTop >= mViewport.getWindowSize().y * 0.5f)
		{
			mContent.setPosition(new(0.0f, contentTop - mContent.getPivot().y * mContent.getWindowSize().y));
		}
		else
		{
			mContent.alignParentTopCenter();
		}
		updateDisplayItem();
	}
	public List<DataType> getDataList() { return mDataList; }
	// 每次设置DateList时,可以先使用getTempDataList获取一个临时列表,然后填充列表,再调用setDataList将填充好的临时列表传进来
	public List<DataType> startSetDataList() 
	{
		mTempDataList ??= new();
		mTempDataList.Clear();
		return mTempDataList; 
	}
	public void setData(int index, DataType data, bool forceRefresh = true)
	{
		UN_CLASS(mDataList[index]);
		mDataList[index] = data;
		if (forceRefresh)
		{
			foreach (T item in mDisplayItemPool.getUsedList())
			{
				if (item.getIndex() == index)
				{
					item.setData(data);
					break;
				}
			}
		}
	}
	public void setDragEnable(bool enable)
	{
		mContent.getDragViewComponent()?.setActive(enable);
	}
	public void setNeedUpdateItems(bool needUpdate) { mNeedUpdateItems = needUpdate; } 
	public void updateDisplayItem(Predicate<DataType> match, DataType newData, bool forceRefresh = true)
	{
		if (mDataList.findIndex(match, out int index))
		{
			setData(index, newData, forceRefresh);
		}
	}
	public void updateDisplayItem(bool forceRefresh = true)
	{
		// 根据当前Content的位置,计算出当前显示的节点
		float viewportTopToContentTop = mContent.getWindowSize().y * (1 - mContent.getPivot().y) + mContent.getPosition().y - mViewport.getWindowSize().y * (1 - mViewport.getPivot().y);
		int topRowIndex = floor(viewportTopToContentTop / (mItemSize.y + mInterval.y));
		int startItemIndex = clampMin(topRowIndex * mColCount);
		if (mLastStartItemIndex != startItemIndex || forceRefresh)
		{
			var displayItemList = mDisplayItemPool.getUsedList();
			for (int i = 0; i < displayItemList.Count; ++i)
			{
				T item = displayItemList[i];
				int dataIndex = i + startItemIndex;
				item.setActive(dataIndex < mDataList.Count);
				item.setIndex(dataIndex);
				if (item.isActive())
				{
					item.getRoot().setName("Item" + IToS(dataIndex));
					item.setData(mDataList[dataIndex]);
					item.setPosition(mAllItemPos[dataIndex]);
				}
			}
		}
		mLastRefreshedContentPos = mContent.getPosition();
		mLastStartItemIndex = startItemIndex;
	}
	public void updateDragView()
	{
		// 这一帧Content移动过位置,就需要刷新显示
		if (!isVectorEqual(mContent.getPosition(), mLastRefreshedContentPos))
		{
			updateDisplayItem(false);
		}
		// 更新
		if (mNeedUpdateItems)
		{
			foreach (T item in mDisplayItemPool.getUsedList())
			{
				item.update();
			}
		}
	}
}