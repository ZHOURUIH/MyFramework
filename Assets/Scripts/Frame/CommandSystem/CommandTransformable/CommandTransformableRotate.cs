using UnityEngine;
using System.Collections;

public class CommandTransformableRotate : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public Vector3 mStartRotation;
	public Vector3 mTargetRotation;
	public string mName;
	public float mOnceLength;
	public float mAmplitude;
	public float mOffset;
	public bool mRandomOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void init()
	{
		base.init();
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mStartRotation = Vector3.zero;
		mTargetRotation = Vector3.zero;
		mName = EMPTY_STRING;
		mOnceLength = 1.0f;
		mAmplitude = 1.0f;
		mOffset = 0.0f;
		mRandomOffset = false;
		mFullOnce = true;
		mLoop = false;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		TransformableComponentRotate component = obj.getComponent(out component);
		// 停止其他旋转组件
		obj.breakComponent<IComponentModifyRotation>(component.GetType());
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		if (mRandomOffset)
		{
			mOffset = randomFloat(0.0f, mOnceLength);
		}
		component.setTargetRotation(mTargetRotation);
		component.setStartRotation(mStartRotation);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mLoop:" + mLoop +
			", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce + ", mRandomOffset:" + mRandomOffset + ", mStartRotation:" + mStartRotation
			+ ", mTargetRotation:" + mTargetRotation;
	}
}