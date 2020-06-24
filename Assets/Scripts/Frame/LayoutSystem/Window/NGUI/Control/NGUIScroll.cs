using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if USE_NGUI

public class NGUIScroll : GameBase
{
	protected LayoutScript mScript;
	//------------------------------------------------------------------------------------------------------------------------------------------------------
	// 用于设置的参数
	protected DRAG_DIRECTION mDragDirection;        // 拖动方向,横向或纵向
	protected txNGUITexture mBackground;            // 用于检测鼠标的按下,移动,抬起
	protected List<IScrollContainer> mContainerList;// 容器列表,用于获取位置缩放旋转等属性
	protected List<IScrollItem> mItemList;          // 物体列表,用于显示
	protected float mFocusSpeedThreshhold = 2.0f;   // 开始聚焦的速度阈值,当滑动速度正在自由降低的阶段时,速度低于该值则会以恒定速度自动聚焦到一个最近的项
	protected bool mLoop = false;                   // 是否循环滚动
	protected bool mItemOnCenter = true;            // 最终停止时是否聚焦到某一项上
	protected int mDefaultFocus;                    // 容器默认聚焦的下标
	protected float mMaxControlValue;               // 物体列表中最大的控制值
	protected float mMaxContainerValue;				// 容器中最大的控制值
	protected float mDragSensitive = 1.0f;          // 拖动的灵敏度
	protected OnScrollItem mOnScrollItem;
	//------------------------------------------------------------------------------------------------------------------------------------------------------
	// 用于实时计算的参数
	protected bool mMouseDown = false;              // 鼠标是否在窗口内按下,鼠标抬起或者离开窗口都会设置为false,鼠标按下时,跟随鼠标移动,鼠标放开时,按惯性移动
	protected float mAttenuateFactor = 2.0f;        // 移动速度衰减系数,鼠标在放开时移动速度会逐渐降低,衰减系数越大.速度降低越快
	protected float mTargetOffsetValue;             // 本次移动的目标值
	protected float mCurOffset;						// 整体的偏移值,并且会与每一项的原始偏移值叠加
	protected SCROLL_STATE mState;                  // 当前状态
	protected float mScrollSpeed;                   // 当前滚动速度
	public NGUIScroll(LayoutScript script)
	{
		mScript = script;
		mState = SCROLL_STATE.SS_NONE;
		mDragDirection = DRAG_DIRECTION.DD_HORIZONTAL;
		mContainerList = new List<IScrollContainer>();
		mItemList = new List<IScrollItem>();
	}
	public void setBackground(txNGUITexture background)
	{
		mBackground = background;
		mBackground.setOnMouseDown(onMouseDown);
		mBackground.setOnScreenMouseUp(onScreenMouseUp);
		mBackground.setOnMouseMove(onMouseMove);
		mBackground.setOnMouseStay(onMouseStay);
		mScript.registeBoxCollider(mBackground, true);
	}
	public int getFocusIndex()
	{
		float curControlValue = mContainerList[mDefaultFocus].getControlValue() - mCurOffset;
		return getItemIndex(curControlValue, true, mLoop);
	}
	public void setItemList<T>(List<T> itemList) where T : IScrollItem
	{
		mItemList.Clear();
		float singleInterval = 1.0f;
		int itemCount = itemList.Count;
		for (int i = 0; i < itemCount; ++i)
		{
			itemList[i].setCurControlValue(i * singleInterval);
			mItemList.Add(itemList[i]);
		}
		if(mItemList.Count > 0)
		{
			if (mLoop)
			{
				mMaxControlValue = mItemList[mItemList.Count - 1].getCurControlValue() + singleInterval;
			}
			else
			{
				mMaxControlValue = mItemList[mItemList.Count - 1].getCurControlValue();
			}
		}
		else
		{
			mMaxControlValue = 0.0f;
		}
	}
	public void setContainerList<T>(List<T> containerList, int defaultFocus) where T : IScrollContainer
	{
		mContainerList.Clear();
		int containerCount = containerList.Count;
		for (int i = 0; i < containerCount; ++i)
		{
			containerList[i].setIndex(i);
			containerList[i].initOrigin();
			containerList[i].setControlValue(i);
			mContainerList.Add(containerList[i]);
		}
		if(mContainerList.Count > 0)
		{
			mMaxContainerValue = mContainerList[mContainerList.Count - 1].getControlValue();
			mDefaultFocus = defaultFocus;
		}
	}
	public void update(float elapsedTime)
	{
		// 自动匀速滚动到目标点
		if (mState == SCROLL_STATE.SS_SCROLL_TARGET)
		{
			float preOffset = mCurOffset;
			mCurOffset += elapsedTime * mScrollSpeed;
			if (isReachTarget(preOffset, mCurOffset, mTargetOffsetValue, mScrollSpeed))
			{
				mCurOffset = mTargetOffsetValue;
				mState = SCROLL_STATE.SS_NONE;
			}
			updateItem(mCurOffset);
		}
		// 鼠标拖动
		else if (mState == SCROLL_STATE.SS_DRAGING)
		{
			float curOffset = mContainerList[mDefaultFocus].getControlValue() - mCurOffset;
			scroll(curOffset + elapsedTime * mScrollSpeed, false);
		}
		// 鼠标抬起后自动减速到停止
		else if (mState == SCROLL_STATE.SS_SCROLL_TO_STOP)
		{
			float curControlValue = mContainerList[mDefaultFocus].getControlValue() - mCurOffset;
			// 非循环模式下,当前偏移值小于0或者大于最大值时,需要回到正常的范围,偏移值越小,减速越快
			bool autoClamp = !mLoop && !isInRange(curControlValue, 0.0f, mMaxControlValue, true);
			if (!isFloatZero(mScrollSpeed))
			{
				if(autoClamp)
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
					mScrollSpeed = lerp(mScrollSpeed, 0.0f, elapsedTime * mAttenuateFactor * delta * delta * 200.0f, 0.1f);
				}
				else
				{
					mScrollSpeed = lerp(mScrollSpeed, 0.0f, elapsedTime * mAttenuateFactor, 0.1f);
				}
				curControlValue += elapsedTime * mScrollSpeed;
				scroll(curControlValue, false);
				// 当速度小于一定值时才开始选择聚焦到某一项
				if (mItemOnCenter)
				{
					if(autoClamp)
					{
						if(isFloatZero(mScrollSpeed))
						{
							scrollToTarget(curControlValue, 0.1f, false);
						}
					}
					else
					{
						if (abs(mScrollSpeed) < mFocusSpeedThreshhold)
						{
							scrollToTarget(curControlValue);
						}
					}
				}
				// 逐渐减速到0
				else
				{
					if (isFloatZero(mScrollSpeed))
					{
						if (autoClamp)
						{
							scrollToTarget(curControlValue, 0.1f, false);
						}
						else
						{
							mState = SCROLL_STATE.SS_NONE;
						}
					}
				}
			}
			// 鼠标停止后才抬起结束拖拽
			else
			{
				// 超出范围则回到正常范围内
				if(autoClamp)
				{
					scrollToTarget(curControlValue, 0.1f, false);
				}
				else
				{
					if (mItemOnCenter)
					{
						scrollToTarget(curControlValue);
					}
					else
					{
						mState = SCROLL_STATE.SS_NONE;
					}
				}
			}
		}
	}
	// 直接设置到指定位置
	public void scroll(float controlValue, bool checkValueRange = true)
	{
		float offset = mContainerList[mDefaultFocus].getControlValue() - controlValue;
		if(checkValueRange && !mLoop)
		{
			clamp(ref offset, mContainerList[mDefaultFocus].getControlValue() - mMaxControlValue, mContainerList[mDefaultFocus].getControlValue());
		}
		updateItem(offset);
	}
	public void scroll(int index)
	{
		if (mItemList.Count == 0)
		{
			return;
		}
		clamp(ref index, 0, mItemList.Count - 1);
		scroll(mItemList[index].getCurControlValue());
		mOnScrollItem?.Invoke(mItemList[index], index);
	}
	// 滚动到指定下标
	public void scroll(int index, float time)
	{
		if (mItemList.Count == 0)
		{
			return;
		}
		clampMin(ref time, 0.05f);
		clamp(ref index, 0, mItemList.Count - 1);
		// 设置目标值
		mTargetOffsetValue = mContainerList[mDefaultFocus].getControlValue() - mItemList[index].getCurControlValue();
		if (mLoop && abs(mTargetOffsetValue - mCurOffset) > mMaxControlValue * 0.5f)
		{
			// 当起始值与目标值差值超过了最大值的一半时,则以当前值为基准,调整目标值的范围
			clampValue(ref mTargetOffsetValue, mCurOffset - (mMaxControlValue + 1) * 0.5f, mCurOffset + (mMaxControlValue + 1) * 0.5f, mMaxControlValue + 1, false);
			mScrollSpeed = (mTargetOffsetValue - mCurOffset) / time;
		}
		else
		{
			mScrollSpeed = (mTargetOffsetValue - mCurOffset) / time;
		}
		mState = isFloatZero(mScrollSpeed) ? SCROLL_STATE.SS_NONE : SCROLL_STATE.SS_SCROLL_TARGET;
		mOnScrollItem?.Invoke(mItemList[index], index);
	}
	public void setDragDirection(DRAG_DIRECTION direction) { mDragDirection = direction; }
	public void setLoop(bool loop) { mLoop = loop; }
	public float getCurOffsetValue() { return mCurOffset; }
	public void setItemOnCenter(bool center) { mItemOnCenter = center; }
	public void setDragSensitive(float sensitive) { mDragSensitive = sensitive; }
	public void setFocusSpeedThreshhold(float threshold) { mFocusSpeedThreshhold = threshold; }
	public void setAttenuateFactor(float factor) { mAttenuateFactor = factor; }
	public void setOnScrollItem(OnScrollItem callback) { mOnScrollItem = callback; }
	//---------------------------------------------------------------------------------------------------------------------------------------
	protected void updateItem(float controlValue)
	{
		// 变化时需要随时更新当前值
		mCurOffset = controlValue;
		if (mLoop)
		{
			clampValue(ref mCurOffset, 0.0f, mMaxControlValue + 1, mMaxControlValue + 1, false);
		}
		int itemCount = mItemList.Count;
		for (int i = 0; i < itemCount; ++i)
		{
			float newControlValue = mItemList[i].getCurControlValue() + mCurOffset;
			if(mLoop)
			{
				clampValue(ref newControlValue, 0, mMaxControlValue + 1, mMaxControlValue + 1, false);
			}
			else
			{
				// 非循环模式下,新的控制值不在容器控制值范围内时,表示已经不在容器范围内了
				if (!isInRange(newControlValue, 0.0f, mMaxContainerValue))
				{
					mItemList[i].setVisible(false);
					continue;
				}
			}
			
			int containerIndex = getContainerIndex(newControlValue, false, mLoop);
			if(!isInRange(containerIndex, 0, mContainerList.Count - 1))
			{
				mItemList[i].setVisible(false);
				continue;
			}
			else
			{
				mItemList[i].setVisible(true);
			}
			int nextcontainerIndex = containerIndex + 1;
			if (isInRange(nextcontainerIndex, 0, mContainerList.Count - 1))
			{
				float curItemOffsetValue = mContainerList[containerIndex].getControlValue();
				float nextItemOffsetValue = mContainerList[nextcontainerIndex].getControlValue();
				float percent = inverseLerp(curItemOffsetValue, nextItemOffsetValue, newControlValue);
				checkInt(ref percent);
				saturate(ref percent);
				mItemList[i].lerp(mContainerList[containerIndex], mContainerList[nextcontainerIndex], percent);
			}
			else
			{
				mItemList[i].lerp(mContainerList[containerIndex], mContainerList[containerIndex], 1.0f);
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
			clampValue(ref controlValue, 0.0f, mMaxControlValue + 1, mMaxControlValue + 1, false);
		}
		int index = -1;
		for (int i = 0; i < itemCount; ++i)
		{
			if (isFloatEqual(mItemList[i].getCurControlValue(), controlValue))
			{
				index = i;
				break;
			}
			// 找到第一个比controlValue大的项
			if (mItemList[i].getCurControlValue() >= controlValue)
			{
				if (!nearest)
				{
					index = i > 0 ? i - 1 : 0;
				}
				else
				{
					if (i - 1 >= 0)
					{
						if (abs(mItemList[i].getCurControlValue() - controlValue) < abs(mItemList[i - 1].getCurControlValue() - controlValue))
						{
							index = i;
						}
						else
						{
							index = i - 1;
						}
					}
					else
					{
						index = i;
					}
				}
				break;
			}
		}
		// 如果找不到比当前ControlValue大的项
		if(index < 0)
		{
			// 非循环模式下,则固定范围最后一个
			if (!loop)
			{
				index = itemCount - 1;
			}
			else
			{
				if (!nearest)
				{
					index = itemCount - 1;
				}
				else
				{
					if (abs(mItemList[0].getCurControlValue() - (mMaxControlValue + 1 - controlValue)) < abs(mItemList[itemCount - 1].getCurControlValue() - controlValue))
					{
						index = 0;
					}
					else
					{
						index = itemCount - 1;
					}
				}
			}
		}
		return index;
	}
	// 根据controlValue查找在ContainerList中的对应下标,nearest为true则表示查找离该controlValue最近的下标
	protected int getContainerIndex(float controlValue, bool nearest, bool loop)
	{
		if (loop)
		{
			clampValue(ref controlValue, 0.0f, mMaxControlValue + 1, mMaxControlValue + 1, false);
		}
		// -1表示找不到对应的容器下标
		if(!isInRange(controlValue, 0.0f, mMaxContainerValue))
		{
			return -1;
		}
		int index = -1;
		int containerCount = mContainerList.Count;
		for (int i = 0; i < containerCount; ++i)
		{
			if (isFloatEqual(mContainerList[i].getControlValue(), controlValue))
			{
				index = i;
				break;
			}
			// 找到第一个比controlValue大的项
			if (mContainerList[i].getControlValue() >= controlValue)
			{
				if(!nearest)
				{
					index = i - 1;
				}
				else
				{
					if (i - 1 > 0)
					{
						if (abs(mContainerList[i].getControlValue() - controlValue) > abs(mContainerList[i - 1].getControlValue() - controlValue))
						{
							index = i;
						}
						else
						{
							index = i - 1;
						}
					}
					else
					{
						index = i;
					}
				}
				break;
			}
		}
		return index;
	}
	protected void scrollToTarget(float curOffset, float time = 0.3f, bool timeBaseOnSpeed = true)
	{
		int focusIndex = getItemIndex(curOffset, true, mLoop);
		float focusTime = time;
		if (!isFloatZero(mScrollSpeed) && timeBaseOnSpeed)
		{
			focusTime = abs(0.5f / mScrollSpeed);
			clampMax(ref focusTime, 0.4f);
		}
		scroll(focusIndex, focusTime);
	}
	protected void onMouseDown(Vector2 mousePos)
	{
		mMouseDown = true;
		mState = SCROLL_STATE.SS_DRAGING;
		mScrollSpeed = 0.0f;
	}
	// 鼠标在屏幕上抬起
	protected void onScreenMouseUp(IMouseEventCollect obj, Vector2 mousePos)
	{
		mMouseDown = false;
		// 正在拖动时鼠标抬起,则开始逐渐减速到0
		if (mState == SCROLL_STATE.SS_DRAGING)
		{
			mState = SCROLL_STATE.SS_SCROLL_TO_STOP;
		}
	}
	protected void onMouseMove(ref Vector3 mousePos, ref Vector3 moveDelta, float moveTime)
	{
		// 鼠标未按下时不允许改变移动速度
		if (!mMouseDown)
		{
			return;
		}
		if (mDragDirection == DRAG_DIRECTION.DD_HORIZONTAL)
		{
			mScrollSpeed = sign(moveDelta.x) * abs(moveDelta.x / moveTime) * mDragSensitive * 0.01f;
		}
		else if(mDragDirection == DRAG_DIRECTION.DD_VERTICAL)
		{
			mScrollSpeed = sign(moveDelta.y) * abs(moveDelta.y / moveTime) * mDragSensitive * 0.01f;
		}
		else
		{
			return;
		}
	}
	protected void onMouseStay(Vector2 mousePos)
	{
		if (!mMouseDown)
		{
			return;
		}
		mScrollSpeed = 0.0f;
	}
}

#endif