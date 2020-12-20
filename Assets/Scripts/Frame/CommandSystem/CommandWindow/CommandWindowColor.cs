using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandWindowColor : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public Color mStartColor;
	public Color mTargetColor;
	public KEY_FRAME mKeyframe;
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
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mStartColor = Color.white;
		mTargetColor = Color.white;
		mAmplitude = 1.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		myUIObject obj = mReceiver as myUIObject;
		WindowComponentColor component = obj.getComponent(out component);
		// 停止其他相关组件
		obj.breakComponent<IComponentModifyColor>(Typeof(component));
		obj.breakComponent<IComponentModifyAlpha>(Typeof(component));
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStart(mStartColor);
		component.setTarget(mTargetColor);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartColor:" + mStartColor +
			", mTargetColor:" + mTargetColor + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}
