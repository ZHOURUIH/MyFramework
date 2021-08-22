using System;
using UnityEngine;

public class COMWindowHSL : ComponentKeyFrameNormal
{
	protected Vector3 mStart;
	protected Vector3 mTarget;
	public void setStart(Vector3 hsl) { mStart = hsl; }
	public void setTarget(Vector3 hsl) { mTarget = hsl; }
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = Vector3.zero;
		mTarget = Vector3.zero;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		if(!(mComponentOwner is IShaderWindow))
		{
			logError("window is not a IShaderWindow! can not offset hsl!");
			return;
		}
		var hslOffset = (mComponentOwner as IShaderWindow).getWindowShader() as WindowShaderHSLOffset;
		if(hslOffset == null)
		{
			logError("window has no hsl offset shader! can not offset hsl!");
			return;
		}
		hslOffset.setHSLOffset(lerpSimple(mStart, mTarget, value));
	}
}