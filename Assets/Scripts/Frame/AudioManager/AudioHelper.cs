using UnityEngine;

public class AudioHelper : MovableObject
{
	public float mRemainTime;
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