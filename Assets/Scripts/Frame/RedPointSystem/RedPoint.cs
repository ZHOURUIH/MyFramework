using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBase;

// 红点的基类
public class RedPoint : ClassObject
{
	protected HashSet<myUGUIObject> mPointUIList;	// 关联的红点UI,一个红点数据可以有多个红点UI表现
	protected List<RedPoint> mChildren;				// 子节点列表
	protected RedPoint mParent;						// 父节点
	protected int mEventID;							// 会触发此红点改变的事件ID,在子类中设置
	protected int mID;								// 红点ID,用来索引
	protected bool mEnable;							// 是否显示红点
	public RedPoint()
	{
		mChildren = new List<RedPoint>();
		mPointUIList = new HashSet<myUGUIObject>();
	}
	public virtual void init() 
	{
		if (mEventID > 0)
		{
			mEventSystem.listenEvent(mEventID, onEventTrigger, this);
		}
	}
	public virtual void destroy() 
	{
		// 由于大部分都是会注册事件监听,所以基类中默认注销事件监听
		mEventSystem.unlistenEvent(this);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mChildren.Clear();
		mParent = null;
		mPointUIList.Clear();
		mEnable = false;
		mEventID = 0;
		mID = 0;
	}
	public void setParent(RedPoint parent) 
	{
		// 如果当前有父节点了,则从父节点上取下来
		if (mParent != null)
		{
			mParent.mChildren.Remove(this);
		}
		mParent = parent;
		if (mParent != null)
		{
			mParent.mChildren.Add(this);
		}
	}
	public void addPointUI(myUGUIObject point)
	{ 
		if (point == null)
		{
			logError("添加的红点UI不能为空");
			return;
		}
		mPointUIList.Add(point);
		foreach (var item in mPointUIList)
		{
			item.setActive(mEnable);
		}
	}
	public void removePointUI(myUGUIObject point)
	{
		mPointUIList.Remove(point);
	}
	public void setID(int id) { mID = id; }
	public void setEnable(bool enable) 
	{ 
		mEnable = enable;
		foreach (var item in mPointUIList)
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
		int count = mChildren.Count;
		for(int i = 0; i < count; ++i)
		{
			if (mChildren[i].isEnable())
			{
				enable = true;
				break;
			}
		}
		setEnable(enable);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onEventTrigger(GameEvent gameEvent)
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