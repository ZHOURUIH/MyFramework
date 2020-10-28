using UnityEngine;
using System;
using System.Collections;

public class WindowComponentColor : ComponentKeyFrameNormal, IComponentModifyAlpha, IComponentModifyColor
{
	protected Color mStart;
	protected Color mTarget;
	public void setStart(Color color) {mStart = color; }
	public void setTarget(Color color) {mTarget = color; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		myUIObject obj = mComponentOwner as myUIObject;
		obj.setColor(lerpSimple(mStart, mTarget, value));
	}
}
