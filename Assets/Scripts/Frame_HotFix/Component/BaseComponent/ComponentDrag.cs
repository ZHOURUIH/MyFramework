using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static MathUtility;

// 拖拽组件基类
public class ComponentDrag : GameComponent
{	
	protected DragStartCallback mDragStartCallback;		// 开始拖拽的回调
	protected DragEndCallback mDragEndCallback;			// 在结束拖拽,还未进行接收拖拽处理之前调用
	protected DragEndCallback mDragEndTotallyCallback;	// 在结束拖拽和接收拖拽处理全部完成以后调用
	protected DragCallback mDragingCallback;			// 拖拽过程中的回调
	protected TouchPoint mTouchPoint;					// 拖动的触点
	protected Vector3 mPrepareDragMousePosition;		// 鼠标刚按下时的坐标
	protected Vector3 mDragMouseOffset;					// 开始拖拽时,鼠标位置与窗口位置的偏移
	protected Vector2 mAllowDragDirection;              // 允许开始拖拽的方向,为0则表示不限制开始拖拽的方向
	protected float mStartDragThreshold;				// 开始拖拽的阈值,在准备拖拽阶段,拖拽的距离超过阈值后进入拖拽状态
	protected float mDragStartAngleThreshold;			// 如果有允许开始拖拽的方向,则表示当实际拖拽方向与允许的方向夹角,弧度制
	protected bool mPreparingDrag;						// 鼠标按下时只是准备开始拖动,当鼠标在准备状态下移动一定距离以后才会开始真正的拖动
	protected bool mDraging;							// 当前是否正在拖拽
	protected bool mObjectCenterAlignMouse;             // 拖拽过场中物体的中心是否对齐鼠标位置,设置为true会使mDragMouseOffset始终为0
	protected bool mMovable;                            // 拖拽时是否允许当前窗口跟随移动
	protected bool mBreakDragWhenMultiTouch;			// 是否当有多个触点时就中断拖拽,避免与多指操作冲突
	public ComponentDrag()
	{
		mStartDragThreshold = 20.0f;
		mDragStartAngleThreshold = toRadian(45.0f);
		mMovable = true;
		mBreakDragWhenMultiTouch = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mDragStartCallback = null;
		mDragEndCallback = null;
		mDragEndTotallyCallback = null;
		mDragingCallback = null;
		mTouchPoint = null;
		mPrepareDragMousePosition = Vector3.zero;
		mDragMouseOffset = Vector3.zero;
		mAllowDragDirection = Vector2.zero;
		mStartDragThreshold = 20.0f;
		mDragStartAngleThreshold = toRadian(45.0f);
		mPreparingDrag = false;
		mDraging = false;
		mMovable = true;
		mObjectCenterAlignMouse = false;
		mBreakDragWhenMultiTouch = true;
	}
	public virtual void initDrag(Vector2 dragDirection, float dragStartAngleRadian, bool centerAlignMouse, bool movable)
	{
		setAllowDragDirection(dragDirection);
		setDragStartAngleThreshold(dragStartAngleRadian);
		setObjectCenterAlignMouse(centerAlignMouse);
		setMovable(movable);
	}
	public override void update(float elapsedTime)
	{
		// 有触点按下时,检查是否可以开始拖动
		if (!mDraging && !mPreparingDrag && mInputSystem.getTouchDown(out mTouchPoint))
		{
			checkStartDrag();
		}
		if (mTouchPoint != null)
		{
			if (mTouchPoint.isCurrentUp() || (mBreakDragWhenMultiTouch && mInputSystem.getTouchPointDownCount() > 1))
			{
				touchUp(false);
			}
		}
		if (mDraging || mPreparingDrag)
		{
			touchMove();
		}
	}
	public bool isDraging()												{ return mDraging; }
	public bool isBreakDragWhenMultiTouch()								{ return mBreakDragWhenMultiTouch; }
	public TouchPoint getTouchPoint()									{ return mTouchPoint; }
	public void setObjectCenterAlignMouse(bool align)					{ mObjectCenterAlignMouse = align; }
	public void setDragStartCallback(DragStartCallback callback)		{ mDragStartCallback = callback;}
	public void setDragEndCallback(DragEndCallback callback)			{ mDragEndCallback = callback;}
	public void setDragEndTotallyCallback(DragEndCallback callback)	{ mDragEndTotallyCallback = callback; }
	public void setDragingCallback(DragCallback callback)				{ mDragingCallback = callback;}
	public void setStartDragThreshold(float threshold)					{ mStartDragThreshold = threshold; }
	public void setAllowDragDirection(Vector2 allowDirection)			{ mAllowDragDirection = allowDirection; }
	public void setDragStartAngleThreshold(float radian)				{ mDragStartAngleThreshold = radian; }
	public void setMovable(bool movable)								{ mMovable = movable; }
	public void setBreakDragWhenMultiTouch(bool breakDrag)				{ mBreakDragWhenMultiTouch = breakDrag; }
	public void setDragCallback(DragStartCallback start, DragCallback draging, DragEndCallback end)
	{
		mDragStartCallback = start;
		mDragingCallback = draging;
		mDragEndCallback = end;
	}
	public void cancelDrag()
	{
		touchUp(true);
	}
	public override void setActive(bool active)
	{
		if(active == isActive())
		{
			return;
		}
		base.setActive(active);
		touchUp(true);
		// 激活组件时,如果鼠标在当前物体上已经是按下状态,则开始拖动
		if (active)
		{
			if (!mGlobalTouchSystem.isColliderRegisted(mComponentOwner as IMouseEventCollect))
			{
				logError("组件所属物体未注册碰撞体,无法进行拖拽. " + mComponentOwner.getName() + ", desc:" + (mComponentOwner as IMouseEventCollect).getDescription());
			}
			if (mInputSystem.getTouchDown(out mTouchPoint))
			{
				checkStartDrag();
			}
		}
	}
	public override void notifyOwnerActive(bool active)
	{
		touchUp(true);
	}
	public override void destroy()
	{
		base.destroy();
		touchUp(true);
	}
	// 手动调用开始拖拽
	public bool startDrag(TouchPoint touchPoint, Vector3 dragOffset)
	{
		mDraging = onDragStart(touchPoint);
		if(!mDraging)
		{
			return false;
		}
		mTouchPoint = touchPoint;
		mDragMouseOffset = dragOffset;
		mPreparingDrag = false;
		Vector3 mousePosition = touchPoint.getCurPosition();
		if (mMovable)
		{
			applyScreenPosition(mousePosition);
		}
		onDraging(mousePosition);
		return true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected bool checkStartDrag()
	{
		// 触点位置不在当前物体上则不能拖拽
		Vector3 touchPosition = mTouchPoint.getCurPosition();
		if (!mouseInObject(touchPosition))
		{
			mTouchPoint = null;
			return false;
		}

		bool result = false;
		// 拖拽消息不向下传递,从上往下查找,如果前面没有窗口需要有拖拽消息被处理,则当前窗口响应拖拽消息
		using var a = new ListScope<IMouseEventCollect>(out var hoverList);
		mGlobalTouchSystem.getAllHoverObject(hoverList, touchPosition);
		foreach (IMouseEventCollect item in hoverList)
		{
			if (item == mComponentOwner)
			{
				touchDown();
				result = true;
				break;
			}
			else if (!item.isPassDragEvent())
			{
				break;
			}
		}
		if (!result)
		{
			mTouchPoint = null;
		}
		return result;
	}
	protected void touchDown()
	{
		mPreparingDrag = true;
		mPrepareDragMousePosition = mTouchPoint.getCurPosition();
	}
	protected void touchUp(bool cancel)
	{
		// 确保鼠标抬起时所有状态标记为false
		mPreparingDrag = false;
		if (mDraging)
		{
			mDraging = false;
			Vector3 endPosition = mTouchPoint.getCurPosition();
			mTouchPoint = null;
			onDragEnd(endPosition, cancel);
		}
	}
	protected void touchMove()
	{
		Vector3 touchPosition = mTouchPoint.getCurPosition();
		if (mPreparingDrag)
		{
			Vector2 mouseDelta = touchPosition - mPrepareDragMousePosition;
			if (lengthGreater(mouseDelta, mStartDragThreshold))
			{
				// 有拖拽方向要求时,只有拖拽方向与设置方向夹角不超过指定角度时才开始拖动
				if (isVectorZero(mAllowDragDirection) || getAngleBetweenVector(mouseDelta, mAllowDragDirection) < mDragStartAngleThreshold)
				{
					mDraging = onDragStart(mTouchPoint);
				}
				mPreparingDrag = false;
			}
		}
		if (mDraging)
		{
			if (mMovable)
			{
				applyScreenPosition(touchPosition);
			}
			onDraging(touchPosition);
		}
	}
	protected virtual void applyScreenPosition(Vector3 screenPos) { }
	protected virtual Vector3 getScreenPosition() { return Vector3.zero; }
	protected virtual bool mouseInObject(Vector3 mousePosition) { return false; }
	// 物体开始拖动,返回值表示是否允许开始拖动
	protected virtual bool onDragStart(TouchPoint touchPoint)
	{
		mDragMouseOffset = mObjectCenterAlignMouse ? Vector3.zero : touchPoint.getCurPosition() - getScreenPosition();
		if (mDragStartCallback != null)
		{
			bool allowDrag = true;
			mDragStartCallback(mComponentOwner, touchPoint, ref allowDrag);
			return allowDrag;
		}
		return true;
	}
	// 当前物体拖动结束
	protected virtual void onDragEnd(Vector3 mousePos, bool cancel)
	{
		mDragEndCallback?.Invoke(mComponentOwner, mousePos, cancel);
	}
	// 当前物体正在拖动
	protected virtual void onDraging(Vector3 mousePos)
	{
		mDragingCallback?.Invoke(mComponentOwner, mousePos);
	}
	// 由子类调用,在所有逻辑都处理完以后调用
	protected void notifyDragEndTotally(Vector3 mousePos, bool cancel)
	{
		mDragEndTotallyCallback?.Invoke(mComponentOwner, mousePos, cancel);
	}
}