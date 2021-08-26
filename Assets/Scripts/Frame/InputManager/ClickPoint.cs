using UnityEngine;
using System.Collections.Generic;
using System;

// 点击操作逻辑,为了统一鼠标和触屏的操作,所以就点击操作封装起来
// 判断鼠标和触点,然后生成点击操作对象
public class ClickPoint : FrameBase
{
	protected DateTime mDownTime;		// 触点按下的时间
	protected DateTime mUpTime;			// 触点抬起的时间
	protected Vector3 mDownPosition;	// 触点按下的坐标
	protected Vector3 mUpPosition;		// 触点抬起的坐标
	protected int mPointerID;			// 触点ID
	protected bool mClick;				// 是否已点击
	protected bool mFinish;				// 是否已完成
	public override void resetProperty()
	{
		base.resetProperty();
		mDownTime = DateTime.Now;
		mUpTime = DateTime.Now;
		mDownPosition = Vector3.zero;
		mUpPosition = Vector3.zero;
		mPointerID = 0;
		mClick = false;
		mFinish = false;
	}
	// 触点按下
	public void pointDown(int pointerID, Vector3 position)
	{
		mDownPosition = position;
		mDownTime = DateTime.Now;
		mPointerID = pointerID;
	}
	// 触点抬起
	public void pointUp(Vector3 position)
	{
		mUpPosition = position;
		mUpTime = DateTime.Now;
		mClick = lengthLess(mDownPosition - mUpPosition, FrameDefine.CLICK_THRESHOLD);
		mFinish = true;
	}
	public int getPointerID() { return mPointerID; }
	public bool isClick() { return mClick; }
	public bool isFinish() { return mFinish; }
	public Vector3 getClickPosition() { return mUpPosition; }
}