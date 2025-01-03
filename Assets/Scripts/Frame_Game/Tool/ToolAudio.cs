using static UnityUtility;
using static FrameBase;
using static FrameEditorUtility;

public class AT
{
	// 背景音乐和音效都可以通过音效辅助物体的方式播放,且不会有GameObject的隐藏问题
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
	// 播放2D音效
	public static AudioHelper SOUND_2D(int sound)
	{
		if (sound == 0)
		{
			return null;
		}
		return SOUND_2D_Internal(sound, null, 1.0f, false);
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
		playInternal(mAudioManager.getMusicHelper(), soundName, volume, loop);
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
		playInternal(helper, soundName, volume, loop);
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
				if (MathUtility.isFloatZero(helper.mRemainTime))
				{
					logWarning("webgl中需要提前加载音频,才能设置为非循环播放,");
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