using System;

public class COMCameraOrthoSize : ComponentKeyFrameNormal
{
	protected float mStart;
	protected float mTarget;
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
	}
	public void setStart(float size) { mStart = size; }
	public void setTarget(float size) { mTarget = size; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var obj = mComponentOwner as GameCamera;
		obj.setOrthoSize(lerpSimple(mStart, mTarget, value));
	}
}