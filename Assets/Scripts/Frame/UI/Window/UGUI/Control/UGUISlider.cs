using System;
using System.Collections.Generic;
using UnityEngine;

// 自定义的滑动条
public class UGUISlider : WindowObjectUGUI, ISlider
{
	protected SliderCallback mSliderStartCallback;	// 开始拖拽滑动的回调
	protected SliderCallback mSliderEndCallback;	// 结束拖拽滑动的回调
	protected SliderCallback mSliderCallback;		// 滑动条改变的回调
	protected myUGUIObject mBackground;				// 滑动条背景窗口
	protected myUGUIImage mForeground;				// 滑动条中显示进度的窗口
	protected myUGUIObject mThumb;					// 滑块窗口
	protected Vector3 mOriginForegroundPosition;	// 进度窗口初始的位置
	protected Vector2 mOriginForegroundSize;		// 进度窗口初始的大小
	protected float mSliderValue;					// 当前的滑动值
	protected bool mDraging;						// 是否正在拖拽滑动
	protected DRAG_DIRECTION mDirection;			// 滑动方向
	protected SLIDER_MODE mMode;					// 滑动条显示的实现方式
	public UGUISlider()
	{
		mDirection = DRAG_DIRECTION.HORIZONTAL;
		mMode = SLIDER_MODE.FILL;
	}
	public override void assignWindow(myUIObject parent, string name)
	{
		base.assignWindow(parent, name);
		mBackground = mRoot;
		mScript.newObject(out mForeground, mRoot, "Foreground");
		mScript.newObject(out mThumb, mForeground, "Thumb", false);
	}
	public override void init()
	{
		if (mForeground == null)
		{
			logError("UGUISlider需要有一个名为Foreground的节点");
		}
		if (mThumb != null && mThumb.getParent() != mForeground)
		{
			logError("Foreground must be parent of Thumb");
			return;
		}
		mOriginForegroundSize = mForeground.getWindowSize();
		// 需要在初始化之前就设置Mode
		if (mMode == SLIDER_MODE.SIZING)
		{
			mOriginForegroundPosition = mForeground.getPosition();
			if(mBackground == null)
			{
				logError("Background can not be null while slider mode is SIZING");
				return;
			}
			if (mForeground.getParent() != mBackground)
			{
				logError("Background must be parent of Foreground");
				return;
			}
		}
		if(mBackground != null)
		{
			mBackground.setOnMouseDown(onMouseDown);
			mBackground.setOnScreenMouseUp(onScreenMouseUp);
			mBackground.setOnMouseMove(onMouseMove);
			if (mBackground.getCollider() != null)
			{
				mScript.registeCollider(mBackground);
			}
		}
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mScript = null;
		mBackground = null;
		mForeground = null;
		mThumb = null;
		mSliderStartCallback = null;
		mSliderEndCallback = null;
		mSliderCallback = null;
		mDirection = DRAG_DIRECTION.HORIZONTAL;
		mMode = SLIDER_MODE.FILL;
		mOriginForegroundSize = Vector2.zero;
		mOriginForegroundPosition = Vector3.zero;
		mSliderValue = 0.0f;
		mDraging = false;
	}
	public void setEnable(bool enable) { mBackground.setHandleInput(enable); }
	public void setDirection(DRAG_DIRECTION direction) { mDirection = direction; }
	public void setStartCallback(SliderCallback callback) { mSliderStartCallback = callback; }
	public void setEndCallback(SliderCallback callback) { mSliderEndCallback = callback; }
	public void setSliderCallback(SliderCallback callback) { mSliderCallback = callback; }
	public void setValue(float value) { updateSlider(value); }
	public float getValue() { return mSliderValue; }
	public bool isDraging() { return mDraging; }
	public void setSliderMode(SLIDER_MODE mode) { mMode = mode; }
	public SLIDER_MODE getSliderMode() { return mMode; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void updateSlider(float value)
	{
		mSliderValue = value;
		saturate(ref mSliderValue);
		if(mThumb != null)
		{
			FT.MOVE(mThumb, sliderValueToThumbPos(mSliderValue));
		}
		if (mForeground != null)
		{
			if(mMode == SLIDER_MODE.FILL)
			{
				mForeground.setFillPercent(mSliderValue);
			}
			else if(mMode == SLIDER_MODE.SIZING)
			{
				if (mDirection == DRAG_DIRECTION.HORIZONTAL)
				{
					float newWidth = mSliderValue * mOriginForegroundSize.x;
					Vector3 newForePos = mOriginForegroundPosition;
					newForePos.x = mOriginForegroundPosition.x - mOriginForegroundSize.x * 0.5f + newWidth * 0.5f;
					FT.MOVE(mForeground, newForePos);
					mForeground.setWindowSize(new Vector2(newWidth, mOriginForegroundSize.y));
				}
				else if (mDirection == DRAG_DIRECTION.VERTICAL)
				{
					float newHeight = mSliderValue * mOriginForegroundSize.y;
					Vector3 newForePos = mOriginForegroundPosition;
					newForePos.y = mOriginForegroundPosition.y - mOriginForegroundSize.y * 0.5f + newHeight * 0.5f;
					FT.MOVE(mForeground, newForePos);
					mForeground.setWindowSize(new Vector2(mOriginForegroundSize.x, newHeight));
				}
			}
		}
	}
	protected Vector3 sliderValueToThumbPos(float value)
	{
		Vector3 pos = Vector3.zero;
		if (mDirection == DRAG_DIRECTION.HORIZONTAL)
		{
			pos = new Vector3(value * mOriginForegroundSize.x - mOriginForegroundSize.x * 0.5f, 0.0f);
		}
		else if (mDirection == DRAG_DIRECTION.VERTICAL)
		{
			pos = new Vector3(0.0f, value * mOriginForegroundSize.y - mOriginForegroundSize.y * 0.5f);
		}
		return pos;
	}
	protected float localPosToSliderValue(Vector3 posInForeground)
	{
		float value = 0.0f;
		if (mOriginForegroundSize.x > 0.0f && mOriginForegroundSize.y > 0.0f)
		{
			if (mDirection == DRAG_DIRECTION.HORIZONTAL)
			{
				value = (posInForeground.x + mOriginForegroundSize.x * 0.5f) / mOriginForegroundSize.x;
			}
			else if (mDirection == DRAG_DIRECTION.VERTICAL)
			{
				value = (posInForeground.y + mOriginForegroundSize.y * 0.5f) / mOriginForegroundSize.y;
			}
			saturate(ref value);
		}
		return value;
	}
	protected void onMouseDown(Vector3 mousePos, int touchID)
	{
		// 先调用开始回调
		mSliderStartCallback?.Invoke();
		// 计算当前值
		updateSlider(screenPosToSliderValue(mousePos));
		mSliderCallback?.Invoke();
		mDraging = true;
	}
	protected void onScreenMouseUp(IMouseEventCollect obj, Vector3 mousePos, int touchID)
	{
		// 调用结束回调
		if (!mDraging)
		{
			return;
		}
		mDraging = false;
		mSliderEndCallback?.Invoke();
	}
	protected void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		if (!mDraging)
		{
			return;
		}
		updateSlider(screenPosToSliderValue(mousePos));
		mSliderCallback?.Invoke();
	}
	protected float screenPosToSliderValue(Vector3 screenPos)
	{
		Vector3 posInForeground = Vector3.zero;
		// 只转换到进度条窗口中的坐标
		if(mMode == SLIDER_MODE.FILL)
		{
			posInForeground = screenPosToWindow(screenPos, mForeground);
		}
		// 先将屏幕坐标转换到Background中的坐标,再转换到原始进度条的坐标系中
		else if (mMode == SLIDER_MODE.SIZING)
		{
			Vector3 posInBackground = screenPosToWindow(screenPos, mBackground);
			posInForeground = posInBackground - mOriginForegroundPosition;
		}
		return localPosToSliderValue(posInForeground);
	}
}