using UnityEngine;
using System;

public class COMGameSceneAudio : ComponentAudio
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		var gameScene = mComponentOwner as GameScene;
		AudioSource audioSource = gameScene.getAudioSource();
		if (audioSource == null)
		{
			audioSource = gameScene.createAudioSource();
		}
		setAudioSource(audioSource);
	}
}