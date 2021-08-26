using UnityEngine;

// 音频播放的辅助物体
public class AudioHelper : MovableObject
{
	public float mRemainTime;		// 剩余的持续时间
	public AudioHelper()
	{
		mAutoManageObject = true;
	}
	public override void init()
	{
		base.init();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mRemainTime = 0.0f;
		mAutoManageObject = true;
	}
	// 0表示2D音效,1表示3D音效
	public void setSpatialBlend(float blend)
	{
		getComponent<COMMovableObjectAudio>().setSpatialBlend(blend);
	}
}