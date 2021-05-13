using System;

public class COMWindowLum : ComponentKeyFrameNormal
{
	protected float mStart;
	protected float mTarget;
	public void setStart(float lum) { mStart = lum; }
	public void setTarget(float lum) { mTarget = lum; }
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
	}
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		if (!(mComponentOwner is IShaderWindow))
		{
			logError("window is not a IShaderWindow! can not offset hsl!");
			return;
		}
		var lumOffset = (mComponentOwner as IShaderWindow).getWindowShader() as WindowShaderLumOffset;
		if(lumOffset == null)
		{
			logError("window has no WindowShaderLumOffset!");
			return;
		}
		lumOffset.setLumOffset(lerpSimple(mStart, mTarget, value));
	}
}