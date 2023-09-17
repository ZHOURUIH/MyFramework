using UnityEngine;
using static UnityUtility;
using static FrameUtility;
using static FrameBase;
using static StringUtility;

public class AT
{
	// 背景音乐和音效都可以通过音效辅助物体的方式播放,且不会有GameObject的隐藏问题
	// 只是设置AudioSource组件上的音量,而且只能设置背景音乐的音量,一般不会动态地去修改音效地音量
	// 设置的是绝对音量
	public static void MUSIC_VOLUME(float target)
	{
		MUSIC_VOLUME_EX(KEY_CURVE.ZERO_ONE, target, target, 0.0f, false, null, null);
	}
	public static void MUSIC_VOLUME(float start, float target, float onceLength)
	{
		MUSIC_VOLUME_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, null, null);
	}
	public static void MUSIC_VOLUME(int keyframe, float start, float target, float onceLength, bool loop)
	{
		MUSIC_VOLUME_EX(keyframe, start, target, onceLength, loop, null, null);
	}
	public static void MUSIC_VOLUME_EX(float start, float target, float onceLength, KeyFrameCallback fadeDoneCallback)
	{
		MUSIC_VOLUME_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, null, fadeDoneCallback);
	}
	public static void MUSIC_VOLUME_EX(int keyframe, float start, float target, float onceLength, bool loop, KeyFrameCallback fadeDoneCallback)
	{
		MUSIC_VOLUME_EX(keyframe, start, target, onceLength, loop, null, fadeDoneCallback);
	}
	public static void MUSIC_VOLUME_EX(int keyframe, float start, float target, float onceLength, bool loop, KeyFrameCallback fadingCallback, KeyFrameCallback fadeDoneCallback)
	{
		CMD(out CmdMovableObjectVolume cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mStartVolume = start;
		cmd.mTargetVolume = target;
		cmd.mOnceLength = onceLength;
		cmd.mLoop = loop;
		cmd.mDoingCallback = fadingCallback;
		cmd.mDoneCallback = fadeDoneCallback;
		pushCommand(cmd, mAudioManager.getMusicHelper());
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 通过AudioHelper播放音频,MUSIC是背景音乐,一般比较长且大多都是循环的,SOUND是音效,一般很短,且不循环
	public static void MUSIC(bool unloadCurMusic = true)
	{
		AudioHelper helper = mAudioManager.getMusicHelper();
		string curAudioName = helper.getComponent<COMMovableObjectAudio>().getCurAudioName();
		pushCommand<CmdMovableObjectPlayAudio>(helper, LOG_LEVEL.LOW);
		if (unloadCurMusic)
		{
			mAudioManager.unload(curAudioName);
		}
	}
	public static void MUSIC(int sound)
	{
		AUDIO(mAudioManager.getMusicHelper(), sound, null, true, mCOMGameSettingAudio.getMusicVolume());
	}
	public static void MUSIC(int sound, bool loop)
	{
		AUDIO(mAudioManager.getMusicHelper(), sound, null, loop, mCOMGameSettingAudio.getMusicVolume());
	}
	public static void MUSIC(int sound, bool loop, float volume)
	{
		AUDIO(mAudioManager.getMusicHelper(), sound, null, loop, volume * mCOMGameSettingAudio.getMusicVolume());
	}
	public static CmdMovableObjectPlayAudio MUSIC_DELAY(DelayCmdWatcher watcher, float delayTime)
	{
		return pushDelayCommand<CmdMovableObjectPlayAudio>(watcher, mAudioManager.getMusicHelper(), delayTime, LOG_LEVEL.LOW);
	}
	public static CmdMovableObjectPlayAudio MUSIC_DELAY(DelayCmdWatcher watcher, float delayTime, int sound)
	{
		return AUDIO_DELAY(watcher, mAudioManager.getMusicHelper(), delayTime, sound, null, false, mCOMGameSettingAudio.getMusicVolume());
	}
	public static CmdMovableObjectPlayAudio MUSIC_DELAY(DelayCmdWatcher watcher, float delayTime, int sound, float volume)
	{
		return AUDIO_DELAY(watcher, mAudioManager.getMusicHelper(), delayTime, sound, null, false, volume * mCOMGameSettingAudio.getMusicVolume());
	}
	public static CmdMovableObjectPlayAudio MUSIC_DELAY(DelayCmdWatcher watcher, float delayTime, int sound, bool loop)
	{
		return AUDIO_DELAY(watcher, mAudioManager.getMusicHelper(), delayTime, sound, null, loop, mCOMGameSettingAudio.getMusicVolume());
	}
	// 播放音效
	public static void SOUND(int sound)
	{
		if (sound == 0)
		{
			return;
		}
		SOUND(Vector3.zero, sound, null, mCOMGameSettingAudio.getSoundVolume());
	}
	public static void SOUND(Vector3 pos, int sound)
	{
		if (sound == 0)
		{
			return;
		}
		SOUND(pos, sound, null, mCOMGameSettingAudio.getSoundVolume());
	}
	public static void SOUND(int sound, float volume)
	{
		if (sound == 0)
		{
			return;
		}
		SOUND(Vector3.zero, sound, null, volume * mCOMGameSettingAudio.getSoundVolume());
	}
	public static void SOUND(Vector3 pos, int sound, float volume)
	{
		if (sound == 0)
		{
			return;
		}
		SOUND(pos, sound, null, volume * mCOMGameSettingAudio.getSoundVolume());
	}
	public static void SOUND(string sound, float volume)
	{
		if (sound == null)
		{
			return;
		}
		SOUND(Vector3.zero, 0, sound, volume * mCOMGameSettingAudio.getSoundVolume());
	}
	public static void SOUND(Vector3 pos, string sound, float volume)
	{
		if (sound == null)
		{
			return;
		}
		SOUND(pos, 0, sound, volume * mCOMGameSettingAudio.getSoundVolume());
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 播放音频
	protected static void SOUND(Vector3 pos, int sound, string soundName, float volume)
	{
		AudioHelper helper = mAudioManager.getAudioHelper(0.0f);
		helper.setPosition(pos);
		AUDIO(helper, sound, soundName, false, volume);
		// 因为可能播放时才会加载音效,所以在AUDIO_HELPER以后再设置音效时长
		helper.mRemainTime = mAudioManager.getAudioLength(sound);
	}
	protected static void AUDIO(MovableObject obj, int sound, string soundName, bool loop, float volume)
	{
		if (obj == null)
		{
			return;
		}
		if (isEmpty(sound != 0 ? mAudioManager.getAudioName(sound) : soundName))
		{
			logError("sound name must be valid, use void AUDIO(AudioHelper obj) to stop sound");
			return;
		}
		CMD(out CmdMovableObjectPlayAudio cmd, LOG_LEVEL.LOW);
		cmd.mSoundFileName = soundName;
		cmd.mSound = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		pushCommand(cmd, obj);
	}
	protected static CmdMovableObjectPlayAudio AUDIO_DELAY(DelayCmdWatcher watcher, MovableObject obj, float delayTime, int sound, string soundName, bool loop, float volume)
	{
		if (obj == null)
		{
			return null;
		}
		if (isEmpty(sound != 0 ? mAudioManager.getAudioName(sound) : soundName))
		{
			logError("sound name must be valid, use CommandMovableObjectPlayAudio AUDIO_DELAY(AudioHelper obj, float delayTime) to stop sound");
			return null;
		}
		CMD_DELAY(out CmdMovableObjectPlayAudio cmd, LOG_LEVEL.LOW);
		cmd.mSoundFileName = soundName;
		cmd.mSound = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
	}
}