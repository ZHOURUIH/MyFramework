using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

// SceneTools
public class ST : GameBase
{
	// 场景音效
	#region 播放场景音效
	public static void AUDIO()
	{
		pushCommand<CommandGameScenePlayAudio>(mGameSceneManager.getCurScene(), false);
	}
	public static void AUDIO(SOUND_DEFINE sound)
	{
		AUDIO(sound, true, 1.0f);
	}
	public static void AUDIO(SOUND_DEFINE sound, bool loop, float volume)
	{
		CommandGameScenePlayAudio cmd = newCmd(out cmd, false);
		cmd.mSound = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static void AUDIO(string sound, bool loop, float volume)
	{
		CommandGameScenePlayAudio cmd = newCmd(out cmd, false);
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
		pushCommand<CommandGameSceneAudioVolume>(mGameSceneManager.getCurScene(), false);
	}
	public static void AUDIO_VOLUME(float start, float target, float onceLength, SOUND_DEFINE volumeCoeSound)
	{
		AUDIO_VOLUME_EX(CommonDefine.ZERO_ONE, start, target, onceLength, volumeCoeSound, false, null, null);
	}
	public static void AUDIO_VOLUME(string keyFrameName, float start, float target, float onceLength, SOUND_DEFINE volumeCoeSound, bool loop)
	{
		AUDIO_VOLUME_EX(keyFrameName, start, target, onceLength, volumeCoeSound, loop, null, null);
	}
	public static void AUDIO_VOLUME_EX(float start, float target, float onceLength, SOUND_DEFINE volumeCoeSound, KeyFrameCallback fadeDoneCallback)
	{
		AUDIO_VOLUME_EX(CommonDefine.ZERO_ONE, start, target, onceLength, volumeCoeSound, false, null, fadeDoneCallback);
	}
	public static void AUDIO_VOLUME_EX(string keyFrameName, float start, float target, float onceLength, SOUND_DEFINE volumeCoeSound, bool loop, KeyFrameCallback fadeDoneCallback)
	{
		AUDIO_VOLUME_EX(keyFrameName, start, target, onceLength, volumeCoeSound, loop, null, fadeDoneCallback);
	}
	public static void AUDIO_VOLUME_EX(string keyFrameName, float start, float target, float onceLength, SOUND_DEFINE volumeCoeSound, bool loop, KeyFrameCallback fadingCallback, KeyFrameCallback fadeDoneCallback)
	{
		CommandGameSceneAudioVolume cmd = newCmd(out cmd, false);
		cmd.mKeyFrameName = keyFrameName;
		cmd.mStartVolume = start;
		cmd.mTargetVolume = target;
		cmd.mSoundVolumeCoe = volumeCoeSound;
		cmd.mOnceLength = onceLength;
		cmd.mLoop = loop;
		cmd.mFadingCallback = fadingCallback;
		cmd.mFadeDoneCallback = fadeDoneCallback;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	#endregion
}