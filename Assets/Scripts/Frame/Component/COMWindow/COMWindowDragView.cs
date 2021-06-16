using UnityEngine;
using System;
using System.Collections.Generic;

public class COMWindowDragView : GameComponent
{
	protected List<COMWindowDragView> mMutexDragView; // 与当前组件互斥的滑动组件,两个组件中同一时间只有一个组件响应滑动
	protected OnDragViewStartCallback mOnDragViewStartCallback;
	protected OnDragViewCallback mPositionChangeCallback;
	protected OnDragViewCallback mReleaseDragCallback;
	protected OnDragViewCallback mDragingCallback;
	protected myUIObject mWindow;
	protected Vector3 mMinRelativePos;		// 左边界和下边界或者窗口中心的最小值,具体需要由mClampType决定,-1到1的相对值,相对于父窗口的宽高
	protected Vector3 mMaxRelativePos;		// 右边界和上边界或者窗口中心的最大值,具体需要由mClampType决定,-1到1的相对值,相对于父窗口的宽高
	protected float mDragLengthThreshold;   // 拖拽长度的最小值,当拖动距离大于该值时才允许开始拖拽
	protected float mDragAngleThreshold;    // 拖拽方向与允许拖拽方向的夹角绝对值最大值,弧度制
	protected float mAttenuateFactor;		// 移动速度衰减系数,鼠标在放开时移动速度会逐渐降低,衰减系数越大.速度降低越快
	protected float mMoveToEdgeSpeed;		// 自动停靠到最近的边的速度
	protected float mMoveSpeedScale;		// 鼠标放开后自由移动时的速度缩放
	protected float mAutoClampSpeed;        // 当列表拖拽到合法范围外时恢复到正常范围内时的速度
	protected bool mAutoMoveToEdge;			// 是否自动停靠到最近的边
	protected bool mAlignTopOrLeft;         // 是否对齐左边或上边
	protected bool mClampInRange;           // 拖拽时是否始终限制在正常范围内
	// 为true表示DragView只能在父节点的区域内滑动,父节点区域小于DragView区域时不能滑动
	// false表示DragView只能在父节点的区域外滑动,父节点区域大于DragView区域时不能滑动
	protected bool mClampInner;
	protected DRAG_DIRECTION mDragDirection;
	protected CLAMP_TYPE mClampType;
	//-------------------------------------------------------------------------------------------------------------------------
	// 用于实时计算的参数
	protected Vector3 mStartDragWindowPosition;
	protected Vector3 mStartDragMousePosition;
	protected Vector3 mMouseDownPos;
	protected Vector3 mMoveNormal;
	protected BOOL mDraging;			// 是否正在拖动,鼠标按下并且移动速度大于一定值时开始拖动,鼠标放开时,按惯性移动
	protected float mMoveSpeed;
	protected bool mMouseDown;			// 鼠标是否在窗口内按下,鼠标抬起会设置为false,但是鼠标离开窗口时仍然为true
	//-------------------------------------------------------------------------------------------------------------------------
	// 用于避免GC的参数
	private Vector3[] mMinMaxPos;
	private bool mMinMaxPosDirty;
	public COMWindowDragView()
	{
		mDraging = new BOOL();
		mDragDirection = DRAG_DIRECTION.HORIZONTAL;
		mClampType = CLAMP_TYPE.EDGE_IN_RECT;
		mMinRelativePos = -Vector3.one;
		mMaxRelativePos = Vector3.one;
		mMinMaxPos = new Vector3[2];
		mMutexDragView = new List<COMWindowDragView>();
		mDragAngleThreshold = toRadian(45.0f);
		mMoveSpeedScale = 1.0f;
		mAttenuateFactor = 2.0f;
		mMoveToEdgeSpeed = 5.0f;
		mDragAngleThreshold = 0.0f;
		mDragLengthThreshold = 10.0f;
		mAutoClampSpeed = 10.0f;
		mClampInner = true;
		mAlignTopOrLeft = true;
		mMinMaxPosDirty = true;
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mWindow = owner as myUIObject;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mMutexDragView.Clear();
		mDragingCallback = null;
		mReleaseDragCallback = null;
		mPositionChangeCallback = null;
		mOnDragViewStartCallback = null;
		mWindow = null;
		mDragDirection = DRAG_DIRECTION.HORIZONTAL;
		mClampType = CLAMP_TYPE.EDGE_IN_RECT;
		mMinRelativePos = -Vector3.one;
		mMaxRelativePos = Vector3.one;
		mMoveSpeedScale = 1.0f;
		mAttenuateFactor = 2.0f;
		mMoveToEdgeSpeed = 5.0f;
		mDragAngleThreshold = 0.0f;
		mDragLengthThreshold = 10.0f;
		mAutoClampSpeed = 10.0f;
		mClampInner = true;
		mAlignTopOrLeft = true;
		mMinMaxPosDirty = true;
		mAutoMoveToEdge = false;
		mClampInRange = false;
		mStartDragWindowPosition = Vector3.zero;
		mStartDragMousePosition = Vector3.zero;
		mMouseDownPos = Vector3.zero;
		mMoveNormal = Vector3.zero;
		mMoveSpeed = 0.0f;
		mMouseDown = false;
		mDraging.set(false);
		memset(mMinMaxPos, Vector3.zero);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if(mDraging.mValue)
		{
			Vector3 curPosition = mWindow.getPosition();
			Vector3 prePos = curPosition;
			// 拖拽状态时,鼠标移动量就是窗口的移动量,此处未考虑父窗口的缩放不为1的情况
			Vector3 moveDelta = getMousePosition() - mStartDragMousePosition;
			if(mDragDirection == DRAG_DIRECTION.HORIZONTAL)
			{
				moveDelta.y = 0.0f;
			}
			else if(mDragDirection == DRAG_DIRECTION.VERTICAL)
			{
				moveDelta.x = 0.0f;
			}
			curPosition = mStartDragWindowPosition + moveDelta;
			if(mClampInRange)
			{
				clampPosition(ref curPosition);
			}
			if(!isVectorEqual(ref prePos, ref curPosition))
			{
				setPosition(curPosition);
				mDragingCallback?.Invoke();
			}
		}
		else
		{
			// 自动停靠最近的边
			if(mAutoMoveToEdge)
			{
				Vector3[] minMaxPos = getLocalMinMaxPixelPos();
				Vector2 minPos = minMaxPos[0];
				Vector2 maxPos = minMaxPos[1];
				Vector3 curPosition = mWindow.getPosition();
				Vector3 targetPosition = curPosition;
				// 获得当前最近的边
				if(mDragDirection == DRAG_DIRECTION.HORIZONTAL)
				{
					targetPosition.x = getNearest(curPosition.x, minPos.x, maxPos.x);
				}
				else if(mDragDirection == DRAG_DIRECTION.VERTICAL)
				{
					targetPosition.y = getNearest(curPosition.y, minPos.y, maxPos.y);
				}
				else
				{
					ARRAY(out float[] disArray, 4);
					disArray[0] = abs(curPosition.x - minPos.x);
					disArray[1] = abs(curPosition.x - maxPos.x);
					disArray[2] = abs(curPosition.y - minPos.y);
					disArray[3] = abs(curPosition.y - maxPos.y);
					int minIndex = -1;
					float minDistance = 0.0f;
					for(int i = 0; i < 4; ++i)
					{
						if(minIndex < 0 || minDistance > disArray[i])
						{
							minIndex = i;
							minDistance = disArray[i];
						}
					}
					UN_ARRAY(disArray);

					if(minIndex == 0)
					{
						targetPosition.x = minPos.x;
					}
					else if(minIndex == 1)
					{
						targetPosition.x = maxPos.x;
					}
					else if(minIndex == 2)
					{
						targetPosition.y = minPos.y;
					}
					else if(minIndex == 3)
					{
						targetPosition.y = maxPos.y;
					}
				}
				if(!isVectorEqual(ref curPosition, ref targetPosition))
				{
					Vector3 pos = lerp(curPosition, targetPosition, elapsedTime * mMoveToEdgeSpeed);
					setPosition(pos);
				}
			}
			else
			{
				Vector3 curPosition = mWindow.getPosition();
				// 如果滑动已经超过了正常区域,则手指放开时回弹到正常范围内
				Vector3 validPos = curPosition;
				if(!isValidPosition(ref curPosition, ref validPos, mAlignTopOrLeft))
				{
					Vector3 newPos = lerp(curPosition, validPos, elapsedTime * mAutoClampSpeed);
					if(!isVectorEqual(ref newPos, ref curPosition))
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
					if(mMoveSpeed > 0.0f)
					{
						// 只有鼠标未按下并且不自动停靠到最近的边时才衰减速度
						Vector3 prePos = curPosition;
						mMoveSpeed = lerp(mMoveSpeed, 0.0f, elapsedTime * mAttenuateFactor, 10.0f);
						curPosition += mMoveNormal * mMoveSpeed * mMoveSpeedScale * elapsedTime;
						clampPosition(ref curPosition);
						if(!isVectorEqual(ref prePos, ref curPosition))
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
	public void onMouseDown(Vector3 mousePos)
	{
		mMouseDown = true;
		mMoveSpeed = 0.0f;
		mMouseDownPos = mousePos;
	}
	// 鼠标在屏幕上抬起
	public void onScreenMouseUp()
	{
		mMouseDown = false;
		mDraging.set(false);
		mReleaseDragCallback?.Invoke();
	}
	public void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime)
	{
		// 鼠标未按下时不允许改变移动速度
		if(!mMouseDown)
		{
			return;
		}
		Vector3 delta = moveDelta;
		if(mDragDirection == DRAG_DIRECTION.HORIZONTAL)
		{
			delta.y = 0.0f;
		}
		else if(mDragDirection == DRAG_DIRECTION.VERTICAL)
		{
			delta.x = 0.0f;
		}
		float speed = getLength(ref delta) / moveTime;
		if(!mDraging.mValue)
		{
			Vector3 dragDir = mousePos - mMouseDownPos;
			// 拖动距离大于一定值时才会判断是否开始拖拽
			if(lengthGreater(ref dragDir, mDragLengthThreshold))
			{
				// 鼠标滑动的方向需要与当前方向一致,否则不能开始滑动
				bool validDrag = true;
				if(mDragDirection == DRAG_DIRECTION.HORIZONTAL)
				{
					float angleBetweenLeft = getAngleBetweenVector(dragDir, Vector3.left);
					float angleBetweenRight = PI_RADIAN - angleBetweenLeft;
					// 滑动方向与left的夹角更小,如果夹角大于阈值,则不能开始拖动
					if(angleBetweenLeft < angleBetweenRight)
					{
						validDrag = angleBetweenLeft <= mDragAngleThreshold;
					}
					else
					{
						validDrag = angleBetweenRight <= mDragAngleThreshold;
					}
				}
				else if(mDragDirection == DRAG_DIRECTION.VERTICAL)
				{
					float angleBetweenUp = getAngleBetweenVector(dragDir, Vector3.up);
					float angleBetweenDown = PI_RADIAN - angleBetweenUp;
					// 滑动方向与up的夹角更小,如果夹角大于阈值,则不能开始拖动
					if(angleBetweenUp < angleBetweenDown)
					{
						validDrag = angleBetweenUp <= mDragAngleThreshold;
					}
					else
					{
						validDrag = angleBetweenDown <= mDragAngleThreshold;
					}
				}
				// 不允许拖拽
				if(!validDrag)
				{
					mMouseDown = false;
					return;
				}
				// 当互斥的滑动组件正在滑动时不允许滑动
				bool mutexDraging = false;
				int count = mMutexDragView.Count;
				for(int i = 0; i < count; ++i)
				{
					if(mMutexDragView[i].isActive() && mMutexDragView[i].isDraging())
					{
						mutexDraging = true;
						break;
					}
				}
				// 不允许拖拽
				if(mutexDraging)
				{
					mMouseDown = false;
					return;
				}
				mDraging.set(true);
				mOnDragViewStartCallback?.Invoke(mDraging);
				// 不允许拖拽
				if(!mDraging.mValue)
				{
					mMouseDown = false;
					return;
				}
				mStartDragWindowPosition = mWindow.getPosition();
				mStartDragMousePosition = mousePos;
				mMoveSpeed = speed;
				mMoveNormal = normalize(delta);
			}
		}
		else
		{
			mMoveSpeed = speed;
			mMoveNormal = normalize(delta);
		}
	}
	public void onMouseStay()
	{
		// 鼠标在窗口内为按下状态,并且没有移动时,确保速度为0
		if(!mMouseDown)
		{
			return;
		}
		mMoveSpeed = 0.0f;
	}
	public void setClampInner(bool inner) { mClampInner = inner; }
	public bool isClampInner() { return mClampInner; }
	public void setDragDirection(DRAG_DIRECTION direction) { mDragDirection = direction; }
	public void setMaxRelativePos(Vector3 max) { mMaxRelativePos = max; mMinMaxPosDirty = true; }
	public void setMinRelativePos(Vector3 min) { mMinRelativePos = min; mMinMaxPosDirty = true; }
	public void setMoveSpeedScale(float scale) { mMoveSpeedScale = scale; }
	public void setDragViewStartCallback(OnDragViewStartCallback callback) { mOnDragViewStartCallback = callback; }
	public void setDragingCallback(OnDragViewCallback draging) { mDragingCallback = draging; }
	public void setReleaseDragCallback(OnDragViewCallback releaseDrag) { mReleaseDragCallback = releaseDrag; }
	public void setPositionChangeCallback(OnDragViewCallback positionChange) { mPositionChangeCallback = positionChange; }
	public void setClampType(CLAMP_TYPE clampType) { mClampType = clampType; }
	public bool isDraging() { return mDraging.mValue; }
	public DRAG_DIRECTION getDragDirection() { return mDragDirection; }
	public Vector3 getMaxRelativePos() { return mMaxRelativePos; }
	public Vector3 getMinRelativePos() { return mMinRelativePos; }
	public void setAutoMoveToEdge(bool autoMove) { mAutoMoveToEdge = autoMove; }
	public void setDragLengthThreshold(float value) { mDragLengthThreshold = value; }
	public void setDragAngleThreshold(float radian) { mDragAngleThreshold = radian; }
	public void setAttenuateFactor(float value) { mAttenuateFactor = value; }
	public void setAutoClampSpeed(float speed) { mAutoClampSpeed = speed; }
	public void setAlignTopOrLeft(bool topOrLeft) { mAlignTopOrLeft = topOrLeft; }
	// 将两个滑动组件设置为互斥组件
	public static void mutexDragView(COMWindowDragView dragView0, COMWindowDragView dragView1)
	{
		if(dragView0 == null || dragView1 == null)
		{
			return;
		}
		if (!dragView0.mMutexDragView.Contains(dragView1))
		{
			dragView0.mMutexDragView.Add(dragView1);
		}
		if (!dragView1.mMutexDragView.Contains(dragView0))
		{
			dragView1.mMutexDragView.Add(dragView0);
		}
	}
	public static void mutexDragView(myUIObject dragView0, myUIObject dragView1)
	{
		mutexDragView(dragView0.getComponent<COMWindowDragView>(),
					  dragView1.getComponent<COMWindowDragView>());
	}
	public static void releaseMutexDragView(COMWindowDragView dragView0, COMWindowDragView dragView1)
	{
		if(dragView0 == null || dragView1 == null)
		{
			return;
		}
		dragView0.mMutexDragView.Remove(dragView1);
		dragView1.mMutexDragView.Remove(dragView0);
	}
	public static void releaseMutexDragView(myUIObject dragView0, myUIObject dragView1)
	{
		releaseMutexDragView(dragView0.getComponent<COMWindowDragView>(),
							 dragView1.getComponent<COMWindowDragView>());
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
		mMoveNormal = Vector3.zero;
		mMouseDown = false;
		mDraging.set(false);
		mStartDragWindowPosition = Vector3.zero;
		mStartDragMousePosition = Vector3.zero;
	}
	//------------------------------------------------------------------------------------------------------------------------------------------
	protected Vector3[] getLocalMinMaxPixelPos()
	{
		if(mMinMaxPosDirty)
		{
			Vector2 parentWidgetSize = mWindow.getParent().getWindowSize();
			// 计算父节点的世界缩放
			Vector2 worldScale = getMatrixScale(mWindow.getTransform().parent.localToWorldMatrix);
			Vector2 uiRootScale = mLayoutManager.getUIRoot().getScale();
			Vector2 parentScale = worldScale / uiRootScale;
			// 计算移动的位置范围
			Vector2 minPos = parentWidgetSize * 0.5f * mMinRelativePos / parentScale;
			Vector2 maxPos = parentWidgetSize * 0.5f * mMaxRelativePos / parentScale;
			if(mClampType == CLAMP_TYPE.EDGE_IN_RECT)
			{
				Vector2 thisSize = mWindow.getWindowSize(true);
				minPos += thisSize * 0.5f;
				maxPos -= thisSize * 0.5f;
				if(!mClampInner)
				{
					swap(ref minPos, ref maxPos);
				}
			}
			else if(mClampType == CLAMP_TYPE.CENTER_IN_RECT)
			{ }
			mMinMaxPos[0] = minPos;
			mMinMaxPos[1] = maxPos;
			mMinMaxPosDirty = false;
		}
		return mMinMaxPos;
	}
	protected void resetPosition(ref Vector3 position, bool alignTopOrLeft = true)
	{
		// 计算移动的位置范围
		Vector3[] minMaxPos = getLocalMinMaxPixelPos();
		Vector2 minPos = minMaxPos[0];
		Vector2 maxPos = minMaxPos[1];
		if(mDragDirection == DRAG_DIRECTION.HORIZONTAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(minPos.x, maxPos.x);
			float minValue = getMin(minPos.x, maxPos.x);
			if(minPos.x <= maxPos.x)
			{
				if(mClampInner)
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
				if(mClampInner)
				{
					position.x = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					position.x = alignTopOrLeft ? minValue : maxValue;
				}
			}
		}
		if(mDragDirection == DRAG_DIRECTION.VERTICAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(minPos.y, maxPos.y);
			float minValue = getMin(minPos.y, maxPos.y);
			if(minPos.y <= maxPos.y)
			{
				if(mClampInner)
				{
					position.y = alignTopOrLeft ? minValue : maxValue;
				}
				else
				{
					position.y = alignTopOrLeft ? maxValue : minValue;
				}
			}
			else
			{
				if(mClampInner)
				{
					position.y = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					position.y = alignTopOrLeft ? minValue : maxValue;
				}
			}
		}
	}
	// alignTopOrLeft表示当控件不允许拖动时,是否将上边界或者左边界(或中心点)与拖动范围对齐
	protected void clampPosition(ref Vector3 position, bool alignTopOrLeft = true)
	{
		// 计算移动的位置范围
		Vector3[] minMaxPos = getLocalMinMaxPixelPos();
		Vector2 minPos = minMaxPos[0];
		Vector2 maxPos = minMaxPos[1];
		if(mDragDirection == DRAG_DIRECTION.HORIZONTAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(minPos.x, maxPos.x);
			float minValue = getMin(minPos.x, maxPos.x);
			// 滑动范围有效时需要限定在一定范围
			if(minPos.x <= maxPos.x)
			{
				clamp(ref position.x, minValue, maxValue);
			}
			// 不能滑动的情况下
			else
			{
				if(mClampInner)
				{
					position.x = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					position.x = alignTopOrLeft ? minValue : maxValue;
				}
			}
		}
		if(mDragDirection == DRAG_DIRECTION.VERTICAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(minPos.y, maxPos.y);
			float minValue = getMin(minPos.y, maxPos.y);
			// 滑动范围有效时需要限定在一定范围
			if(minPos.y <= maxPos.y)
			{
				clamp(ref position.y, minValue, maxValue);
			}
			// 不能滑动的情况下
			else
			{
				if(mClampInner)
				{
					position.y = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					position.y = alignTopOrLeft ? minValue : maxValue;
				}
			}
		}
	}
	// 位置是否位于合法范围内,如果不合法,则validPos会返回合法位置
	protected bool isValidPosition(ref Vector3 position, ref Vector3 validPos, bool alignTopOrLeft = true)
	{
		validPos = position;
		Vector3[] minMaxPos = getLocalMinMaxPixelPos();
		Vector2 minPos = minMaxPos[0];
		Vector2 maxPos = minMaxPos[1];
		bool horiValid = false;
		bool vertValid = false;
		if(mDragDirection == DRAG_DIRECTION.HORIZONTAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(minPos.x, maxPos.x);
			float minValue = getMin(minPos.x, maxPos.x);
			horiValid = false;
			if(minPos.x <= maxPos.x)
			{
				horiValid = inRange(position.x, minValue, maxValue);
				if(!horiValid)
				{
					float x = position.x;
					clamp(ref x, minValue, maxValue);
					validPos.x = x;
				}
			}
			// 不能滑动的情况下
			else
			{
				if(mClampInner)
				{
					validPos.x = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					validPos.x = alignTopOrLeft ? minValue : maxValue;
				}
			}
		}
		if(mDragDirection == DRAG_DIRECTION.VERTICAL || mDragDirection == DRAG_DIRECTION.FREE)
		{
			float maxValue = getMax(minPos.y, maxPos.y);
			float minValue = getMin(minPos.y, maxPos.y);
			vertValid = false;
			if(minPos.y <= maxPos.y)
			{
				vertValid = inRange(position.y, minValue, maxValue);
				if(!vertValid)
				{
					float y = position.y;
					clamp(ref y, minValue, maxValue);
					validPos.y = y;
				}
			}
			// 不能滑动的情况下
			else
			{
				if(mClampInner)
				{
					validPos.y = alignTopOrLeft ? maxValue : minValue;
				}
				else
				{
					validPos.y = alignTopOrLeft ? minValue : maxValue;
				}
			}
		}
		if(mDragDirection == DRAG_DIRECTION.HORIZONTAL)
		{
			return horiValid;
		}
		else if(mDragDirection == DRAG_DIRECTION.VERTICAL)
		{
			return vertValid;
		}
		return horiValid && vertValid;
	}
}