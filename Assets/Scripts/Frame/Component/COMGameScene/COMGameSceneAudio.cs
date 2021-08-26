using UnityEngine;
using System;

// 场景音效组件
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