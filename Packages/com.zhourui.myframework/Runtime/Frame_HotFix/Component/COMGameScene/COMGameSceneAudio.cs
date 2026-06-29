using UnityEngine;
using static FrameBaseUtility;

// 场景音效组件
public class COMGameSceneAudio : ComponentAudio
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		var gameScene = mComponentOwner as GameScene;
		setAudioSource(getOrAddComponent<AudioSource>(gameScene.getObject()));
	}
}