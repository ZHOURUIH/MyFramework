using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;

// 自定义的进度条,跟滑动条的区别就是不能拖拽,实现更加简单,适用于加载进度条等等的功能
public class UGUIProgress : WindowObjectUGUI, ISlider, ICommonUI
{
	protected myUGUIImageSimple mProgressBar;       // 进度条中显示进度的窗口
	protected myUGUIObject mThumb;					// 显示当前进度的点
	protected Vector3 mOriginProgressPosition;		// 进度窗口初始的位置
	protected Vector2 mOriginProgressSize;			// 进度窗口初始的大小
	protected float mProgressValue;					// 当前的进度值
	protected SLIDER_MODE mMode;                    // 进度条显示的实现方式
	public UGUIProgress(IWindowObjectOwner parent) : base(parent)
	{
		mMode = SLIDER_MODE.FILL;
	}
	protected override void assignWindowInternal()
	{
		base.assignWindowInternal();
		newObject(out mProgressBar, "ProgressBar");
		newObject(out mThumb, "Thumb", false);
	}
	public override void init()
	{
		base.init();
		if (mProgressBar.getImage().type == Image.Type.Filled)
		{
			mMode = SLIDER_MODE.FILL;
		}
		else
		{
			mMode = SLIDER_MODE.SIZING;
		}
		mOriginProgressSize = mProgressBar.getWindowSize();
		mOriginProgressPosition = mProgressBar.getPosition();
	}
	public void setValue(float value) 
	{
		if (isVectorZero(mOriginProgressSize))
		{
			logError("ProgressBar的size为0,是否忘记调用了UGUIProgress的initProgress?");
			return;
		}
		mProgressValue = value;
		saturate(ref mProgressValue);
		if (mMode == SLIDER_MODE.FILL)
		{
			mProgressBar.setFillPercent(mProgressValue);
		}
		else if (mMode == SLIDER_MODE.SIZING)
		{
			float newWidth = mProgressValue * mOriginProgressSize.x;
			mProgressBar.setPositionX(mOriginProgressPosition.x - mOriginProgressSize.x * 0.5f + newWidth * 0.5f);
			mProgressBar.setWindowSize(new(newWidth, mOriginProgressSize.y));
		}
		mThumb?.setPositionX((mProgressValue - 0.5f) * mOriginProgressSize.x);
	}
	public float getValue() { return mProgressValue; }
	public void setSliderMode(SLIDER_MODE mode) { mMode = mode; }
	public SLIDER_MODE getSliderMode() { return mMode; }
	public void showForeground(bool show) { mProgressBar.getImage().enabled = show; }
	public Vector2 getOriginProgressSize() { return mOriginProgressSize; }
	public Vector3 getOriginProgressPosition() { return mOriginProgressPosition; }
}