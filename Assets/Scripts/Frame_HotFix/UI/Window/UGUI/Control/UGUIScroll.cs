using UnityEngine;
using System.Collections.Generic;
using static MathUtility;
using static FrameBaseHotFix;

// 自定义的滑动列表,基于容器(相当于状态的预设),物体(用于显示的物体),物体的各个状态在每个容器之间插值计算
// 需要在初始化时主动调用initScroll,也需要主动调用update
// 一般制作Container时需要多两个结束的Container放在两端,使Item在超出Container时不至于突然消失
public class UGUIScroll : WindowObjectUGUI, ICommonUI
{
	protected List<IScrollContainer> mContainerList = new();// 容器列表,用于获取位置缩放旋转等属性,每一容器的控制值就是其下标
	protected List<IScrollItem> mItemList = new();          // 物体列表,用于显示,每一项的控制值就是其下标,所以在下面的代码中会频繁使用其下标来计算位置
	protected ScrollItemCallback mOnScrollItem;					// 滚动的回调
	protected MyCurve mScrollToTargetCurve;					// 滚动到指定项使用的曲线
	protected DRAG_DIRECTION mDragDirection;				// 拖动方向,横向或纵向
	protected float mFocusSpeedThreshold;					// 开始聚焦的速度阈值,当滑动速度正在自由降低的阶段时,速度低于该值则会以恒定速度自动聚焦到一个最近的项
	protected float mMaxContainerValue;						// 容器中最大的控制值
	protected float mMaxControlValue;						// 物体列表中最大的控制值,也代表一个完整周期
	protected float mDragSensitive;							// 拖动的灵敏度
	protected float mAttenuateFactor;						// 移动速度衰减系数,鼠标在放开时移动速度会逐渐降低,衰减系数越大.速度降低越快
	protected float mScrollToTargetStartValue;				// 开始插值滚动到指定项时的控制值
	protected float mScrollToTargetTimer;					// 插值滚动到指定项的计时
	protected float mScrollToTargetMaxTime;					// 插值滚动到指定项的最大时间
	protected int mMainFocus;								// 容器默认聚焦的下标
	protected bool mLoop;									// 是否循环滚动
	// 以下是用于实时计算的参数
	protected SCROLL_STATE mState;							// 当前状态
	protected float mTargetOffsetValue;						// 本次移动的目标值
	protected float mCurOffset;								// 整体的偏移值,并且会与每一项的原始偏移值叠加
	protected float mScrollSpeed;							// 当前滚动速度
	protected bool mMouseDown;								// 鼠标是否在窗口内按下,鼠标抬起或者离开窗口都会设置为false,鼠标按下时,跟随鼠标移动,鼠标放开时,按惯性移动
	public UGUIScroll(IWindowObjectOwner parent) : base(parent)
	{
		mDragDirection = DRAG_DIRECTION.HORIZONTAL;
		mFocusSpeedThreshold = 1.5f;
		mDragSensitive = 1.0f;
		mAttenuateFactor = 3.0f;
		mScrollToTargetMaxTime = 0.2f;
	}
	protected override void assignWindowInternal(){}
	public override void init()
	{
		base.init();
		mRoot.setOnTouchDown(onMouseDown);
		mRoot.setOnScreenTouchUp(onScreenMouseUp);
		mRoot.setOnTouchMove(onMouseMove);
		mRoot.setOnTouchStay(onMouseStay);
		mRoot.registeCollider(true);
		// 为了能拖拽,所以根节点的深度需要在所有子节点之上
		mRoot.setDepthOverAllChild(true);
	}
	public void initScroll<T>(List<T> containerList, int mainContainer = -1) where T : IScrollContainer
	{
		mContainerList.setRangeDerived(containerList);
		if (mContainerList.Count > 0)
		{
			mMaxContainerValue = mContainerList.Count - 1;
			mMainFocus = mainContainer;
			// 默认聚焦中间的Container
			if (mMainFocus < 0)
			{
				mMainFocus = mContainerList.Count >> 1;
			}
		}
		setScrollToTargetCurve(KEY_CURVE.CUBIC_IN);
	}
	public void setScrollToTargetCurve(int curve)
	{
		mScrollToTargetCurve = mKeyFrameManager.getKeyFrame(curve);
	}
	public int getNearIndex()
	{
		return getItemIndex(mMainFocus - mCurOffset, true, mLoop);
	}
	public int getItemCount() { return mItemList.Count; }
	public void setItemList<T>(List<T> itemList, int defaultIndex = 0) where T : class, IScrollItem
	{
		// 每一项的控制值就是其下标,所以在
		mItemList.setRange(itemList);
		if (mItemList.Count == 0)
		{
			mMaxControlValue = 0.0f;
			return;
		}
		mMaxControlValue = mItemList.Count - 1;
		// 循环时首尾相接,但是首位之间间隔一个单位,所以整体长度需要加1
		if (mLoop)
		{
			mMaxControlValue += 1.0f;
		}
		if (defaultIndex >= 0)
		{
			scrollToIndex(defaultIndex);
		}
	}
	public void update(float elapsedTime)
	{
		// 自动匀速滚动到目标点
		if (mState == SCROLL_STATE.SCROLL_TO_TARGET)
		{
			float curOffset = mCurOffset;
			// 速度逐渐降低到速度阈值的一半,这里会将速度转化为绝对值再计算,但是为了避免可能对其他逻辑产生的影响,计算后会恢复其符号
			float speedSign = sign(mScrollSpeed);
			mScrollSpeed = abs(mScrollSpeed) - elapsedTime * 1.0f;
			clampMin(ref mScrollSpeed, mFocusSpeedThreshold * 0.5f);
			checkReachTarget(ref curOffset, elapsedTime * sign(mTargetOffsetValue - curOffset) * mScrollSpeed, mTargetOffsetValue);
			mScrollSpeed *= speedSign;
			updateItem(curOffset);
			if (isFloatEqual(mCurOffset, mTargetOffsetValue))
			{
				stop();
			}
		}
		else if (mState == SCROLL_STATE.LERP_SCROLL_TO_TARGET)
		{
			mScrollToTargetTimer += elapsedTime;
			float percent = mScrollToTargetCurve.evaluate(divide(mScrollToTargetTimer, mScrollToTargetMaxTime));
			updateItem(lerp(mScrollToTargetStartValue, mTargetOffsetValue, percent));
			if (isFloatEqual(mCurOffset, mTargetOffsetValue))
			{
				stop();
			}
		}
		// 鼠标拖动
		else if (mState == SCROLL_STATE.DRAGING)
		{
			scroll(mMainFocus - mCurOffset + elapsedTime * mScrollSpeed, false);
		}
		// 鼠标抬起后自动减速到停止,或者减速到一定阈值,再自动滚动到某个项
		else if (mState == SCROLL_STATE.SCROLL_TO_STOP)
		{
			float curControlValue = mMainFocus - mCurOffset;
			bool needClamp = !mLoop && !inRangeFixed(curControlValue, 0.0f, mMaxControlValue);
			// 非循环模式下,当前偏移值小于0或者大于最大值时,需要回到正常的范围,偏移值越小,减速越快
			if (!isFloatZero(mScrollSpeed))
			{
				float t;
				// 超出范围后快速减速至0,然后回弹
				if (needClamp)
				{
					float delta = 0.0f;
					if (curControlValue < 0.0f)
					{
						delta = 1.0f - curControlValue * 10.0f;
					}
					else if (curControlValue > mMaxControlValue)
					{
						delta = 1.0f + (curControlValue - mMaxControlValue) * 10.0f;
					}
					t = elapsedTime * mAttenuateFactor * delta * delta * 200.0f;
				}
				else
				{
					t = elapsedTime * mAttenuateFactor;
				}
				mScrollSpeed = lerp(mScrollSpeed, 0.0f, t, 0.1f);
				curControlValue += elapsedTime * mScrollSpeed;
				scroll(curControlValue, false);
				int willFocusIndex = getNearIndex();
				if (needClamp)
				{
					// 超出范围在移动停止后回弹
					if (isFloatZero(mScrollSpeed))
					{
						lerpToTarget(willFocusIndex);
					}
				}
				else
				{
					// 当速度小于一定值时才开始选择聚焦到某一项
					if (abs(mScrollSpeed) < mFocusSpeedThreshold)
					{
						scrollToTargetWithSpeed(willFocusIndex);
					}
				}
			}
			else
			{
				int willFocusIndex = getNearIndex();
				if (needClamp)
				{
					lerpToTarget(willFocusIndex);
				}
				else
				{
					scrollToTargetWithSpeed(willFocusIndex);
				}
			}
		}
	}
	public void stop()
	{
		mState = SCROLL_STATE.NONE;
		mScrollSpeed = 0.0f;
	}
	public SCROLL_STATE getState() { return mState; }
	// 直接设置到指定位置
	public void scroll(float controlValue, bool checkValueRange = true)
	{
		float offset = mMainFocus - controlValue;
		if (checkValueRange && !mLoop)
		{
			clamp(ref offset, mMainFocus - mMaxControlValue, mMainFocus);
		}
		updateItem(offset);
	}
	// 立即设置到指定下标
	public void scrollToIndex(int index)
	{
		if (mItemList.Count == 0)
		{
			return;
		}
		clamp(ref index, 0, mItemList.Count - 1);
		scroll(index);
		mOnScrollItem?.Invoke(mItemList[index], index);
	}
	// 一定时间滚动到指定下标
	public void scrollToIndexWithTime(int index, float time)
	{
		if (mItemList.Count == 0)
		{
			return;
		}
		clampMin(ref time, 0.03f);
		clamp(ref index, 0, mItemList.Count - 1);
		// 设置目标值
		mTargetOffsetValue = mMainFocus - index;
		if (mLoop)
		{
			clampCycle(ref mCurOffset, 0, mMaxControlValue, mMaxControlValue, false);
			clampCycle(ref mTargetOffsetValue, 0, mMaxControlValue, mMaxControlValue, false);
			// 当起始值与目标值差值超过了最大值的一半时,则以当前值为基准,调整目标值的范围
			float halfMax = mMaxControlValue * 0.5f;
			if (abs(mTargetOffsetValue - mCurOffset) > halfMax)
			{
				clampCycle(ref mCurOffset, -halfMax, halfMax, mMaxControlValue, false);
				clampCycle(ref mTargetOffsetValue, -halfMax, halfMax, mMaxControlValue, false);
			}
		}
		mScrollSpeed = divide(mTargetOffsetValue - mCurOffset, time);
		mState = isFloatZero(mScrollSpeed) ? SCROLL_STATE.NONE : SCROLL_STATE.SCROLL_TO_TARGET;
		mOnScrollItem?.Invoke(mItemList[index], index);
	}
	// 根据当前速度计算出滚动时间,匀减速滚动到指定下标
	public void scrollToTargetWithSpeed(int focusIndex)
	{
		float focusTime = clamp(abs(divide(0.4f, mScrollSpeed)), 0.1f, 0.3f);
		scrollToIndexWithTime(focusIndex, focusTime);
	}
	// 按照指定曲线插值聚焦到指定下标
	public void lerpToTarget(int index)
	{
		mState = SCROLL_STATE.LERP_SCROLL_TO_TARGET;
		mScrollToTargetStartValue = mCurOffset;
		mScrollToTargetTimer = 0.0f;
		clamp(ref index, 0, mItemList.Count - 1);
		// 设置目标值
		mTargetOffsetValue = mMainFocus - index;
		if (mLoop)
		{
			clampCycle(ref mCurOffset, 0, mMaxControlValue, mMaxControlValue, false);
			clampCycle(ref mTargetOffsetValue, 0, mMaxControlValue, mMaxControlValue, false);
			// 当起始值与目标值差值超过了最大值的一半时,则以当前值为基准,调整目标值的范围
			float halfMax = mMaxControlValue * 0.5f;
			if (abs(mTargetOffsetValue - mCurOffset) > halfMax)
			{
				clampCycle(ref mCurOffset, -halfMax, halfMax, mMaxControlValue, false);
				clampCycle(ref mTargetOffsetValue, -halfMax, halfMax, mMaxControlValue, false);
			}
		}
		mOnScrollItem?.Invoke(mItemList[index], index);
	}
	public void setDragDirection(DRAG_DIRECTION direction)		{ mDragDirection = direction; }
	public void setLoop(bool loop)								{ mLoop = loop; }
	public float getCurOffsetValue()							{ return mCurOffset; }
	public void setDragSensitive(float sensitive)				{ mDragSensitive = sensitive; }
	public void setFocusSpeedThreshold(float threshold)			{ mFocusSpeedThreshold = threshold; }
	public void setAttenuateFactor(float factor)				{ mAttenuateFactor = factor; }
	public void setOnScrollItem(ScrollItemCallback callback)	{ mOnScrollItem = callback; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void updateItem(float controlValue)
	{
		if (mContainerList.Count == 0)
		{
			return;
		}

		// 变化时需要随时更新当前值
		mCurOffset = controlValue;
		if (mLoop)
		{
			clampCycle(ref mCurOffset, -mMaxControlValue, mMaxControlValue, mMaxControlValue, false);
		}
		int itemCount = mItemList.Count;
		for (int i = 0; i < itemCount; ++i)
		{
			IScrollItem item = mItemList[i];
			myUGUIObject itemRoot = item.getItemRoot();
			float newControlValue = i + mCurOffset;
			itemRoot.setActive(true);
			if (mLoop)
			{
				clampCycle(ref newControlValue, -mMaxControlValue, mMaxControlValue, mMaxControlValue, false);
			}
			else
			{
				// 非循环模式下,新的控制值不在容器控制值范围内时,表示已经不在容器范围内了
				if (!inRangeFixed(newControlValue, 0.0f, mMaxContainerValue))
				{
					itemRoot.setActive(false);
				}
			}
			if (!itemRoot.isActiveInHierarchy())
			{
				continue;
			}
			// 找到当前项位于哪两个容器之间,并且计算插值系数
			int containerIndex = getContainerIndex(newControlValue, false);
			itemRoot.setActive(inRangeFixed(containerIndex, 0, mContainerList.Count - 1));
			if (!itemRoot.isActiveInHierarchy())
			{
				continue;
			}
			int nextContainerIndex = containerIndex + 1;
			if (mLoop)
			{
				nextContainerIndex %= mContainerList.Count;
			}
			if (inRangeFixed(nextContainerIndex, 0, mContainerList.Count - 1))
			{
				float curItemOffsetValue = containerIndex;
				float nextItemOffsetValue = nextContainerIndex;
				// 下一个下标比当前下标还小时,说明下一个下标已经从头开始了,需要重新调整下标
				if (nextContainerIndex < containerIndex && mLoop)
				{
					nextItemOffsetValue = curItemOffsetValue + 1;
				}
				clampCycle(ref newControlValue, curItemOffsetValue, nextItemOffsetValue, mMaxControlValue);
				float percent = inverseLerp(curItemOffsetValue, nextItemOffsetValue, newControlValue);
				checkInt(ref percent);
				saturate(ref percent);
				item.lerpItem(mContainerList[containerIndex], mContainerList[nextContainerIndex], percent);
			}
			else
			{
				item.lerpItem(mContainerList[containerIndex], mContainerList[containerIndex], 1.0f);
			}
		}
	}
	// 根据controlValue查找在ItemList中的对应下标
	protected int getItemIndex(float controlValue, bool nearest, bool loop)
	{
		int itemCount = mItemList.Count;
		if (itemCount == 0)
		{
			return -1;
		}
		if (loop)
		{
			clampCycle(ref controlValue, 0.0f, mMaxControlValue, mMaxControlValue, false);
		}
		int index = -1;
		for (int i = 0; i < itemCount; ++i)
		{
			float thisControllValue = i;
			if (isFloatEqual(thisControllValue, controlValue))
			{
				index = i;
				break;
			}
			// 找到第一个比controlValue大的项
			if (thisControllValue >= controlValue)
			{
				if (nearest)
				{
					if (i > 0 && abs(thisControllValue - controlValue) >= abs(i - 1 - controlValue))
					{
						index = i - 1;
					}
					else
					{
						index = i;
					}
				}
				else
				{
					index = clampMin(i - 1);
				}
				break;
			}
		}
		// 如果找不到比当前ControlValue大的项
		if (index < 0)
		{
			index = itemCount - 1;
			// 非循环模式下,则固定范围最后一个,循环模式就找最后一个或者第一个中最近的一个
			if (loop && nearest)
			{
				if (abs(0 - (mMaxControlValue - controlValue)) < abs(itemCount - 1 - controlValue))
				{
					index = 0;
				}
			}
		}
		return index;
	}
	// 根据controlValue查找在ContainerList中的对应下标,nearest为true则表示查找离该controlValue最近的下标
	protected int getContainerIndex(float controlValue, bool nearest)
	{
		if (mLoop)
		{
			clampCycle(ref controlValue, 0.0f, mMaxControlValue, mMaxControlValue, false);
		}
		if (controlValue > mMaxContainerValue)
		{
			return mContainerList.Count - 1;
		}
		if (controlValue < 0.0f)
		{
			return -1;
		}
		int index = -1;
		int containerCount = mContainerList.Count;
		for (int i = 0; i < containerCount; ++i)
		{
			float curControlValue = i;
			if (isFloatEqual(curControlValue, controlValue))
			{
				index = i;
				break;
			}
			// 找到第一个比controlValue大的项
			if (curControlValue >= controlValue)
			{
				if (nearest)
				{
					if (i > 0 && abs(curControlValue - controlValue) <= abs(i - 1 - controlValue))
					{
						index = i - 1;
					}
					else
					{
						index = i;
					}
				}
				else
				{
					index = i - 1;
				}
				break;
			}
		}
		return index;
	}
	protected void onMouseDown(Vector3 touchPos, int touchID)
	{
		mMouseDown = true;
		mState = SCROLL_STATE.DRAGING;
		mScrollSpeed = 0.0f;
	}
	// 鼠标在屏幕上抬起
	protected void onScreenMouseUp(Vector3 touchPos, int touchID)
	{
		mMouseDown = false;
		// 正在拖动时鼠标抬起,则开始逐渐减速到0
		if (mState == SCROLL_STATE.DRAGING)
		{
			mState = SCROLL_STATE.SCROLL_TO_STOP;
		}
	}
	protected void onMouseMove(Vector3 touchPos, Vector3 moveDelta, float moveTime, int touchID)
	{
		// 鼠标未按下时不允许改变移动速度
		if (!mMouseDown)
		{
			return;
		}
		if (mDragDirection == DRAG_DIRECTION.HORIZONTAL)
		{
			mScrollSpeed = sign(-moveDelta.x) * abs(divide(moveDelta.x, moveTime)) * mDragSensitive * 0.01f;
		}
		else if (mDragDirection == DRAG_DIRECTION.VERTICAL)
		{
			mScrollSpeed = sign(moveDelta.y) * abs(divide(moveDelta.y, moveTime)) * mDragSensitive * 0.01f;
		}
	}
	protected void onMouseStay(Vector3 touchPos, int touchID)
	{
		if (!mMouseDown)
		{
			return;
		}
		mScrollSpeed = 0.0f;
	}
}