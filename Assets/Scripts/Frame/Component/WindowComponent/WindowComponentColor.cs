using UnityEngine;
using System;

public class WindowComponentColor : ComponentKeyFrameNormal, IComponentModifyAlpha, IComponentModifyColor
{
	protected Color mStart;
	protected Color mTarget;
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = Color.black;
		mTarget = Color.black;
	}
	public void setStart(Color color) {mStart = color; }
	public void setTarget(Color color) {mTarget = color; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var obj = mComponentOwner as myUIObject;
		obj.setColor(lerpSimple(mStart, mTarget, value));
	}
}
