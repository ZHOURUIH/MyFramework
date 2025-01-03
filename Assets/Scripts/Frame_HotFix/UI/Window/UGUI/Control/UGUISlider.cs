using System;
using UnityEngine;
using static UnityUtility;
using static MathUtility;

// 自定义的滑动条
public class UGUISlider : WindowObjectUGUI, ISlider
{
	protected Action mSliderStartCallback;			// 开始拖拽滑动的回调
	protected Action mSliderEndCallback;			// 结束拖拽滑动的回调
	protected Action mSliderCallback;				// 滑动条改变的回调
	protected myUGUIImageSimple mForeground;		// 滑动条中显示进度的窗口
	protected myUGUIObject mThumb;					// 滑块窗口
	protected Vector3 mOriginForegroundPosition;	// 进度窗口初始的位置
	protected Vector2 mOriginForegroundSize;		// 进度窗口初始的大小
	protected float mSliderValue;					// 当前的滑动值
	protected bool mDraging;						// 是否正在拖拽滑动
	protected DRAG_DIRECTION mDirection;			// 滑动方向
	protected SLIDER_MODE mMode;                    // 滑动条显示的实现方式
	public UGUISlider(LayoutScript script)
		: base(script)
	{
		mDirection = DRAG_DIRECTION.HORIZONTAL;
		mMode = SLIDER_MODE.FILL;
	}
	public override void assignWindow(myUIObject parent, string name)
	{
		base.assignWindow(parent, name);
		newObject(out mForeground, "Foreground");
		newObject(out mThumb, mForeground, "Thumb", false);
	}
	public override void init()
	{
		mOriginForegroundSize = mForeground.getWindowSize();
		mOriginForegroundPosition = mForeground.getPosition();
		mRoot.registeCollider();
		mRoot.setOnMouseDown(onMouseDown);
		mRoot.setOnScreenMouseUp(onScreenMouseUp);
		mRoot.setOnMouseMove(onMouseMove);
	}
	public void setEnable(bool enable) { mRoot.setHandleInput(enable); }
	public void setDirection(DRAG_DIRECTION direction) { mDirection = direction; }
	public void setStartCallback(Action callback) { mSliderStartCallback = callback; }
	public void setEndCallback(Action callback) { mSliderEndCallback = callback; }
	public void setSliderCallback(Action callback) { mSliderCallback = callback; }
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
				mForeground.setWindowSize(new(newWidth, mOriginForegroundSize.y));
			}
			else if (mDirection == DRAG_DIRECTION.VERTICAL)
			{
				float newHeight = mSliderValue * mOriginForegroundSize.y;
				Vector3 newForePos = mOriginForegroundPosition;
				newForePos.y = mOriginForegroundPosition.y - mOriginForegroundSize.y * 0.5f + newHeight * 0.5f;
				FT.MOVE(mForeground, newForePos);
				mForeground.setWindowSize(new(mOriginForegroundSize.x, newHeight));
			}
		}
		if (mThumb != null)
		{
			FT.MOVE(mThumb, sliderValueToThumbPos(mSliderValue));
		}
	}
	protected Vector3 sliderValueToThumbPos(float value)
	{
		Vector3 pos = Vector3.zero;
		if (mDirection == DRAG_DIRECTION.HORIZONTAL)
		{
			if (mMode == SLIDER_MODE.FILL)
			{
				pos = new(value * mOriginForegroundSize.x - mOriginForegroundSize.x * 0.5f, 0.0f);
			}
			else if (mMode == SLIDER_MODE.SIZING)
			{
				pos = new(mForeground.getWindowRight(), 0.0f);
			}
		}
		else if (mDirection == DRAG_DIRECTION.VERTICAL)
		{
			if (mMode == SLIDER_MODE.FILL)
			{
				pos = new(0.0f, value * mOriginForegroundSize.y - mOriginForegroundSize.y * 0.5f);
			}
			else if (mMode == SLIDER_MODE.SIZING)
			{
				pos = new(0.0f, mForeground.getWindowTop());
			}
		}
		return pos;
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
			posInForeground = (Vector3)screenPosToWindow(screenPos, mRoot) - mOriginForegroundPosition;
		}
		// 将本地坐标转换为滑动条的值
		float value = 0.0f;
		if (mDirection == DRAG_DIRECTION.HORIZONTAL)
		{
			value = divide(posInForeground.x + mOriginForegroundSize.x * 0.5f, mOriginForegroundSize.x);
		}
		else if (mDirection == DRAG_DIRECTION.VERTICAL)
		{
			value = divide(posInForeground.y + mOriginForegroundSize.y * 0.5f, mOriginForegroundSize.y);
		}
		return saturate(value);
	}
}