
// 音频相关设置
public class COMGameSettingAudio : GameComponent
{
	protected float mSoundVolume;	// 音效音量大小
	protected float mMusicVolume;	// 背景音乐音量大小
	public COMGameSettingAudio()
	{
		mSoundVolume = 1.0f;
		mMusicVolume = 1.0f;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mSoundVolume = 1.0f;
		mMusicVolume = 1.0f;
	}
	// 获得成员变量
	public float getSoundVolume() { return mSoundVolume; }
	public float getMusicVolume() { return mMusicVolume; }
	// 设置成员变量
	public void setSoundVolume(float volume) { mSoundVolume = volume; }
	public void setMusicVolume(float volume) { mMusicVolume = volume; }
}