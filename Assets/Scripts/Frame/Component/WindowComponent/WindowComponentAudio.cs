using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WindowComponentAudio : ComponentAudio
{
	//--------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		txUIObject window = mComponentOwner as txUIObject;
		AudioSource audioSource = window.getAudioSource();
		if (audioSource == null)
		{
			audioSource = window.createAudioSource();
		}
		setAudioSource(audioSource);
	}
}