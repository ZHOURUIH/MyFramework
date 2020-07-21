using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowComponentHSL : ComponentKeyFrameNormal
{
	protected Vector3 mStartHSL;
	protected Vector3 mTargetHSL;
	public void setStartHSL(Vector3 hsl) { mStartHSL = hsl; }
	public void setTargetHSL(Vector3 hsl) { mTargetHSL = hsl; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		if(!(mComponentOwner is IShaderWindow))
		{
			logError("window is not a IShaderWindow! can not offset hsl!");
			return;
		}
		var hslOffset = (mComponentOwner as IShaderWindow).getWindowShader<WindowShaderHSLOffset>();
		if(hslOffset == null)
		{
			logError("window has no hsl offset shader! can not offset hsl!");
			return;
		}
		hslOffset.setHSLOffset(lerpSimple(mStartHSL, mTargetHSL, value));
	}
}