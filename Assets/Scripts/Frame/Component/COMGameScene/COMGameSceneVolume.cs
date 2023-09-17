using System;
using static MathUtility;

// 场景音量组件,用于实现音量的变化
public class COMGameSceneVolume : ComponentKeyFrameNormal
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
		var gameScene = mComponentOwner as GameScene;
		gameScene.getComponent<COMGameSceneAudio>().setVolume(lerpSimple(mStart, mTarget, value));
	}
}