using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static MathUtility;

// UGUIWindow的鼠标相关事件的逻辑
public class COMWindowUGUIInteractive : GameComponent
{
	protected Action<PointerEventData, GameObject> mOnUGUIMouseEnter;           // 鼠标进入的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIMouseLeave;           // 鼠标离开的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIMouseDown;            // 鼠标按下的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIMouseUp;              // 鼠标抬起的回调,由UGUI触发
	protected Action<PointerEventData, GameObject> mOnUGUIClick;                // 鼠标点击的回调,由UGUI触发
	protected Action<Vector2, Vector3> mOnUGUIMouseMove;                        // 鼠标移动的回调,由UGUI触发,第一个参数是这一帧触点的移动量,第二个参数是触点当前的位置
	protected Action<Vector3> mOnUGUIMouseStay;                                 // 鼠标在窗口内停止的回调,由UGUI触发,参数是触点当前的位置
	protected EventTriggerListener mEventTriggerListener;                       // UGUI事件监听器,用于接收UGUI的事件
	protected PointerEventData mMousePointer;                                   // 鼠标在当前窗口按下时的触点信息
	public override void resetProperty()
	{
		base.resetProperty();
		mOnUGUIMouseEnter = null;
		mOnUGUIMouseLeave = null;
		mOnUGUIMouseDown = null;
		mOnUGUIMouseUp = null;
		mOnUGUIClick = null;
		mOnUGUIMouseMove = null;
		mOnUGUIMouseStay = null;
		mEventTriggerListener = null;
		mMousePointer = null;
	}
	public override void destroy()
	{
		base.destroy();
		if (mEventTriggerListener != null)
		{
			mEventTriggerListener.mOnClick -= onUGUIClick;
			mEventTriggerListener.mOnDown -= onUGUIMouseDown;
			mEventTriggerListener.mOnUp -= onUGUIMouseUp;
			mEventTriggerListener.mOnEnter -= onUGUIMouseEnter;
			mEventTriggerListener.mOnExit -= onUGUIMouseLeave;
		}
		mMousePointer = null;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
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
	public void setUGUIClick(Action<PointerEventData, GameObject> callback)
	{
		checkEventTrigger();
		mOnUGUIClick = callback;
	}
	public void setUGUIMouseDown(Action<PointerEventData, GameObject> callback)
	{
		checkEventTrigger();
		mOnUGUIMouseDown = callback;
	}
	public void setUGUIMouseUp(Action<PointerEventData, GameObject> callback)
	{
		checkEventTrigger();
		mOnUGUIMouseUp = callback;
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
	}
	public void setUGUIMouseStay(Action<Vector3> callback)
	{
		checkEventTrigger();
		mOnUGUIMouseStay = callback;
	}
	public void clearMousePointer() { mMousePointer = null; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void checkEventTrigger()
	{
		if (mEventTriggerListener != null)
		{
			return;
		}
		mEventTriggerListener = (mComponentOwner as myUGUIObject).getOrAddUnityComponent<EventTriggerListener>();
		mEventTriggerListener.mOnClick += onUGUIClick;
		mEventTriggerListener.mOnDown += onUGUIMouseDown;
		mEventTriggerListener.mOnUp += onUGUIMouseUp;
		mEventTriggerListener.mOnEnter += onUGUIMouseEnter;
		mEventTriggerListener.mOnExit += onUGUIMouseLeave;
	}
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