using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandWindowLum : Command
{
	public string mName;
	public float mOnceLength;
	public float mOffset;
	public float mStartLum;
	public float mTargetLum;
	public bool mLoop;
	public float mAmplitude;
	public bool mFullOnce;
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public override void init()
	{
		base.init();
		mName = EMPTY_STRING;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mStartLum = 0.0f;
		mTargetLum = 0.0f;
		mLoop = false;
		mAmplitude = 1.0f;
		mFullOnce = true;
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		WindowComponentLum component = obj.getComponent(out component);
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStartLum(mStartLum);
		component.setTargetLum(mTargetLum);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartLum:" + mStartLum +
			", mTargetLum:" + mTargetLum + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}
