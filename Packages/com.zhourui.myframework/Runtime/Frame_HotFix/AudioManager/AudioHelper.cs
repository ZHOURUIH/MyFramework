using UnityEngine;
using static FrameBaseUtility;

// 音频播放的辅助物体
public class AudioHelper : MovableObject
{
	public float mRemainTime;					// 剩余的持续时间
	protected COMMovableObjectAudio mCOMAudio;	// 音频组件
	public override void init()
	{
		base.init();
		mObject.AddComponent<AudioSource>();
	}
	public override void destroy()
	{
		mObject.TryGetComponent<AudioSource>(out var audioSource);
		destroyUnityObject(audioSource);
		base.destroy();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mRemainTime = 0.0f;
		mCOMAudio = null;
	}
	// 0表示2D音效,1表示3D音效
	public void setSpatialBlend(float blend)
	{
		mCOMAudio ??= getOrAddComponent<COMMovableObjectAudio>();
		mCOMAudio.setSpatialBlend(blend);
	}
	public void setAudioEnable(bool enable)
	{
		mCOMAudio ??= getOrAddComponent<COMMovableObjectAudio>();
		mCOMAudio.setActive(enable);
	}
}