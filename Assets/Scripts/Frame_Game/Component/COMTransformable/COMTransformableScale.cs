using UnityEngine;
using static MathUtility;

// 缩放物体的组件
public class COMTransformableScale : ComponentKeyFrame, IComponentModifyScale
{
	protected Vector3 mStart;		// 起始缩放值
	protected Vector3 mTarget;		// 目标缩放值
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = Vector3.zero;
		mTarget = Vector3.zero;
	}
	public void setStart(Vector3 start) { mStart = start; }
	public void setTarget(Vector3 target) { mTarget = target; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		(mComponentOwner as Transformable).setScale(lerpSimple(mStart, mTarget, value));
	}
}