using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;

public class LongPressData : ClassObject
{
	public OnLongPress mOnLongPress;            // 达到长按时间阈值时的回调,由GlobalTouchSystem驱动
	public float mLongPressTime;				// 长按的时间阈值,超过阈值时检测为长按
	public OnLongPressing mOnLongPressing;      // 长按进度回调,可获取当前还有多久到达长按时间阈值
	public bool mFinish;						// 是否已经完成了长按事件,完成后将不会再检测,
	public void update(float pressedTime)
	{
		// 长按事件已经结束,不需要再检测
		if ((mOnLongPress == null && mOnLongPressing == null) || mFinish)
		{
			return;
		}
		// 长按事件被中断,比如长按触点距离移动过远,触点抬起
		if (pressedTime < 0.0f)
		{
			mFinish = true;
			mOnLongPressing?.Invoke(0.0f);
			return;
		}

		// 还未到达长按时间
		if (mOnLongPressing != null)
		{
			float progress = !isFloatZero(mLongPressTime) ? pressedTime / mLongPressTime : 0.0f;
			clampMax(ref progress, 1.0f);
			mOnLongPressing(progress);
		}
		// 已到达长按时间,触发长按事件
		if (pressedTime >= mLongPressTime)
		{
			mFinish = true;
			mOnLongPress?.Invoke();
		}
	}
	public void reset()
	{
		mFinish = false;
	}
}