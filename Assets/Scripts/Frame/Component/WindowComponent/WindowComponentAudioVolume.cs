using System;

public class WindowComponentAudioVolume : ComponentKeyFrameNormal
{
	protected float mStartVolume;
	protected float mTargetVolume;
	public override void resetProperty()
	{
		base.resetProperty();
		mStartVolume = 0.0f;
		mTargetVolume = 0.0f;
	}
	public void setStartVolume(float volume) { mStartVolume = volume; }
	public void setTargetVolume(float volume) { mTargetVolume = volume; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		float newVolume = lerpSimple(mStartVolume, mTargetVolume, value);
		mComponentOwner.getComponent<WindowComponentAudio>().setVolume(newVolume);
	}
}
