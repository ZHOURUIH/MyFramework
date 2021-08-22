using System;

public class COMGameSceneVolume : ComponentKeyFrameNormal
{
	protected float mStart;
	protected float mTarget;
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
		var gameScene = mComponentOwner as GameScene;
		gameScene.getComponent<COMGameSceneAudio>().setVolume(lerpSimple(mStart, mTarget, value));
	}
}