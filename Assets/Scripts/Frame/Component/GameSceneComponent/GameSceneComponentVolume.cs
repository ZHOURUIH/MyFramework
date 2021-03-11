using System;

public class GameSceneComponentVolume : ComponentKeyFrameNormal
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
		var gameScene = mComponentOwner as GameScene;
		float newVolume = lerpSimple(mStartVolume, mTargetVolume, value);
		gameScene.getComponent<GameSceneComponentAudio>().setVolume(newVolume);
	}
}