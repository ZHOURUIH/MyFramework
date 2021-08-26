using UnityEngine;

// 包含加载的音频信息
public class AudioInfo : FrameBase
{
	public AudioClip mClip;		// 音频资源
	public string mAudioName;	// 音效名,含路径,不带后缀名,如果是GameResources中的资源则是相对于GameResources/Sound的路径
	public bool mIsResource;	// 是否为固定资源,如果为false则是通过链接加载的,可以是网络链接也可以是本地链接
	public LOAD_STATE mState;	// 音频加载状态
	public override void resetProperty()
	{
		base.resetProperty();
		mClip = null;
		mState = LOAD_STATE.NONE;
		mAudioName = null;
		mIsResource = false;
	}
}