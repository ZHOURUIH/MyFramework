using System;

public class CameraComponentFOV : ComponentKeyFrameNormal
{
    protected float mStartFOV;
    protected float mTargetFOV;
    public void setStartFOV(float fov) { mStartFOV = fov; }
	public void setTargetFOV(float fov) { mTargetFOV = fov; }
	//-------------------------------------------------------------------------------------------------------------
    protected override void applyTrembling(float value)
    {
        GameCamera obj = mComponentOwner as GameCamera;
        obj.setFOVY(lerpSimple(mStartFOV, mTargetFOV, value));
    }
}