using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// UGUI窗口的基类
public class myUGUIObject : myUIObject
{
	protected static Comparison<Transform> mCompareDescend = compareZDecending;		// 避免GC的回调
	protected Action<PointerEventData, GameObject> mOnUGUIMouseEnter;		// 鼠标进入的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIMouseLeave;       // 鼠标离开的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIMouseDown;        // 鼠标按下的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIMouseUp;          // 鼠标抬起的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIClick;            // 鼠标点击的回调,由UGUI触发
	protected EventTriggerListener mEventTriggerListener;					// UGUI事件监听器,用于接收UGUI的事件
	protected Action<Vector2, Vector3> mOnUGUIMouseMove;					// 鼠标移动的回调,由UGUI触发,第一个参数是这一帧触点的移动量,第二个参数是触点当前的位置
	protected Action<Vector3> mOnUGUIMouseStay;								// 鼠标在窗口内停止的回调,由UGUI触发,参数是触点当前的位置
	protected PointerEventData mMousePointer;								// 鼠标在当前窗口按下时的触点信息
	protected RectTransform mRectTransform;									// UGUI的Transform
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
		mMousePointer = null;
	}
	public override void destroy()
	{
		if (mEventTriggerListener != null)
		{
			mEventTriggerListener.mOnClick -= onUGUIClick;
			mEventTriggerListener.mOnDown -= onUGUIMouseDown;
			mEventTriggerListener.mOnUp -= onUGUIMouseUp;
			mEventTriggerListener.mOnEnter -= onUGUIMouseEnter;
			mEventTriggerListener.mOnExit -= onUGUIMouseLeave;
		}
		mMousePointer = null;
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
		if (mMousePointer != null)
		{
			// 此处应该获取touchID的移动量
			Vector3 delta = mMousePointer.delta;
			if (!isVectorZero(delta))
			{
				mOnUGUIMouseMove?.Invoke(delta, mMousePointer.position);
			}
			else
			{
				mOnUGUIMouseStay?.Invoke(mMousePointer.position);
			}
		}
	}
	public override void onLayoutHide() 
	{
		// 布局隐藏时需要将触点清除
		mMousePointer = null;
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
	public void setUGUIClick(Action<PointerEventData, GameObject> callback) 
	{
		checkEventTrigger();
		mOnUGUIClick = callback; 
	}
	public void setUGUIMouseDown(Action<PointerEventData, GameObject> callback) 
	{
		checkEventTrigger();
		mOnUGUIMouseDown = callback;
		// 因为点击事件会使用触点,为了确保触点的正确状态,所以需要在布局隐藏时清除触点
		mReceiveLayoutHide = true;
	}
	public void setUGUIMouseUp(Action<PointerEventData, GameObject> callback) 
	{
		checkEventTrigger();
		mOnUGUIMouseUp = callback;
		// 因为点击事件会使用触点,为了确保触点的正确状态,所以需要在布局隐藏时清除触点
		mReceiveLayoutHide = true;
	}
	public void setUGUIMouseEnter(Action<PointerEventData, GameObject> callback) 
	{
		checkEventTrigger();
		mOnUGUIMouseEnter = callback; 
	}
	public void setUGUIMouseExit(Action<PointerEventData, GameObject> callback) 
	{
		checkEventTrigger();
		mOnUGUIMouseLeave = callback; 
	}
	public void setUGUIMouseMove(Action<Vector2, Vector3> callback) 
	{
		checkEventTrigger();
		mOnUGUIMouseMove = callback;
		// 如果设置了要监听鼠标移动,则需要激活当前窗口
		mEnable = true;
	}
	public void setUGUIMouseStay(Action<Vector3> callback)
	{
		checkEventTrigger();
		mOnUGUIMouseStay = callback;
		mEnable = true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void checkEventTrigger()
	{
		if (mEventTriggerListener != null)
		{
			return;
		}
		mEventTriggerListener = getUnityComponent<EventTriggerListener>();
		mEventTriggerListener.mOnClick += onUGUIClick;
		mEventTriggerListener.mOnDown += onUGUIMouseDown;
		mEventTriggerListener.mOnUp += onUGUIMouseUp;
		mEventTriggerListener.mOnEnter += onUGUIMouseEnter;
		mEventTriggerListener.mOnExit += onUGUIMouseLeave;
	}
	protected static int compareZDecending(Transform a, Transform b) { return (int)sign(b.localPosition.z - a.localPosition.z); }
	protected void onUGUIMouseDown(PointerEventData eventData, GameObject go)
	{
		// 如果当前正在被按下,则不允许再响应按下事件,否则会影响正在进行的按下逻辑
		if (mMousePointer != null)
		{
			return;
		}		
		mMousePointer = eventData;
		mOnUGUIMouseDown?.Invoke(eventData, go);
	}
	protected void onUGUIMouseUp(PointerEventData eventData, GameObject go)
	{
		// 不是来自于当前按下的触点的事件不需要处理
		if (mMousePointer != eventData)
		{
			return;
		}
		mMousePointer = null;
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