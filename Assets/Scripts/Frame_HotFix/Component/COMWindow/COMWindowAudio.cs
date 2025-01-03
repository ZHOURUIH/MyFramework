using UnityEngine;

// UI音效组件,用于播放音效
public class COMWindowAudio : ComponentAudio
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		var window = mComponentOwner as myUIObject;
		setAudioSource(window.getOrAddUnityComponent<AudioSource>());
	}
}