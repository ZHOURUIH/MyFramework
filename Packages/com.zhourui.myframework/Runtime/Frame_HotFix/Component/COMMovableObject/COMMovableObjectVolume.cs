using static MathUtility;

// 物体的音量组件
public class COMMovableObjectVolume : ComponentKeyFrame
{
	protected float mStart;		// 起始音量
	protected float mTarget;	// 目标音量
	public override void resetProperty()
	{
		base.resetProperty();
		mStart = 0.0f;
		mTarget = 0.0f;
	}
	public void setStart(float volume) { mStart = volume; }
	public void setTarget(float volume) { mTarget = volume; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		mComponentOwner.getOrAddComponent<COMMovableObjectAudio>().setVolume(lerpSimple(mStart, mTarget, value));
	}
}