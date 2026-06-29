using static UnityUtility;
using static MathUtility;

// 渐变UI亮度的组件
public class COMWindowLum : ComponentKeyFrame
{
	protected float mStart;		// 起始亮度
	protected float mTarget;	// 目标亮度
	public void setStart(float lum) { mStart = lum; }
	public void setTarget(float lum) { mTarget = lum; }
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		if (mComponentOwner is not IShaderWindow shaderWindow)
		{
			logError("window is not a IShaderWindow! can not offset hsl!");
			return;
		}
		if (shaderWindow.getWindowShader() is not WindowShaderLumOffset lumOffset)
		{
			logError("window has no WindowShaderLumOffset!");
			return;
		}
		lumOffset.setLumOffset(lerpSimple(mStart, mTarget, value));
	}
}