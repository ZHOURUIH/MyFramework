using static UnityUtility;
using static MathUtility;

// 渐变UI滑动条值的组件
public class COMWindowSlider : ComponentKeyFrame
{
	protected float mStart;		// 起始值
	protected float mTarget;	// 目标值
	public void setTarget(float value) { mTarget = value; }
	public void setStart(float value) { mStart = value; }
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		if (mComponentOwner is not ISlider slider)
		{
			logError("window is not a Slider Window!");
			return;
		}
		slider.setValue(lerpSimple(mStart, mTarget, value));
	}
}