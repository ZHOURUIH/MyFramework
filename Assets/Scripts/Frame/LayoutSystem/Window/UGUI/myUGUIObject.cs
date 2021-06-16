using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class myUGUIObject : myUIObject
{
	protected Action<PointerEventData, GameObject> mOnUGUIClick;
	protected Action<PointerEventData, GameObject> mOnUGUIMouseDown;
	protected Action<PointerEventData, GameObject> mOnUGUIMouseUp;
	protected Action<PointerEventData, GameObject> mOnUGUIMouseEnter;
	protected Action<PointerEventData, GameObject> mOnUGUIMouseLeave;
	protected Action<Vector2> mOnUGUIMouseMove;
	protected EventTriggerListener mEventTriggerListener;
	protected RectTransform mRectTransform;
	protected float mDefaultAlpha;
	protected bool mUGUIMouseDown;
	protected static Comparison<Transform> mCompareDescend = compareZDecending;
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
		mEventTriggerListener = EventTriggerListener.Get(mObject);
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
		}
	}
	public RectTransform getRectTransform() { return mRectTransform; }
	public override bool selfAlphaChild() { return false; }
	public override void setWindowSize(Vector2 size)
	{
		setRectSize(mRectTransform, size, false);
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
	//--------------------------------------------------------------------------------------------------------
	protected static int compareZDecending(Transform a, Transform b)
	{
		return (int)sign(b.localPosition.z - a.localPosition.z);
	}
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