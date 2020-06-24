using UnityEngine;
using System.Collections;

public class CommandTransformableLerpPosition : Command
{
	public LerpCallback mLerpingCallBack;
	public LerpCallback mLerpDoneCallBack;
	public Vector3 mTargetPosition;
	public float mLerpSpeed;
	public override void init()
	{
		base.init();
		mLerpingCallBack = null;
		mLerpDoneCallBack = null;
		mTargetPosition = Vector3.zero;
		mLerpSpeed = 0.0f;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		TransformableComponentLerpPosition component = obj.getComponent(out component);
		// 停止其他移动组件
		obj.breakComponent<IComponentModifyPosition>(component.GetType());
		component.setLerpingCallback(mLerpingCallBack);
		component.setLerpDoneCallback(mLerpDoneCallBack);
		component.setActive(true);
		component.setTargetPosition(mTargetPosition);
		component.setLerpSpeed(mLerpSpeed);
		component.play();
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLerpSpeed:" + mLerpSpeed + ", mTargetPosition:" + mTargetPosition;
	}
}