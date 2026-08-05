using UnityEngine;
using System.Collections.Generic;
using static FrameBaseHotFix;

// 存放一个触点的悬停和按下的物体
public class TouchInfo : ClassObject
{
	protected HashSet<IMouseEventCollect> mHoverList = new();	// 触点当前悬停的物体列表
	protected SafeList<IMouseEventCollect> mPressList = new();	// 保存鼠标按下时所选中的所有物体,需要给这些窗口发送鼠标移动的消息
	protected TouchPoint mTouch;								// 触点信息
	public void init(TouchPoint touch)
	{
		mTouch = touch;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mHoverList.Clear();
		mPressList.clear();
		mTouch = null;
	}
	// 每帧更新,处理触点移动、悬停进入/离开事件
	public void update(float elapsedTime)
	{
		int touchID = mTouch.getTouchID();
		Vector3 curPos = mTouch.getCurPosition();
		// 通知触点移动,只通知触点按下时的窗口列表
		foreach (IMouseEventCollect obj in mPressList)
		{
			if (mTouch.getMoveDelta().isZero())
			{
				obj.onTouchStay(curPos, mTouch.getTouchID());
			}
			else
			{
				obj.onTouchMove(curPos, mTouch.getMoveDelta(), elapsedTime, mTouch.getTouchID());
			}
		}

		using var b = new HashSetScope<IMouseEventCollect>(out var newList);
		mGlobalTouchSystem.getAllHoverObject(newList, curPos);
		// 需要先判断离开,再判断进入,逻辑更通顺一些
		// 触点是否刚离开了某个窗口,只有触点移动时才检测
		foreach (IMouseEventCollect item in mHoverList)
		{
			// 不过也许此时悬停窗口已经不接收输入事件了或者碰撞盒子被禁用了,需要判断一下
			if (!newList.Contains(item) && item.isActiveInHierarchy() && item.isHandleInput())
			{
				item.onTouchLeave(curPos, touchID);
			}
		}
		// 触点是否刚进入了某个窗口,只有触点移动时才检测
		foreach (IMouseEventCollect item in newList)
		{
			// 不过也许此时悬停窗口已经不接收输入事件了或者碰撞盒子被禁用了,需要判断一下
			if (!mHoverList.Contains(item) && item.isActiveInHierarchy() && item.isHandleInput())
			{
				item.onTouchEnter(curPos, touchID);
			}
		}
		mHoverList.setRange(newList);
	}
	// 触点按下时记录当前悬停的物体列表
	public void touchPress()
	{
		mGlobalTouchSystem.getAllHoverObject(mHoverList, mTouch.getCurPosition());
		mPressList.addRange(mHoverList);
	}
	public void clearPressList() { mPressList.clear(); }
	public SafeList<IMouseEventCollect> getPressList() { return mPressList; }
	public TouchPoint getTouch() { return mTouch; }
	public void removeObject(IMouseEventCollect obj)
	{
		mPressList.remove(obj);
		mHoverList.Remove(obj);
	}	
}