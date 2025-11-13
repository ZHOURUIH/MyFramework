using UnityEngine;
using static MathUtility;

// 使物体始终朝向目标的组件
public class COMTransformableRotateFocus : GameComponent, IComponentModifyRotation, IComponentBreakable
{
	protected ITransformable mFocusTarget;	// 朝向的目标
	protected Vector3 mFocusOffset;			// 目标的位置偏移
	public override void resetProperty()
	{
		base.resetProperty();
		mFocusTarget = null;
		mFocusOffset = Vector3.zero;
	}
	public void setFocusTarget(ITransformable obj)
	{
		mFocusTarget = obj;
		if(mFocusTarget == null)
		{
			setActive(false);
		}
	}
	public void setFocusOffset(Vector3 offset) { mFocusOffset = offset; }
	public override void update(float elapsedTime)
	{
		var obj = mComponentOwner as ITransformable;
		Vector3 dir = mFocusTarget.localToWorldDirection(mFocusOffset) + mFocusTarget.getWorldPosition() - obj.getWorldPosition();
		obj.setWorldRotation(getLookAtQuaternion(dir));
		base.update(elapsedTime);
	}
	public void notifyBreak() { }
}