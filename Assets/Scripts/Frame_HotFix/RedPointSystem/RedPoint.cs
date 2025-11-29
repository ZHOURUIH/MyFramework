using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseHotFix;

// 红点的基类
public class RedPoint : ClassObject
{
	protected Dictionary<myUGUIObject, string> mPointUIMap = new();	// 关联的红点UI,一个红点数据可以有多个红点UI表现,为了方便调试,Value存储的是节点的路径
	protected List<Type> mEventTypeList = new();		// 会触发此红点改变的事件类型,在子类中设置
	protected List<RedPoint> mChildren = new();			// 子节点列表
	protected RedPoint mParent;							// 父节点
	protected bool mEnable;                             // 是否显示红点
	protected bool mIsDirty;							// 是否需要刷新状态,每帧只刷新一次状态
	public virtual void init() 
	{
		initEventType();
		foreach (Type type in mEventTypeList.safe())
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
		mPointUIMap.Clear();
		mChildren.Clear();
		mParent = null;
		mEventTypeList?.Clear();
		mEnable = false;
		mIsDirty = false;
	}
	public void setParent(RedPoint parent) 
	{
		if (parent != null && parent.GetType() != typeof(RedPoint))
		{
			logError("父节点只能是RedPoint类型的,父节点的显示只与子节点的显示有关,只有叶节点才是派生类型");
			return;
		}
		// 如果当前有父节点了,则从父节点上取下来
		mParent?.mChildren?.Remove(this);
		mParent = parent;
		mParent?.mChildren?.Add(this);
	}
	public void bindPointUI(myUGUIObject point, bool showError = true)
	{ 
		if (point == null)
		{
			logError("添加的红点UI不能为空");
			return;
		}
		if (!mPointUIMap.TryAdd(point, getGameObjectPath(point.getObject())))
		{
			if (showError)
			{
				logError("重复绑定红点UI节点, hash:" + point.GetHashCode());
			}
		}
		point.setActive(mEnable);
	}
	public void removePointUI(myUGUIObject point)
	{
		if (point == null || !mPointUIMap.Remove(point))
		{
			logWarning("移除绑定红点UI失败, hash:" + point.GetHashCode());
		}
	}
	public void setEnable(bool enable) 
	{ 
		mEnable = enable;
		foreach (var item in mPointUIMap)
		{
			// 此处能检测到的很有限,因为有些红点的UI节点可能并不是因为需要销毁才会移除绑定
			if (item.Key.isDestroy())
			{
				logError("红点的UI节点已经被销毁了,但是仍然没有被移除绑定,name:" + item.Value);
				continue;
			}
			item.Key.setActive(mEnable);
		}
	}
	public bool isEnable() { return mEnable; }
	public int getChildCount() { return mChildren.Count; }
	public RedPoint getParent() { return mParent; }
	public List<RedPoint> getChildren() { return mChildren; }
	public bool isDirty() { return mIsDirty; }
	public void setDirty(bool dirty) { mIsDirty = dirty; }
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
	protected void addEvent<T>() where T : GameEvent 
	{
		mEventTypeList.Add(typeof(T));
	}
	// 子类在这个虚函数中去调用addEvent来添加需要关心的事件类型
	protected virtual void initEventType() { }
	protected void onEventTrigger()
	{
		// 只有叶节点才会监听事件,有子节点的红点只会受其子节点控制,不能受其他控制
		if (mChildren.Count > 0)
		{
			return;
		}
		mIsDirty = true;
	}
}