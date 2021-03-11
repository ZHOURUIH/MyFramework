using UnityEngine;
using System;

public class WindowComponentAudio : ComponentAudio
{
	//--------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		var window = mComponentOwner as myUIObject;
		AudioSource audioSource = window.getAudioSource();
		if (audioSource == null)
		{
			audioSource = window.createAudioSource();
		}
		setAudioSource(audioSource);
	}
}