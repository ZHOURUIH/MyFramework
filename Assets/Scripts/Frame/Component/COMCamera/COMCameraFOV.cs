using System;
using static MathUtility;

// 渐变摄像机FOV的组件
public class COMCameraFOV : ComponentKeyFrameNormal
{
	protected float mTargetFOV;		// 目标FOV
	protected float mStartFOV;		// 起始FOV
	public override void resetProperty()
	{
		base.resetProperty();
		mTargetFOV = 0.0f;
		mStartFOV = 0.0f;
	}
	public void setStartFOV(float fov) { mStartFOV = fov; }
	public void setTargetFOV(float fov) { mTargetFOV = fov; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var obj = mComponentOwner as GameCamera;
		obj.setFOVY(lerpSimple(mStartFOV, mTargetFOV, value));
	}
}