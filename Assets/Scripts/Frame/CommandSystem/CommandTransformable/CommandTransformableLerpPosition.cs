using UnityEngine;

public class CommandTransformableLerpPosition : Command
{
	public LerpCallback mLerpingCallBack;
	public LerpCallback mLerpDoneCallBack;
	public Vector3 mTargetPosition;
	public float mLerpSpeed;
	public override void resetProperty()
	{
		base.resetProperty();
		mLerpingCallBack = null;
		mLerpDoneCallBack = null;
		mTargetPosition = Vector3.zero;
		mLerpSpeed = 0.0f;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		obj.getComponent(out TransformableComponentLerpPosition component);
		component.setLerpingCallback(mLerpingCallBack);
		component.setLerpDoneCallback(mLerpDoneCallBack);
		component.setActive(true);
		component.setTargetPosition(mTargetPosition);
		component.setLerpSpeed(mLerpSpeed);
		component.play();
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mLerpSpeed:", mLerpSpeed).
				Append(", mTargetPosition:", mTargetPosition);
	}
}