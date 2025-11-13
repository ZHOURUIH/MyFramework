using System;
using UnityEngine;
using System.Collections.Generic;
using static MathUtility;
using static FrameBaseHotFix;
using static FrameUtility;
using static FrameDefine;
using static AT;

// Window的鼠标相关事件的逻辑
public class ComponentInteractive : GameComponent
{
	protected List<LongPressData> mLongPressList = new();       // 长按事件列表,可同时设置不同时长的长按回调事件
	protected Action mDoubleClickCallback;						// 双击回调,由GlobalTouchSystem驱动
	protected Vector3Callback mDoubleClickDetailCallback;		// 双击回调,由GlobalTouchSystem驱动,带当前触点坐标
	protected Action mPreClickCallback;							// 单击的预回调,单击时会首先调用此回调,由GlobalTouchSystem驱动
	protected Vector3Callback mPreClickDetailCallback;			// 单击的预回调,单击时会首先调用此回调,由GlobalTouchSystem驱动,带当前触点坐标
	protected ReceiveDragCallback mOnReceiveDrag;				// 接收到有物体拖到当前窗口时的回调,由GlobalTouchSystem驱动
	protected DragHoverCallback mOnDragHover;					// 有物体拖拽悬停到当前窗口时的回调,由GlobalTouchSystem驱动
	protected Action mClickCallback;							// 单击回调,在预回调之后调用,由GlobalTouchSystem驱动
	protected Vector3Callback mClickDetailCallback;				// 单击回调,在预回调之后调用,由GlobalTouchSystem驱动,带当前触点坐标
	protected BoolCallback mHoverCallback;                      // 悬停回调,由GlobalTouchSystem驱动
	protected Vector3BoolCallback mHoverDetailCallback;			// 悬停回调,由GlobalTouchSystem驱动,带当前触点坐标
	protected BoolCallback mPressCallback;						// 按下时回调,由GlobalTouchSystem驱动
	protected Vector3BoolCallback mPressDetailCallback;			// 按下时回调,由GlobalTouchSystem驱动,带当前触点坐标
	protected Vector3IntCallback mOnScreenTouchUp;              // 屏幕上触点抬起的回调,无论触点在哪儿,由GlobalTouchSystem驱动
	protected Vector3IntCallback mOnScreenTouchDown;            // 屏幕上触点按下的回调,无论触点在哪儿,由GlobalTouchSystem驱动
	protected Vector3IntCallback mOnTouchEnter;                 // 触点进入时的回调,由GlobalTouchSystem驱动
	protected Vector3IntCallback mOnTouchLeave;                 // 触点离开时的回调,由GlobalTouchSystem驱动
	protected TouchMoveCallback mOnTouchMove;                         // 触点移动的回调,由GlobalTouchSystem驱动
	protected Vector3IntCallback mOnTouchStay;                  // 触点静止在当前窗口内的回调,由GlobalTouchSystem驱动
	protected Vector3IntCallback mOnTouchDown;                  // 触点按下的回调,由GlobalTouchSystem驱动
	protected Vector3IntCallback mOnTouchUp;                    // 触点抬起的回调,由GlobalTouchSystem驱动
	protected DateTime mLastClickTime;                          // 上一次点击操作的时间,用于双击检测
	protected DateTime mTouchDownTime;                          // 触点按下时的时间点
	protected Vector3 mTouchDownPosition;                       // 触点按下时在窗口中的位置,触点在窗口中移动时该值不改变
	protected UIDepth mDepth;                                   // UI深度,深度越大,渲染越靠前,越先接收到输入事件
	protected float mLongPressLengthThreshold = -1.0f;          // 小于0表示不判断触点移动对长按检测的影响
	protected float mPressedTime = -1.0f;                       // 小于0表示未计时,大于等于0表示正在计时长按操作,防止长时间按下时总会每隔指定时间调用一次回调
	protected int mDownTouchID;                                 // 在此窗口下按下的触点ID
	protected int mClickSound;									// 点击时播放的音效ID,由于按钮音效播放的操作较多,所以统一到此处实现最基本的按钮点击音效播放
	protected bool mDepthOverAllChild;                          // 计算深度时是否将深度设置为所有子节点之上,实际调整的是mExtraDepth
	protected bool mMouseHovered;                               // 当前鼠标是否悬停在窗口上
	protected bool mPressing;                                   // 触点当前是否在窗口中处于按下状态,触点离开窗口时认为触点不在按下状态
	protected bool mPassRay = true;                             // 当存在且注册了碰撞体时是否允许射线穿透
	protected bool mHandleInput = true;							// 是否接收触点输入事件
	protected bool mColliderForClick = true;                    // 窗口上的碰撞体是否是用于触点点击的
	protected bool mAllowGenerateDepth = true;                  // 是否允许为当前窗口以及所有子节点计算深度,此变量只是计算深度的条件之一,一般由外部设置
	protected bool mPassDragEvent;                              // 是否将开始拖拽的事件穿透下去,使自己的下层也能够同时响应拖拽
	public ComponentInteractive()
	{
		mLastClickTime = DateTime.Now;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLongPressList.Clear();
		mDoubleClickCallback = null;
		mDoubleClickDetailCallback = null;
		mPreClickCallback = null;
		mPreClickDetailCallback = null;
		mOnReceiveDrag = null;
		mOnDragHover = null;
		mClickCallback = null;
		mClickDetailCallback = null;
		mHoverCallback = null;
		mHoverDetailCallback = null;
		mPressCallback = null;
		mPressDetailCallback = null;
		mOnScreenTouchUp = null;
		mOnScreenTouchDown = null;
		mOnTouchEnter = null;
		mOnTouchLeave = null;
		mOnTouchMove = null;
		mOnTouchStay = null;
		mOnTouchDown = null;
		mOnTouchUp = null;
		mLastClickTime = DateTime.Now;
		mTouchDownTime = DateTime.Now;
		mTouchDownPosition = Vector3.zero;
		mDepth = null;
		mLongPressLengthThreshold = -1.0f;
		mPressedTime = -1.0f;
		mDownTouchID = 0;
		mClickSound = 0;
		mDepthOverAllChild = false;
		mMouseHovered = false;
		mPressing = false;
		mPassRay = true;
		mHandleInput = true;
		mColliderForClick = true;
		mAllowGenerateDepth = true;
		mPassDragEvent = false;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 长按检测,mPressedTime小于0表示长按计时无效
		if (mPressing &&
			(mLongPressLengthThreshold < 0.0f ||
			lengthLess(mTouchDownPosition - mInputSystem.getTouchPoint(mDownTouchID).getCurPosition(), mLongPressLengthThreshold)))
		{
			mPressedTime += elapsedTime;
		}
		else
		{
			mPressedTime = -1.0f;
		}

		if (mPressing)
		{
			foreach (LongPressData item in mLongPressList)
			{
				item?.update(mPressedTime);
			}
		}
		// 因为外部移除长按事件时只是将列表中对象设置为空,这是为了避免在遍历中移除而出错,所以此处需要再检查有没有已经被移除的长按事件
		for (int i = 0; i < mLongPressList.Count; ++i)
		{
			if (mLongPressList[i] == null)
			{
				mLongPressList.RemoveAt(i--);
			}
		}
	}
	public bool isHandleInput()											{ return mHandleInput; }
	public bool isReceiveScreenMouse()									{ return mOnScreenTouchUp != null; }
	public Vector3IntCallback getOnScreenTouchUp()						{ return mOnScreenTouchUp; }
	public Vector3IntCallback getOnScreenTouchDown()					{ return mOnScreenTouchDown; }
	public bool isPassRay()												{ return mPassRay; }
	public bool isPassDragEvent()										{ return mPassDragEvent; }
	public bool isMouseHovered()										{ return mMouseHovered; }
	public float getLongPressLengthThreshold()							{ return mLongPressLengthThreshold; }
	public bool isColliderForClick()									{ return mColliderForClick; }
	public UIDepth getDepth()											{ return mDepth ??= new(); }
	public bool isDepthOverAllChild()									{ return mDepthOverAllChild; }
	public bool isAllowGenerateDepth()									{ return mAllowGenerateDepth; }
	public int getClickSound()											{ return mClickSound; }
	public void setDepth(UIDepth parentDepth, int orderInParent)
	{
		mDepth ??= new();
		mDepth.setDepthValue(parentDepth, orderInParent, mDepthOverAllChild);
	}
	public void setHandleInput(bool handleInput)						{ mHandleInput = handleInput; }
	public void setDepthOverAllChild(bool depthOver)					{ mDepthOverAllChild = depthOver; }
	public void setAllowGenerateDepth(bool allowGenerate)				{ mAllowGenerateDepth = allowGenerate; }
	public void setPassRay(bool passRay)								{ mPassRay = passRay; }
	public void setPassDragEvent(bool pass)								{ mPassDragEvent = pass; }
	public void setLongPressLengthThreshold(float threshold)			{ mLongPressLengthThreshold = threshold; }
	public void setClickSound(int sound)								{ mClickSound = sound; }
	public void setPreClickCallback(Action callback)					{ mPreClickCallback = callback; }
	public void setPreClickDetailCallback(Vector3Callback callback)		{ mPreClickDetailCallback = callback; }
	public void setClickCallback(Action callback)						{ mClickCallback = callback; }
	public void setClickDetailCallback(Vector3Callback callback)		{ mClickDetailCallback = callback; }
	public void setHoverCallback(BoolCallback callback)					{ mHoverCallback = callback; }
	public void setHoverDetailCallback(Vector3BoolCallback callback)	{ mHoverDetailCallback = callback; }
	public void setPressCallback(BoolCallback callback)					{ mPressCallback = callback; }
	public void setPressDetailCallback(Vector3BoolCallback callback)	{ mPressDetailCallback = callback; }
	public void setDoubleClickCallback(Action callback)					{ mDoubleClickCallback = callback; }
	public void setDoubleClickDetailCallback(Vector3Callback callback)	{ mDoubleClickDetailCallback = callback; }
	public void setOnReceiveDrag(ReceiveDragCallback callback)			{ mOnReceiveDrag = callback; }
	public void setOnDragHover(DragHoverCallback callback)				{ mOnDragHover = callback; }
	public void setOnTouchEnter(Vector3IntCallback callback)			{ mOnTouchEnter = callback; }
	public void setOnTouchLeave(Vector3IntCallback callback)			{ mOnTouchLeave = callback; }
	public void setOnTouchDown(Vector3IntCallback callback)				{ mOnTouchDown = callback; }
	public void setOnTouchUp(Vector3IntCallback callback)				{ mOnTouchUp = callback; }
	public void setOnTouchMove(TouchMoveCallback callback)					{ mOnTouchMove = callback; }
	public void setOnTouchStay(Vector3IntCallback callback)				{ mOnTouchStay = callback; }
	public void setOnScreenTouchUp(Vector3IntCallback callback)			{ mOnScreenTouchUp = callback; }
	public void setOnScreenTouchDown(Vector3IntCallback callback)		{ mOnScreenTouchDown = callback; }
	public void setColliderForClick(bool forClick)						{ mColliderForClick = forClick; }
	public void addLongPress(Action callback, float pressTime, FloatCallback pressingCallback = null)
	{
		// 先判断是否已经有此长按回调了
		foreach (LongPressData item in mLongPressList)
		{
			if (item.mOnLongPress == callback)
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
	public void removeLongPress(Action callback)
	{
		foreach (LongPressData data in mLongPressList)
		{
			if (data.mOnLongPress == callback)
			{
				UN_CLASS(data);
				break;
			}
		}
	}
	public void clearLongPress()
	{
		UN_CLASS_LIST(mLongPressList);
	}
	public void onTouchEnter(Vector3 touchPos, int touchID)
	{
		if (!mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(true);
			mHoverDetailCallback?.Invoke(touchPos, true);
		}
		mOnTouchEnter?.Invoke(touchPos, touchID);
		mOnTouchMove?.Invoke(touchPos, Vector3.zero, 0.0f, touchID);
	}
	public void onTouchLeave(Vector3 touchPos, int touchID)
	{
		if (mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(false);
			mHoverDetailCallback?.Invoke(touchPos, false);
		}

		mPressing = false;
		mPressedTime = -1.0f;
		mOnTouchLeave?.Invoke(touchPos, touchID);
		foreach (LongPressData data in mLongPressList)
		{
			data.mOnLongPressing?.Invoke(0.0f);
		}
	}
	// 鼠标左键在窗口内按下
	public void onTouchDown(Vector3 touchPos, int touchID)
	{
		mPressing = true;
		mPressedTime = 0.0f;
		mTouchDownPosition = touchPos;
		mTouchDownTime = DateTime.Now;
		mDownTouchID = touchID;
		mPressCallback?.Invoke(true);
		mPressDetailCallback?.Invoke(touchPos, true);
		mOnTouchDown?.Invoke(touchPos, touchID);
		foreach (LongPressData data in mLongPressList)
		{
			data?.mOnLongPressing?.Invoke(0.0f);
			data?.reset();
		}

		// 如果是触屏的触点,则触点在当前窗口内按下时,认为时开始悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && !mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(true);
			mHoverDetailCallback?.Invoke(touchPos, true);
		}
	}
	// 鼠标左键在窗口内放开
	public void onTouchUp(Vector3 touchPos, int touchID)
	{
		mPressing = false;
		mPressedTime = -1.0f;
		mPressCallback?.Invoke(false);
		mPressDetailCallback?.Invoke(touchPos, false);
		if (lengthLess(mTouchDownPosition - touchPos, CLICK_LENGTH) &&
		   (DateTime.Now - mTouchDownTime).TotalSeconds < CLICK_TIME)
		{
			mPreClickCallback?.Invoke();
			mPreClickDetailCallback?.Invoke(touchPos);
			mClickCallback?.Invoke();
			mClickDetailCallback?.Invoke(touchPos);
			if (mClickSound > 0)
			{
				SOUND_2D(mClickSound);
			}
			if ((DateTime.Now - mLastClickTime).TotalSeconds < DOUBLE_CLICK_TIME)
			{
				mDoubleClickCallback?.Invoke();
				mDoubleClickDetailCallback?.Invoke(touchPos);
			}
			mLastClickTime = DateTime.Now;
		}
		mOnTouchUp?.Invoke(touchPos, touchID);
		foreach (LongPressData data in mLongPressList)
		{
			data.mOnLongPressing?.Invoke(0.0f);
		}

		// 如果是触屏的触点,则触点在当前窗口内抬起时,认为已经取消悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(false);
			mHoverDetailCallback?.Invoke(touchPos, false);
		}
	}
	// 触点在移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public void onTouchMove(Vector3 touchPos, Vector3 moveDelta, float moveTime, int touchID)
	{
		mOnTouchMove?.Invoke(touchPos, moveDelta, moveTime, touchID);
	}
	// 触点没有移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public void onTouchStay(Vector3 touchPos, int touchID)
	{
		mOnTouchStay?.Invoke(touchPos, touchID);
	}
	// 鼠标在屏幕上抬起
	public void onScreenTouchUp(Vector3 touchPos, int touchID)
	{
		mPressing = false;
		mPressedTime = -1.0f;
		mOnScreenTouchUp?.Invoke(touchPos, touchID);
	}
	// 鼠标在屏幕上按下
	public void onScreenTouchDown(Vector3 touchPos, int touchID) { mOnScreenTouchDown?.Invoke(touchPos, touchID); }
	// 有物体拖动到了当前对象上
	public void onReceiveDrag(IMouseEventCollect dragObj, Vector3 touchPos, ref bool continueEvent)
	{
		if (mOnReceiveDrag != null)
		{
			continueEvent = false;
			mOnReceiveDrag(dragObj, touchPos, ref continueEvent);
		}
	}
	// 有物体拖动到了当前对象上
	public void onDragHovered(IMouseEventCollect dragObj, Vector3 touchPos, bool hover)
	{
		mOnDragHover?.Invoke(dragObj, touchPos, hover);
	}
	public void onMultiTouchStart(Vector3 touch0, Vector3 touch1) 
	{
		// 暂时没有实现
	}
	public void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1) 
	{
		// 暂时没有实现
	}
	public void onMultiTouchEnd() 
	{
		// 暂时没有实现
	}
}