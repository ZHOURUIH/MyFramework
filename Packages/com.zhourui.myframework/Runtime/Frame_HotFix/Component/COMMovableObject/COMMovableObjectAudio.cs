using UnityEngine;

// 物体的音效组件
public class COMMovableObjectAudio : ComponentAudio
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void assignAudioSource()
	{
		var movableObject = mComponentOwner as MovableObject;
		setAudioSource(movableObject.getOrAddUnityComponent<AudioSource>());
		// 可移动物体的音效默认都是3D音效
		setSpatialBlend(1.0f);
	}
}