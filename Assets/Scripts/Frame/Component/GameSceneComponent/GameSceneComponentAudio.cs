using UnityEngine;
using System;
using System.Collections;

public class GameSceneComponentAudio : ComponentAudio
{
	//---------------------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		GameScene gameScene = mComponentOwner as GameScene;
		AudioSource audioSource = gameScene.getAudioSource();
		if (audioSource == null)
		{
			audioSource = gameScene.createAudioSource();
		}
		setAudioSource(audioSource);
	}
}