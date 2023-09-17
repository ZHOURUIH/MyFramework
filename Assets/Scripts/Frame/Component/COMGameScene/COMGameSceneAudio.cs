using UnityEngine;
using System;

// 场景音效组件
public class COMGameSceneAudio : ComponentAudio
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		var gameScene = mComponentOwner as GameScene;
		AudioSource audioSource = gameScene.getObject().GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameScene.getObject().AddComponent<AudioSource>();
		}
		setAudioSource(audioSource);
	}
}