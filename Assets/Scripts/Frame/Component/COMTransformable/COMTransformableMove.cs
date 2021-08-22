using UnityEngine;
using System;

public class COMTransformableMove : ComponentKeyFrameNormal, IComponentModifyPosition
{
	protected Vector3 mStart;   // 移动开始时的位置
	protected Vector3 mTarget;
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = Vector3.zero;
		mTarget = Vector3.zero;
	}
	public void setTarget(Vector3 pos) { mTarget = pos; }
	public void setStart(Vector3 pos) { mStart = pos; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		(mComponentOwner as Transformable).setPosition(lerpSimple(mStart, mTarget, value));
	}
}