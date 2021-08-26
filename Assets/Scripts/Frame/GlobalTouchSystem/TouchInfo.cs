using UnityEngine;
using System.Collections.Generic;

// 存放一个触点的所有信息
public class TouchInfo : FrameBase
{
	protected HashSet<IMouseEventCollect> mHoverList;	// 触点当前悬停的物体列表
	protected List<IMouseEventCollect> mPressList;		// 保存鼠标按下时所选中的所有物体,需要给这些窗口发送鼠标移动的消息
	protected Vector3 mLastPosition;					// 这一帧触点的位置
	protected Vector3 mCurPosition;						// 上一帧触点的位置
	protected int mTouchID;								// 触点ID
	public TouchInfo()
	{
		mHoverList = new HashSet<IMouseEventCollect>();
		mPressList = new List<IMouseEventCollect>();
	}
	public void init(int touchID, Vector3 pos)
	{
		mTouchID = touchID;
		mCurPosition = pos;
		mLastPosition = pos;
		mGlobalTouchSystem.getAllHoverWindow(mHoverList, mCurPosition);
		mPressList.AddRange(mHoverList);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mHoverList.Clear();
		mPressList.Clear();
		mLastPosition = Vector3.zero;
		mCurPosition = Vector3.zero;
		mTouchID = 0;
	}
	public void update(float elapsedTime)
	{
		if (isVectorEqual(mCurPosition, mLastPosition))
		{
			return;
		}
		// 通知触点移动,只通知触点按下时的窗口列表
		foreach (var obj in mPressList)
		{
			obj.onMouseMove(mCurPosition, mCurPosition - mLastPosition, elapsedTime, mTouchID);
		}
		LIST(out HashSet<IMouseEventCollect> newList);
		mGlobalTouchSystem.getAllHoverWindow(newList, mCurPosition);
		// 触点是否刚进入了某个窗口,只有触点移动时才检测
		foreach(var item in newList)
		{
			// 不过也许此时悬停窗口已经不接收输入事件了或者碰撞盒子被禁用了,需要判断一下
			if (!mHoverList.Contains(item) && item.isActive() && item.isHandleInput())
			{
				item.onMouseEnter(mTouchID);
			}
		}
		// 触点是否刚离开了某个窗口,只有触点移动时才检测
		foreach (var item in mHoverList)
		{
			// 不过也许此时悬停窗口已经不接收输入事件了或者碰撞盒子被禁用了,需要判断一下
			if (!newList.Contains(item) && item.isActive() && item.isHandleInput())
			{
				item.onMouseLeave(mTouchID);
			}
		}
		mHoverList.Clear();
		foreach (var item in newList)
		{
			mHoverList.Add(item);
		}
		mLastPosition = mCurPosition;
		UN_LIST(newList);
	}
	public void clearPressList() { mPressList.Clear(); }
	public List<IMouseEventCollect> getPressList() { return mPressList; }
	public void setCurPosition(Vector3 pos)
	{
		mLastPosition = mCurPosition;
		mCurPosition = pos;
	}
	public void removeObject(IMouseEventCollect obj)
	{
		mPressList.Remove(obj);
		mHoverList.Remove(obj);
	}	
}