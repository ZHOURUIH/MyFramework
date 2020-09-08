using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandWindowAlpha : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public string mName;
	public float mOnceLength;
	public float mOffset;
	public float mStartAlpha;
	public float mTargetAlpha;
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
		mStartAlpha = 1.0f;
		mTargetAlpha = 1.0f;
		mAmplitude = 1.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		txUIObject obj = mReceiver as txUIObject;
		WindowComponentAlpha component = obj.getComponent(out component);
		// 停止其他相关组件
		obj.breakComponent<IComponentModifyAlpha>(component.GetType());
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStartAlpha(mStartAlpha);
		component.setTargetAlpha(mTargetAlpha);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
		if (component.getState() == PLAY_STATE.PS_PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartAlpha:" + mStartAlpha +
			", mTargetAlpha:" + mTargetAlpha + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}
