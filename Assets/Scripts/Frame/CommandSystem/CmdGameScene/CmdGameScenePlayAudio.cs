using System;

// 播放场景的音效,已弃用
public class CmdGameScenePlayAudio : Command
{
	public string mSoundFileName;	// 音效名
	public float mVolume;			// 音量大小
	public int mSound;				// 音效ID,音效名和音效ID填一个即可,都填则使用ID
	public bool mUseVolumeCoe;		// 是否启用数据表格中的音量系数
	public bool mLoop;				// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mSound = 0;
		mSoundFileName = null;
		mLoop = false;
		mVolume = 1.0f;
		mUseVolumeCoe = true;
	}
	public override void execute()
	{
		var gameScene = mReceiver as GameScene;
		gameScene.getComponent(out COMGameSceneAudio com);
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
				append(", mVolume:", mVolume);
	}
}