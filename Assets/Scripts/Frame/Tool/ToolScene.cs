using System;

// SceneTools
public class ST : FrameBase
{
	// 场景音效
	#region 播放场景音效
	public static void AUDIO()
	{
		pushCommand<CmdGameScenePlayAudio>(mGameSceneManager.getCurScene(), false);
	}
	public static void AUDIO(int sound)
	{
		AUDIO(sound, true, 1.0f);
	}
	public static void AUDIO(int sound, bool loop, float volume)
	{
		CMD(out CmdGameScenePlayAudio cmd, false);
		cmd.mSound = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static void AUDIO(string sound, bool loop, float volume)
	{
		CMD(out CmdGameScenePlayAudio cmd, false);
		cmd.mSoundFileName = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	#endregion
	// 场景音效音量
	#region 场景音效音量
	public static void AUDIO_VOLUME()
	{
		pushCommand<CmdGameSceneAudioVolume>(mGameSceneManager.getCurScene(), false);
	}
	public static void AUDIO_VOLUME(float start, float target, float onceLength, int volumeCoeSound)
	{
		AUDIO_VOLUME_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, volumeCoeSound, false, null, null);
	}
	public static void AUDIO_VOLUME(int keyframe, float start, float target, float onceLength, int volumeCoeSound, bool loop)
	{
		AUDIO_VOLUME_EX(keyframe, start, target, onceLength, volumeCoeSound, loop, null, null);
	}
	public static void AUDIO_VOLUME_EX(float start, float target, float onceLength, int volumeCoeSound, KeyFrameCallback fadeDoneCallback)
	{
		AUDIO_VOLUME_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, volumeCoeSound, false, null, fadeDoneCallback);
	}
	public static void AUDIO_VOLUME_EX(int keyframe, float start, float target, float onceLength, int volumeCoeSound, bool loop, KeyFrameCallback fadeDoneCallback)
	{
		AUDIO_VOLUME_EX(keyframe, start, target, onceLength, volumeCoeSound, loop, null, fadeDoneCallback);
	}
	public static void AUDIO_VOLUME_EX(int keyframe, float start, float target, float onceLength, int volumeCoeSound, bool loop, KeyFrameCallback fadingCallback, KeyFrameCallback fadeDoneCallback)
	{
		CMD(out CmdGameSceneAudioVolume cmd, false);
		cmd.mKeyframe = keyframe;
		cmd.mStartVolume = start;
		cmd.mTargetVolume = target;
		cmd.mSoundVolumeCoe = volumeCoeSound;
		cmd.mOnceLength = onceLength;
		cmd.mLoop = loop;
		cmd.mDoingCallback = fadingCallback;
		cmd.mDoneCallback = fadeDoneCallback;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	#endregion
}