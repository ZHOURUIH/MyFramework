using UnityEngine;
using System.Collections;

#if USE_NGUI

// 完全使用NGUI的Button功能
public class myNGUIButton : myNGUIObject
{
	protected UIButton	  mButton;
	public static bool mFadeColor = true;
	public override void init()
	{
		base.init();
		mButton = getUnityComponent<UIButton>();
		setFadeColour(mFadeColor);
	}
	// 当按钮需要改变透明度或者附加颜色变化时,需要禁用按钮的颜色渐变
	public void setFadeColour(bool fade)
	{
		if(mButton != null)
		{
			mButton.mFadeColour = fade;
			mButton.mUseState = fade;
		}
	}
	public UIButton getButton() {return mButton;}
	public override void setHandleInput(bool enable)
	{
		base.setHandleInput(enable);
		mButton?.SetState(enable ? UIButtonColor.State.Normal : UIButtonColor.State.Disabled, true);
	}
}

#endif