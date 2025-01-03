using UnityEngine;
using static FrameBase;

// 音频组件,用于执行播放音效相关
public class ComponentAudio : GameComponent
{
	protected AudioSource mAudioSource;		// 音频源
	protected string mAudioName;			// 音频文件名字
	public void setLoop(bool loop)
	{
		if (mAudioSource != null)
		{
			mAudioSource.loop = loop;
		}
	}
	public void setVolume(float volume)
	{
		if (mAudioSource != null)
		{
			mAudioSource.volume = volume;
		}
	}
	public string getCurAudioName() { return mAudioName; }
	public virtual void play(string name, bool isLoop, float volume)
	{
		// 先确定音频源已经设置
		if (mAudioSource == null)
		{
			assignAudioSource();
		}
		if (name.isEmpty())
		{
			stop();
			return;
		}
		mAudioSource.enabled = true;
		mAudioManager?.playClip(mAudioSource, name, isLoop, volume);
		mAudioName = name;
	}
	public void play(int sound, bool loop, float volume)
	{
		play(mAudioManager?.getAudioName(sound), loop, volume);
	}
	// 暂时停止所有通道的音效
	public void stop()
	{
		// 先确定音频源已经设置
		if(mAudioSource == null)
		{
			assignAudioSource();
		}
		mAudioManager?.stopClip(mAudioSource);
		mAudioName = null;
	}
	public bool isPlaying()
	{
		return mAudioSource != null && mAudioSource.isPlaying;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mAudioSource = null;
		mAudioName = null;
	}
	// 0表示2D音效,1表示3D音效
	public void setSpatialBlend(float blend) 
	{
		if (mAudioSource == null)
		{
			assignAudioSource();
		}
		mAudioSource.spatialBlend = blend; 
	}
	public float getSpatialBlend() 
	{
		if (mAudioSource == null)
		{
			assignAudioSource();
		}
		return mAudioSource.spatialBlend; 
	}
	public override void setActive(bool active)
	{
		base.setActive(active);
		if (mAudioSource != null)
		{
			mAudioSource.enabled = active;
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void assignAudioSource() { }
	protected void setAudioSource(AudioSource source)
	{
		mAudioSource = source;
		if (mAudioSource != null)
		{
			mAudioSource.playOnAwake = false;
		}
	}
}