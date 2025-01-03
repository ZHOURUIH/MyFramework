using UnityEngine;

// 场景音效组件
public class COMGameSceneAudio : ComponentAudio
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		var gameScene = mComponentOwner as GameScene;
		if (!gameScene.getObject().TryGetComponent<AudioSource>(out var audioSource))
		{
			audioSource = gameScene.getObject().AddComponent<AudioSource>();
		}
		setAudioSource(audioSource);
	}
}