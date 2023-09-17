using System;
using UnityEngine;
using static UnityUtility;
using static MathUtility;

// 用于渐变窗口HSL空间颜色的组件
public class COMWindowHSL : ComponentKeyFrameNormal
{
	protected Vector3 mStart;		// 起始偏移值
	protected Vector3 mTarget;		// 目标偏移值
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