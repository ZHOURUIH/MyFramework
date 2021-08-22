using UnityEngine;
using System;

public class COMMovableObjectAudio : ComponentAudio
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		var movableObject = mComponentOwner as MovableObject;
		AudioSource audioSource = movableObject.getAudioSource();
		if (audioSource == null)
		{
			audioSource = movableObject.createAudioSource();
		}
		setAudioSource(audioSource);
	}
}