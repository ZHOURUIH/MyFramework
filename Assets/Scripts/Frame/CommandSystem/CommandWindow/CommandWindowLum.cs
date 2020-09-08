using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandWindowLum : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public string mName;
	public float mOnceLength;
	public float mOffset;
	public float mStartLum;
	public float mTargetLum;
	public float mAmplitude;
	public bool mFullOnce;
	public bool mLoop;
	public override void init()
	{
		base.init();
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mName = null;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mStartLum = 0.0f;
		mTargetLum = 0.0f;
		mAmplitude = 1.0f;
		mFullOnce = true;
		mLoop = false;
	}
	public override void execute()
	{
		txUIObject obj = mReceiver as txUIObject;
		WindowComponentLum component = obj.getComponent(out component);
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStartLum(mStartLum);
		component.setTargetLum(mTargetLum);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
		if (component.getState() == PLAY_STATE.PS_PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartLum:" + mStartLum +
			", mTargetLum:" + mTargetLum + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}
