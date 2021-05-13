using System;

public class CmdMovableObjectPlayAudio : Command
{
	public int mSound;
	public string mSoundFileName;
	public float mVolume;
	public bool mUseVolumeCoe;       // 是否启用数据表格中的音量系数
	public bool mLoop;
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
		var obj = mReceiver as ComponentOwner;
		obj.getComponent(out COMMovableObjectAudio com);
		com.setActive(true);
		string soundName = mSound != 0 ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		if (mUseVolumeCoe)
		{
			mVolume *= mAudioManager.getVolumeScale(mSound);
		}
		com.play(soundName, mLoop, mVolume);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		string soundName = mSound != 0 ? mAudioManager.getAudioName(mSound) : mSoundFileName;
		builder.Append(": mSound:", mSound).
				Append(", soundName:", soundName).
				Append(", mLoop:", mLoop).
				Append(", mVolume:", mVolume);
	}
}