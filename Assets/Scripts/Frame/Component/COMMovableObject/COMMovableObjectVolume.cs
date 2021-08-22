using UnityEngine;
using System;

public class COMMovableObjectVolume : ComponentKeyFrameNormal
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
		float newVolume = lerpSimple(mStart, mTarget, value);
		mComponentOwner.getComponent<COMMovableObjectAudio>().setVolume(newVolume);
	}
}