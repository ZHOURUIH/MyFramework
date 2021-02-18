using UnityEngine;

public class AudioInfo
{
	public AudioClip mClip;
	public LOAD_STATE mState;
	public string mAudioName;   // 音效名,不含路径和后缀名
	public string mAudioPath;   // 相对于Sound的路径
	public string mSuffix;      // 后缀名
	public bool mIsResource;    // 是否为固定资源,如果为false则是通过链接加载的,可以是网络链接也可以是本地链接
}