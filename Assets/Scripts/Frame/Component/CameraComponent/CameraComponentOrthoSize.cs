using System;

public class CameraComponentOrthoSize : ComponentKeyFrameNormal
{
    protected float mStartOrthoSize;
    protected float mTargetOrthoSize;
    public void setStartOrthoSize(float size) { mStartOrthoSize = size; }
	public void setTargetOrthoSize(float size) { mTargetOrthoSize = size; }
	//-------------------------------------------------------------------------------------------------------------
    protected override void applyTrembling(float value)
    {
        GameCamera obj = mComponentOwner as GameCamera;
        obj.setOrthoSize(lerpSimple(mStartOrthoSize, mTargetOrthoSize, value));
    }
}