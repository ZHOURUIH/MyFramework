using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandWindowHSL : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public Vector3 mStartHSL;
	public Vector3 mTargetHSL;
	public string mName;
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
		mStartHSL = Vector3.zero;
		mTargetHSL = Vector3.zero;
		mName = null;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mAmplitude = 1.0f;
		mFullOnce = true;
		mLoop = false;
	}
	public override void execute()
	{
		txUIObject obj = mReceiver as txUIObject;
		WindowComponentHSL component = obj.getComponent(out component);
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStartHSL(mStartHSL);
		component.setTargetHSL(mTargetHSL);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
		if (component.getState() == PLAY_STATE.PS_PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartHSL:" + mStartHSL
			+ ", mTargetHSL:" + mTargetHSL + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}
