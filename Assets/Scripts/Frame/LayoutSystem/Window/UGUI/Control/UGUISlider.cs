using System;
using System.Collections.Generic;
using UnityEngine;

public class UGUISlider : ComponentOwner, ISlider
{
	protected LayoutScript mScript;
	protected txUGUIObject mBackground;
	protected txUGUIObject mForeground;
	protected txUGUIObject mThumb;
	protected SliderCallback mSliderStartCallback;
	protected SliderCallback mSliderEndCallback;
	protected SliderCallback mSliderCallback;
	protected DRAG_DIRECTION mDirection;
	protected SLIDER_MODE mMode;
	protected Vector2 mOriginForegroundSize;
	protected Vector3 mOriginForegroundPosition;
	protected float mSliderValue;
	protected bool mDraging;
	public UGUISlider(LayoutScript script)
		:base("Slider")
	{
		mScript = script;
		mDirection = DRAG_DIRECTION.DD_HORIZONTAL;
		mMode = SLIDER_MODE.SM_FILL;
	}
	public void init(txUGUIObject background, txUGUIObject foreground, txUGUIObject thumb = null, SLIDER_MODE mode = SLIDER_MODE.SM_FILL)
	{
		mMode = mode;
		mBackground = background;
		mForeground = foreground;
		mThumb = thumb;
		if(mThumb != null && mThumb.getParent() != mForeground)
		{
			logError("Foreground must be parent of Thumb");
			return;
		}
		if(mMode == SLIDER_MODE.SM_SIZING)
		{
			mOriginForegroundSize = mForeground.getWindowSize();
			mOriginForegroundPosition = mForeground.getPosition();
			if(mBackground == null)
			{
				logError("Background can not be null while slider mode is SM_SIZING");
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
				mScript.registeBoxCollider(mBackground);
			}
		}
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
			OT.MOVE(mThumb, sliderValueToThumbPos(mSliderValue));
		}
		if (mForeground != null)
		{
			if(mMode == SLIDER_MODE.SM_FILL)
			{
				mForeground.setFillPercent(mSliderValue);
			}
			else if(mMode == SLIDER_MODE.SM_SIZING)
			{
				if (mDirection == DRAG_DIRECTION.DD_HORIZONTAL)
				{
					float newWidth = mSliderValue * mOriginForegroundSize.x;
					Vector3 newForePos = mOriginForegroundPosition;
					newForePos.x = mOriginForegroundPosition.x - mOriginForegroundSize.x * 0.5f + newWidth * 0.5f;
					OT.MOVE(mForeground, newForePos);
					mForeground.setWindowSize(new Vector2(newWidth, mOriginForegroundSize.y));
				}
				else if (mDirection == DRAG_DIRECTION.DD_VERTICAL)
				{
					float newHeight = mSliderValue * mOriginForegroundSize.y;
					Vector3 newForePos = mOriginForegroundPosition;
					newForePos.y = mOriginForegroundPosition.y - mOriginForegroundSize.y * 0.5f + newHeight * 0.5f;
					OT.MOVE(mForeground, newForePos);
					mForeground.setWindowSize(new Vector2(mOriginForegroundSize.x, newHeight));
				}
			}
		}
	}
	protected Vector3 sliderValueToThumbPos(float value)
	{
		Vector3 pos = Vector3.zero;
		if (mDirection == DRAG_DIRECTION.DD_HORIZONTAL)
		{
			pos = new Vector3(value * mOriginForegroundSize.x - mOriginForegroundSize.x * 0.5f, 0.0f);
		}
		else if (mDirection == DRAG_DIRECTION.DD_VERTICAL)
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
			if (mDirection == DRAG_DIRECTION.DD_HORIZONTAL)
			{
				value = (posInForeground.x + mOriginForegroundSize.x * 0.5f) / mOriginForegroundSize.x;
			}
			else if (mDirection == DRAG_DIRECTION.DD_VERTICAL)
			{
				value = (posInForeground.y + mOriginForegroundSize.y * 0.5f) / mOriginForegroundSize.y;
			}
			saturate(ref value);
		}
		return value;
	}
	protected void onMouseDown(Vector2 mousePos)
	{
		// 先调用开始回调
		mSliderStartCallback?.Invoke();
		// 计算当前值
		updateSlider(screenPosToSliderValue(mousePos));
		mDraging = true;
	}
	protected void onScreenMouseUp(IMouseEventCollect obj, Vector2 mousePos)
	{
		// 调用结束回调
		if (!mDraging)
		{
			return;
		}
		mDraging = false;
		mSliderEndCallback?.Invoke();
	}
	protected void onMouseMove(ref Vector3 mousePos, ref Vector3 moveDelta, float moveTime)
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
		if(mMode == SLIDER_MODE.SM_FILL)
		{
			posInForeground = screenPosToWindowPos(screenPos, mForeground);
		}
		// 先将屏幕坐标转换到Background中的坐标,再转换到原始进度条的坐标系中
		else if (mMode == SLIDER_MODE.SM_SIZING)
		{
			Vector3 posInBackground = screenPosToWindowPos(screenPos, mBackground);
			posInForeground = posInBackground - mOriginForegroundPosition;
		}
		return localPosToSliderValue(posInForeground);
	}
}