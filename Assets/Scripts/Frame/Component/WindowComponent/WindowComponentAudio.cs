using UnityEngine;
using System;

public class WindowComponentAudio : ComponentAudio
{
	//--------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		myUIObject window = mComponentOwner as myUIObject;
		AudioSource audioSource = window.getAudioSource();
		if (audioSource == null)
		{
			audioSource = window.createAudioSource();
		}
		setAudioSource(audioSource);
	}
}