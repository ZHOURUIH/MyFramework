using UnityEngine;
using static UnityUtility;
using static FrameUtility;
using static MathUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

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
		if (mAudioManager == null)
		{
			return;
		}
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
		if (mAudioManager == null)
		{
			return;
		}
		AudioHelper helper = mAudioManager.getMusicHelper();
		string curAudioName = helper.getOrAddComponent<COMMovableObjectAudio>().getCurAudioName();
		CmdMovableObjectPlayAudio.execute(helper);
		if (unloadCurMusic)
		{
			mAudioManager.unload(curAudioName);
		}
	}
	public static void MUSIC(int sound)
	{
		MUSIC_Internal(sound, null, true, 1.0f);
	}
	public static void MUSIC(int sound, bool loop)
	{
		MUSIC_Internal(sound, null, loop, 1.0f);
	}
	public static void MUSIC(int sound, bool loop, float volume)
	{
		MUSIC_Internal(sound, null, loop, volume);
	}
	public static void MUSIC(string soundName)
	{
		MUSIC_Internal(0, soundName, true, 1.0f);
	}
	public static void MUSIC(string soundName, bool loop)
	{
		MUSIC_Internal(0, soundName, loop, 1.0f);
	}
	public static void MUSIC(string soundName, bool loop, float volume)
	{
		MUSIC_Internal(0, soundName, loop, volume);
	}
	// 播放3D音效
	public static AudioHelper SOUND_3D(int sound)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_3D_Internal(Vector3.zero, sound, null, 1.0f, false);
	}
	public static AudioHelper SOUND_3D(int sound, bool loop)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_3D_Internal(Vector3.zero, sound, null, 1.0f, loop);
	}
	public static AudioHelper SOUND_3D(Vector3 pos, int sound)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_3D_Internal(pos, sound, null, 1.0f, false);
	}
	public static AudioHelper SOUND_3D(Vector3 pos, int sound, bool loop)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_3D_Internal(pos, sound, null, 1.0f, loop);
	}
	public static AudioHelper SOUND_3D(int sound, float volume)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_3D_Internal(Vector3.zero, sound, null, volume, false);
	}
	public static AudioHelper SOUND_3D(Vector3 pos, int sound, float volume)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_3D_Internal(pos, sound, null, volume, false);
	}
	public static AudioHelper SOUND_3D(string soundName, float volume)
	{
		if (soundName == null)
		{
			return null;
		}
		return SOUND_3D_Internal(Vector3.zero, 0, soundName, volume, false);
	}
	public static AudioHelper SOUND_3D(Vector3 pos, string soundName, float volume)
	{
		if (soundName == null)
		{
			return null;
		}
		return SOUND_3D_Internal(pos, 0, soundName, volume, false);
	}
	// 播放2D音效
	public static AudioHelper SOUND_2D(int sound)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_2D_Internal(sound, null, 1.0f, false);
	}
	public static AudioHelper SOUND_2D(int sound, bool loop)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_2D_Internal(sound, null, 1.0f, loop);
	}
	public static AudioHelper SOUND_2D(int sound, float volume)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_2D_Internal(sound, null, volume, false);
	}
	public static AudioHelper SOUND_2D(string soundName, float volume)
	{
		if (soundName == null)
		{
			return null;
		} 
		return SOUND_2D_Internal(0, soundName, volume, false);
	}
	// 停止在obj上播放的音效
	public static void SOUND(MovableObject obj)
	{
		if (!obj.isValid())
		{
			return;
		}
		CmdMovableObjectPlayAudio.execute(obj);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void MUSIC_Internal(int sound, string soundName, bool loop, float volume)
	{
		if (mAudioManager == null)
		{
			return;
		}
		if (sound != 0)
		{
			soundName = mAudioManager.getAudioName(sound);
		}
		if (soundName.isEmpty())
		{
			logError("sound name must be valid, use void MUSIC(bool unloadCurMusic) to stop music");
			return;
		}
		playInternal(mAudioManager.getMusicHelper(), soundName, volume * mAudioManager.getMusicVolume(), loop);
	}
	// 播放音频
	protected static AudioHelper SOUND_3D_Internal(Vector3 pos, int sound, string soundName, float volume, bool loop)
	{
		if (mAudioManager == null)
		{
			return null;
		}
		if (sound != 0)
		{
			soundName = mAudioManager.getAudioName(sound);
		}
		if (soundName.isEmpty())
		{
			logError("sound name must be valid, use void SOUND(MovableObject obj) to stop sound");
			return null;
		}

		AudioHelper helper = mAudioManager.getAudioHelper(0.0f);
		helper.setSpatialBlend(1.0f);
		helper.setPosition(pos);
		playInternal(helper, soundName, volume * mAudioManager.getSoundVolume(), loop);
		return helper;
	}
	protected static AudioHelper SOUND_2D_Internal(int sound, string soundName, float volume, bool loop)
	{
		if (mAudioManager == null)
		{
			return null;
		}
		if (sound != 0)
		{
			soundName = mAudioManager.getAudioName(sound);
		}
		if (soundName.isEmpty())
		{
			logError("sound name must be valid, use void SOUND(MovableObject obj) to stop sound");
			return null;
		}

		AudioHelper helper = mAudioManager.getAudioHelper(0.0f);
		helper.setSpatialBlend(0.0f);
		playInternal(helper, soundName, volume * mAudioManager.getSoundVolume(), loop);
		return helper;
	}
	protected static void playInternal(AudioHelper helper, string soundName, float volume, bool loop)
	{
		if (isWebGL())
		{
			CmdMovableObjectPlayAudio.execute(helper, soundName, volume, loop);
			if (loop)
			{
				helper.mRemainTime = -1.0f;
			}
			else
			{
				helper.mRemainTime = mAudioManager.getAudioLength(soundName);
				if (isFloatZero(helper.mRemainTime))
				{
					logWarning("webgl中需要提前加载音频,才能设置为非循环播放,name:" + soundName);
				}
			}
		}
		else
		{
			// 如果后面需要获取音频长度,则需要提前加载
			if (!loop)
			{
				mAudioManager.loadAudio(soundName);
			}
			CmdMovableObjectPlayAudio.execute(helper, soundName, volume, loop);
			helper.mRemainTime = loop ? -1.0f : mAudioManager.getAudioLength(soundName);
		}
	}
}