using System;

public class CameraComponentOrthoSize : ComponentKeyFrameNormal
{
    protected float mStartOrthoSize;
    protected float mTargetOrthoSize;
	public override void resetProperty()
	{
		base.resetProperty();
		mStartOrthoSize = 0.0f;
		mTargetOrthoSize = 0.0f;
	}
	public void setStartOrthoSize(float size) { mStartOrthoSize = size; }
	public void setTargetOrthoSize(float size) { mTargetOrthoSize = size; }
	//-------------------------------------------------------------------------------------------------------------
    protected override void applyTrembling(float value)
    {
		var obj = mComponentOwner as GameCamera;
        obj.setOrthoSize(lerpSimple(mStartOrthoSize, mTargetOrthoSize, value));
    }
}