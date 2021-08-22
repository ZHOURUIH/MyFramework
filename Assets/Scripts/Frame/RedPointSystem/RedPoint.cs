using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RedPoint : FrameBase
{
	protected List<RedPoint> mChildren;
	protected RedPoint mParent;
	protected myUGUIObject mPointUI;
	protected string mName;
	protected bool mEnable;
	public RedPoint()
	{
		mChildren = new List<RedPoint>();
	}
	public virtual void init() { }
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
		mEnable = false;
		mName = null;
		mPointUI = null;
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
	public void setPointUI(myUGUIObject point) 
	{ 
		mPointUI = point;
		LT.ACTIVE(mPointUI, mEnable);
	}
	public void setName(string name) { mName = name; }
	public void setEnable(bool enable) 
	{ 
		mEnable = enable; 
		LT.ACTIVE(mPointUI, enable);
	}
	public bool isEnable() { return mEnable; }
	public int getChildCount() { return mChildren.Count; }
	public string getName() { return mName; }
	public RedPoint getParent() { return mParent; }
	public List<RedPoint> getChildren() { return mChildren; }
	// 根据所有子节点的状态刷新当前节点的状态
	public virtual void refresh()
	{
		// 刷新父节点的变化
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
}