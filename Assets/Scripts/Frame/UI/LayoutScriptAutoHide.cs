using System;
using static MathUtility;

// 可自动隐藏的布局,用得比较少了
public abstract class LayoutScriptAutoHide : LayoutScript
{
	protected MyTimer mTimer;		// 自动隐藏的计时器
	protected bool mShowDone;		// 是否显示完毕,需要子类主动通知
	protected bool mHideDone;		// 是否隐藏完毕,需要子类主动通知
	public LayoutScriptAutoHide()
	{
		mTimer = new MyTimer();
		mHideDone = true;
	}
	public override void init()
	{
		base.init();
		mTimer.init(-1.0f, 3.0f, false);
	}
	public override void onReset()
	{
		base.onReset();
		mTimer.stop(false);
		mShowDone = false;
		mHideDone = true;
	}
	public override void onShow(bool immediately, string param)
	{
		startShowOrHide();
		if (immediately)
		{
			showDone();
		}
	}
	public override void onHide(bool immediately, string param)
	{
		startShowOrHide();
		if (immediately)
		{
			hideDone();
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mTimer.tickTimer(elapsedTime))
		{
			LT.HIDE_LAYOUT(mID);
		}
	}
	public void setAutoHide(bool autoHide)
	{
		if(autoHide)
		{
			mTimer.start();
		}
		else
		{
			mTimer.stop(false);
		}
	}
	public bool isShowDone(){return mShowDone;}
	public bool isHideDone(){return mHideDone;}
	public void resetHideTime()
	{
		// 只有当需要自动隐藏时,点击屏幕才会重置时间
		clampMax(ref mTimer.mCurTime, 0.0f);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void startShowOrHide()
	{
		mHideDone = false;
		mShowDone = false;
	}
	protected void showDone()
	{
		mHideDone = false;
		mShowDone = true;
		mTimer.start();
	}
	protected void hideDone()
	{
		mHideDone = true;
		mShowDone = false;
		LT.HIDE_LAYOUT_FORCE(mID);
	}
}