using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandTimeManagerScaleTime : Command
{
	public KeyFrameCallback mDoingCallBack;
	public KeyFrameCallback mDoneCallBack;
	public string mName;
	public float mStartScale;
	public float mTargetScale;
	public float mOnceLength;
	public float mAmplitude;
	public float mOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void init()
	{
		base.init();
		mDoingCallBack = null;
		mDoneCallBack = null;
		mName = EMPTY_STRING;
		mStartScale = 1.0f;
		mTargetScale = 1.0f;
		mOnceLength = 1.0f;
		mAmplitude = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		TimeComponentScale component = mTimeManager.getComponent(out component);
		component.setTremblingCallback(mDoingCallBack);
		component.setTrembleDoneCallback(mDoneCallBack);
		component.setActive(true);
		component.setStartScale(mStartScale);
		component.setTargetScale(mTargetScale);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartScale:" + mStartScale +
			", mTargetScale:" + mTargetScale + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}
