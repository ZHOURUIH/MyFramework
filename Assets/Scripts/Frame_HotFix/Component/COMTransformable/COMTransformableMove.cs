using UnityEngine;
using static MathUtility;

// 物体的平移组件,用于实现物体的平移功能
public class COMTransformableMove : ComponentKeyFrame, IComponentModifyPosition
{
	protected Vector3 mStart;   // 移动开始时的位置
	protected Vector3 mTarget;	// 移动的目标位置
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
		(mComponentOwner as ITransformable).setPosition(lerpSimple(mStart, mTarget, value));
	}
}