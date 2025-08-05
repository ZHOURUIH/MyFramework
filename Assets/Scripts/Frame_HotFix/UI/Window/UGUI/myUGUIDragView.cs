using System;
using UnityEngine;
using static UnityUtility;
using static MathUtility;

// 可拖拽滑动的窗口,类似于ScrollView
// 一般父节点是一个viewport
public class myUGUIDragView : myUGUIObject
{
	protected COMWindowDragView mDragViewComponent;     // 拖拽滑动组件
	protected bool mInited;
	public myUGUIDragView()
	{
		mNeedUpdate = true;
	}
	public void initDragView()
	{
		initDragView(DRAG_DIRECTION.VERTICAL, toRadian(45.0f), false, true, true);
	}
	public void initDragView(DRAG_DIRECTION direction)
	{
		initDragView(direction, toRadian(45.0f), false, true, true);
	}
	public void initDragView(DRAG_DIRECTION direction, float angleThresholdRadian)
	{
		initDragView(direction, angleThresholdRadian, false, true, true);
	}
	// angleThresholdRadian表示拖拽方向与允许拖拽方向的夹角绝对值最大值,弧度制
	// clampInner为true表示DragView只能在父节点的区域内滑动,父节点区域小于DragView区域时不能滑动
	// clampInner为false表示DragView只能在父节点的区域外滑动,父节点区域大于DragView区域时不能滑动
	// 一般情况下作为滑动列表时可填false
	// allowDragOnlyOverParentSize表示是否只有大小超过父节点时才能拖拽,当前节点没有超过父节点时不允许拖拽
	// clampInRange为true表示拖拽时始终限制在正常范围内
	public void initDragView(DRAG_DIRECTION direction, float angleThresholdRadian, bool clampInner, bool allowDragOnlyOverParentSize, bool clampInRange)
	{
		// 这里直接获取父节点作为viewport
		mLayout.getScript().bindPassOnlyParent(mParent as myUGUIObject);
		registeCollider(true);
		setDepthOverAllChild(true);
		setDragDirection(direction);
		setDragAngleThreshold(angleThresholdRadian);
		setClampInner(clampInner);
		setAllowDragOnlyOverParentSize(allowDragOnlyOverParentSize);
		setClampInRange(clampInRange);
		mInited = true;
	}
	public override bool isReceiveScreenMouse() { return true; }
	public myUGUIObject getViewport() { return mParent as myUGUIObject; }
	// 显式调用调整窗口位置
	public void autoClampPosition()
	{
		mDragViewComponent.autoClampPosition();
	}
	public void autoResetPosition()
	{
		mDragViewComponent.autoResetPosition();
	}
	public override void onMouseDown(Vector3 mousePos, int touchID)
	{
		base.onMouseDown(mousePos, touchID);
		mDragViewComponent.onMouseDown(mousePos, touchID);
		if (!mInited)
		{
			logError("COMWindowDragView组件未初始化,是否忘记调用了myUGUIDragView的initDragView?");
		}
	}
	// 鼠标在屏幕上抬起
	public override void onScreenMouseUp(Vector3 mousePos, int touchID)
	{
		base.onScreenMouseUp(mousePos, touchID);
		mDragViewComponent.onScreenMouseUp();
	}
	public override void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		base.onMouseMove(mousePos, moveDelta, moveTime, touchID);
		mDragViewComponent.onMouseMove(mousePos, moveDelta, moveTime, touchID);
	}
	public override void onMouseStay(Vector3 mousePos, int touchID)
	{
		base.onMouseStay(mousePos, touchID);
		mDragViewComponent.onMouseStay(touchID);
	}
	public override void setWindowSize(Vector2 size)
	{
		base.setWindowSize(size);
		mCOMWindowCollider?.setColliderSize(getWindowSize(true));
		mDragViewComponent.onWindowSizeChange();
	}
	public void setAlignTopOrLeft(bool alignTopOrLeft) { mDragViewComponent.setAlignTopOrLeft(alignTopOrLeft); }
	// 当DragView的父节点的大小改变时,需要调用该函数重新计算可拖动的范围
	public void notifyParentSizeChange()
	{
		mDragViewComponent.notifyWindowParentSizeChange();
	}
	public void stopMoving()													{ mDragViewComponent.stopMoving(); }
	public void setClampInner(bool inner)										{ mDragViewComponent.setClampInner(inner); }
	public void setDragDirection(DRAG_DIRECTION direction)						{ mDragViewComponent.setDragDirection(direction); }
	public void setMaxRelativePos(Vector3 max)									{ mDragViewComponent.setMaxRelativePos(max); }
	public void setMinRelativePos(Vector3 min)									{ mDragViewComponent.setMinRelativePos(min); }
	public void setMoveSpeedScale(float scale)									{ mDragViewComponent.setMoveSpeedScale(scale); }
	public void setDragViewStartCallback(OnDragViewStartCallback callback)		{ mDragViewComponent.setDragViewStartCallback(callback); }
	public void setDragingCallback(Action draging)								{ mDragViewComponent.setDragingCallback(draging); }
	public void setReleaseDragCallback(Action releaseDrag)						{ mDragViewComponent.setReleaseDragCallback(releaseDrag); }
	public void setPositionChangeCallback(Action positionChange)				{ mDragViewComponent.setPositionChangeCallback(positionChange); }
	public void setClampType(CLAMP_TYPE clampType)								{ mDragViewComponent.setClampType(clampType); }
	public void setClampInRange(bool clampInRange)								{ mDragViewComponent.setClampInRange(clampInRange); }
	public void setAllowDragOnlyOverParentSize(bool dragOnly)					{ mDragViewComponent.setAllowDragOnlyOverParentSize(dragOnly); }
	public void setAutoMoveToEdge(bool autoMove)								{ mDragViewComponent.setAutoMoveToEdge(autoMove); }
	public void setAttenuateFactor(float value)									{ mDragViewComponent.setAttenuateFactor(value); }
	public void setDragLengthThreshold(float value)								{ mDragViewComponent.setDragLengthThreshold(value); }
	public void setDragAngleThreshold(float radian)								{ mDragViewComponent.setDragAngleThreshold(radian); }
	public void setAutoClampSpeed(float speed)									{ mDragViewComponent.setAutoClampSpeed(speed); }
	public DRAG_DIRECTION getDragDirection()									{ return mDragViewComponent.getDragDirection(); }
	public Vector3 getMaxRelativePos()											{ return mDragViewComponent.getMaxRelativePos(); }
	public Vector3 getMinRelativePos()											{ return mDragViewComponent.getMinRelativePos(); }
	public bool isClampInner()													{ return mDragViewComponent.isClampInner(); }
	public bool isDraging()														{ return mDragViewComponent.isDraging(); }
	public bool isAllowDragOnlyOverParentSize()									{ return mDragViewComponent.isAllowDragOnlyOverParentSize(); }
	public COMWindowDragView getDragViewComponent()								{ return mDragViewComponent; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addInitComponent(out mDragViewComponent, true);
	}
}