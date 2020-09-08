using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandWindowAlphaPath : Command
{
	public Dictionary<float, float> mValueKeyFrame;
	public KeyFrameCallback mDoingCallBack;
	public KeyFrameCallback mDoneCallBack;
	public float mValueOffset;			// 透明偏移,计算出的值会再加上这个偏移作为最终透明
	public float mAmplitude;
	public float mOffset;
	public float mSpeed;
	public bool mFullOnce;
	public bool mLoop;
	public override void init()
	{
		base.init();
		mValueKeyFrame = null;
		mDoingCallBack = null;
		mDoneCallBack = null;
		mOffset = 0.0f;
		mAmplitude = 1.0f;
		mSpeed = 1.0f;
		mValueOffset = 1.0f;
		mLoop = false;
		mFullOnce = false;
	}
	public override void execute()
	{
		txUIObject obj = mReceiver as txUIObject;
		WindowComponentAlphaPath component = obj.getComponent(out component);
		// 停止其他相关组件
		obj.breakComponent<IComponentModifyAlpha>(component.GetType());
		component.setTremblingCallback(mDoingCallBack);
		component.setTrembleDoneCallback(mDoneCallBack);
		component.setActive(true);
		component.setValueKeyFrame(mValueKeyFrame);
		component.setSpeed(mSpeed);
		component.setValueOffset(mValueOffset);
		component.play(mLoop, mOffset, mFullOnce);
		if (component.getState() == PLAY_STATE.PS_PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mSpeed:" + mSpeed + ", mOffset:" + mOffset + 
			", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}