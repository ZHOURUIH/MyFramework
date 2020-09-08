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
		Transformable obj = mReceiver as Transformable;
		TransformableComponentLerpPosition component = obj.getComponent(out component);
		// 停止其他移动组件
		obj.breakComponent<IComponentModifyPosition>(component.GetType());
		component.setLerpingCallback(mLerpingCallBack);
		component.setLerpDoneCallback(mLerpDoneCallBack);
		component.setActive(true);
		component.setTargetPosition(mTargetPosition);
		component.setLerpSpeed(mLerpSpeed);
		component.play();
		if (component.getState() == PLAY_STATE.PS_PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLerpSpeed:" + mLerpSpeed + ", mTargetPosition:" + mTargetPosition;
	}
}