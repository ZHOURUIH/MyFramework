using UnityEngine;
using static MathUtility;

// 变化UI颜色的组件
public class COMWindowColor : ComponentKeyFrame, IComponentModifyAlpha, IComponentModifyColor
{
	protected Color mStart;		// 起始颜色值
	protected Color mTarget;	// 目标颜色值
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = Color.black;
		mTarget = Color.black;
	}
	public void setStart(Color color) { mStart = color; }
	public void setTarget(Color color) { mTarget = color; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var obj = mComponentOwner as myUIObject;
		obj.setColor(lerpSimple(mStart, mTarget, value));
	}
}