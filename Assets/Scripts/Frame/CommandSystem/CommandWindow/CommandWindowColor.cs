using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandWindowColor : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public Color mStartColor;
	public Color mTargetColor;
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
		mName = null;
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
		ComponentOwner obj = mReceiver as ComponentOwner;
		WindowComponentColor component = obj.getComponent(out component);
		// 停止其他相关组件
		obj.breakComponent<IComponentModifyColor>(component.GetType());
		obj.breakComponent<IComponentModifyAlpha>(component.GetType());
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStart(mStartColor);
		component.setTarget(mTargetColor);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mStartColor:" + mStartColor +
			", mTargetColor:" + mTargetColor + ", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}
