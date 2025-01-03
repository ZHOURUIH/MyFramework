using System;
using UnityEngine;
using static MathUtility;
using static FrameBase;
using static FrameDefine;

// 物体的鼠标事件逻辑组件
public class COMMovableObjectInteractive : GameComponent
{
	protected Action mClickCallback;						// 鼠标点击物体的回调
	protected ClickCallback mClickDetailCallback;			// 鼠标点击物体的回调,带当前触点坐标
	protected BoolCallback mHoverCallback;					// 鼠标悬停在物体上的持续回调
	protected HoverCallback mHoverDetailCallback;			// 鼠标悬停在物体上的持续回调,带当前触点坐标
	protected BoolCallback mPressCallback;					// 鼠标在物体上处于按下状态的持续回调
	protected PressCallback mPressDetailCallback;			// 鼠标在物体上处于按下状态的持续回调,带当前触点坐标
	protected OnScreenMouseUp mOnScreenMouseUp;				// 鼠标在任意位置抬起的回调
	protected OnMouseEnter mOnMouseEnter;					// 鼠标进入物体的回调
	protected OnMouseLeave mOnMouseLeave;					// 鼠标离开物体的回调
	protected Vector3IntCallback mOnMouseDown;				// 鼠标在物体上按下的回调
	protected OnMouseMove mOnMouseMove;						// 鼠标在物体上移动的回调
	protected Vector3IntCallback mOnMouseUp;				// 鼠标在物体上抬起的回调
	protected Vector3 mMouseDownPosition;					// 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
	protected DateTime mMouseDownTime;						// 鼠标按下时的时间点
	protected int mClickSound;                              // 点击时播放的音效ID,由于音效播放的操作较多,所以统一到此处实现最基本的点击音效播放
	protected bool mMouseHovered;							// 鼠标当前是否悬停在物体上
	protected bool mHandleInput;							// 是否接收鼠标输入事件
	protected bool mPassDragEvent;							// 是否将开始拖拽的事件穿透下去,使自己的下层也能够同时响应拖拽
	protected bool mPassRay;								// 是否允许射线穿透
	public COMMovableObjectInteractive()
	{
		mHandleInput = true;
		mPassRay = true;
		mMouseDownTime = DateTime.Now;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mClickCallback = null;
		mClickDetailCallback = null;
		mHoverCallback = null;
		mHoverDetailCallback = null;
		mPressCallback = null;
		mPressDetailCallback = null;
		mOnScreenMouseUp = null;
		mOnMouseEnter = null;
		mOnMouseLeave = null;
		mOnMouseDown = null;
		mOnMouseMove = null;
		mOnMouseUp = null;
		mMouseDownPosition = Vector3.zero;
		mMouseDownTime = DateTime.Now;
		mClickSound = 0;
		mMouseHovered = false;
		mHandleInput = true;
		mPassDragEvent = false;
		mPassRay = true;
	}
	public bool isHandleInput()											{ return mHandleInput; }
	public bool isReceiveScreenMouse()									{ return mOnScreenMouseUp != null; }
	public bool isPassRay()												{ return mPassRay; }
	public bool isPassDragEvent()										{ return mPassDragEvent; }
	public bool isMouseHovered()										{ return mMouseHovered; }
	public int getClickSound()											{ return mClickSound; }
	public void setPassRay(bool passRay)								{ mPassRay = passRay; }
	public void setHandleInput(bool handleInput)						{ mHandleInput = handleInput; }
	public void setOnMouseEnter(OnMouseEnter callback)					{ mOnMouseEnter = callback; }
	public void setOnMouseLeave(OnMouseLeave callback)					{ mOnMouseLeave = callback; }
	public void setOnMouseDown(Vector3IntCallback callback)				{ mOnMouseDown = callback; }
	public void setOnMouseUp(Vector3IntCallback callback)				{ mOnMouseUp = callback; }
	public void setOnMouseMove(OnMouseMove callback)					{ mOnMouseMove = callback; }
	public void setClickCallback(Action callback)						{ mClickCallback = callback; }
	public void setClickDetailCallback(ClickCallback callback)			{ mClickDetailCallback = callback; }
	public void setHoverCallback(BoolCallback callback)					{ mHoverCallback = callback; }
	public void setHoverDetailCallback(HoverCallback callback)			{ mHoverDetailCallback = callback; }
	public void setPressCallback(BoolCallback callback)					{ mPressCallback = callback; }
	public void setPressDetailCallback(PressCallback callback)			{ mPressDetailCallback = callback; }
	public void setOnScreenMouseUp(OnScreenMouseUp callback)			{ mOnScreenMouseUp = callback; }
	public void setClickSound(int sound)								{ mClickSound = sound; }
	public void onMouseEnter(Vector3 mousePos, int touchID)
	{
		var obj = mComponentOwner as MovableObject;
		if (!mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(true);
			mHoverDetailCallback?.Invoke(obj, mousePos, true);
		}
		mOnMouseEnter?.Invoke(obj, mousePos, touchID);
		mOnMouseMove?.Invoke(mousePos, Vector3.zero, 0.0f, touchID);
	}
	public void onMouseLeave(Vector3 mousePos, int touchID)
	{
		var obj = mComponentOwner as MovableObject;
		if (mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(false);
			mHoverDetailCallback?.Invoke(obj, mousePos, false);
		}
		mOnMouseLeave?.Invoke(obj, mousePos, touchID);
	}
	// 鼠标左键在窗口内按下
	public void onMouseDown(Vector3 mousePos, int touchID)
	{
		mMouseDownPosition = mousePos;
		mMouseDownTime = DateTime.Now;
		mPressCallback?.Invoke(true);
		var obj = mComponentOwner as MovableObject;
		mPressDetailCallback?.Invoke(obj, mousePos, true);
		mOnMouseDown?.Invoke(mousePos, touchID);

		// 如果是触屏的触点,则触点在当前物体内按下时,认为时开始悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && !mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(true);
			mHoverDetailCallback?.Invoke(obj, mousePos, true);
		}
	}
	// 鼠标左键在窗口内放开
	public void onMouseUp(Vector3 mousePos, int touchID)
	{
		var obj = mComponentOwner as MovableObject;
		mPressCallback?.Invoke(false);
		mPressDetailCallback?.Invoke(obj, mousePos, false);
		if (lengthLess(mMouseDownPosition - mousePos, CLICK_LENGTH) &&
			(DateTime.Now - mMouseDownTime).TotalSeconds < CLICK_TIME)
		{
			mClickCallback?.Invoke();
			mClickDetailCallback?.Invoke(obj, mousePos);
			if (mClickSound > 0)
			{
				AT.SOUND_2D(mClickSound);
			}
		}
		mOnMouseUp?.Invoke(mousePos, touchID);

		// 如果是触屏的触点,则触点在当前物体内抬起时,认为已经取消悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(false);
			mHoverDetailCallback?.Invoke(obj, mousePos, false);
		}
	}
	// 鼠标在窗口内,并且有移动
	public void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		mOnMouseMove?.Invoke(mousePos, moveDelta, moveTime, touchID);
	}
	public void onMouseStay(Vector3 mousePos, int touchID) { }
	public void onScreenMouseDown(Vector3 mousePos, int touchID) { }
	// 鼠标在屏幕上抬起
	public void onScreenMouseUp(Vector3 mousePos, int touchID)
	{
		mOnScreenMouseUp?.Invoke(mComponentOwner as MovableObject, mousePos, touchID);
	}
	public void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, ref bool continueEvent) { }
	public void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover) { }
	public void onMultiTouchStart(Vector3 touch0, Vector3 touch1) { }
	public void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1) { }
	public void onMultiTouchEnd() { }
}