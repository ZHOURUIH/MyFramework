using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
        float curFOV = lerpSimple(mStartFOV, mTargetFOV, value);
        obj.setFOVY(curFOV);
    }
}