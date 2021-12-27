using UnityEngine;

// 音频播放的辅助物体
public class AudioHelper : MovableObject
{
	public float mRemainTime;		// 剩余的持续时间
	public override void init()
	{
		base.init();
		setObject(mObjectPool.createObject(FrameDefine.R_PREFAB_PATH + "AudioHelper", 0));
	}
	public override void destroy()
	{
		mObjectPool.destroyObject(ref mObject, false);
		base.destroy();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mRemainTime = 0.0f;
	}
	// 0表示2D音效,1表示3D音效
	public void setSpatialBlend(float blend)
	{
		getComponent<COMMovableObjectAudio>().setSpatialBlend(blend);
	}
}