using UnityEngine;
using System;

// 物体的音量组件
public class COMMovableObjectVolume : ComponentKeyFrameNormal
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
		float newVolume = lerpSimple(mStart, mTarget, value);
		mComponentOwner.getComponent<COMMovableObjectAudio>().setVolume(newVolume);
	}
}