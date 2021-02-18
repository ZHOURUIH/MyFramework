using UnityEngine;

public class ComponentAudio : GameComponent
{
	protected AudioSource mAudioSource;
	protected string mAudioName;
	public void setLoop(bool loop = false)
	{
		mAudioManager.setLoop(mAudioSource, loop);
	}
	public void setVolume(float vol)
	{
		mAudioManager.setVolume(mAudioSource, vol);
	}
	public string getCurAudioName() { return mAudioName; }
	public virtual void play(string name, bool isLoop, float volume)
	{
		// 先确定音频源已经设置
		if (mAudioSource == null)
		{
			assignAudioSource();
		}
		if (isEmpty(name))
		{
			stop();
			return;
		}
		mAudioManager.playClip(mAudioSource, name, isLoop, volume);
		mAudioName = name;
	}
	public void play(SOUND_DEFINE sound, bool loop, float volume)
	{
		play(mAudioManager.getAudioName(sound), loop, volume);
	}
	// 暂时停止所有通道的音效
	public void stop()
	{
		// 先确定音频源已经设置
		if(mAudioSource == null)
		{
			assignAudioSource();
		}
		mAudioManager.stopClip(mAudioSource);
	}
	public bool isPlaying()
	{
		return mAudioSource != null && mAudioSource.isPlaying;
	}
	//--------------------------------------------------------------------------------------------------------------------------
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