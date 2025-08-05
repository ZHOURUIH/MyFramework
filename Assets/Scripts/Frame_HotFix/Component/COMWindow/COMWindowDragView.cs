using UnityEngine;
using System;
using System.Collections.Generic;
using static MathUtility;
using static FrameBaseHotFix;

// 实现类似与ScrollView的功能的组件,可滑动组件,用于定义窗口的拖拽滑动操作
public class COMWindowDragView : GameComponent
{
	protected List<COMWindowDragView> mMutexDragView;           // 与当前组件互斥的滑动组件,两个组件中同一时间只有一个组件响应滑动
	protected OnDragViewStartCallback mOnDragViewStartCallback; // 开始拖拽滑动的回调
	protected Action mPositionChangeCallback;					// 组件所属窗口位置改变的回调
	protected Action mReleaseDragCallback;						// 结束拖拽的回调
	protected Action mDragingCallback;							// 拖拽中的回调
	protected myUGUIObject mWindow;                             // 组件所属窗口
	protected Vector3 mMinRelativePos;							// 左边界和下边界或者窗口中心的最小值,具体需要由mClampType决定,-1到1的相对值,相对于父窗口的宽高
	protected Vector3 mMaxRelativePos;							// 右边界和上边界或者窗口中心的最大值,具体需要由mClampType决定,-1到1的相对值,相对于父窗口的宽高
	protected float mDragLengthThreshold;						// 拖拽长度的最小值,当拖动距离大于该值时才允许开始拖拽
	protected float mDragAngleThreshold;						// 拖拽方向与允许拖拽方向的夹角绝对值最大值,弧度制
	protected float mAttenuateFactor;							// 移动速度衰减系数,鼠标在放开时移动速度会逐渐降低,衰减系数越大.速度降低越快
	protected float mMoveToEdgeSpeed;							// 自动停靠到最近的边的速度
	protected float mMoveSpeedScale;							// 鼠标放开后自由移动时的速度缩放
	protected float mAutoClampSpeed;							// 当列表拖拽到合法范围外时恢复到正常范围内时的速度
	protected bool mAutoMoveToEdge;								// 是否自动停靠到最近的边
	protected bool mAlignTopOrLeft = true;						// 是否对齐左边或上边
	protected bool mClampInRange;                               // 拖拽时是否始终限制在正常范围内
	protected bool mAllowDragOnlyOverParentSize;				// 只有大小超过父节点时才能拖拽,当前节点没有超过父节点时不允许拖拽
	// 为true表示DragView只能在父节点的区域内滑动,父节点区域小于DragView区域时不能滑动
	// false表示DragView只能在父节点的区域外滑动,父节点区域大于DragView区域时不能滑动
	protected bool mClampInner = true;							// 滑动区域限制类型
	protected DRAG_DIRECTION mDragDirection;					// 滑动方向
	protected CLAMP_TYPE mClampType;							// 限制子节点的滑动范围,与mClampInner有点像,但是作用不同
	//------------------------------------------------------------------------------------------------------------------------------
	// 用于实时计算的参数
	protected Vector3 mStartDragWindowPosition;					// 开始拖拽时的窗口的坐标
	protected Vector3 mStartDragMousePosition;					// 开始拖拽时的触点坐标
	protected Vector3 mMouseDownPos;							// 触点按下时的坐标
	protected Vector3 mMoveDirection;							// 当前拖拽移动的方向
	protected BOOL mDraging = new();							// 是否正在拖动,鼠标按下并且移动速度大于一定值时开始拖动,鼠标放开时,按惯性移动
	protected float mMoveSpeed;									// 移动速度
	protected int mTouchID;										// 操作的触点ID
	protected bool mMouseDown;									// 鼠标是否在窗口内按下,鼠标抬起会设置为false,但是鼠标离开窗口时仍然为true
	//------------------------------------------------------------------------------------------------------------------------------
	// 用于避免GC的参数
	private Vector3 mMinPos;									// 缓存的当前窗口在父节点中可移动的最小的位置
	private Vector3 mMaxPos;									// 缓存的当前窗口在父节点中可移动的最大的位置
	private bool mMinMaxPosDirty = true;                        // mMinPos和mMaxPos是否需要重新计算
	public COMWindowDragView()
	{
		mDragDirection = DRAG_DIRECTION.HORIZONTAL;
		mClampType = CLAMP_TYPE.EDGE_IN_RECT;
		mMinRelativePos = -Vector3.one;
		mMaxRelativePos = Vector3.one;
		mDragAngleThreshold = toRadian(45.0f);
		mMoveSpeedScale = 1.0f;
		mAttenuateFactor = 2.0f;
		mMoveToEdgeSpeed = 5.0f;
		mDragLengthThreshold = 10.0f;
		mAutoClampSpeed = 10.0f;
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mWindow = owner as myUGUIObject;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mMutexDragView?.Clear();
		mDragingCallback = null;
		mReleaseDragCallback = null;
		mPositionChangeCallback = null;
		mOnDragViewStartCallback = null;
		mWindow = null;
		mMinRelativePos = -Vector3.one;
		mMaxRelativePos = Vector3.one;
		mDragLengthThreshold = 10.0f;
		mDragAngleThreshold = toRadian(45.0f);
		mAttenuateFactor = 2.0f;
		mMoveToEdgeSpeed = 5.0f;
		mMoveSpeedScale = 1.0f;
		mAutoClampSpeed = 10.0f;
		mAutoMoveToEdge = false;
		mAlignTopOrLeft = true;
		mClampInRange = false;
		mAllowDragOnlyOverParentSize = false;
		mClampInner = true;
		mDragDirection = DRAG_DIRECTION.HORIZONTAL;
		mClampType = CLAMP_TYPE.EDGE_IN_RECT;
		mStartDragWindowPosition = Vector3.zero;
		mStartDragMousePosition = Vector3.zero;
		mMouseDownPos = Vector3.zero;
		mMoveDirection = Vector3.zero;
		mDraging.set(false);
		mMoveSpeed = 0.0f;
		mTouchID = 0;
		mMouseDown = false;
		mMinPos = Vector3.zero;
		mMaxPos = Vector3.zero;
		mMinMaxPosDirty = true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mDraging.mValue)
		{
			Vector3 curPosition = mWindow.getPosition();
			Vector3 prePos = curPosition;
			// 拖拽状态时,鼠标移动量就是窗口的移动量,此处未考虑父窗口的缩放不为1的情况
			Vector3 moveDelta = mInputSystem.getTouchPoint(mTouchID).getCurPosition() - mStartDragMousePosition;
			if (!isVectorZero(moveDelta))
			{
				if (mDragDirection == DRAG_DIRECTION.HORIZONTAL)
				{
					moveDelta.y = 0.0f;
				}
				else if (mDragDirection == DRAG_DIRECTION.VERTICAL)
				{
					moveDelta.x = 0.0f;
				}
				curPosition = mStartDragWindowPosition + moveDelta;
				if (mClampInRange)
				{
					clampPosition(ref curPosition);
				}
				if (!isVectorEqual(prePos, curPosition))
				{
					setPosition(curPosition);
					mDragingCallback?.Invoke();
				}
			}
		}
		else
		{
			// 自动停靠最近的边
			if (mAutoMoveToEdge)
			{
				getLocalMinMaxPixelPos(out Vector3 min, out Vector3 max);
				Vector3 curPosition = mWindow.getPosition();
				Vector3 targetPosition = curPosition;
				// 获得当前最近的边
				if (mDragDirection == DRAG_DIRECTION.HORIZONTAL)
				{
					targetPosition.x = getNearest(curPosition.x, min.x, max.x);
				}
				else if (mDragDirection == DRAG_DIRECTION.VERTICAL)
				{
					targetPosition.y = getNearest(curPosition.y, min.y, max.y);
				}
				else
				{
					int minIndex = -1;
					Span<float> disArray = stackalloc float[4] 
					{
						abs(curPosition.x - min.x),
						abs(curPosition.x - max.x),
						abs(curPosition.y - min.y),
						abs(curPosition.y - max.y),
					};
					float minDistance = 0.0f;
					for (int i = 0; i < 4; ++i)
					{
						if (minIndex < 0 || minDistance > disArray[i])
						{
							minIndex = i;
							minDistance = disArray[i];
						}
					}

					if (minIndex == 0)
					{
						targetPosition.x = min.x;
					}
					else if (minIndex == 1)
					{
						targetPosition.x = max.x;
					}
					else if (minIndex == 2)
					{
						targetPosition.y = min.y;
					}
					else if (minIndex == 3)
					{
						targetPosition.y = max.y;
					}
				}
				if (!isVectorEqual(curPosition, targetPosition))
				{
					setPosition(lerp(curPosition, targetPosition, elapsedTime * mMoveToEdgeSpeed));
				}
			}
			else
			{
				Vector3 curPosition = mWindow.getPosition();
				// 如果滑动已经超过了正常区域,则手指放开时回弹到正常范围内
				Vector3 validPos = curPosition;
				if (!isValidPosition(ref curPosition, ref validPos, mAlignTopOrLeft))
				{
					Vector3 newPos = lerp(curPosition, validPos, elapsedTime * mAutoClampSpeed);
					if (!isVectorEqual(newPos, curPosition))
					{
						setPosition(newPos);
					}
					else
					{
						mMoveSpeed = 0.0f;
					}
				}
				else
				{
					// 按照当前滑动速度移动
					if (mMoveSpeed > 0.0f)
					{
						// 只有鼠标未按下并且不自动停靠到最近的边时才衰减速度
						Vector3 prePos = curPosition;
						mMoveSpeed = lerp(mMoveSpeed, 0.0f, elapsedTime * mAttenuateFactor, 10.0f);
						curPosition += mMoveSpeed * mMoveSpeedScale * elapsedTime * mMoveDirection;
						clampPosition(ref curPosition);
						if (!isVectorEqual(prePos, curPosition))
						{
							setPosition(curPosition);
						}
						else
						{
							mMoveSpeed = 0.0f;
						}
					}
				}
			}
		}
	}
	// 显式调用调整窗口位置
	public void autoClampPosition()
	{
		Vector3 curPosition = mWindow.getPosition();
		clampPosition(ref curPosition, mAlignTopOrLeft);
		setPosition(curPosition);
	}
	public void autoResetPosition()
	{
		Vector3 curPosition = mWindow.getPosition();
		resetPosition(ref curPosition, mAlignTopOrLeft);
		setPosition(curPosition);
	}
	public void onMouseDown(Vector3 mousePos, int touchID)
	{
		mMouseDown = true;
		mMoveSpeed = 0.0f;
		mMouseDownPos = mousePos;
		mTouchID = touchID;
	}
	// 鼠标在屏幕上抬起
	public void onScreenMouseUp()
	{
		mMouseDown = false;
		mDraging.set(false);
		mReleaseDragCallback?.Invoke();
		mTouchID = 0;
	}
	public void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		// 触点不一致或鼠标未按下时不允许改变移动速度
		if (mTouchID != touchID || !mMouseDown)
		{
			return;
		}
		Vector3 delta = moveDelta;
		if (mDragDirection == DRAG_DIRECTION.HORIZONTAL)
		{
			delta.y = 0.0f;
		}
		else if (mDragDirection == DRAG_DIRECTION.VERTICAL)
		{
			delta.x = 0.0f;
		}
		float speed = divide(getLength(delta), moveTime);
		if (!mDraging.mValue)
		{
			Vector3 dragDir = mousePos - mMouseDownPos;
			// 拖动距离大于一定值时才会判断是否开始拖拽,还要继续检测鼠标移动才能判断是否可以开始拖拽
			if (lengthGreater(dragDir, mDragLengthThreshold))
			{
				if (!checkCanDrag(dragDir))
				{
					mMouseDown = false;
					return;
				}
				mStartDragWindowPosition = mWindow.getPosition();
				mStartDragMousePosition = mousePos;
				mMoveSpeed = speed;
				mMoveDirection = normalize(delta);
			}
		}
		else
		{
			mMoveSpeed = speed;
			mMoveDirection = normalize(delta);
		}
	}
	public void onMouseStay(int touchID)
	{
		if (mTouchID != touchID)
		{
			return;
		}
		// 鼠标在窗口内为按下状态,并且没有移动时,确保速度为0
		if (!mMouseDown)
		{
			return;
		}
		mMoveSpeed = 0.0f;
	}
	public void stopMoving()
	{
		mMoveSpeed = 0.0f;
	}
	public void setClampInner(bool inner)										{ mClampInner = inner; }
	public void setDragDirection(DRAG_DIRECTION direction)						{ mDragDirection = direction; }
	public void setMaxRelativePos(Vector3 max)									{ mMaxRelativePos = max; mMinMaxPosDirty = true; }
	public void setMinRelativePos(Vector3 min)									{ mMinRelativePos = min; mMinMaxPosDirty = true; }
	public void setMoveSpeedScale(float scale)									{ mMoveSpeedScale = scale; }
	public void setDragViewStartCallback(OnDragViewStartCallback callback)		{ mOnDragViewStartCallback = callback; }
	public void setDragingCallback(Action draging)								{ mDragingCallback = draging; }
	public void setReleaseDragCallback(Action releaseDrag)						{ mReleaseDragCallback = releaseDrag; }
	public void setPositionChangeCallback(Action positionChange)				{ mPositionChangeCallback = positionChange; }
	public void setClampType(CLAMP_TYPE clampType)								{ mClampType = clampType; }
	public void setClampInRange(bool clampInRange)								{ mClampInRange = clampInRange; }
	public void setAutoMoveToEdge(bool autoMove)								{ mAutoMoveToEdge = autoMove; }
	public void setDragLengthThreshold(float value)								{ mDragLengthThreshold = value; }
	public void setDragAngleThreshold(float radian)								{ mDragAngleThreshold = radian; }
	public void setAttenuateFactor(float value)									{ mAttenuateFactor = value; }
	public void setAutoClampSpeed(float speed)									{ mAutoClampSpeed = speed; }
	public void setAlignTopOrLeft(bool topOrLeft)								{ mAlignTopOrLeft = topOrLeft; }
	public void setAllowDragOnlyOverParentSize(bool dragOnly)					{ mAllowDragOnlyOverParentSize = dragOnly; }
	public bool isClampInner()													{ return mClampInner; }
	public bool isDraging()														{ return mDraging.mValue; }
	public DRAG_DIRECTION getDragDirection()									{ return mDragDirection; }
	public Vector3 getMaxRelativePos()											{ return mMaxRelativePos; }
	public Vector3 getMinRelativePos()											{ return mMinRelativePos; }
	public bool isAllowDragOnlyOverParentSize()									{ return mAllowDragOnlyOverParentSize; }
	// 将两个滑动组件设置为互斥组件
	public static void mutexDragView(COMWindowDragView dragView0, COMWindowDragView dragView1)
	{
		if (dragView0 == null || dragView1 == null)
		{
			return;
		}
		dragView0.mMutexDragView ??= new();
		dragView1.mMutexDragView ??= new();
		dragView0.mMutexDragView.addUnique(dragView1);
		dragView1.mMutexDragView.addUnique(dragView0);
	}
	public static void mutexDragView(myUGUIObject dragView0, myUGUIObject dragView1)
	{
		mutexDragView(dragView0.getOrAddComponent<COMWindowDragView>(),
					  dragView1.getOrAddComponent<COMWindowDragView>());
	}
	public static void releaseMutexDragView(COMWindowDragView dragView0, COMWindowDragView dragView1)
	{
		if (dragView0 == null || dragView1 == null)
		{
			return;
		}
		dragView0.mMutexDragView?.Remove(dragView1);
		dragView1.mMutexDragView?.Remove(dragView0);
	}
	public static void releaseMutexDragView(myUGUIObject dragView0, myUGUIObject dragView1)
	{
		releaseMutexDragView(dragView0.getOrAddComponent<COMWindowDragView>(),
							 dragView1.getOrAddComponent<COMWindowDragView>());
	}
	public void setPosition(Vector3 pos)
	{
		mWindow.setPosition(pos);
		mPositionChangeCallback?.Invoke();
	}
	public void onWindowSizeChange()
	{
		mMinMaxPosDirty = true;
	}
	public void notifyWindowParentSizeChange()
	{
		mMinMaxPosDirty = true;
	}
	public override void setActive(bool active)
	{
		base.setActive(active);
		// 启用或禁用组件时,需要将实时计算用的参数重置
		mMoveSpeed = 0.0f;
		mMoveDirection = Vector3.zero;
		mMouseDown = false;
		mDraging.set(false);
		mStartDragWindowPosition = Vector3.zero;
		mStartDragMousePosition = Vector3.zero;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void getLocalMinMaxPixelPos(out Vector3 min, out Vector3 max)
	{
		if (mMinMaxPosDirty)
		{
			Vector2 parentWidgetSize = mWindow.getParent().getWindowSize();
			// 计算父节点的世界缩放
			Vector2 parentScale = divideVector3(mWindow.getTransform().parent.lossyScale, mLayoutManager.getUIRoot().getScale());
			// 计算移动的位置范围
			Vector2 minPos = parentWidgetSize * 0.5f * divideVector2(mMinRelativePos, parentScale);
			Vector2 maxPos = parentWidgetSize * 0.5f * divideVector2(mMaxRelativePos, parentScale);
			if (mClampType == CLAMP_TYPE.EDGE_IN_RECT)
			{
				Vector2 thisSize = mWindow.getWindowSize(true);
				minPos += thisSize * 0.5f;
				maxPos -= thisSize * 0.5f;
				if (!mClampInner)
				{
					swap(ref minPos, ref maxPos);
				}
			}
			else if (mClampType == CLAMP_TYPE.CENTER_IN_RECT){}
			mMinPos = minPos;
			mMaxPos = maxPos;
			mMinMaxPosDirty = false;
		}
		min = mMinPos;
		max = mMaxPos;
	}
	protected void resetPosition(ref Vector3 position, bool alignTopOrLeft = true)
	{
		// 计算移动的位置范围
		getLocalMinMaxPixelPos(out Vector3 min, out Vector3 max);
		if (mDragDirection == DRAG_DIRECTION.HORIZONTAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(min.x, max.x);
			float minValue = getMin(min.x, max.x);
			if (min.x <= max.x)
			{
				if (mClampInner)
				{
					position.x = alignTopOrLeft ? minValue : maxValue;
				}
				else
				{
					position.x = alignTopOrLeft ? maxValue : minValue;
				}
			}
			else
			{
				if (mClampInner)
				{
					position.x = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					position.x = alignTopOrLeft ? minValue : maxValue;
				}
			}
		}
		if (mDragDirection == DRAG_DIRECTION.VERTICAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(min.y, max.y);
			float minValue = getMin(min.y, max.y);
			if (min.y <= max.y)
			{
				if (mClampInner)
				{
					position.y = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					position.y = alignTopOrLeft ? minValue : maxValue;
				}
			}
			else
			{
				if (mClampInner)
				{
					position.y = alignTopOrLeft ? minValue : maxValue;
				}
				else
				{
					position.y = alignTopOrLeft ? maxValue : minValue;
				}
			}
		}
	}
	// alignTopOrLeft表示当控件不允许拖动时,是否将上边界或者左边界(或中心点)与拖动范围对齐
	protected void clampPosition(ref Vector3 position, bool alignTopOrLeft = true)
	{
		// 计算移动的位置范围
		getLocalMinMaxPixelPos(out Vector3 min, out Vector3 max);
		if (mDragDirection == DRAG_DIRECTION.HORIZONTAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(min.x, max.x);
			float minValue = getMin(min.x, max.x);
			// 滑动范围有效时需要限定在一定范围
			if (min.x <= max.x)
			{
				clamp(ref position.x, minValue, maxValue);
			}
			// 不能滑动的情况下
			else
			{
				if (mClampInner)
				{
					position.x = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					position.x = alignTopOrLeft ? minValue : maxValue;
				}
			}
		}
		if (mDragDirection == DRAG_DIRECTION.VERTICAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(min.y, max.y);
			float minValue = getMin(min.y, max.y);
			// 滑动范围有效时需要限定在一定范围
			if (min.y <= max.y)
			{
				clamp(ref position.y, minValue, maxValue);
			}
			// 不能滑动的情况下
			else
			{
				if (mClampInner)
				{
					position.y = alignTopOrLeft ? minValue : maxValue;
				}
				else
				{
					position.y = alignTopOrLeft ? maxValue : minValue;
				}
			}
		}
	}
	// 位置是否位于合法范围内,如果不合法,则validPos会返回合法位置
	protected bool isValidPosition(ref Vector3 position, ref Vector3 validPos, bool alignTopOrLeft = true)
	{
		validPos = position;
		getLocalMinMaxPixelPos(out Vector3 min, out Vector3 max);
		bool horiValid = false;
		bool vertValid = false;
		if (mDragDirection == DRAG_DIRECTION.HORIZONTAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(min.x, max.x);
			float minValue = getMin(min.x, max.x);
			horiValid = false;
			if (min.x <= max.x)
			{
				horiValid = inRangeFixed(position.x, minValue, maxValue);
				if (!horiValid)
				{
					float x = position.x;
					clamp(ref x, minValue, maxValue);
					validPos.x = x;
				}
			}
			// 不能滑动的情况下
			else
			{
				if (mClampInner)
				{
					validPos.x = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					validPos.x = alignTopOrLeft ? minValue : maxValue;
				}
			}
		}
		if (mDragDirection == DRAG_DIRECTION.VERTICAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(min.y, max.y);
			float minValue = getMin(min.y, max.y);
			vertValid = false;
			if (min.y <= max.y)
			{
				vertValid = inRangeFixed(position.y, minValue, maxValue);
				if (!vertValid)
				{
					float y = position.y;
					clamp(ref y, minValue, maxValue);
					validPos.y = y;
				}
			}
			// 不能滑动的情况下
			else
			{
				if (mClampInner)
				{
					validPos.y = alignTopOrLeft ? minValue : maxValue;
				}
				else
				{
					validPos.y = alignTopOrLeft ? maxValue : minValue;
				}
			}
		}
		if (mDragDirection == DRAG_DIRECTION.HORIZONTAL)
		{
			return horiValid;
		}
		else if (mDragDirection == DRAG_DIRECTION.VERTICAL)
		{
			return vertValid;
		}
		return horiValid && vertValid;
	}
	protected bool checkCanDrag(Vector3 dragDir)
	{
		// 鼠标滑动的方向需要与当前方向一致,否则不能开始滑动
		if (mDragDirection == DRAG_DIRECTION.HORIZONTAL)
		{
			// 判断当前窗口的宽度是否超过了父节点的宽度
			if (mAllowDragOnlyOverParentSize && mWindow.getWindowSize().x < mWindow.getParent().getWindowSize().x)
			{
				return false;
			}
			float angleBetweenLeft = getAngleBetweenVector(dragDir, Vector3.left);
			float angleBetweenRight = PI_RADIAN - angleBetweenLeft;
			// 如果夹角大于阈值,则不能开始拖动
			if (mDragAngleThreshold > 0.0f && getMin(angleBetweenLeft, angleBetweenRight) > mDragAngleThreshold)
			{
				return false;
			}
		}
		else if (mDragDirection == DRAG_DIRECTION.VERTICAL)
		{
			// 判断当前窗口的高度是否超过了父节点的高度
			if (mAllowDragOnlyOverParentSize && mWindow.getWindowSize().y < mWindow.getParent().getWindowSize().y)
			{
				return false;
			}
			float angleBetweenUp = getAngleBetweenVector(dragDir, Vector3.up);
			float angleBetweenDown = PI_RADIAN - angleBetweenUp;
			// 如果夹角大于阈值,则不能开始拖动
			if (mDragAngleThreshold > 0.0f && getMin(angleBetweenUp, angleBetweenDown) > mDragAngleThreshold)
			{
				return false;
			}
		}
		// 当互斥的滑动组件正在滑动时不允许滑动
		foreach (COMWindowDragView item in mMutexDragView.safe())
		{
			// 互斥时不允许拖拽
			if (item.isActive() && item.isDraging())
			{
				return false;
			}
		}
		mDraging.set(true);
		mOnDragViewStartCallback?.Invoke(ref mDraging.mValid);
		// 不允许拖拽
		return mDraging.mValue;
	}
}