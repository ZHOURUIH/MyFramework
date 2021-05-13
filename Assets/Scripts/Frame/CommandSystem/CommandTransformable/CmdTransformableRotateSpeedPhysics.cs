using UnityEngine;

public class CmdTransformableRotateSpeedPhysics : Command
{
	public Vector3 mRotateAcceleration;
	public Vector3 mRotateSpeed;
	public Vector3 mStartAngle;
	public override void resetProperty()
	{
		base.resetProperty();
		mStartAngle = Vector3.zero;
		mRotateSpeed = Vector3.zero;
		mRotateAcceleration = Vector3.zero;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
#if UNITY_EDITOR
		if (obj is myUIObject)
		{
			var uiObj = obj as myUIObject;
			if ((!isVectorZero(mRotateSpeed) || !isVectorZero(mRotateAcceleration)) && !uiObj.getLayout().canUIObjectUpdate(uiObj))
			{
				logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
			}
		}
#endif
		obj.getComponent(out COMTransformableRotateSpeedPhysics com);
		com.setActive(true);
		com.startRotateSpeed(mStartAngle, mRotateSpeed, mRotateAcceleration);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mStartAngle:", mStartAngle).
				Append(", mRotateSpeed:", mRotateSpeed).
				Append(", mRotateAcceleration:", mRotateAcceleration);
	}
}