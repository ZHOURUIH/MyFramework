using System;
using UnityEngine;

// 窗口对象池基类
public class WindowStructPoolBase
{
	protected WindowObjectBase mOwnerObject;	// 如果是WindowObject中创建的对象池,则会存储此WindowObject
	protected LayoutScript mScript;				// 所属的布局脚本
	protected myUGUIObject mItemParent;			// 创建节点时默认的父节点
	protected myUGUIObject mTemplate;			// 创建节点时使用的模板
	protected string mPreName;					// 创建物体的名字前缀
	protected Type mObjectType;					// 物体类型
	protected static long mAssignIDSeed;		// 分配ID种子,用于设置唯一分配ID,只会递增,不会减少
	protected bool mNewItemMoveToLast;			// 新创建的物体是否需要放到父节点的最后,也就是是否在意其渲染顺序
	protected bool mInited;						// 是否已经初始化
	public WindowStructPoolBase(IWindowObjectOwner parent)
	{
		if (parent is WindowObjectBase objBase)
		{
			mScript = objBase.getScript();
			mOwnerObject = objBase;
		}
		else if (parent is LayoutScript script)
		{
			mScript = script;
			mOwnerObject = null;
		}
		mOwnerObject?.addWindowPool(this);
		mScript.addWindowStructPool(this);
		mNewItemMoveToLast = true;
	}
	public virtual void destroy() {}
	public void init(myUGUIObject parent, Type objectType, bool newItemToLast = true)
	{
		mItemParent = parent;
		mNewItemMoveToLast = newItemToLast;
		mPreName = mTemplate?.getName();
		mObjectType = objectType;
		mTemplate.setActive(false);
		mInited = true;
	}
	public void assignTemplate(myUGUIObject parent, string name)
	{
		mScript.newObject(out mTemplate, parent, name);
	}
	public void assignTemplate<T>(myUGUIObject parent, string name) where T : myUGUIObject, new()
	{
		mScript.newObject(out T obj, parent, name);
		mTemplate = obj;
	}
	public void assignTemplate(string name)
	{
		mScript.newObject(out mTemplate, name);
	}
	public void assignTemplate(myUGUIObject template)
	{
		mTemplate = template;
	}
	public myUGUIObject getInUseParent() { return mItemParent; }
	public void setActive(bool active) { mItemParent.setActive(active); }
	public myUGUIObject getTemplate() { return mTemplate; }
	public void setItemPreName(string preName) { mPreName = preName; }
	public virtual void unuseAll() { }
	public bool isRootPool() { return mOwnerObject == null; }
	public void refreshUIDepth(bool ignoreInactive = true)
	{
		mScript.getLayout().refreshUIDepth(mItemParent, ignoreInactive);
	}
	// 将自动排列的方法直接写到对象池中,方便使用
	public void autoGridFixedRootWidth(bool autoRefreshUIDepth = true, bool refreshIgnoreInactive = true, CORNER startCorner = CORNER.LEFT_TOP)
	{
		WidgetUtility.autoGridFixedRootWidth(mItemParent, mTemplate.getWindowSize(), Vector2.zero, autoRefreshUIDepth, refreshIgnoreInactive, startCorner);
	}
	// 保持父节点的宽度,从指定角开始横向排列子节点,并且会改变子节点的大小,gridSize是子节点的大小,startCorner是开始排列的位置
	public void autoGridFixedRootWidth(Vector2 interval, bool autoRefreshUIDepth = true, bool refreshIgnoreInactive = true, CORNER startCorner = CORNER.LEFT_TOP)
	{
		WidgetUtility.autoGridFixedRootWidth(mItemParent, mTemplate.getWindowSize(), interval, autoRefreshUIDepth, refreshIgnoreInactive, startCorner);
	}
	public void autoGridFixedRootHeight(bool autoRefreshUIDepth = true, CORNER startCorner = CORNER.LEFT_TOP)
	{
		WidgetUtility.autoGridFixedRootHeight(mItemParent, mTemplate.getWindowSize(), Vector2.zero, autoRefreshUIDepth, startCorner);
	}
	// 保持父节点的高度,从指定角开始纵向排列子节点,并且会改变子节点的大小,gridSize是子节点的大小,startCorner是开始排列的位置
	public void autoGridFixedRootHeight(Vector2 interval, bool autoRefreshUIDepth = true, CORNER startCorner = CORNER.LEFT_TOP)
	{
		WidgetUtility.autoGridFixedRootHeight(mItemParent, mTemplate.getWindowSize(), interval, autoRefreshUIDepth, startCorner);
	}
	public void autoGridHorizontal()
	{
		WidgetUtility.autoGridHorizontal(mItemParent, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridHorizontal(float interval)
	{
		WidgetUtility.autoGridHorizontal(mItemParent, true, true, interval, true, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridHorizontal(bool keepLeftSide)
	{
		WidgetUtility.autoGridHorizontal(mItemParent, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, keepLeftSide);
	}
	public void autoGridHorizontal(float interval, bool keepLeftSide)
	{
		WidgetUtility.autoGridHorizontal(mItemParent, true, true, interval, true, 0.0f, 0.0f, 0.0f, keepLeftSide);
	}
	public void autoGridHorizontal(bool autoRefreshUIDepth, bool refreshIgnoreInactive)
	{
		WidgetUtility.autoGridHorizontal(mItemParent, autoRefreshUIDepth, refreshIgnoreInactive, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
	}
	// 自动排列一个节点下的所有子节点的位置,从左往右紧密排列,并且不改变子节点的大小,keepLeftSide为true表示改变大小后保持父节点的左边界位置不变,false表示保持右边界位置不变
	public void autoGridHorizontal(bool autoRefreshUIDepth, bool refreshIgnoreInactive, float interval, bool changeRootPosSize = true, float minWidth = 0.0f, float extraLeftWidth = 0.0f, float extraRightWidth = 0.0f, bool keepLeftSide = true)
	{
		WidgetUtility.autoGridHorizontal(mItemParent, autoRefreshUIDepth, refreshIgnoreInactive, interval, changeRootPosSize, minWidth, extraLeftWidth, extraRightWidth, keepLeftSide);
	}
	public void autoGridVertical()
	{
		WidgetUtility.autoGridVertical(mItemParent, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridVertical(bool keepTopSide)
	{
		WidgetUtility.autoGridVertical(mItemParent, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, keepTopSide);
	}
	public void autoGridVertical(float interval)
	{
		WidgetUtility.autoGridVertical(mItemParent, true, true, interval, true, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridVertical(bool autoRefreshUIDepth, bool refreshIgnoreInactive)
	{
		WidgetUtility.autoGridVertical(mItemParent, autoRefreshUIDepth, refreshIgnoreInactive, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
	}
	// 自动排列一个节点下的所有子节点的位置,从上往下紧密排列,并且不改变子节点的大小
	public void autoGridVertical(bool autoRefreshUIDepth, bool refreshIgnoreInactive, float interval, bool changeRootPosSize = true, float minHeight = 0.0f, float extraTopHeight = 0.0f, float extraBottomHeight = 0.0f, bool keepTopSide = true)
	{
		WidgetUtility.autoGridVertical(mItemParent, autoRefreshUIDepth, refreshIgnoreInactive, interval, changeRootPosSize, minHeight, extraTopHeight, extraBottomHeight, keepTopSide);
	}
	public void autoGrid()
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getWindowSize(), Vector2.zero, true, true, HORIZONTAL_DIRECTION.CENTER, VERTICAL_DIRECTION.CENTER);
	}
	public void autoGrid(bool autoRefreshUIDepth)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getWindowSize(), Vector2.zero, autoRefreshUIDepth, true, HORIZONTAL_DIRECTION.CENTER, VERTICAL_DIRECTION.CENTER);
	}
	public void autoGrid(Vector2 interval)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getWindowSize(), interval, true, true, HORIZONTAL_DIRECTION.CENTER, VERTICAL_DIRECTION.CENTER);
	}
	public void autoGrid(HORIZONTAL_DIRECTION horizontal)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getWindowSize(), Vector2.zero, true, true, horizontal, VERTICAL_DIRECTION.CENTER);
	}
	public void autoGrid(VERTICAL_DIRECTION vertical)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getWindowSize(), Vector2.zero, true, true, HORIZONTAL_DIRECTION.CENTER, vertical);
	}
	public void autoGrid(Vector2 interval, HORIZONTAL_DIRECTION horizontal)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getWindowSize(), interval, true, true, horizontal, VERTICAL_DIRECTION.CENTER);
	}
	public void autoGrid(Vector2 interval, VERTICAL_DIRECTION vertical)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getWindowSize(), interval, true, true, HORIZONTAL_DIRECTION.CENTER, vertical);
	}
	// 保持父节点的大小和位置,从左上角开始横向排列子节点,并且会改变子节点的大小,gridSize是子节点的大小,horizontal是不超过1排时,水平方向的停靠方式,vertical是整体竖直方向上的停靠方式
	public void autoGrid(Vector2 interval, bool autoRefreshUIDepth, bool refreshIgnoreInactive, HORIZONTAL_DIRECTION horizontal = HORIZONTAL_DIRECTION.CENTER, VERTICAL_DIRECTION vertical = VERTICAL_DIRECTION.CENTER)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getWindowSize(), interval, autoRefreshUIDepth, refreshIgnoreInactive, horizontal, vertical);
	}
}