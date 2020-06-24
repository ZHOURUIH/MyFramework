using UnityEngine;
using System;
using System.Collections;

public class WindowComponentAudioVolume : ComponentKeyFrameNormal
{
	protected float mStartVolume;
	protected float mTargetVolume;
	public void setStartVolume(float volume) { mStartVolume = volume; }
	public void setTargetVolume(float volume) { mTargetVolume = volume; }
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float offset)
	{
		float newVolume = lerpSimple(mStartVolume, mTargetVolume, offset);
		mComponentOwner.getComponent<WindowComponentAudio>().setVolume(newVolume);
	}
}
