using System;

public class COMCameraFOV : ComponentKeyFrameNormal
{
	protected float mTargetFOV;
	protected float mStartFOV;
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