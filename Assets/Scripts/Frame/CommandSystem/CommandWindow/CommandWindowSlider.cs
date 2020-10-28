using System;
using System.Collections.Generic;

public class CommandWindowSlider : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public string mTremblingName;
	public float mStartValue;
	public float mTargetValue;
	public float mOnceLength;
	public float mOffset;
	public float mAmplitude;
	public bool mFullOnce;
	public bool mLoop;
	public override void init()
	{
		base.init();
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mTremblingName = null;
		mStartValue = 0.0f;
		mTargetValue = 0.0f;
		mOnceLength = 0.0f;
		mOffset = 0.0f;
		mAmplitude = 1.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		myUIObject obj = mReceiver as myUIObject;
		WindowComponentSlider component = obj.getComponent(out component);
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStartValue(mStartValue);
		component.setTargetValue(mTargetValue);
		component.play(mTremblingName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
}