using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WindowComponentLum : ComponentKeyFrameNormal
{
	protected float mStartLum;
	protected float mTargetLum;
	public void setStartLum(float lum) { mStartLum = lum; }
	public void setTargetLum(float lum) { mTargetLum = lum; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float offset)
	{
		if (!(mComponentOwner is IShaderWindow))
		{
			logError("window is not a IShaderWindow! can not offset hsl!");
			return;
		}
		var lumOffset = (mComponentOwner as IShaderWindow).getWindowShader<WindowShaderLumOffset>();
		if(lumOffset == null)
		{
			logError("window has no WindowShaderLumOffset!");
			return;
		}
		lumOffset.setLumOffset(lerpSimple(mStartLum, mTargetLum, offset));
	}
}