using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseHotFix;

// 红点的基类
public class RedPoint : ClassObject
{
	protected HashSet<myUGUIObject> mPointUIList = new();	// 关联的红点UI,一个红点数据可以有多个红点UI表现
	protected List<RedPoint> mChildren = new();				// 子节点列表
	protected RedPoint mParent;								// 父节点
	protected List<Type> mEventTypeList = new();			// 会触发此红点改变的事件类型,在子类中设置
	protected Type mEventType;								// 会触发此红点改变的事件类型,在子类中设置
	protected int mID;										// 红点ID,用来索引
	protected bool mEnable;									// 是否显示红点
	public virtual void init() 
	{
		if (mEventType != null)
		{
			mEventSystem.listenEvent(mEventType, onEventTrigger, this);
		}
		foreach (Type type in mEventTypeList)
		{
			mEventSystem.listenEvent(type, onEventTrigger, this);
		}
	}
	public override void destroy() 
	{
		base.destroy();
		// 由于大部分都是会注册事件监听,所以基类中默认注销事件监听
		mEventSystem?.unlistenEvent(this);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mPointUIList.Clear();
		mChildren.Clear();
		mParent = null;
		mEventTypeList.Clear();
		mEventType = null;
		mID = 0;
		mEnable = false;
	}
	public void setParent(RedPoint parent) 
	{
		// 如果当前有父节点了,则从父节点上取下来
		mParent?.mChildren?.Remove(this);
		mParent = parent;
		mParent?.mChildren?.Add(this);
	}
	public void addPointUI(myUGUIObject point)
	{ 
		if (point == null)
		{
			logError("添加的红点UI不能为空");
			return;
		}
		mPointUIList.Add(point);
		point.setActive(mEnable);
	}
	public void removePointUI(myUGUIObject point)
	{
		mPointUIList.Remove(point);
	}
	public void setID(int id) { mID = id; }
	public void setEnable(bool enable) 
	{ 
		mEnable = enable;
		foreach (myUGUIObject item in mPointUIList)
		{
			item.setActive(mEnable);
		}
	}
	public bool isEnable() { return mEnable; }
	public int getChildCount() { return mChildren.Count; }
	public int getID() { return mID; }
	public RedPoint getParent() { return mParent; }
	public List<RedPoint> getChildren() { return mChildren; }
	// 根据所有子节点的状态刷新当前节点的状态
	public virtual void refresh()
	{
		// 默认只会根据子节点的状态刷新自己的状态,子类需要根据实际需求进行重写,确保数据正确
		bool enable = false;
		foreach (RedPoint point in mChildren)
		{
			if (point.isEnable())
			{
				enable = true;
				break;
			}
		}
		setEnable(enable);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onEventTrigger()
	{
		// 只有叶节点才会监听事件,有子节点的红点只会受其子节点控制,不能受其他控制
		if (mChildren.Count > 0)
		{
			return;
		}
		refresh();
		mRedPointSystem.notifyRedPointChanged(this);
	}
}