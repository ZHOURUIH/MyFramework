using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 使物体旋转
public class CmdTransformableRotateSpeed : Command
{
	public Vector3 mRotateAcceleration;		// 旋转加速度
	public Vector3 mRotateSpeed;			// 旋转起始速度
	public Vector3 mStartAngle;             // 旋转起始角度
	public bool mUpdateInFixedTick;			// 是否在物理更新中执行
	public override void resetProperty()
	{
		base.resetProperty();
		mRotateAcceleration = Vector3.zero;
		mRotateSpeed = Vector3.zero;
		mStartAngle = Vector3.zero;
		mUpdateInFixedTick = false;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		if (isEditor() && 
			obj is myUIObject uiObj &&
			(!isVectorZero(mRotateSpeed) || !isVectorZero(mRotateAcceleration)) && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableRotateSpeed com);
		com.setUpdateInFixedTick(mUpdateInFixedTick);
		com.setActive(true);
		com.startRotateSpeed(mStartAngle, mRotateSpeed, mRotateAcceleration);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setNeedUpdate(true);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mStartAngle:", mStartAngle).
				append(", mRotateSpeed:", mRotateSpeed).
				append(", mRotateAcceleration:", mRotateAcceleration);
	}
}