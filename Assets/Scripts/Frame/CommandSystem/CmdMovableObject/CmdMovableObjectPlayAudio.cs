using System;
using static FrameBase;

// 播放一个物体的音频
public class CmdMovableObjectPlayAudio : Command
{
	public string mSoundFileName;	// 音效文件名,带相对于Sound的路径,带后缀
	public float mVolume;			// 播放的音量
	public int mSound;				// 音效ID,如果音效ID有效,则根据ID进行播放,否则根据音效文件名播放
	public bool mLoop;				// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mSound = 0;
		mSoundFileName = null;
		mVolume = 1.0f;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as ComponentOwner;
		obj.getComponent(out COMMovableObjectAudio com);
		com.setActive(true);
		com.play(mSound != 0 ? mAudioManager.getAudioName(mSound) : mSoundFileName, mLoop, mVolume);
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