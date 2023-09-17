using System;
using UnityEngine;
using System.Collections.Generic;
using static MathUtility;
using static FrameBase;
using static FrameUtility;
using static FrameDefine;

// Window的鼠标相关事件的逻辑
public class COMWindowInteractive : GameComponent
{
	protected List<LongPressData> mLongPressList;               // 长按事件列表,可同时设置不同时长的长按回调事件
	protected ObjectDoubleClickCallback mDoubleClickCallback;   // 双击回调,由GlobalTouchSystem驱动
	protected ObjectPreClickCallback mObjectPreClickCallback;   // 单击的预回调,单击时会首先调用此回调,由GlobalTouchSystem驱动
	protected OnReceiveDrag mOnReceiveDrag;                     // 接收到有物体拖到当前窗口时的回调,由GlobalTouchSystem驱动
	protected OnDragHover mOnDragHover;                         // 有物体拖拽悬停到当前窗口时的回调,由GlobalTouchSystem驱动
	protected ObjectClickCallback mClickCallback;               // 单击回调,在预回调之后调用,由GlobalTouchSystem驱动
	protected ObjectHoverCallback mHoverCallback;               // 悬停回调,由GlobalTouchSystem驱动
	protected ObjectPressCallback mPressCallback;               // 按下时回调,由GlobalTouchSystem驱动
	protected OnScreenMouseUp mOnScreenMouseUp;                 // 屏幕上鼠标抬起的回调,无论鼠标在哪儿,由GlobalTouchSystem驱动
	protected OnMouseEnter mOnMouseEnter;                       // 鼠标进入时的回调,由GlobalTouchSystem驱动
	protected OnMouseLeave mOnMouseLeave;                       // 鼠标离开时的回调,由GlobalTouchSystem驱动
	protected OnMouseMove mOnMouseMove;                         // 鼠标移动的回调,由GlobalTouchSystem驱动
	protected OnMouseStay mOnMouseStay;                         // 鼠标静止在当前窗口内的回调,由GlobalTouchSystem驱动
	protected OnMouseDown mOnMouseDown;                         // 鼠标按下的回调,由GlobalTouchSystem驱动
	protected OnMouseUp mOnMouseUp;                             // 鼠标抬起的回调,由GlobalTouchSystem驱动
	protected DateTime mLastClickTime;                          // 上一次点击操作的时间,用于双击检测
	protected DateTime mMouseDownTime;                          // 鼠标按下时的时间点
	protected Vector3 mMouseDownPosition;                       // 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
	protected UIDepth mDepth;                                   // UI深度,深度越大,渲染越靠前,越先接收到输入事件
	protected object mObjectPreClickUserData;                   // 回调函数参数
	protected float mLongPressLengthThreshold;                  // 小于0表示不判断鼠标移动对长按检测的影响
	protected float mPressedTime;                               // 小于0表示未计时,大于等于0表示正在计时长按操作,防止长时间按下时总会每隔指定时间调用一次回调
	protected int mDownTouchID;                                 // 在此窗口下按下的触点ID
	protected bool mDepthOverAllChild;                          // 计算深度时是否将深度设置为所有子节点之上,实际调整的是mExtraDepth
	protected bool mMouseHovered;                               // 当前鼠标是否悬停在窗口上
	protected bool mPressing;                                   // 鼠标当前是否在窗口中处于按下状态,鼠标离开窗口时认为鼠标不在按下状态
	protected bool mPassRay;                                    // 当存在且注册了碰撞体时是否允许射线穿透
	protected bool mColliderForClick;                           // 窗口上的碰撞体是否是用于鼠标点击的
	protected bool mAllowGenerateDepth;                         // 是否允许为当前窗口以及所有子节点计算深度,此变量只是计算深度的条件之一,一般由外部设置
	public COMWindowInteractive()
	{
		mLongPressList = new List<LongPressData>();
		mLastClickTime = DateTime.Now;
		mLongPressLengthThreshold = -1.0f;
		mPressedTime = -1.0f;
		mPassRay = true;
		mColliderForClick = true;
		mAllowGenerateDepth = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLongPressList.Clear();
		mDoubleClickCallback = null;
		mObjectPreClickCallback = null;
		mOnReceiveDrag = null;
		mOnDragHover = null;
		mClickCallback = null;
		mHoverCallback = null;
		mPressCallback = null;
		mOnScreenMouseUp = null;
		mOnMouseEnter = null;
		mOnMouseLeave = null;
		mOnMouseMove = null;
		mOnMouseStay = null;
		mOnMouseDown = null;
		mOnMouseUp = null;
		mLastClickTime = DateTime.Now;
		mMouseDownTime = DateTime.Now;
		mMouseDownPosition = Vector3.zero;
		mDepth = null;
		mObjectPreClickUserData = null;
		mLongPressLengthThreshold = -1.0f;
		mPressedTime = -1.0f;
		mDownTouchID = 0;
		mDepthOverAllChild = false;
		mMouseHovered = false;
		mPressing = false;
		mPassRay = true;
		mColliderForClick = true;
		mAllowGenerateDepth = true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 长按检测,mPressedTime小于0表示长按计时无效
		if (mPressing &&
			(mLongPressLengthThreshold < 0.0f ||
			lengthLess(mMouseDownPosition - mInputSystem.getTouchPoint(mDownTouchID).getCurPosition(), mLongPressLengthThreshold)))
		{
			mPressedTime += elapsedTime;
		}
		else
		{
			mPressedTime = -1.0f;
		}

		if (mPressing)
		{
			for (int i = 0; i < mLongPressList.Count; ++i)
			{
				mLongPressList[i]?.update(mPressedTime);
			}
		}
		// 因为外部移除长按事件时只是将列表中对象设置为空,这是为了避免在遍历中移除而出错,所以此处需要再检查有没有已经被移除的长按事件
		for (int i = 0; i < mLongPressList.Count; ++i)
		{
			if (mLongPressList[i] == null)
			{
				mLongPressList.RemoveAt(i);
				--i;
			}
		}
	}
	public OnScreenMouseUp getOnScreenMouseUp() { return mOnScreenMouseUp; }
	public bool isPassRay() { return mPassRay; }
	public bool isMouseHovered() { return mMouseHovered; }
	public float getLongPressLengthThreshold() { return mLongPressLengthThreshold; }
	public bool isColliderForClick() { return mColliderForClick; }
	public UIDepth getDepth() 
	{
		if (mDepth == null)
		{
			mDepth = new UIDepth();
		}
		return mDepth; 
	}
	public bool isDepthOverAllChild() { return mDepthOverAllChild; }
	public bool isAllowGenerateDepth() { return mAllowGenerateDepth; }
	public void setDepth(UIDepth parentDepth, int orderInParent)
	{
		if (mDepth == null)
		{
			mDepth = new UIDepth();
		}
		mDepth.setDepthValue(parentDepth, orderInParent, mDepthOverAllChild);
	}
	public void setDepthOverAllChild(bool depthOver) { mDepthOverAllChild = depthOver; }
	public void setAllowGenerateDepth(bool allowGenerate) { mAllowGenerateDepth = allowGenerate; }
	public void setPassRay(bool passRay) { mPassRay = passRay; }
	public void setLongPressLengthThreshold(float threshold) { mLongPressLengthThreshold = threshold; }
	public void setPreClickCallback(ObjectPreClickCallback callback, object userData) { mObjectPreClickCallback = callback; mObjectPreClickUserData = userData; }
	public void setClickCallback(ObjectClickCallback callback) { mClickCallback = callback; }
	public void setHoverCallback(ObjectHoverCallback callback) { mHoverCallback = callback; }
	public void setPressCallback(ObjectPressCallback callback) { mPressCallback = callback; }
	public void setDoubleClickCallback(ObjectDoubleClickCallback callback) { mDoubleClickCallback = callback; }
	public void setOnReceiveDrag(OnReceiveDrag callback) { mOnReceiveDrag = callback; }
	public void setOnDragHover(OnDragHover callback) { mOnDragHover = callback; }
	public void setOnMouseEnter(OnMouseEnter callback) { mOnMouseEnter = callback; }
	public void setOnMouseLeave(OnMouseLeave callback) { mOnMouseLeave = callback; }
	public void setOnMouseDown(OnMouseDown callback) { mOnMouseDown = callback; }
	public void setOnMouseUp(OnMouseUp callback) { mOnMouseUp = callback; }
	public void setOnMouseMove(OnMouseMove callback) { mOnMouseMove = callback; }
	public void setOnMouseStay(OnMouseStay callback) { mOnMouseStay = callback; }
	public void setOnScreenMouseUp(OnScreenMouseUp callback) { mOnScreenMouseUp = callback; }
	public void setColliderForClick(bool forClick) { mColliderForClick = forClick; }
	public void addLongPress(OnLongPress callback, OnLongPressing pressingCallback, float pressTime)
	{
		// 先判断是否已经有此长按回调了
		for (int i = 0; i < mLongPressList.Count; ++i)
		{
			if (mLongPressList[i].mOnLongPress == callback)
			{
				return;
			}
		}
		CLASS(out LongPressData data);
		data.mOnLongPress = callback;
		data.mOnLongPressing = pressingCallback;
		data.mLongPressTime = pressTime;
		mLongPressList.Add(data);
	}
	public void removeLongPress(OnLongPress callback)
	{
		for (int i = 0; i < mLongPressList.Count; ++i)
		{
			LongPressData data = mLongPressList[i];
			if (data.mOnLongPress == callback)
			{
				UN_CLASS(ref data);
				break;
			}
		}
	}
	public void clearLongPress()
	{
		UN_CLASS_LIST(mLongPressList);
	}
	public void onMouseEnter(Vector3 mousePos, int touchID)
	{
		if (!mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(mComponentOwner as myUIObject, mousePos, true);
		}
		mOnMouseEnter?.Invoke(mComponentOwner as myUIObject, mousePos, touchID);
		mOnMouseMove?.Invoke(mousePos, Vector3.zero, 0.0f, touchID);
	}
	public void onMouseLeave(Vector3 mousePos, int touchID)
	{
		if (mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(mComponentOwner as myUIObject, mousePos, false);
		}

		mPressing = false;
		mPressedTime = -1.0f;
		mOnMouseLeave?.Invoke(mComponentOwner as myUIObject, mousePos, touchID);
		for (int i = 0; i < mLongPressList.Count; ++i)
		{
			mLongPressList[i].mOnLongPressing?.Invoke(0.0f);
		}
	}
	// 鼠标左键在窗口内按下
	public void onMouseDown(Vector3 mousePos, int touchID)
	{
		mPressing = true;
		mPressedTime = 0.0f;
		mMouseDownPosition = mousePos;
		mMouseDownTime = DateTime.Now;
		mDownTouchID = touchID;
		mPressCallback?.Invoke(mComponentOwner as myUIObject, mousePos, true);
		mOnMouseDown?.Invoke(mousePos, touchID);
		for (int i = 0; i < mLongPressList.Count; ++i)
		{
			mLongPressList[i]?.mOnLongPressing?.Invoke(0.0f);
			mLongPressList[i]?.reset();
		}

		// 如果是触屏的触点,则触点在当前窗口内按下时,认为时开始悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && !mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(mComponentOwner as myUIObject, mousePos, true);
		}
	}
	// 鼠标左键在窗口内放开
	public void onMouseUp(Vector3 mousePos, int touchID)
	{
		mPressing = false;
		mPressedTime = -1.0f;
		mPressCallback?.Invoke(mComponentOwner as myUIObject, mousePos, false);
		if (lengthLess(mMouseDownPosition - mousePos, CLICK_LENGTH) &&
			(DateTime.Now - mMouseDownTime).TotalSeconds < CLICK_TIME)
		{
			mObjectPreClickCallback?.Invoke(mComponentOwner as myUIObject, mObjectPreClickUserData);
			mClickCallback?.Invoke(mComponentOwner as myUIObject, mousePos);
			if (mDoubleClickCallback != null && (DateTime.Now - mLastClickTime).TotalSeconds < DOUBLE_CLICK_TIME)
			{
				mDoubleClickCallback(mComponentOwner as myUIObject, mousePos);
			}
			mLastClickTime = DateTime.Now;
		}
		mOnMouseUp?.Invoke(mousePos, touchID);
		for (int i = 0; i < mLongPressList.Count; ++i)
		{
			mLongPressList[i].mOnLongPressing?.Invoke(0.0f);
		}

		// 如果是触屏的触点,则触点在当前窗口内抬起时,认为已经取消悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(mComponentOwner as myUIObject, mousePos, false);
		}
	}
	// 触点在移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		mOnMouseMove?.Invoke(mousePos, moveDelta, moveTime, touchID);
	}
	// 触点没有移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public void onMouseStay(Vector3 mousePos, int touchID)
	{
		mOnMouseStay?.Invoke(mousePos, touchID);
	}
	// 鼠标在屏幕上抬起
	public void onScreenMouseUp(Vector3 mousePos, int touchID)
	{
		mPressing = false;
		mPressedTime = -1.0f;
		mOnScreenMouseUp?.Invoke(mComponentOwner as myUIObject, mousePos, touchID);
	}
	// 鼠标在屏幕上按下
	public void onScreenMouseDown(Vector3 mousePos, int touchID) { }
	// 有物体拖动到了当前窗口上
	public void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, BOOL continueEvent)
	{
		if (mOnReceiveDrag != null)
		{
			continueEvent.set(false);
			mOnReceiveDrag(dragObj, mousePos, continueEvent);
		}
	}
	// 有物体拖动到了当前窗口上
	public void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover)
	{
		mOnDragHover?.Invoke(dragObj, mousePos, hover);
	}
	//------------------------------------------------------------------------------------------------------------------------------
}