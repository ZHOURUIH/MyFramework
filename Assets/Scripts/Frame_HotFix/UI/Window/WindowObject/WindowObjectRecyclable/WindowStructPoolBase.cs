using System;
using UnityEngine;
using static UnityUtility;

// 窗口对象池基类
public class WindowStructPoolBase : IWindowObjectOwner
{
	protected WindowObjectBase mOwnerObject;	// 如果是WindowObject中创建的对象池,则会存储此WindowObject
	protected LayoutScript mScript;				// 所属的布局脚本
	protected myUGUIObject mItemParent;			// 创建节点时默认的父节点
	protected myUGUIObject mTemplate;			// 创建节点时使用的模板
	protected string mPreName;					// 创建物体的名字前缀
	protected Type mObjectType;					// 物体类型
	protected static long mAssignIDSeed;		// 分配ID种子,用于设置唯一分配ID,只会递增,不会减少
	protected bool mNewItemMoveToLast;			// 新创建的物体是否需要放到父节点的最后,也就是是否在意其渲染顺序
	protected bool mInited;                     // 是否已经初始化
	public WindowStructPoolBase(IWindowObjectOwner parent)
	{
		if (parent is WindowObjectBase objBase)
		{
			mScript = objBase.getScript();
			mOwnerObject = objBase;
			mOwnerObject.addWindowStructPool(this);
		}
		else if (parent is LayoutScript script)
		{
			mScript = script;
			mScript.addWindowStructPool(this);
		}
		else if (parent is WindowStructPoolBase)
		{
			logError("对象池的父节点不能是对象池");
		}
		mNewItemMoveToLast = true;
	}
	public virtual void destroy() {}
	public virtual void init(){}
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
	public myUGUIObject getItemParent()					{ return mItemParent; }
	public myUGUIObject getTemplate()					{ return mTemplate; }
	public virtual int getInUseCount()					{ return 0; }
	public bool isRootPool()							{ return mOwnerObject == null; }
	public WindowObjectBase getOwnerObject()			{ return mOwnerObject; }
	public LayoutScript getLayoutScript()				{ return mScript; }
	public void setItemParent(myUGUIObject parent)		{ mItemParent = parent; }
	public void setActive(bool active)					{ mItemParent.setActive(active); }
	public void setItemPreName(string preName)			{ mPreName = preName; }
	public void setObjectType(Type type)				{ mObjectType = type; }
	public void setMoveToLast(bool moveToLast)	{ mNewItemMoveToLast = moveToLast; }
	public virtual void unuseAll() { }
	public void refreshUIDepth(bool ignoreInactive = true)
	{
		mScript.getLayout().refreshUIDepth(mItemParent, ignoreInactive);
	}
	// 将自动排列的方法直接写到对象池中,方便使用
	// 一般对于排列是有两个地方有需求,1:滑动列表,2:不可滑动,但是有多个相似节点需要动态创建后排列好
	// 一般滑动列表都会需要在排列好后将边对齐父节点,而对于不可滑动的列表,需求会根据实际情况有不同
	public void autoGridHorizontalForDragView()
	{
		WidgetUtility.autoGridHorizontal(mItemParent, true, true, 0.0f, true, 0.0f, 0.0f, 0.0f, true);
		mItemParent.setLeftCenterToParentLeftCenter();
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
	public void autoGridHorizontalCenter()
	{
		WidgetUtility.autoGridHorizontalCenter(mItemParent, true, true, 0.0f);
	}
	public void autoGridVerticalForDragView()
	{
		WidgetUtility.autoGridVertical(mItemParent, true, true, 0.0f, 0.0f, 0.0f, 0.0f, true);
		mItemParent.setTopCenterToParentTopCenter();
	}
	public void autoGridVerticalForDragView(float interval)
	{
		WidgetUtility.autoGridVertical(mItemParent, true, true, interval, 0.0f, 0.0f, 0.0f, true);
		mItemParent.setTopCenterToParentTopCenter();
	}
	public void autoGridVertical()
	{
		WidgetUtility.autoGridVertical(mItemParent, true, true, 0.0f, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridVertical(bool keepTopSide)
	{
		WidgetUtility.autoGridVertical(mItemParent, true, true, 0.0f, 0.0f, 0.0f, 0.0f, keepTopSide);
	}
	public void autoGridVertical(float interval)
	{
		WidgetUtility.autoGridVertical(mItemParent, true, true, interval, 0.0f, 0.0f, 0.0f, true);
	}
	public void autoGridVertical(bool autoRefreshUIDepth, bool refreshIgnoreInactive)
	{
		WidgetUtility.autoGridVertical(mItemParent, autoRefreshUIDepth, refreshIgnoreInactive, 0.0f, 0.0f, 0.0f, 0.0f, true);
	}
	// 自动排列一个节点下的所有子节点的位置,从上往下紧密排列,并且不改变子节点的大小
	public void autoGridVertical(bool autoRefreshUIDepth, bool refreshIgnoreInactive, float interval, float minHeight = 0.0f)
	{
		WidgetUtility.autoGridVertical(mItemParent, autoRefreshUIDepth, refreshIgnoreInactive, interval, minHeight, 0.0f, 0.0f, true);
	}
	public void autoGridForDragView()
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getSize(), Vector2.zero, true, true, true, HORIZONTAL_DIRECTION.LEFT, VERTICAL_DIRECTION.TOP);
		// 根据排列后的子节点,计算出父节点的高度
		WidgetUtility.setWindowBestHeight(mItemParent, true, true);
		mItemParent.setTopCenterToParentTopCenter();
	}
	public void autoGrid()
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getSize(), Vector2.zero, true, true, true, HORIZONTAL_DIRECTION.LEFT, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(bool autoRefreshUIDepth)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getSize(), Vector2.zero, autoRefreshUIDepth, true, true, HORIZONTAL_DIRECTION.LEFT, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(Vector2 interval)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getSize(), interval, true, true, true, HORIZONTAL_DIRECTION.LEFT, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(HORIZONTAL_DIRECTION horizontal)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getSize(), Vector2.zero, true, true, true, horizontal, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(VERTICAL_DIRECTION vertical)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getSize(), Vector2.zero, true, true, true, HORIZONTAL_DIRECTION.LEFT, vertical);
	}
	public void autoGrid(Vector2 interval, HORIZONTAL_DIRECTION horizontal)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getSize(), interval, true, true, true, horizontal, VERTICAL_DIRECTION.TOP);
	}
	public void autoGrid(Vector2 interval, VERTICAL_DIRECTION vertical)
	{
		WidgetUtility.autoGrid(mItemParent, mTemplate.getSize(), interval, true, true, true, HORIZONTAL_DIRECTION.LEFT, vertical);
	}
}