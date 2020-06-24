using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandTransformableScalePath : Command
{
	public Dictionary<float, Vector3> mValueKeyFrame;
	public KeyFrameCallback mDoingCallBack;
	public KeyFrameCallback mDoneCallBack;
	public Vector3 mValueOffset;			// 缩放偏移,计算出的位置会再乘上这个偏移作为最终世界缩放
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
		mValueOffset = Vector3.zero;
		mAmplitude = 1.0f;
		mOffset = 0.0f;
		mSpeed = 1.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		TransformableComponentScalePath component = obj.getComponent(out component);
		// 停止其他移动组件
		obj.breakComponent<IComponentModifyScale>(component.GetType());
		component.setTremblingCallback(mDoingCallBack);
		component.setTrembleDoneCallback(mDoneCallBack);
		component.setActive(true);
		component.setValueKeyFrame(mValueKeyFrame);
		component.setSpeed(mSpeed);
		component.setValueOffset(mValueOffset);
		component.setOffsetBlendAdd(false);
		component.play(mLoop, mOffset, mFullOnce);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mSpeed:" + mSpeed + ", mOffset:" + mOffset + 
			", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}