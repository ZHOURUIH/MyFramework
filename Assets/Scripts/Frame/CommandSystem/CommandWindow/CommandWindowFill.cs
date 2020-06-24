using System;
using System.Collections.Generic;

public class CommandWindowFill : Command
{
	public string mTremblingName;
	public float mStartValue;
	public float mTargetValue;
	public float mOnceLength;
	public float mOffset;
	public bool mLoop;
	public bool mFullOnce;
	public float mAmplitude;
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public override void init()
	{
		base.init();
		mTremblingName = EMPTY_STRING;
		mStartValue = 0.0f;
		mTargetValue = 0.0f;
		mOnceLength = 0.0f;
		mOffset = 0.0f;
		mLoop = false;
		mFullOnce = false;
		mAmplitude = 1.0f;
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		WindowComponentFill component = obj.getComponent(out component);
		component.setActive(true);
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setStartValue(mStartValue);
		component.setTargetValue(mTargetValue);
		component.play(mTremblingName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo();
	}
}