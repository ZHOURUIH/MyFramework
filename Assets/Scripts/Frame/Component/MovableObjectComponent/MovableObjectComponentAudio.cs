using UnityEngine;
using System;
using System.Collections;

public class MovableObjectComponentAudio : ComponentAudio
{
	//---------------------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		MovableObject movableObject = mComponentOwner as MovableObject;
		AudioSource audioSource = movableObject.getAudioSource();
		if (audioSource == null)
		{
			audioSource = movableObject.createAudioSource();
		}
		setAudioSource(audioSource);
	}
}