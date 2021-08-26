using System;

// 播放一个窗口音效,已弃用
public class CmdWindowPlayAudio : Command
{
	public string mSoundFileName;	// 音效文件名
	public float mVolume;			// 音量
	public int mSound;				// 音效ID
	public bool mUseVolumeCoe;		// 是否启用数据表格中的音量系数
	public bool mLoop;				// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mSound = 0;
		mSoundFileName = null;
		mVolume = 1.0f;
		mUseVolumeCoe = true;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as myUIObject;
		obj.getComponent(out COMWindowAudio com);
		com.setActive(true);
		string soundName = mSound != 0 ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		if (mUseVolumeCoe)
		{
			mVolume *= mAudioManager.getVolumeScale(mSound);
		}
		com.play(soundName, mLoop, mVolume);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		string soundName = mSound != 0 ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		builder.append(": mSound:", mSound).
				append(", soundName:", soundName).
				append(", mLoop:", mLoop).
				append(", mVolume:", mVolume).
				append(", mSoundFileName:", mSoundFileName).
				append(", mUseVolumeCoe:", mUseVolumeCoe);
	}
}