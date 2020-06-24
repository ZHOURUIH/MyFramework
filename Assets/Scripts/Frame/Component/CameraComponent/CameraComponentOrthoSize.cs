using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
        float curOrthoSize = lerpSimple(mStartOrthoSize, mTargetOrthoSize, value);
        obj.setOrthoSize(curOrthoSize);
    }
}