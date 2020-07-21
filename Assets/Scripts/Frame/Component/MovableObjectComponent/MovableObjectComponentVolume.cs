using UnityEngine;
using System;
using System.Collections;

public class MovableObjectComponentVolume : ComponentKeyFrameNormal
{
	protected float mStartVolume;
	protected float mTargetVolume;
	public void setStartVolume(float volume) { mStartVolume = volume; }
	public void setTargetVolume(float volume) { mTargetVolume = volume; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		float newVolume = lerpSimple(mStartVolume, mTargetVolume, value);
		mComponentOwner.getComponent<MovableObjectComponentAudio>().setVolume(newVolume);
	}
}