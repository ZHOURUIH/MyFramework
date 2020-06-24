using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandWindowAlpha : Command
{
	public string mName;
	public float mOnceLength;
	public float mOffset;
	public float mStartAlpha;
	public float mTargetAlpha;
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
		mStartAlpha = 1.0f;
		mTargetAlpha = 1.0f;
		mLoop = false;
		mAmplitude = 1.0f;
		mFullOnce = false;
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		WindowComponentAlpha component = obj.getComponent(out component);
		// 停止其他相关组件
		obj.breakComponent<IComponentModifyAlpha>(component.GetType());
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStartAlpha(mStartAlpha);
		component.setTargetAlpha(mTargetAlpha);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartAlpha:" + mStartAlpha +
			", mTargetAlpha:" + mTargetAlpha + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}
