using UnityEngine;
using System;
using static MathUtility;
using static FrameBase;
using static FrameDefine;

// 物体的鼠标事件逻辑组件
public class COMMovableObjectInteractive : GameComponent
{
	protected ObjectClickCallback mClickCallback;   // 鼠标点击物体的回调
	protected ObjectHoverCallback mHoverCallback;   // 鼠标悬停在物体上的持续回调
	protected ObjectPressCallback mPressCallback;   // 鼠标在物体上处于按下状态的持续回调
	protected OnScreenMouseUp mOnScreenMouseUp;     // 鼠标在任意位置抬起的回调
	protected OnMouseEnter mOnMouseEnter;           // 鼠标进入物体的回调
	protected OnMouseLeave mOnMouseLeave;           // 鼠标离开物体的回调
	protected OnMouseDown mOnMouseDown;             // 鼠标在物体上按下的回调
	protected OnMouseMove mOnMouseMove;             // 鼠标在物体上移动的回调
	protected OnMouseUp mOnMouseUp;                 // 鼠标在物体上抬起的回调
	protected Vector3 mMouseDownPosition;           // 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
	protected bool mMouseHovered;                   // 鼠标当前是否悬停在物体上
	protected bool mHandleInput;                    // 是否接收鼠标输入事件
	protected bool mPassRay;                        // 是否允许射线穿透
	public COMMovableObjectInteractive()
	{
		mHandleInput = true;
		mPassRay = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mClickCallback = null;
		mHoverCallback = null;
		mPressCallback = null;
		mOnScreenMouseUp = null;
		mOnMouseEnter = null;
		mOnMouseLeave = null;
		mOnMouseDown = null;
		mOnMouseMove = null;
		mOnMouseUp = null;
		mMouseDownPosition = Vector3.zero;
		mMouseHovered = false;
		mHandleInput = true;
		mPassRay = true;
	}
	public bool isHandleInput() { return mHandleInput; }
	public bool isReceiveScreenMouse() { return mOnScreenMouseUp != null; }
	public bool isPassRay() { return mPassRay; }
	public bool isMouseHovered() { return mMouseHovered; }
	public void setPassRay(bool passRay) { mPassRay = passRay; }
	public void setHandleInput(bool handleInput) { mHandleInput = handleInput; }
	public void setOnMouseEnter(OnMouseEnter callback) { mOnMouseEnter = callback; }
	public void setOnMouseLeave(OnMouseLeave callback) { mOnMouseLeave = callback; }
	public void setOnMouseDown(OnMouseDown callback) { mOnMouseDown = callback; }
	public void setOnMouseUp(OnMouseUp callback) { mOnMouseUp = callback; }
	public void setOnMouseMove(OnMouseMove callback) { mOnMouseMove = callback; }
	public void setClickCallback(ObjectClickCallback callback) { mClickCallback = callback; }
	public void setHoverCallback(ObjectHoverCallback callback) { mHoverCallback = callback; }
	public void setPressCallback(ObjectPressCallback callback) { mPressCallback = callback; }
	public void setOnScreenMouseUp(OnScreenMouseUp callback) { mOnScreenMouseUp = callback; }
	public void onMouseEnter(Vector3 mousePos, int touchID)
	{
		if (!mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(mComponentOwner as MovableObject, mousePos, true);
		}
		mOnMouseEnter?.Invoke(mComponentOwner as MovableObject, mousePos, touchID);
		mOnMouseMove?.Invoke(mousePos, Vector3.zero, 0.0f, touchID);
	}
	public void onMouseLeave(Vector3 mousePos, int touchID)
	{
		if (mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(mComponentOwner as MovableObject, mousePos, false);
		}
		mOnMouseLeave?.Invoke(mComponentOwner as MovableObject, mousePos, touchID);
	}
	// 鼠标左键在窗口内按下
	public void onMouseDown(Vector3 mousePos, int touchID)
	{
		mMouseDownPosition = mousePos;
		mPressCallback?.Invoke(mComponentOwner as MovableObject, mousePos, true);
		mOnMouseDown?.Invoke(mousePos, touchID);

		// 如果是触屏的触点,则触点在当前物体内按下时,认为时开始悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && !mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(mComponentOwner as MovableObject, mousePos, true);
		}
	}
	// 鼠标左键在窗口内放开
	public void onMouseUp(Vector3 mousePos, int touchID)
	{
		mPressCallback?.Invoke(mComponentOwner as MovableObject, mousePos, false);
		if (lengthLess(mMouseDownPosition - mousePos, CLICK_LENGTH))
		{
			mClickCallback?.Invoke(mComponentOwner as MovableObject, mousePos);
		}
		mOnMouseUp?.Invoke(mousePos, touchID);

		// 如果是触屏的触点,则触点在当前物体内抬起时,认为已经取消悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(mComponentOwner as MovableObject, mousePos, false);
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
	public void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, BOOL continueEvent) { }
	public void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover) { }
	public void onMultiTouchStart(Vector3 touch0, Vector3 touch1) { }
	public void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1) { }
	public void onMultiTouchEnd() { }
}