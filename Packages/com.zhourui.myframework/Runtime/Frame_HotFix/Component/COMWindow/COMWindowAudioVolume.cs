using static MathUtility;

// UI音量组件,用于控制UI的音量,已弃用
public class COMWindowAudioVolume : ComponentKeyFrame
{
	protected float mStart;		// 起始音量值
	protected float mTarget;	// 目标音量值
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
		mComponentOwner.getOrAddComponent<COMWindowAudio>().setVolume(lerpSimple(mStart, mTarget, value));
	}
}