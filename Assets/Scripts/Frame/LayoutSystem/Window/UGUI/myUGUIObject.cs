using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class myUGUIObject : myUIObject
{
	protected static Comparison<Transform> mCompareDescend = compareZDecending;
	protected Action<PointerEventData, GameObject> mOnUGUIMouseEnter;		// 鼠标进入的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIMouseLeave;       // 鼠标离开的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIMouseDown;        // 鼠标按下的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIMouseUp;          // 鼠标抬起的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIClick;            // 鼠标点击的回调,由UGUI触发
	protected EventTriggerListener mEventTriggerListener;					// UGUI事件监听器,用于接收UGUI的事件
	protected RectTransform mRectTransform;									// UGUI的Transform
	protected Action<Vector2> mOnUGUIMouseMove;                             // 鼠标移动的回调,由UGUI触发
	protected Action mOnUGUIMouseStay;                                      // 鼠标在窗口内停止的回调,由UGUI触发
	protected bool mUGUIMouseDown;											// 鼠标当前是否在窗口内按下
	public override void init()
	{
		base.init();
		mRectTransform = mObject.GetComponent<RectTransform>();
		// 因为在使用UGUI时,原来的Transform会被替换为RectTransform,所以需要重新设置Transform组件
		if (mRectTransform != null)
		{
			mTransform = mRectTransform;
		}
		if (mBoxCollider != null && mRectTransform != null)
		{
			mBoxCollider.size = mRectTransform.rect.size;
			mBoxCollider.center = multiVector2(mRectTransform.rect.size, new Vector2(0.5f, 0.5f) - mRectTransform.pivot);
		}
		// 一开始就注册所有的UI事件,方便后续做事件转发
		mEventTriggerListener = getUnityComponent<EventTriggerListener>();
		mEventTriggerListener.onClick += onUGUIClick;
		mEventTriggerListener.onDown += onUGUIMouseDown;
		mEventTriggerListener.onUp += onUGUIMouseUp;
		mEventTriggerListener.onEnter += onUGUIMouseEnter;
		mEventTriggerListener.onExit += onUGUIMouseLeave;
	}
	public override void destroy()
	{
		mEventTriggerListener.onClick -= onUGUIClick;
		mEventTriggerListener.onDown -= onUGUIMouseDown;
		mEventTriggerListener.onUp -= onUGUIMouseUp;
		mEventTriggerListener.onEnter -= onUGUIMouseEnter;
		mEventTriggerListener.onExit -= onUGUIMouseLeave;
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 确保RectTransform和BoxCollider一样大
		if (mRectTransform != null && mBoxCollider != null)
		{
			if (!isFloatEqual(mRectTransform.rect.width, mBoxCollider.size.x) || !isFloatEqual(mRectTransform.rect.height, mBoxCollider.size.y))
			{
				mBoxCollider.size = mRectTransform.rect.size;
				mBoxCollider.center = multiVector2(mRectTransform.rect.size, new Vector2(0.5f, 0.5f) - mRectTransform.pivot);
			}
		}
		if (mUGUIMouseDown)
		{
			Vector3 delta = mInputSystem.getMouseDelta();
			if (!isVectorZero(delta))
			{
				mOnUGUIMouseMove?.Invoke(delta);
			}
			else
			{
				mOnUGUIMouseStay?.Invoke();
			}
		}
	}
	// 获得窗口顶部在窗口中的相对于窗口pivot的Y坐标
	public float getWindowTop() { return getWindowSize().y * (1.0f - getPivot().y); }
	// 获得窗口底部在窗口中的相对于窗口pivot的Y坐标
	public float getWindowBottom() { return -getWindowSize().y * getPivot().y; }
	// 获得窗口左边界在窗口中的相对于窗口pivot的X坐标
	public float getWindowLeft() { return -getWindowSize().x * getPivot().x; }
	// 获得窗口右边界在窗口中的相对于窗口pivot的X坐标
	public float getWindowRight() { return getWindowSize().x * (1.0f - getPivot().x); }
	// 获取不考虑中心点偏移的坐标,也就是固定获取窗口中心的坐标
	// 由于pivot的影响,Transform.localPosition获得的坐标并不一定等于窗口中心的坐标
	public Vector3 getPositionNoPivot()
	{
		Vector2 size = getWindowSize();
		Vector2 pivot = getPivot();
		Vector2 pivotPosOffset = new Vector2((pivot.x - 0.5f) * size.x, (pivot.y - 0.5f) * size.y);
		return getPosition() - (Vector3)pivotPosOffset;
	}
	public Vector2 getPivot() { return mRectTransform.pivot; }
	public void setPivot(Vector2 pivot) { mRectTransform.pivot = pivot; }
	public RectTransform getRectTransform() { return mRectTransform; }
	public override void setWindowSize(Vector2 size)
	{
		setRectSize(mRectTransform, size);
	}
	public override Vector2 getWindowSize(bool transformed = false)
	{
		Vector2 windowSize = mRectTransform.rect.size;
		if (transformed)
		{
			windowSize = multiVector2(windowSize, getWorldScale());
		}
		return windowSize;
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		if (fadeChild)
		{
			setUGUIChildAlpha(mObject, alpha);
		}
	}
	public void refreshChildDepthByPositionZ()
	{
		// z值越大的子节点越靠后
		LIST(out List<Transform> tempList);
		tempList.Clear();
		int childCount = getChildCount();
		for (int i = 0; i < childCount; ++i)
		{
			tempList.Add(mTransform.GetChild(i));
		}
		quickSort(tempList, mCompareDescend);
		int count = tempList.Count;
		for (int i = 0; i < count; ++i)
		{
			tempList[i].SetSiblingIndex(i);
		}
		UN_LIST(tempList);
	}
	public void setUGUIClick(Action<PointerEventData, GameObject> callback) { mOnUGUIClick = callback; }
	public void setUGUIMouseDown(Action<PointerEventData, GameObject> callback) { mOnUGUIMouseDown = callback; }
	public void setUGUIMouseUp(Action<PointerEventData, GameObject> callback) { mOnUGUIMouseUp = callback; }
	public void setUGUIMouseEnter(Action<PointerEventData, GameObject> callback) { mOnUGUIMouseEnter = callback; }
	public void setUGUIMouseExit(Action<PointerEventData, GameObject> callback) { mOnUGUIMouseLeave = callback; }
	public void setUGUIMouseMove(Action<Vector2> callback) 
	{
		mOnUGUIMouseMove = callback;
		// 如果设置了要监听鼠标移动,则需要激活当前窗口
		mEnable = true;
	}
	public void setUGUIMouseStay(Action callback) 
	{
		mOnUGUIMouseStay = callback;
		mEnable = true;
	}
	//--------------------------------------------------------------------------------------------------------
	protected static int compareZDecending(Transform a, Transform b) { return (int)sign(b.localPosition.z - a.localPosition.z); }
	protected void onUGUIMouseDown(PointerEventData eventData, GameObject go)
	{
		mUGUIMouseDown = true;
		mOnUGUIMouseDown?.Invoke(eventData, go);
	}
	protected void onUGUIMouseUp(PointerEventData eventData, GameObject go)
	{
		mUGUIMouseDown = false;
		mOnUGUIMouseUp?.Invoke(eventData, go);
	}
	protected void onUGUIClick(PointerEventData eventData, GameObject go)
	{
		mOnUGUIClick?.Invoke(eventData, go);
	}
	protected void onUGUIMouseEnter(PointerEventData eventData, GameObject go)
	{
		mOnUGUIMouseEnter?.Invoke(eventData, go);
	}
	protected void onUGUIMouseLeave(PointerEventData eventData, GameObject go)
	{
		mOnUGUIMouseLeave?.Invoke(eventData, go);
	}
}