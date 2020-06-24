using UnityEngine;
using System.Collections;

public class CommandTransformableLerpRotation : Command
{
	public LerpCallback mLerpingCallBack;
	public LerpCallback mLerpDoneCallBack;
	public Vector3 mTargetRotation;
	public float mLerpSpeed;
	public override void init()
	{
		base.init();
		mLerpingCallBack = null;
		mLerpDoneCallBack = null;
		mTargetRotation = Vector3.zero;
		mLerpSpeed = 0.0f;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		TransformableComponentLerpRotation component = obj.getComponent(out component);
		// 停止其他旋转组件
		obj.breakComponent<IComponentModifyRotation>(component.GetType());
		component.setLerpingCallback(mLerpingCallBack);
		component.setLerpDoneCallback(mLerpDoneCallBack);
		component.setActive(true);
		component.setTargetRotation(mTargetRotation);
		component.setLerpSpeed(mLerpSpeed);
		component.play();
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLerpSpeed:" + mLerpSpeed + ", mTargetRotation:" + mTargetRotation;
	}
}