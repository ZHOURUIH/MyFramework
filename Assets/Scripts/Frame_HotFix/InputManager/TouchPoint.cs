using UnityEngine;
using System.Collections.Generic;
using System;
using static MathUtility;
using static FrameDefine;

// 触摸点信息
public class TouchPoint : ClassObject
{
	protected DateTime mDownTime;		// 触点按下的时间
	protected DateTime mUpTime;			// 触点抬起的时间
	protected Vector3 mDownPosition;	// 触点按下的坐标
	protected Vector3 mLastPosition;	// 上一帧的触点位置
	protected Vector3 mCurPosition;		// 当前的触点位置
	protected Vector2 mMoveDelta;       // 触点在这一帧的移动量
	protected int mTouchID;				// 触点ID
	protected bool mCurrentDown;		// 触点是否在这一帧才按下的
	protected bool mDoubleClick;		// 是否为双击操作
	protected bool mCurrentUp;          // 触点是否在这一帧抬起
	protected bool mMouse;				// 是否为鼠标的触点
	protected bool mClick;				// 是否已点击
	protected bool mDown;               // 当前是否按下
	public override void resetProperty()
	{
		base.resetProperty();
		mDownTime = DateTime.Now;
		mUpTime = DateTime.Now;
		mDownPosition = Vector3.zero;
		mLastPosition = Vector3.zero;
		mCurPosition = Vector3.zero;
		mMoveDelta = Vector2.zero;
		mTouchID = 0;
		mCurrentDown = false;
		mDoubleClick = false;
		mCurrentUp = false;
		mMouse = false;
		mClick = false;
		mDown = false;
	}
	public void pointDown(Vector3 pos)
	{
		mDownPosition = pos;
		mDownTime = DateTime.Now;
		mCurPosition = pos;
		mLastPosition = pos;
		mCurrentDown = true;
		mDown = true;
	}
	public void pointUp(Vector3 pos, List<DeadClick> deadTouchList)
	{
		mCurrentUp = true;
		mDown = false;
		mUpTime = DateTime.Now;
		mCurPosition = pos;
		mClick = lengthLess(mDownPosition - mCurPosition, CLICK_LENGTH);
		// 遍历一段时间内已经完成点击的触点列表,查看有没有点坐标相近,时间相近的触点
		if (mClick)
		{
			foreach (DeadClick deadClick in deadTouchList)
			{
				if ((mUpTime - deadClick.mClickTime).TotalSeconds < DOUBLE_CLICK_TIME && 
					lengthLess(mCurPosition - deadClick.mClickPosition, CLICK_LENGTH))
				{
					mDoubleClick = true;
					break;
				}
			}
		}
	}
	public void update(Vector3 newPosition)
	{
		mLastPosition = mCurPosition;
		mCurPosition = newPosition;
		mMoveDelta = mCurPosition - mLastPosition;
	}
	public void lateUpdate()
	{
		mCurrentDown = false;
		mCurrentUp = false;
		mClick = false;
		mDoubleClick = false;
	}
	public void setMouse(bool mouse) { mMouse = mouse; }
	public void setTouchID(int touchID) { mTouchID = touchID; }
	public int getTouchID() { return mTouchID; }
	public bool isMouse() { return mMouse; }
	public bool isCurrentUp() { return mCurrentUp; }
	public bool isCurrentDown() { return mCurrentDown; }
	public bool isDoubleClick() { return mDoubleClick; }
	public bool isClick() { return mClick; }
	public bool isDown() { return mDown; }
	public Vector3 getDownPosition() { return mDownPosition; }
	public Vector3 getMoveDelta() { return mMoveDelta; }
	public Vector3 getLastPosition() { return mLastPosition; }
	public Vector3 getCurPosition() { return mCurPosition; }
	public DateTime getDownTime() { return mDownTime; }
	public DateTime getUpTime() { return mUpTime; }
	public void resetState()
	{
		mCurrentUp = false;
		mDown = false;
		mUpTime = DateTime.Now;
		mClick = false;
		mDoubleClick = false;
	}
}