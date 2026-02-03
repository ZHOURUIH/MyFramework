using System;
using UnityEngine;

[CommonControl]
public class LegendButton : WindowObjectUGUI
{
	protected myUGUITextTMP mText;
	protected Vector3 mOriginTextPosition;
	protected Color mOriginTextColor;
	protected static int mDefaultClickSound;
	public LegendButton(IWindowObjectOwner parent) : base(parent) { }
	protected override void assignWindowInternal()
	{
		newObject(out mText, "Text", false);
	}
	public override void init()
	{
		base.init();
		if (mRoot == null)
		{
			return;
		}
		mRoot.setHoverDetailCallback(onButtonHover);
		mRoot.setPressDetailCallback(onButtonPress);
		mRoot.setOnScreenTouchUp(onScreenTouchUp);
		if (mText != null)
		{
			mOriginTextPosition = mText.getPosition();
			mOriginTextColor = mText.getColor();
		}
	}
	public override void reset()
	{
		base.reset();
		mText?.setPosition(mOriginTextPosition);
		mText?.setColor(mOriginTextColor);
	}
	public static void setDefaultClickSound(int sound) { mDefaultClickSound = sound; }
	public void registeCollider(Action clickCallback, int clickSound = 0)
	{
		mRoot?.registeCollider(clickCallback, clickSound != 0 ? clickSound : mDefaultClickSound);
	}
	public void registeCollider(Vector3Callback clickCallback, int clickSound = 0)
	{
		mRoot?.registeCollider(clickCallback, clickSound != 0 ? clickSound : mDefaultClickSound);
	}
	public void unregisteCollider()
	{
		mRoot?.unregisteCollider();
	}
	public void setText(string str) { mText?.setText(str); }
	public void setText(int value) { mText?.setText(value); }
	public myUGUITextTMP getTextObject() { return mText; }
	public void setHandleInput(bool handle) { mRoot?.setHandleInput(handle); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onButtonPress(Vector3 touchPos, bool press)
	{
		// 只执行按下时的缩小,抬起的恢复由全局鼠标检测执行
		if (press && mText != null)
		{
			mText.setPosition(mOriginTextPosition + new Vector3(0.0f, -2.0f, 0.0f));
			mText.setColor(mOriginTextColor);
		}
	}
	protected void onButtonHover(Vector3 touchPos, bool hover)
	{
		mText?.setColor(hover ? Color.white : mOriginTextColor);
	}
	protected void onScreenTouchUp(Vector3 touchPos, int touchID)
	{
		if (mText != null)
		{
			mText.setPosition(mOriginTextPosition);
			mText.setColor(mOriginTextColor);
		}
	}
}